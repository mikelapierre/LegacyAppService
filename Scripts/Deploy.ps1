$ErrorActionPreference = "Stop"

$nugetPath = "$PSScriptRoot\nuget.exe"
if (!(Test-Path $nugetPath))
{
	Write-Host "Downloading nuget..." -ForegroundColor Yellow
	Invoke-WebRequest -Uri "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe" -OutFile $nugetPath
}

Write-Host "Restoring packages..." -ForegroundColor Yellow
& $nugetPath restore "$PSScriptRoot\..\LegacyAppService.sln"

$vswherePath = "$PSScriptRoot\vswhere.exe"
if (!(Test-Path $vswherePath))
{
	Write-Host "Downloading vswhere..." -ForegroundColor Yellow
	$latest = Invoke-RestMethod https://api.github.com/repos/microsoft/vswhere/releases/latest
	$latest = $latest.assets | ? { $_.name -eq "vswhere.exe" } | select -ExpandProperty browser_download_url
	Invoke-WebRequest -Uri $latest -OutFile $vswherePath
}

Write-Host "Finding Visual Studio installation..." -ForegroundColor Yellow
$vs = & $vswherePath -all -prerelease -latest -format json | ConvertFrom-Json
$msBuild = Join-Path $vs.installationPath "MSBuild"
$msBuild = Get-ChildItem -Path $msBuild -Filter "MSBuild.exe" -Recurse | select -First 1 -ExpandProperty FullName
$msBuild

Write-Host "Deploying applications..." -ForegroundColor Yellow
& $msBuild "$PSScriptRoot\..\LegacyWebApp\LegacyWebApp.csproj" /p:Configuration=Release /p:DeployOnBuild=true /p:PublishProfile=webApp /p:PublishProfileRootFolder="$PSScriptRoot\PublishProfiles"
& $msBuild "$PSScriptRoot\..\LegacyASMXApp\LegacyASMXApp.csproj" /p:Configuration=Release /p:DeployOnBuild=true /p:PublishProfile=asmxApp /p:PublishProfileRootFolder="$PSScriptRoot\PublishProfiles"
& $msBuild "$PSScriptRoot\..\LegacyWCFApp\LegacyWCFApp.csproj" /p:Configuration=Release /p:DeployOnBuild=true /p:PublishProfile=wcfApp /p:PublishProfileRootFolder="$PSScriptRoot\PublishProfiles"
& $msBuild "$PSScriptRoot\..\LegacyAPIApp\LegacyAPIApp.csproj" /p:Configuration=Release /p:DeployOnBuild=true /p:PublishProfile=apiApp /p:PublishProfileRootFolder="$PSScriptRoot\PublishProfiles"

[xml]$pubXml = gc "$PSScriptRoot\PublishProfiles\webApp.pubxml"
Write-Host "Navigate to $($pubXml.Project.PropertyGroup.SiteUrlToLaunchAfterPublish) to test" -ForegroundColor Yellow