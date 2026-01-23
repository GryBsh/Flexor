param(
    [string] $Username = $env:PARAM_Username,
    [string] $Email = $env:PARAM_Email,
    [string] $FirstName = $env:PARAM_FirstName,
    [string] $LastName = $env:PARAM_LastName
)

Import-Module PSFlexor;

$param = Test-MissingParameters -Params $PSBoundParameters;
if ($param.Missing) {
    throw "Missing required parameters: $($param.Parameters -join ', ')"
}

[pscustomobject]@{
    Username = $Username
    Email = $Email
    FirstName = $FirstName
    LastName = $LastName
} | Out-Json;