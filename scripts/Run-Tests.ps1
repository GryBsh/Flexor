[cmdletbinding()]
param (
    [string[]]$Test,
    [switch]$Trace,
    [switch]$Run,
    [switch]$Json
)

function Write-BicepParams {
    param(
        [string]$TemplatePath,
        [string]$Path,
        [hashtable]$Params
    )
    @"
using '$TemplatePath'

$(
    foreach ($key in $Params.Keys) {
        $value = $Params[$key]
        if ($value -is [string]) {
            "param $key = '$value'"
        } elseif ($value -is [bool] -or $value -is [int]) {
            "param $key = $value"
        } else {
            throw "Unsupported parameter type for key '$key'"
        }
    } -join "`n"
)
"@  | Out-File -FilePath $Path -Encoding UTF8 -Force;    
}

$CWD = $PWD;
$PBTS = $env:BICEP_TRACING_ENABLED;

if ($Trace.IsPresent) {
    $env:BICEP_TRACING_ENABLED = $true;
    "" > .\trace.log;
    code trace.log;

}

Set-Location -Path "$CWD/tests";

if ($Trace.IsPresent -or $Run.IsPresent) {
    foreach ($test in $Test) {
        Write-Host "TESTS: Running test: $($test)";
        Write-BicepParams -TemplatePath "$test.test.bicep" -Path "test.bicepparam" -Params @{}
        if ($Trace.IsPresent) {
            if ($Json.IsPresent) {
                bicep local-deploy test.bicepparam --format json;
            }
            else {
                bicep local-deploy test.bicepparam 1> $null 2> ../trace.log;
            }
        }
        elseif ($Run.IsPresent) {
            if ($Json.IsPresent) {
                bicep local-deploy test.bicepparam --format json;
            }
            else {
                bicep local-deploy test.bicepparam;
            }
        } 
    }
}
else {  
    Set-Location -Path $CWD;
    dotnet test;
}

if ($LASTEXITCODE -ne 0) {
    Write-Error "Tests failed with exit code $LASTEXITCODE";
    Set-Location -Path $CWD;
    exit $LASTEXITCODE;
}

if ($PBTS -ne $env:BICEP_TRACING_ENABLED) {
    $env:BICEP_TRACING_ENABLED = $PBTS;
}

Set-Location -Path $CWD;

