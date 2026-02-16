param(
    [string[]]$Tests = @(),
    [switch]$Trace,
    [switch]$Run,
    [switch]$Json
)

if ($Tests.Count -eq 0) {
    $Tests = Get-ChildItem -Path . -Filter *.test.bicep | ForEach-Object { $_.BaseName -replace '\.test$', '' }
}

scripts/Run-Tests.ps1 -Test $Tests -Trace:$Trace.IsPresent -Run:$Run.IsPresent -Json:$Json.IsPresent