targetScope = 'local'
extension flexor with {
  pathRoot: '../.bicep/flexor'
  logOptions: {
    disableRollover: true
  }
}

resource userModule 'Flexor/module@2026-01-01' = {
  name: 'CustomUserManagement'
  shell: 'PowerShell'
  type: 'MyOrg/User'
  version: 'v1'
  get: 'assets/UserMgmt/Get-User.ps1'
  createOrUpdate: 'assets/UserMgmt/Set-User.ps1'
  delete: 'assets/UserMgmt/Remove-User.ps1'
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

var createdUser = json(createUser.output)
output createdUsername string = createdUser.Username
output createdEmail string = createdUser.Email
output createdName string = '${createdUser.FirstName} ${createdUser.LastName}'

var existingUser = json(getExistingUser.output)
output existingUsername string = existingUser.Username
output existingEmail string = existingUser.Email  
output existingName string = '${existingUser.FirstName} ${existingUser.LastName}'
