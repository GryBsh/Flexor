filter Out-Json {
    @($input) | ForEach-Object {
        if ($_ -is [string]) { $_ }
        else { $_ | ConvertTo-Json -Compress -Depth 100 }
    }
}


function Test-MissingParameters {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [hashtable]$Params
    )

    $missingParams = @()
    foreach ($key in $Params.Keys) {
        if ($null -eq $Params[$key] -or [string]::IsNullOrWhiteSpace(($Params[$key]))) {
            $missingParams += $key
        }
    }
    if ($missingParams.Count -gt 0) {
        return [pscustomobject]@{
            Missing = $true
            Parameters = $missingParams
        }
    }
}