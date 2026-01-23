[CmdletBinding()]
param(
    [string[]]$Extension,
    [string]$OutputPath,
    [string]$Sdk,
    [string[]]$Runtimes,
    [switch]$Trace
)

$CWD = $PWD;
$PBTS = $env:BICEP_TRACING_ENABLED;
$env:BICEP_TRACING_ENABLED = $Trace.IsPresent;
try {   
        
    if (-not (Test-Path "$CWD/$OutputPath")) {
        Write-Host "Creating ``$OutputPath`` directory at $CWD/$OutputPath"
        New-Item -ItemType Directory -Path "$CWD/$OutputPath" -Force | Out-Null
    }

    foreach ($current in $Extension) {
        
        Write-Host "Building Bicep Extension ``$current``..."
         
        $currentPath = Resolve-Path $current | Select-Object -ExpandProperty Path
        $current = [System.IO.Path]::GetFileName($currentPath);
        Set-Location $currentPath       
        
        Write-Host "Cleaning and restoring project dependencies for ``$current``"

        dotnet restore --configfile $CWD/nuget.config
        dotnet clean

        foreach ($rid in $Runtimes) {
            Write-Host "Publishing ``$current`` for runtime: $rid"
            dotnet publish --configuration release -r $rid .
        }

        $publishArguments = @(
            foreach ($rid in $Runtimes) {
                if ($rid.Contains("win")) {
                    $ext = "$current.exe"
                } else {
                    $ext = $current
                }
                "--bin-$rid $PWD/bin/release/$Sdk/$rid/publish/$ext" 
            }
        )

        $target = "$CWD/$OutputPath/$($current.ToLowerInvariant())";
        $publishArguments = $($publishArguments -join " ");

        Write-Verbose "Publish Arguments: $publishArguments"
        Write-Verbose "Target: $target"

        Write-Host "Publishing Bicep Extension ``$current`` to $target"
        .$([scriptblock]::Create("bicep publish-extension $publishArguments --target $target --force"))
        
        Set-Location $CWD
    }
    Write-Host "Finished building Bicep Extensions."
} finally {
    Set-Location $CWD
    $env:BICEP_TRACING_ENABLED = $PBTS;
}

dotnet restore --configfile $CWD/nuget.config