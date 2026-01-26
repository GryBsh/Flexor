[cmdletbinding()]
param (
    [switch]$Trace,
    [switch]$Run,
    [switch]$Json
)
$CWD = $PWD;
Set-Location -Path "$CWD/tests";

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

$tests = Get-Item -Path "*.test.bicep";

if ($Trace.IsPresent -or $Run.IsPresent) {
    foreach ($test in $tests) {
        Write-BicepParams -TemplatePath $test.Name -Path "test.bicepparam" -Params @{}
        if ($Trace.IsPresent) {
            $PBTS = $env:BICEP_TRACING_ENABLED;
            $env:BICEP_TRACING_ENABLED = $true;    
            if ($Json.IsPresent) {
                bicep local-deploy test.bicepparam --format json;
            }
            else {
                bicep local-deploy test.bicepparam 1> $null;
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
    Import-Module Pester;
    Invoke-Pester -Path ./tests.pester.ps1 -Output Detailed;
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

