# Flexor

A Bicep extension to flexibly run scripts, commands, HTTP requests, and git operations during local deployments.

```yaml
version: 0.1.2026.1
```

## Why?

Why not? There are frequently times I need something quick and easy to help slap disparate parts of a deployment together, where the structural parts like parameters and parsing, slicing, and converting data types is all handled. So why not Bicep?

## Requirements

- [Bicep CLI](https://learn.microsoft.com/en-us/azure/azure-resource-manager/bicep/install) v0.40.1 or later
- For scripting: the target shell must be installed (PowerShell, Bash, Python, etc.)
- For container execution: Docker or compatible runtime
- For git operations: no additional requirements (bundled libgit2)

## This is a work in progress

Assume the composition of resources for unreleased API versions are subject to potential breaking change. Once released, it will remain fixed until superseded or deprecated/removed.

## Quick Start

1. Download the latest release and extract into your project root
2. Ensure `bicepconfig.json` is configured:

```json
{
  "experimentalFeaturesEnabled": { "localDeploy": true },
  "extensions": { "flexor": ".bicep/bin/flexor" }
}
```

3. Create a Bicep file:

```bicep
targetScope = 'local'
extension flexor

resource hello 'Flexor/script@2026-01-01' = {
  name: 'hello'
  shell: 'Bash'
  contents: '''
    echo '{"message":"Hello from Flexor!"}'
  '''
}

output message string = json(hello.output).message
```

4. Create a params file and deploy:

```bash
bicep local-deploy hello.bicepparam
```

## Features

- Run external commands and executables
- Execute scripts in PowerShell, Bash, Python, or Cmd
- Inline script content or external script files
- Parameterization with Bicep variables and environment variables
- Automatic JSON output parsing with Bicep's `json()` function
- Smart output handling: strings merge, JSON stays structured, mixed output becomes arrays
- HTTP client with auth, headers, query params, and file download
- Git clone and pull operations with credential support
- Container execution via Docker/Podman
- Custom resource types via the module system
- Per-resource logging with rollover and append modes
- Cross-platform support (Windows, Linux, macOS)
- Timeout, cancellation, and error handling

## Resources

### Version: 2026-01-01

#### Run

Execute an external command or program.

```bicep
resource cmd 'Flexor/run@2026-01-01' = {
  name: 'build'
  command: 'bicep'
  args: ['build', 'main.bicep', '--outdir', 'output']
  env: {
    BICEP_TRACE: '1'
  }
  options: {
    timeoutSeconds: 120
  }
}

output result string = cmd.output
```

#### Script

Execute a script in a supported shell.

```bicep
// File-based script with environment variables
resource pwsh 'Flexor/script@2026-01-01' = {
  name: 'deploy'
  shell: 'PowerShell'
  script: 'scripts/deploy.ps1'
  env: {
    Environment: 'production'
  }
}

// Inline script
resource py 'Flexor/script@2026-01-01' = {
  name: 'check'
  shell: 'Python'
  contents: '''
import os, json
print(json.dumps({"Works": True, "EnvVar": os.getenv("EnvVar")}))
  '''
  env: {
    EnvVar: 'Set from Bicep'
  }
}

output works bool = json(py.output).Works
```

Supported shells: `Bash`, `PowerShell`, `Cmd`, `Python`

#### Repo

Clone or pull a git repository.

```bicep
resource clone 'Flexor/repo@2026-01-01' = {
  source: 'https://github.com/org/repo.git'
  path: 'output/repo'
}

resource pull 'Flexor/repo@2026-01-01' existing = {
  path: clone.path
}
```

#### Http

Make HTTP requests.

```bicep
resource get 'Flexor/http@2026-01-01' existing = {
  url: 'https://api.example.com/status'
}

var payload = { name: 'test', value: 42 }

resource post 'Flexor/http@2026-01-01' = {
  url: 'https://api.example.com/data'
  method: 'POST'
  contentType: 'application/json'
  headers: [{ name: 'accept', value: 'application/json' }]
  body: string(payload)
  expectedStatusCodes: [200, 201]
  dependsOn: [get]
}

output statusCode int = post.statusCode
output response object = json(post.output)
```

Options: `timeoutSeconds` (default 300), `ignoreSslErrors`, `followRedirects`

Auth: `authorization.bearerToken` or `authorization.credential { username, password }`

#### Module & Resource

Define reusable custom resource types with handler scripts.

```bicep
// Declare a custom type
resource userModule 'Flexor/module@2026-01-01' = {
  shell: 'PowerShell'
  type: 'MyOrg/User'
  version: 'v1'
  get: 'scripts/Get-User.ps1'
  createOrUpdate: 'scripts/Set-User.ps1'
  delete: 'scripts/Remove-User.ps1'
}

// Create an instance
resource newUser 'Flexor/resource@2026-01-01' = {
  name: 'new-user'
  type: userModule.typeName
  parameters: {
    Username: 'jdoe'
    Email: 'jdoe@example.com'
  }
}

// Read existing
resource existingUser 'Flexor/resource@2026-01-01' existing = {
  name: 'existing-user'
  type: userModule.typeName
  parameters: {
    Username: 'admin'
  }
}

output created object = json(newUser.output)
output existing object = json(existingUser.output)
```

Parameters are passed as environment variables (`PARAM_{name}`) by default. Other modes: `args` (`--name value`), `stdinenv` (`KEY="value"`), `stdinjson` (JSON object).

### Common Execution Options

All execution resources (Run, Script, Module, Resource) share these options:

```bicep
options: {
  workingDirectory: '/path/to/'
  runAsAdmin: false
  timeoutSeconds: 30
  continueOnFailure: false
  noWait: false
  env: {
    overwritePaths: false
    append: { CLASSPATH: ':' }
  }
  // Container options
  useContainer: false
  containerImage: 'debian:latest'
  containerCli: 'docker'
  containerCliArgs: ['run', '--rm', '-i']
  containerMounts: { '/host/path': '/container/path' }
}
```

## Output Behavior

Flexor automatically classifies each line of stdout as JSON or plain text and resolves the final output:

| Scenario | Output Type |
|---|---|
| One or more plain strings | Single multiline string |
| One JSON object (single or multiline) | JSON object |
| Multiple JSON objects | JSON array |
| JSON array(s) | Array value(s) |
| Mix of JSON and strings | Array (each string block and JSON object is one item) |

Consecutive plain strings are merged into a single string item. Blank lines are ignored.

```bicep
// Single JSON → object
resource data 'Flexor/script@2026-01-01' = {
  name: 'data'
  shell: 'Bash'
  contents: 'echo \'{"status":"ok"}\''
}
output status string = json(data.output).status

// Multiple JSON → array
resource multi 'Flexor/script@2026-01-01' = {
  name: 'multi'
  shell: 'Bash'
  contents: '''
    echo '{"a":1}'
    echo '{"b":2}'
  '''
}
output items array = json(multi.output)
```

## Documentation

Detailed documentation is available in the [docs/](docs/) directory:

- [Getting Started](docs/getting-started.md) - Installation and first steps
- [Resource Reference](docs/resources.md) - Complete property reference for all resource types
- [Container Execution](docs/containers.md) - Running scripts in Docker containers
- [Custom Modules](docs/modules.md) - Defining and using custom resource types
- [Configuration](docs/configuration.md) - Extension, logging, and environment configuration
- [Architecture](docs/architecture.md) - Internal design, project structure, and extension points

## Building from Source

Requirements: .NET 10.0 SDK

```powershell
./build.ps1                    # Build for all platforms
```

## Testing

```bash
dotnet test src/Flexor.Tests/  # Run all tests (127 tests: 121 unit + 6 integration)
```

Integration tests require the Bicep CLI and will execute `bicep local-deploy` against test `.bicep` files.

## Known Issues

- PowerShell and Python scripts do not execute under GitHub Actions
  - Investigating environment setup issues for CI test execution
