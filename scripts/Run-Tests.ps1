[cmdletbinding()]
param (
    [switch]$Trace,
    [switch]$Run,
    [switch]$Json
)
$CWD = $PWD;
Set-Location -Path "$CWD/tests";

if ($Trace.IsPresent) {
    $PBTS = $env:BICEP_TRACING_ENABLED;
    $env:BICEP_TRACING_ENABLED = $true;    
    if ($Json.IsPresent) {
        bicep local-deploy tests.bicepparam --format json;
    }
    else {
        bicep local-deploy tests.bicepparam 1> $null;
    }
}
elseif ($Run.IsPresent) {
    if ($Json.IsPresent) {
        bicep local-deploy tests.bicepparam --format json;
    }
    else {
        bicep local-deploy tests.bicepparam;
    }
}
else {  
    ./tests.pester.ps1
} 

if ($LASTEXITCODE -ne 0) {
    Write-Error "Tests failed with exit code $LASTEXITCODE";
    Set-Location -Path $CWD;
    exit $LASTEXITCODE;
}

if ($Trace.IsPresent) {
    if ($PBTS) {
        $env:BICEP_TRACING_ENABLED = $PBTS;
    }
    else {
        Remove-Item Env:\BICEP_TRACING_ENABLED;
    }
}

Set-Location -Path $CWD;

