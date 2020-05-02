function New-ClientSecret
{
    $Bytes = New-Object Byte[] 32
    $Random = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    $Random.GetBytes($Bytes)
    $Random.Dispose()
    return [System.Convert]::ToBase64String($Bytes)
}

function New-WebAppSuffix
{
    return [System.Guid]::NewGuid().ToString("N")
}

function New-WebApp
{
    param (
        [string]$WebAppName,
        [string]$AppServicePlan,
        [string]$ResourceGroupName,
        [string]$AADTenandId
	)

    Write-Host "Creating Azure AD application $WebAppName..." -ForegroundColor Yellow
    $webAppUrl = "https://$WebAppName.azurewebsites.net"
    $webAppReplyUrl = "$webAppUrl/.auth/login/aad/callback"
    $webAppClientSecret = New-ClientSecret
    $resourceAccess = @(@{"resourceAppId" = "00000002-0000-0000-c000-000000000000" 
                          "resourceAccess" = @(@{
                                "id" = "311a71cc-e848-46a1-bdf8-97ff7156d8e6" 
                                "type" = "Scope"
                          })})
    $resourceAccess = (ConvertTo-Json $resourceAccess -Compress -Depth 3) -Replace '"', '\"'
    $adApp = az ad app create --display-name "$WebAppName" --password "$webAppClientSecret" --reply-urls "https://localhost:44357" "$webAppReplyUrl" --required-resource-accesses "$resourceAccess" -o json | ConvertFrom-Json
    Write-Host ($adApp | ConvertTo-Json)

    Write-Host "Creating web application $WebAppName..." -ForegroundColor Yellow
    Write-Host (az webapp create -n "$WebAppName" -g "$ResourceGroupName" -p "$AppServicePlan" -o json | ConvertFrom-Json | ConvertTo-Json)
    Write-Host "Configuring authentication for $WebAppName..." -ForegroundColor Yellow
    Write-Host (az webapp auth update -n "$WebAppName" -g "$ResourceGroupName" --enabled true --action LoginWithAzureActiveDirectory --aad-client-id "$($adApp.AppId)" `
                    --aad-allowed-token-audiences "$webAppReplyUrl" --aad-token-issuer-url "https://sts.windows.net/$AADTenandId/" --token-store true -o json | ConvertFrom-Json | ConvertTo-Json)
    Write-Host "Assigning managed identity to $WebAppName..." -ForegroundColor Yellow
    Write-Host (az webapp identity assign -n "$WebAppName" -g "$ResourceGroupName" -o json | ConvertFrom-Json | ConvertTo-Json)

    return @{ "ClientID" = $adApp.AppId; "ClientSecret" = $webAppClientSecret; }
}

function New-APIApp
{
    param (
        [string]$APIAppName,
        [string]$AppServicePlan,
        [string]$ResourceGroupName,
        [string]$AADTenandId,
        [string]$WebAppClientID
    )

    Write-Host "Creating Azure AD application $APIAppName..." -ForegroundColor Yellow
    $apiAppUrl = "https://$APIAppName.azurewebsites.net"
    $adApp = az ad app create --display-name "$APIAppName" --identifier-uris "$apiAppUrl" -o json | ConvertFrom-Json
    Write-Host ($adApp | ConvertTo-Json)

    Write-Host "Pre-authorizing web and Azure CLI applications (this takes some time)..." -ForegroundColor Yellow
    $permissionIds = $adApp.oauth2Permissions | select -ExpandProperty id
    $preAuthorizedApplication = (@{"api" = @{
                                    "preAuthorizedApplications" = 
                                    @(@{
                                        "appId" = "$WebAppClientID"
                                        "permissionIds" = @($permissionIds)
                                    },
                                    @{
                                        "appId" = "04b07795-8ddb-461a-bbee-02f9e1bf7b46"
                                        "permissionIds" = @($permissionIds)
                                    })}} | ConvertTo-Json -Compress -Depth 4) -Replace '"', '\"'
    $job = Start-Job -ArgumentList @("https://graph.microsoft.com/beta/applications/$($adApp.objectId)", $preAuthorizedApplication, $WebAppClientID) `
        -ScriptBlock {
            $url = $args[0];
            $preAuthorizedApplication = $args[1]
            $WebAppClientID = $args[2]
            do 
            {
                az rest -m PATCH -u "$url" --headers "Content-Type=application/json" -b "$preAuthorizedApplication"
                $restApp = az rest -m GET -u "$url" -o json | ConvertFrom-Json
                $appId = $restApp | select -ExpandProperty api | select -ExpandProperty preAuthorizedApplications | select -ExpandProperty appId | ? { $_ -eq "$WebAppClientID" }
                if ($appId -ne $WebAppClientID) { Sleep -Seconds 1 }
            }
            while ($appId -ne $WebAppClientID)
            Write-Host ($restApp | ConvertTo-Json -Depth 3)
        } | Wait-Job 
    Write-Host ($job.ChildJobs[0].Information)
    Remove-Job -Job $job

    Write-Host "Creating web application $APIAppName..." -ForegroundColor Yellow
    Write-Host (az webapp create -n "$APIAppName" -g "$ResourceGroupName" -p "$AppServicePlan" -o json | ConvertFrom-Json | ConvertTo-Json)
    Write-Host "Configuring authentication for $APIAppName..." -ForegroundColor Yellow
    Write-Host (az webapp auth update -n "$APIAppName" -g "$ResourceGroupName" --enabled true --action LoginWithAzureActiveDirectory --aad-client-id "$($adApp.AppId)" `
                    --aad-allowed-token-audiences "$apiAppUrl" --aad-token-issuer-url "https://sts.windows.net/$AADTenandId/" -o json | ConvertFrom-Json | ConvertTo-Json)

    return @{ "AppURI" = $apiAppUrl }
}

function Update-AppSettings
{
    param (
        [string]$ConfigFile,
        [hashtable]$AppSettings
    )

    [xml]$configXml = gc $ConfigFile
    foreach($add in $configXml.configuration.appSettings.add)
    {
        if ($AppSettings.Contains($add.key))
        {
            $add.Value = $AppSettings[$add.key]
		}
    }
    $configXml.Save($ConfigFile)
}

function New-PublishProfile
{
    param (
        [string]$AppName,
        [string]$ResourceGroupName,
        [string]$OutputFile
    )

    $publishProfile = az webapp deployment list-publishing-profiles -n "$AppName" -g "$ResourceGroupName" -o json | ConvertFrom-Json
    $publishProfile = $publishProfile | ? { $_.publishMethod -eq "MSDeploy"  }

    [xml]$pubXml = gc "$PSScriptRoot\PublishProfile.xml"
    $pubXml.Project.PropertyGroup.SiteUrlToLaunchAfterPublish =  $publishProfile.destinationAppUrl
    $pubXml.Project.PropertyGroup.MSDeployServiceURL =  $publishProfile.publishUrl
    $pubXml.Project.PropertyGroup.DeployIisAppPath =  $publishProfile.msdeploySite
    $pubXml.Project.PropertyGroup.UserName =  $publishProfile.userName
    $pubXml.Project.PropertyGroup.Password = $publishProfile.userPWD 
    $pubXml.Save($OutputFile)
}