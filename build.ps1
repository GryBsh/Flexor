param(
    [string]$Sdk = "net10.0",
    [string[]]$Runtimes = @(
        "osx-arm64", 
        "linux-x64", 
        "linux-arm64",
        "win-x64",
        "win-arm64"
    ),
    [string]$OutputPath = ".bicep",
    [string]$AssetFolders =@(
        "src/.bicep"
    ),
    [string]$FlexorPath = "flexor",
    [switch]$SkipBuild,
    [switch]$Trace
)

$CWD = $PWD;
$BicepBin = Join-Path $OutputPath "bin";

Remove-Item "$CWD/$OutputPath" -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue;

Write-Host "Copying assets to ``$OutputPath``";
foreach ($folder in $AssetFolders) {
    Write-Host " - Copying $folder to $OutputPath/";
    Copy-Item "$CWD/$folder/" "$CWD/" -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue;
}
New-Item -Path "$CWD/$OutputPath/readme" -ItemType Directory -ErrorAction SilentlyContinue | Out-Null;
Copy-Item "$CWD/README.md" "$CWD/$OutputPath/readme/Flexor.md" -Force;

Write-Host "Ensuring Bicep from source...";
$Bicep = .\scripts\Get-Bicep.ps1 -DestinationPath "repos/bicep";
    
$env:PATH = "$(Resolve-Path $Bicep.CliPath)$([System.IO.Path]::PathSeparator)$($env:PATH)";
$env:BICEP_LANGUAGE_SERVER_PATH = "$(Resolve-Path $Bicep.LangServerPath)";

if (-not $SkipBuild) {
    Get-Item "$env:USERPROFILE/.nuget/packages/Azure.Bicep.*/*-g*" | Remove-Item -Recurse -Force;
    Write-Host "Building Flexor extension to $BicepBin for SDK $Sdk and runtimes: $($Runtimes -join ', ') to $BicepBin";
    scripts/Build-Extension.ps1 -Extension src/Flexor -OutputPath $BicepBin -Sdk $Sdk -Runtimes $Runtimes -Trace:$Trace.IsPresent;
}