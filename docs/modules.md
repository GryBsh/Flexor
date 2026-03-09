# Custom Modules

Flexor's module system lets you define reusable custom resource types backed by handler scripts. This enables infrastructure-as-code patterns where you define a resource type once and instantiate it multiple times with different parameters.

## Overview

The module pattern has two parts:

1. **Module Declaration** (`Flexor/module@2026-01-01`) - Defines a custom type and its handler scripts
2. **Resource Instance** (`Flexor/resource@2026-01-01`) - Creates instances of the declared type

## Defining a Module

```bicep
resource userModule 'Flexor/module@2026-01-01' = {
  shell: 'PowerShell'
  type: 'MyOrg/User'
  version: 'v1'
  get: 'scripts/Get-User.ps1'
  createOrUpdate: 'scripts/Set-User.ps1'
  delete: 'scripts/Remove-User.ps1'
}
```

### Handler Scripts

Each module can define up to three handler scripts:

| Handler | When Invoked | Required |
|---|---|---|
| `createOrUpdate` | When the resource is declared normally | yes |
| `get` | When the resource uses the `existing` keyword | no |
| `delete` | When the resource is removed | no |

Handler scripts receive parameters and must write their output (typically JSON) to stdout.

## Using a Module

```bicep
resource newUser 'Flexor/resource@2026-01-01' = {
  name: 'create-user'
  type: userModule.typeName         // References the module's typeName output
  parameters: {
    Username: 'jdoe'
    Email: 'jdoe@example.com'
    FirstName: 'Jane'
    LastName: 'Doe'
  }
}

var user = json(newUser.output)
output username string = user.Username
```

### Reading Existing Resources

Use the `existing` keyword to invoke the `get` handler:

```bicep
resource existingUser 'Flexor/resource@2026-01-01' existing = {
  name: 'lookup-user'
  type: userModule.typeName
  parameters: {
    Username: 'admin'
  }
}
```

## Parameter Passing Modes

The `options.type` property controls how parameters are passed to handler scripts:

### `env` (default)

Parameters are set as environment variables with a `PARAM_` prefix:

```bicep
resource mod 'Flexor/module@2026-01-01' = {
  shell: 'PowerShell'
  type: 'MyOrg/Thing'
  createOrUpdate: 'scripts/handler.ps1'
  options: {
    type: 'env'    // default
  }
}
```

The handler receives parameters as:
- `$env:PARAM_Username` (PowerShell)
- `$PARAM_Username` (Bash)
- `os.getenv('PARAM_Username')` (Python)

### `args`

Parameters are passed as command-line arguments in `--name value` format:

```bicep
options: {
  type: 'args'
}
```

The handler receives: `--Username jdoe --Email jdoe@example.com`

### `stdinenv`

Parameters are written to stdin in `KEY="value"` format (one per line):

```bicep
options: {
  type: 'stdinenv'
}
```

The handler receives via stdin:
```
Username="jdoe"
Email="jdoe@example.com"
```

### `stdinjson`

Parameters are serialized as a JSON object and written to stdin:

```bicep
options: {
  type: 'stdinjson'
}
```

The handler receives via stdin:
```json
{"Username":"jdoe","Email":"jdoe@example.com"}
```

## Module-Level Options

Modules can define default environment variables, arguments, and execution options that apply to all handler invocations:

```bicep
resource mod 'Flexor/module@2026-01-01' = {
  shell: 'Bash'
  type: 'MyOrg/Service'
  version: 'v1'
  createOrUpdate: 'scripts/create.sh'
  get: 'scripts/get.sh'
  delete: 'scripts/delete.sh'
  options: {
    type: 'env'
    env: {
      API_URL: 'https://api.example.com'
    }
    args: ['--verbose']
    exec: {
      timeoutSeconds: 120
      workingDirectory: 'scripts'
    }
  }
}
```

### Per-Handler Option Overrides

Each handler can override execution options:

```bicep
resource mod 'Flexor/module@2026-01-01' = {
  shell: 'Bash'
  type: 'MyOrg/Service'
  createOrUpdate: 'scripts/create.sh'
  delete: 'scripts/delete.sh'
  deleteOptions: {
    exec: {
      timeoutSeconds: 300          // Delete operations get more time
      continueOnFailure: true      // Don't fail if delete errors
    }
  }
}
```

Resource instances can also override execution options:

```bicep
resource instance 'Flexor/resource@2026-01-01' = {
  name: 'my-service'
  type: mod.typeName
  parameters: { ... }
  options: {
    exec: {
      timeoutSeconds: 60           // Override for this instance
    }
  }
}
```

## Complete Example

### Module Definition

```bicep
// modules.bicep
targetScope = 'local'
extension flexor

resource userModule 'Flexor/module@2026-01-01' = {
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
```

### Handler Script (Set-User.ps1)

```powershell
param(
    [string] $Username  = $env:PARAM_Username,
    [string] $Email     = $env:PARAM_Email,
    [string] $FirstName = $env:PARAM_FirstName,
    [string] $LastName  = $env:PARAM_LastName
)

# Validate required parameters
if (-not $Username -or -not $Email) {
    throw "Username and Email are required"
}

# Your logic here...

# Return JSON output
@{
    Username  = $Username
    Email     = $Email
    FirstName = $FirstName
    LastName  = $LastName
} | ConvertTo-Json -Compress
```

### Handler Script (Get-User.ps1)

```powershell
param(
    [string] $Username = $env:PARAM_Username
)

# Look up the user...

@{
    Username  = $Username
    Email     = "$Username@org.local"
    FirstName = "Existing"
    LastName  = "User"
} | ConvertTo-Json -Compress
```
