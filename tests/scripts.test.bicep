targetScope = 'local'
extension flexor with {
  pathRoot: '../.bicep/flexor'
  logOptions: {
    disableRollover: true
  }
}

resource pwsh 'Flexor/script@2026-01-01' = {
  name: 'pwsh'
  shell: 'PowerShell'
  script: 'assets/test.ps1'
  env: {
    EnvVar: 'Set from Bicep'
  }
  options: {
    timeoutSeconds: 60
  }
}

resource preBash 'Flexor/script@2026-01-01' = {
  name: 'preBash'
  shell: 'Bash'
  contents: '''
chmod +x assets/test.sh
'''
}

resource bash 'Flexor/script@2026-01-01' = {
  name: 'bash'
  shell: 'Bash'
  script: 'assets/test.sh'
}


resource pythonFile 'Flexor/script@2026-01-01' = {
  name: 'pythonFile'
  shell: 'Python'
  script: 'assets/test.py'
  options: {
    timeoutSeconds: 30
  }
}

resource pythonLiteral 'Flexor/script@2026-01-01' = {
  name: 'pythonLiteral'
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
    timeoutSeconds: 30
  }
  dependsOn: [pythonFile] 
}

resource scriptLiteral 'Flexor/script@2026-01-01' = {
  name: 'scriptLiteral'
  shell: 'PowerShell'
  contents: '''
# This is a test script literal
Write-Output "Hello from script literal!"
  '''
  options: {
    timeoutSeconds: 30
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

