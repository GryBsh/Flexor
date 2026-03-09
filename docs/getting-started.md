# Getting Started

## Requirements

- [Bicep CLI](https://learn.microsoft.com/en-us/azure/azure-resource-manager/bicep/install) v0.40.1 or later
- .NET 10.0 runtime (bundled with the extension binary)
- For scripting: the target shell must be installed (PowerShell, Bash, Python, etc.)
- For container execution: Docker (or compatible CLI like Podman)
- For git operations: no additional requirements (uses bundled libgit2)

## Installation

1. Download the latest release from [GitHub Releases](https://github.com/GryBsh/Flexor/releases)
2. Extract `flexor.zip` into your project root
3. Ensure `bicepconfig.json` is present with the extension configured:

```json
{
  "experimentalFeaturesEnabled": {
    "localDeploy": true
  },
  "extensions": {
    "flexor": ".bicep/bin/flexor"
  }
}
```

## Your First Bicep File

Create a file called `hello.bicep`:

```bicep
targetScope = 'local'
extension flexor

resource hello 'Flexor/script@2026-01-01' = {
  name: 'hello'
  shell: 'Bash'
  contents: 'echo "Hello from Flexor!"'
}

output message string = hello.output
```

Run it:

```bash
bicep local-deploy hello.bicepparam
```

Where `hello.bicepparam` contains:

```bicep
using 'hello.bicep'
```

## Extension Configuration

The `extension flexor` declaration accepts optional configuration:

```bicep
extension flexor with {
  flexorPath: '.bicep/flexor'         // Working directory for logs and modules
  logOptions: {
    enabled: true                      // Enable/disable logging (default: true)
    append: false                      // Append to existing logs (default: false)
    disableRollover: false             // Delete old logs instead of archiving (default: false)
  }
  enableTraceLogging: false            // Verbose trace logging (default: false)
}
```

The `flexorPath` can also be set via the `FLEXOR_PATH` environment variable.

## Output Handling

Scripts and commands capture their stdout. Flexor automatically classifies output and resolves it:

| Scenario | Output Type |
|---|---|
| One or more plain strings | Single multiline string |
| One JSON object (single or multiline) | JSON object |
| Multiple JSON objects | JSON array |
| Mix of JSON and strings | Array (each string block and JSON is one item) |

When the output is valid JSON, parse it with Bicep's `json()` function:

```bicep
resource script 'Flexor/script@2026-01-01' = {
  name: 'get-data'
  shell: 'PowerShell'
  contents: '''
    @{ Status = "OK"; Count = 42 } | ConvertTo-Json -Compress
  '''
}

var data = json(script.output)
output status string = data.Status    // "OK"
output count int = data.Count         // 42
```

Multiple JSON objects on separate lines are automatically collected into an array:

```bicep
resource multi 'Flexor/script@2026-01-01' = {
  name: 'multi-json'
  shell: 'Bash'
  contents: '''
    echo '{"name":"Alice"}'
    echo '{"name":"Bob"}'
  '''
}

var users = json(multi.output)        // array of two objects
output first string = users[0].name   // "Alice"
```

Consecutive plain text lines are merged into a single string. Blank lines are ignored. Pretty-printed (multiline) JSON is re-parsed correctly.

## Dependency Management

Use `dependsOn` to control execution order:

```bicep
resource setup 'Flexor/script@2026-01-01' = {
  name: 'setup'
  shell: 'Bash'
  contents: 'mkdir -p output'
}

resource build 'Flexor/run@2026-01-01' = {
  name: 'build'
  command: 'make'
  args: ['all']
  dependsOn: [setup]
}
```

## Next Steps

- [Resource Reference](resources.md) - Detailed documentation for all resource types
- [Container Execution](containers.md) - Running scripts in Docker containers
- [Custom Modules](modules.md) - Defining reusable resource types
- [Configuration](configuration.md) - All configuration options
- [Architecture](architecture.md) - Internal design and extension points
