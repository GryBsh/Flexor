Import-Module PSFlexor;

# A simple test to verify that:
# - environment variables are passed correctly
# - object output is working as expected

[pscustomobject]@{
    "Works" = $true  
    "EnvVar" = $env:EnvVar
} | Out-Json;