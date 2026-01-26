param(
    [string] $Username = $env:PARAM_Username
)

Import-Module PSFlexor;

if (-not $Username) {
    throw "Username parameter is required."
}

[pscustomobject]@{
    Username = $Username
    Removed = $true
} | Out-Json;