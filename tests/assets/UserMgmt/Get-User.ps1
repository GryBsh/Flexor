param(
    [string] $Username = $env:PARAM_Username
)

Import-Module PSFlexor;

if (-not $Username) {
    throw "Username parameter is required."
}

[pscustomobject]@{
    Username = $Username
    Email = "euser@org.local"
    FirstName = "Existing"
    LastName = "User"
} | Out-Json;