BeforeAll {

    <#
    .SYNOPSIS
        Invokes Bicep tests and retrieves outputs.
    #>
    function Invoke-TestBicep {
        param (
            [string]$Path
        )
        
        Write-BicepParams -TemplatePath $Path -Path "test.bicepparam" -Params @{}
        
        try {
            return $(bicep local-deploy test.bicepparam --format json | ConvertFrom-Json | ForEach-Object outputs);
        }
        catch {
            throw "Test failed: $($_.Exception.Message)";
        }
    }

    function Get-AssetList {
        @((Get-ChildItem -Path "assets").FullName) -join "`n"
    }

    filter Remove-TerminalLineBreaks {
        return $input -replace "`r`n", "";
    }
}

Describe "Commands" {
    It "should return expected outputs" {
        $outputs = Invoke-TestBicep -Path "commands.test.bicep";
        
        $outputs | Should -Not -BeNullOrEmpty;
        
        $outputs.bicepOutputLength   | Should -Be 0;
        
        # We need to remove terminal line breaks for the comparison
        ($outputs.stringOutput | Remove-TerminalLineBreaks) `
        | Should -Be $(Get-AssetList);
    }
}

Describe "Git" {
    It "should clone repository successfully" {
        $outputs = Invoke-TestBicep -Path "git.test.bicep";
        
        $outputs | Should -Not -BeNullOrEmpty;
        
        #$outputs.clonePath  | Should -Be $(Get-Item -Path "output/Flexor").FullName;
        #$outputs.pullPath   | Should -Be $(Get-Item -Path "output/Flexor").FullName;
        #$outputs.regPulledPath   | Should -Be $(Get-Item -Path "output/Flexor").FullName;
    }
}

Describe "HTTP" {
    It "should perform HTTP operations successfully" {
        $outputs = Invoke-TestBicep -Path "http.test.bicep";
        
        $outputs | Should -Not -BeNullOrEmpty;
        
        $outputs.getStatusCode     | Should -Be 200;
        $outputs.postStatusCode    | Should -Be 200;
    }
}

Describe "Scripts" {
    It "should execute scripts successfully" {
        $outputs = Invoke-TestBicep -Path "scripts.test.bicep";
        
        $outputs | Should -Not -BeNullOrEmpty;
        
        $outputs.scriptLiteralOutput | Should -Be "Hello from script literal!";
        
        $outputs.pwshResult          | Should -Be '{"Works":true,"EnvVar":"Set from Bicep"}';
        $outputs.pwshWorks           | Should -Be $true;

        $outputs.bashResult          | Should -Be '{"Works":true}';
        $outputs.bashWorks           | Should -Be $true;

        $outputs.pythonResult        | Should -Be '{"Works":true,"EnvVar":"Set from Bicep"}';
        $outputs.pythonWorks         | Should -Be $true;
        
        $outputs.pythonFileResult    | Should -Be 'Works!';
    }
}

Describe "Modules" {
    It "should create and use modules successfully" {
        $outputs = Invoke-TestBicep -Path "modules.test.bicep";
        
        $outputs | Should -Not -BeNullOrEmpty;

        $outputs.createdUsername    | Should -Be "tuser";
        $outputs.createdEmail       | Should -Be "tuser@org.local";
        $outputs.createdName        | Should -Be "Test User";
        
        $outputs.existingUsername   | Should -Be "euser";
        $outputs.existingEmail      | Should -Be "euser@org.local";
        $outputs.existingName       | Should -Be "Existing User";
    }
}