param(
    [switch]$Trace,
    [switch]$Run,
    [switch]$Json
)

scripts/Run-Tests.ps1 -Trace:$Trace.IsPresent -Run:$Run.IsPresent -Json:$Json.IsPresent