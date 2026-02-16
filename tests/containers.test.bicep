targetScope = 'local'
extension flexor with {
  flexorPath: '../.bicep/flexor'
  logOptions: {
    disableRollover: true
  }
}

resource stringOutput 'Flexor/run@2026-01-01' = {
  name: 'cntr-stringOutput'
  command: 'pwsh'
  args: [
    '-c'
    '&{ dir /workdir/assets | % { $_.FullName } }'
  ]
  options: {
    useContainer: true
    containerImage: 'mcr.microsoft.com/azure-powershell:latest'
  } 
}

output stringOutput string = stringOutput.output

resource pwsh 'Flexor/script@2026-01-01' = {
  name: 'cntr-pwsh'
  shell: 'PowerShell'
  script: '/workspace/assets/test.ps1'
  env: {
    EnvVar: 'Set from Bicep'
  }
  options: {
    useContainer: true
    containerImage: 'mcr.microsoft.com/azure-powershell:latest'
  }
}

resource preBash 'Flexor/script@2026-01-01' = {
  name: 'cntr-preBash'
  shell: 'Bash'
  contents: 'chmod +x "/workspace/assets/test.sh"'
  options: {
    useContainer: true
    containerImage: 'debian:latest'
  }
}

resource bash 'Flexor/script@2026-01-01' = {
  name: 'cntr-bash'
  shell: 'Bash'
  script: '/workspace/assets/test.sh'
  options: {
    useContainer: true
    containerImage: 'debian:latest'
  }
}


resource pythonFile 'Flexor/script@2026-01-01' = {
  name: 'cntr-pythonFile'
  shell: 'Python'
  script: '/workspace/assets/test.py'
  options: {
    useContainer: true
    containerImage: 'python:3.11-slim'
  }
}

resource pythonLiteral 'Flexor/script@2026-01-01' = {
  name: 'cntr-pythonLiteral'
  shell: 'Python'
  env: {
    EnvVar: 'Set from Bicep'
  }
  contents: '''
import os
import json
result = {
    "Works": True,
    "EnvVar": os.getenv("EnvVar")
}
print(json.dumps(result))
  '''
  options: {
    useContainer: true
    containerImage: 'python:3.11-slim'
  }
  dependsOn: [pythonFile] 
}

resource scriptLiteral 'Flexor/script@2026-01-01' = {
  name: 'cntr-scriptLiteral'
  shell: 'PowerShell'
  contents: '''
# This is a test script literal
Write-Output "Hello from script literal!"
  '''
  options: {
    useContainer: true
    containerImage: 'mcr.microsoft.com/azure-powershell:latest'
  }
  dependsOn: [pwsh]
}

output scriptLiteralOutput string = scriptLiteral.output

output pwshResult string = pwsh.output
output pwshWorks bool = json(pwsh.output).Works

output bashResult string = bash.output
output bashWorks bool = json(bash.output).Works

output pythonResult string = pythonLiteral.output
output pythonWorks bool = json(pythonLiteral.output).Works

output pythonFileResult string = pythonFile.output

