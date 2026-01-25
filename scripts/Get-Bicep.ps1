[cmdletbinding()]
param(
    [string]$DestinationPath = "repos/bicep",
    [scriptblock]$Script
)

$ErrorActionPreference = "Stop";

$CWD = $PWD;
$slash = [System.IO.Path]::DirectorySeparatorChar;

$parentDir = Split-Path -Parent $DestinationPath
if (-not (Test-Path $parentDir)) {
    New-Item -ItemType Directory -Path $parentDir -Force | Out-Null;
}
if (-not (Test-Path $DestinationPath)) {
    git clone "https://github.com/Azure/bicep.git" $DestinationPath --tags "v0.40.1";
    Set-Location $DestinationPath;
    Write-Host "Bicep source code cloned to $DestinationPath";    
}
else {
    Set-Location $DestinationPath;
    git pull;
    Write-Host "Bicep source code updated in $DestinationPath";
}

$bicepExePath = [System.IO.Path]::Join($DestinationPath,"src/Bicep.Cli/bin/Debug/net10.0").Replace("\","$slash").Replace("/","$slash");
$bicepLangServerPath = [System.IO.Path]::Join($DestinationPath,"src/Bicep.LangServer/bin/Debug/net10.0").Replace("\","$slash").Replace("/","$slash");

if (-not (Test-Path $bicepExePath)) {
    Write-Host "Building Bicep...";
    dotnet build;
}

if (-not (Test-Path "$DestinationPath/bin")) {
    Write-Host "Packing Bicep NuGet packages...";
    dotnet pack -o ./bin;
}

Set-Location $CWD;

@"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="BicepLocal" value="$DestinationPath/bin" />
  </packageSources>
</configuration>
"@ | Out-File -FilePath nuget.config -Encoding utf8 -Force;
Write-Host "NuGet config created";

return [PSCustomObject]@{
    CliPath = $bicepExePath;
    LangServerPath = $bicepLangServerPath;
}