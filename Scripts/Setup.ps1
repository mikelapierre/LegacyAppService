$ErrorActionPreference = "Stop"

Import-Module -Name "$PSScriptRoot\Setup.psm1" -Force

$requestContext = $true
try
{
    az account show
    $requestContext = ((Read-Host -Prompt "Enter Y if the above Azure Active Directory tenant and Azure subscription are correct") -ne "Y")
}
catch
{
    Write-Debug "az account show failed"
}
if ($requestContext)
{
    $aadTenant = Read-Host -Prompt "Enter your Azure Active Directory tenant name or ID"
    az login -t "$aadTenant" --only-show-errors
    $subscription  = Read-Host -Prompt "Enter your Azure subscription name or ID"
    az account set -s "$subscription"
}
$context = az account show -o json | ConvertFrom-Json
$aadTenantID = $context.tenantId

$resourceGroupName = Read-Host -Prompt "Enter the Resource Group name or hit Enter for the default (legacywebapp)"
if (!$resourceGroupName) { $resourceGroupName = "legacywebapp" }
$location = Read-Host -Prompt "Enter the location or hit Enter for the default (centralus)"
if (!$location) { $location = "centralus" }
$appServicePlanName = Read-Host -Prompt "Enter the App Service Plan name or hit Enter for the default (legacywebappplan)"
if (!$appServicePlanName) { $appServicePlanName = "legacywebappplan" }

$webAppSufix = New-WebAppSuffix
Write-Host "The following suffix will be used: $webAppSufix" -ForegroundColor Yellow

$webAppName = "legacywebapp-$webAppSufix"
$asmxAppName = "legacyasmxapp-$webAppSufix"
$wcfAppName = "legacywcfapp-$webAppSufix"
$apiAppName = "legacyapiapp-$webAppSufix"

Write-Host "Creating resource group $resourceGroupName..." -ForegroundColor Yellow
az group create -n "$resourceGroupName" -l "$location"
Write-Host "Creating App Service plan $appServicePlanName..." -ForegroundColor Yellow
az appservice plan create -n "$appServicePlanName" -g "$resourceGroupName" --sku "FREE"

$webApp = New-WebApp -WebAppName $webAppName -AppServicePlan $appServicePlanName -ResourceGroupName $resourceGroupName -AADTenandId $aadTenantID
$asmxApp = New-APIApp -APIAppName $asmxAppName -AppServicePlan $appServicePlanName -ResourceGroupName $resourceGroupName -AADTenandId $aadTenantID -WebAppClientID $webApp.ClientID
$wcfApp = New-APIApp -APIAppName $wcfAppName -AppServicePlan $appServicePlanName -ResourceGroupName $resourceGroupName -AADTenandId $aadTenantID -WebAppClientID $webApp.ClientID
$apiApp = New-APIApp -APIAppName $apiAppName -AppServicePlan $appServicePlanName -ResourceGroupName $resourceGroupName -AADTenandId $aadTenantID -WebAppClientID $webApp.ClientID

Write-Host "Updating appSettings for $webAppName..." -ForegroundColor Yellow
$appSettings = @{ 
	"ClientID" = "$($webApp.ClientID)";
	"ClientSecret" = "$($webApp.ClientSecret)";
	"TenantID" = "$aadTenantID";
	"ASMXURL" = "$($asmxApp.AppURI)/IdentityService.asmx"; 
	"ASMXAppURI" = "$($asmxApp.AppURI)";
	"WCFURL" = "$($wcfApp.AppURI)/IdentityService.svc"; 
	"WCFAppURI" = "$($wcfApp.AppURI)"; 
	"WebAPIURL" = "$($apiApp.AppURI)"; 
	"WebAPIAppURI" = "$($apiApp.AppURI)"  
} 
$appSettings | ConvertTo-Json | Set-Content "appsettings.json"
az webapp config appsettings set -n "$webAppName" -g "$resourceGroupName" --settings `@appsettings.json
Remove-Item "appsettings.json"

Write-Host "Updating local configuration files..." -ForegroundColor Yellow
$appSettings.Remove("ASMXURL")
$appSettings.Remove("WCFURL")
$appSettings.Remove("WebAPIURL")
ConvertTo-Json $appSettings | Set-Content -Path "$PSScriptRoot\..\LegacyWebApp\settings.user"
ConvertTo-Json @{ "Audience" = "$($asmxApp.AppURI)"; "TenantID" = "$aadTenantID" } | Set-Content -Path "$PSScriptRoot\..\LegacyASMXApp\settings.user"
ConvertTo-Json @{ "Audience" = "$($wcfApp.AppURI)"; "TenantID" = "$aadTenantID" } | Set-Content -Path "$PSScriptRoot\..\LegacyWCFApp\settings.user"
ConvertTo-Json @{ "Audience" = "$($apiApp.AppURI)"; "TenantID" = "$aadTenantID" } | Set-Content -Path "$PSScriptRoot\..\LegacyAPIApp\settings.user"

Write-Host "Generating publish profiles..." -ForegroundColor Yellow
$publishProfilesFolder = "$PSScriptRoot\PublishProfiles"
New-Item -Path $publishProfilesFolder -ItemType Directory -Force | Out-Null
New-PublishProfile -AppName $webAppName -ResourceGroupName $resourceGroupName -OutputFile "$publishProfilesFolder\webapp.pubxml"
New-PublishProfile -AppName $asmxAppName -ResourceGroupName $resourceGroupName -OutputFile "$publishProfilesFolder\asmxApp.pubxml"
New-PublishProfile -AppName $wcfAppName -ResourceGroupName $resourceGroupName -OutputFile "$publishProfilesFolder\wcfApp.pubxml"
New-PublishProfile -AppName $apiAppName -ResourceGroupName $resourceGroupName -OutputFile "$publishProfilesFolder\apiApp.pubxml"