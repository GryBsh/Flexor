targetScope = 'local'
extension flexor with {
  pathRoot: '../.bicep/flexor'
  logOptions: {
    disableRollover: true
  }
}

// Clean up any previous test artifacts
resource cleanup 'Flexor/script@2026-01-01' = {
  name: 'cleanup'
  shell: 'PowerShell'
  script: 'assets/cleanup.ps1'
}



resource bicep 'Flexor/command@2026-01-01' = {
  name: 'bicep'
  command: 'bicep'
  env: {} 
  args: [
    'build'
    'assets/test.az.bicep'
    '--outdir'
    'output'
  ]
  dependsOn: [cleanup]
}

resource stringOutput 'Flexor/command@2026-01-01' = {
  name: 'stringOutput'
  command: 'pwsh'
  args: [
    '-c'
    '"&{ dir assets | % { $_.FullName } }"'
  ]  
  dependsOn: [cleanup] 
}

output bicepOutputLength int = length(bicep.output)
output stringOutput string = stringOutput.output
