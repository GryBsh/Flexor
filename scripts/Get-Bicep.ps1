[cmdletbinding()]
param(
    [string]$DestinationPath = "repos/bicep",
    [string]$Branch = "main",
    [string[]]$Tags = @("v0.40.2"),
    [string]$Sdk = "net10.0",
    [switch]$Latest,
    [switch]$ForceClone,
    [switch]$ForceBuild
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


if ($ForceClone.IsPresent -or -not (Test-Path $DestinationPath)) {
    if ((Test-Path $DestinationPath)) {
        Remove-Item -Recurse -Force $DestinationPath;
        Write-Host "Removed existing path $DestinationPath";
    }
    if ($Latest -or $Tags.Count -eq 0) {
        Write-Host "Cloning latest Bicep source code ...";
        $gitCmd = "git clone `"https://github.com/Azure/bicep.git`" ${DestinationPath}"
    }
    else {
        Write-Host "Cloning Bicep source code with tags: $($Tags -join ", ") ...";
        $gitCmd = "git clone `"https://github.com/Azure/bicep.git`" ${DestinationPath} -t $($Tags -join " ")"
    }
    Invoke-Expression $gitCmd;    
    Write-Host "Bicep source code cloned to $DestinationPath";    
    Set-Location $DestinationPath;
}
else {
    Set-Location $DestinationPath;
    git pull;
    Write-Host "Bicep source code updated in $DestinationPath";
}

if ($ForceBuild.IsPresent -or -not (Test-Path $bicepExePath)) {
    Write-Host "Building Bicep...";
    dotnet build;
}

if ($ForceBuild.IsPresent -or -not (Test-Path $bicepLocalRepo)) {
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

Get-Item "$env:USERPROFILE/.nuget/packages/Azure.Bicep.*/*-g*" | Remove-Item -Recurse -Force;

$env:PATH = "$(Resolve-Path $bicepExePath)$([System.IO.Path]::PathSeparator)$($env:PATH)";
$env:BICEP_LANGUAGE_SERVER_PATH = "$(Resolve-Path $bicepLangServerPath)";