Import-Module Pester;
    

BeforeAll {
    
    <#
    .SYNOPSIS
        Invokes Bicep tests and retrieves outputs.
    #>
    function Invoke-BicepTests {
        $bicepTestCommand = {
            bicep local-deploy tests.bicepparam --format json | ConvertFrom-Json | ForEach-Object outputs;
        }.ToString().Trim();
        Write-Host "Executing Bicep Tests...";
        $outputs = Invoke-Expression $bicepTestCommand
        if ($LASTEXITCODE -ne 0) {
            throw "Bicep tests failed with exit code $LASTEXITCODE"
        }
        return $outputs
    }

    function Get-ScriptList {
        @((Get-ChildItem -Path "scripts").FullName) -join "`n"
    }

    filter Remove-TerminalLineBreaks {
        return $input -replace "`r`n", "";
    }
}

Describe "Bicep Tests" {
    It "should return expected outputs" {
        $outputs = Invoke-BicepTests
        
        $outputs | Should -Not -BeNullOrEmpty;
        
        $outputs.scriptLiteralOutput | Should -Be "Hello from script literal!";
        $outputs.httpStatusCode      | Should -Be 200
        $outputs.pwshResult          | Should -Be '{"Works":true,"EnvVar":"Set from Bicep"}';
        $outputs.pwshWorks           | Should -Be $true;
        $outputs.bashResult          | Should -Be '{"Works":true}';
        $outputs.bashWorks           | Should -Be $true;
        $outputs.pythonResult        | Should -Be '{"Works":true,"EnvVar":"Set from Bicep"}';
        $outputs.pythonWorks         | Should -Be $true;
        
        $outputs.bicepOutputLength   | Should -Be 0;
        
        # We need to remove terminal line breaks for the comparison
        ($outputs.stringOutput | Remove-TerminalLineBreaks) `
        | Should -Be $(Get-ScriptList);
        
        $outputs.createdUsername    | Should -Be "tuser";
        $outputs.createdEmail       | Should -Be "tuser@org.local";
        $outputs.createdName        | Should -Be "Test User";
        
        $outputs.existingUsername   | Should -Be "euser";
        $outputs.existingEmail      | Should -Be "euser@org.local";
        $outputs.existingName       | Should -Be "Existing User";
    }
}