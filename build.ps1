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
    [switch]$Trace,
    [switch]$Package
)

$CWD = $PWD;
$BicepBin = Join-Path $OutputPath "bin";
$AssetsPath = Join-Path $PSScriptRoot "src/assets";

Remove-Item "$CWD/$OutputPath" -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue;

Write-Host "Copying assets to ``$OutputPath``";

Write-Host " - Copying assets to $OutputPath/";
Copy-Item "$AssetsPath/*" "$CWD/" -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue;

New-Item -Path "$CWD/$OutputPath/readme" -ItemType Directory -ErrorAction SilentlyContinue | Out-Null;
Copy-Item "$CWD/README.md" "$CWD/$OutputPath/readme/Flexor.md" -Force;

Write-Host "Building Flexor extension to $BicepBin for SDK $Sdk and runtimes: $($Runtimes -join ', ') to $BicepBin";
scripts/Build-Extension.ps1 -Extension src/Flexor -OutputPath $BicepBin -Sdk $Sdk -Runtimes $Runtimes -Trace:$Trace.IsPresent;

if ($Package.IsPresent) {
    Write-Host "Packaging Flexor Template to flexor-template.zip";   
    New-Item -Path "$CWD/temp-package" -ItemType Directory -ErrorAction SilentlyContinue | Out-Null;
    Copy-Item -Path "$CWD/$OutputPath/" -Destination "$CWD/temp-package/" -Recurse -Force;
    Copy-Item -Path "$AssetsPath/bicepconfig.json" -Destination "$CWD/temp-package/" -Force;
    Compress-Archive -Path "$CWD/temp-package/*" -DestinationPath "$CWD/flexor-template.zip" -Force;
    Remove-Item "$CWD/temp-package" -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue;
}