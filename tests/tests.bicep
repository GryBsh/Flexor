targetScope = 'local'
extension flexor with {
  pathRoot: '../.bicep/flexor'
  logOptions: {
    disableRollover: true
  }
}

/*
var body = {
  message: 'Hello, Flexor!'
}
*/

resource cleanup 'Flexor/script@2026-01-01' = {
  name: 'cleanup'
  shell: 'PowerShell'
  script: 'scripts/cleanup.ps1'
}

// Using `existing` is always a `GET` request
// When debugging, you'll want to comment this out to avoid unnecessary noise
resource http 'Flexor/http@2026-01-01' existing = {
  name: 'http'
  url: 'https://github.com/example/test.git'
  enableLogging: false
}

resource clone 'Flexor/repo@2026-01-01' = {
  name: 'CloneRepo'
  source: 'https://github.com/example/test.git'
  dependsOn: [ cleanup ]
}
/*
resource pull 'Flexor/repo@2026-01-01' existing = {
  name: 'PullRepo'
  path: clone.path
}*/

resource pwsh 'Flexor/script@2026-01-01' = {
  name: 'pwsh'
  shell: 'PowerShell'
  script: 'scripts/test.ps1'
  env: {
    EnvVar: 'Set from Bicep'
  }
  options: {
    timeoutSeconds: 60
  }
}

resource bash 'Flexor/script@2026-01-01' = {
  name: 'bash'
  shell: 'Bash'
  script: 'scripts/test.sh'
  dependsOn: [pwsh] 
}

resource bicep 'Flexor/command@2026-01-01' = {
  name: 'bicep'
  command: 'bicep'
  env: {} 
  args: [
    'build'
    'scripts/test.az.bicep'
    '--outdir'
    'test'
  ]
  dependsOn: [
    pwsh
    clone
    //pull
  ]
}

resource pythonFile 'Flexor/script@2026-01-01' = {
  name: 'pythonFile'
  shell: 'Python'
  script: 'scripts/test.py'
  options: {
    timeoutSeconds: 30
  }
  dependsOn: [bash] 
}


resource stringOutput 'Flexor/command@2026-01-01' = {
  name: 'stringOutput'
  command: 'pwsh'
  args: [
    '-c'
    '"&{ dir scripts | % { $_.FullName } }"'
  ]  
  dependsOn: [bicep] 
}


resource longFormHttp 'Flexor/http@2026-01-01' = {
  name: 'http'
  url: 'https://google.com'
  method: 'GET'
  //body: string(body)
  dependsOn: [stringOutput] 
}


resource userModule 'Flexor/module@2026-01-01' = {
  name: 'CustomUserManagement'
  shell: 'PowerShell'
  type: 'MyOrg/User'
  version: 'v1'
  get: 'scripts/UserMgmt/Get-User.ps1'
  createOrUpdate: 'scripts/UserMgmt/Set-User.ps1'
  delete: 'scripts/UserMgmt/Remove-User.ps1'
}


resource createUser 'Flexor/resource@2026-01-01' = {
  name: 'createdUser'
  type: userModule.typeName
  parameters: {
    Username: 'tuser'
    Email: 'tuser@org.local'
    FirstName: 'Test'
    LastName: 'User'
  }
}


resource getExistingUser 'Flexor/resource@2026-01-01' existing = {
  name: 'existingUser'
  type: userModule.typeName
  parameters: {
    Username: 'euser'
  }
}

resource python 'Flexor/script@2026-01-01' = {
  name: 'python'
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
  dependsOn: [bash] 
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
output httpStatusCode int = http.statusCode

output pwshResult string = pwsh.output
output pwshWorks bool = json(pwsh.output).Works

output bashResult string = bash.output
output bashWorks bool = json(bash.output).Works

output pythonResult string = python.output
output pythonWorks bool = json(python.output).Works

output bicepOutputLength int = length(bicep.output)
output stringOutput string = stringOutput.output

var createdUser = json(createUser.output)
output createdUsername string = createdUser.Username
output createdEmail string = createdUser.Email
output createdName string = '${createdUser.FirstName} ${createdUser.LastName}'

var existingUser = json(getExistingUser.output)
output existingUsername string = existingUser.Username
output existingEmail string = existingUser.Email  
output existingName string = '${existingUser.FirstName} ${existingUser.LastName}'
