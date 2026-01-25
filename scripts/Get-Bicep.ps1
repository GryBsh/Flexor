[cmdletbinding()]
param(
    [string]$DestinationPath = "repos/bicep",
    [string[]]$Tags = @("v0.40.1"),
    [string]$Sdk = "net10.0",
    [switch]$Latest,
    [switch]$Force
)
$ErrorActionPreference = "Stop";

$CWD = $PWD;

filter Convert-Path {
    $slash = [System.IO.Path]::DirectorySeparatorChar;
    return $input.Replace("\","$slash").Replace("/","$slash");
}

$bicepExePath = [System.IO.Path]::Join($DestinationPath,"src/Bicep.Cli/bin/Debug/$Sdk") | Convert-Path;
$bicepLangServerPath = [System.IO.Path]::Join($DestinationPath,"src/Bicep.LangServer/bin/Debug/$Sdk") | Convert-Path;
$bicepLocalRepo = [System.IO.Path]::Join($DestinationPath,"bin") | Convert-Path;

$parentDir = Split-Path -Parent $DestinationPath
if (-not (Test-Path $parentDir)) {
    New-Item -Path $parentDir -ItemType Directory -Force | Out-Null;
    Write-Host "Path $parentDir created.";
}
else {
    Write-Host "Path $parentDir exists.";
}

$gitArgs = @();

if ($Latest -or $Tags.Count -eq 0) {
    Write-Host "Cloning latest Bicep source code...";
}
else {
    Write-Host "Cloning Bicep source code with tags: $($Tags -join ", ")...";
    $gitArgs += "--tags $($Tags -join " ")"
}

$gitCmdArgs = $gitArgs -join " ";

if (-not (Test-Path $DestinationPath)) {
    git clone "https://github.com/Azure/bicep.git" $DestinationPath $gitCmdArgs;
    Write-Host "Bicep source code cloned to $DestinationPath";    
}
else {
    Set-Location $DestinationPath;
    git pull $gitCmdArgs;
    Write-Host "Bicep source code updated in $DestinationPath";
}

Set-Location $DestinationPath;
if ($Force.IsPresent -or -not (Test-Path $bicepExePath)) {
    Write-Host "Building Bicep...";
    dotnet build;
}

if ($Force.IsPresent -or -not (Test-Path $bicepLocalRepo)) {
    Write-Host "Packing Bicep NuGet packages...";
    dotnet pack -o ./bin;
}
Set-Location $CWD;

@"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
<packageSources>
    <add key="BicepLocal" value="$bicepLocalRepo" />
</packageSources>
</configuration>
"@  | Out-File -FilePath nuget.config -Encoding utf8 -Force;

return [PSCustomObject]@{
    CliPath = $bicepExePath;
    LangServerPath = $bicepLangServerPath;
}