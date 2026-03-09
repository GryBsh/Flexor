# Resource Reference

All Flexor resources use API version `2026-01-01` and target the `local` scope.

## Common Options

Most resources accept an `options` block with these properties:

| Property | Type | Default | Description |
|---|---|---|---|
| `workingDirectory` | string | Current directory | Working directory for execution |
| `runAsAdmin` | bool | false | Run with elevated privileges |
| `timeoutSeconds` | int | none | Timeout before the activity is killed |
| `continueOnFailure` | bool | false | Don't fail the deployment on error |
| `noWait` | bool | false | Don't wait for completion |
| `useContainer` | bool | false | Execute inside a container |
| `containerImage` | string | required if useContainer | Docker image to use |
| `containerCli` | string | `docker` | Container runtime CLI |
| `containerCliArgs` | string[] | `['run', '--rm', '-i']` | CLI arguments |
| `containerMounts` | object | auto | Host-to-container volume mounts |
| `env` | object | | Environment variable handling options |
| `env.overwritePaths` | bool | false | Overwrite PATH-like vars instead of appending |
| `env.append` | object | | Variables to append with custom delimiters |

---

## Run

Execute an external command or program.

**Type**: `Flexor/run@2026-01-01`

### Properties

| Property | Type | Required | Description |
|---|---|---|---|
| `name` | string | yes | Activity name (used for log files) |
| `command` | string | yes | Command or executable to run |
| `args` | string[] | no | Command-line arguments |
| `env` | object | no | Environment variables |
| `options` | object | no | Execution options (see Common Options) |

### Outputs

| Property | Type | Description |
|---|---|---|
| `output` | string | Captured stdout (JSON auto-detected; multiple JSON objects become an array) |

### Example

```bicep
resource build 'Flexor/run@2026-01-01' = {
  name: 'bicep-build'
  command: 'bicep'
  args: [
    'build'
    'main.bicep'
    '--outdir'
    'output'
  ]
  env: {
    BICEP_TRACE: '1'
  }
  options: {
    timeoutSeconds: 120
  }
}

output buildOutputLength int = length(build.output)
```

---

## Script

Execute a script in a supported shell language.

**Type**: `Flexor/script@2026-01-01`

### Properties

| Property | Type | Required | Description |
|---|---|---|---|
| `name` | string | yes | Activity name |
| `shell` | string | yes | Shell type: `Bash`, `PowerShell`, `Cmd`, or `Python` |
| `script` | string | no* | Path to a script file |
| `contents` | string | no* | Inline script content |
| `args` | string[] | no | Arguments (file scripts only) |
| `env` | object | no | Environment variables |
| `options` | object | no | Execution options |

*Exactly one of `script` or `contents` must be provided.

### Outputs

| Property | Type | Description |
|---|---|---|
| `output` | string | Captured stdout (JSON auto-detected; multiple JSON objects become an array) |

### Examples

**PowerShell with file and environment variables:**

```bicep
resource pwsh 'Flexor/script@2026-01-01' = {
  name: 'pwsh-script'
  shell: 'PowerShell'
  script: 'scripts/deploy.ps1'
  env: {
    Environment: 'production'
    Version: '2.0.0'
  }
  options: {
    timeoutSeconds: 60
  }
}

output result string = pwsh.output
output works bool = json(pwsh.output).Works
```

**Bash inline script:**

```bicep
resource bash 'Flexor/script@2026-01-01' = {
  name: 'bash-inline'
  shell: 'Bash'
  contents: '''
    echo '{"status":"ok","timestamp":"'$(date -Iseconds)'"}'
  '''
}
```

**Python inline script:**

```bicep
resource py 'Flexor/script@2026-01-01' = {
  name: 'python-check'
  shell: 'Python'
  env: {
    API_KEY: apiKey
  }
  contents: '''
import os, json
result = {"Works": True, "EnvVar": os.getenv("API_KEY")}
print(json.dumps(result))
  '''
}
```

---

## Repo

Clone or pull a git repository.

**Type**: `Flexor/repo@2026-01-01`

### Properties

| Property | Type | Required | Description |
|---|---|---|---|
| `type` | string | no | Repository type (only `git` supported, default) |
| `source` | string | no* | Repository URL to clone |
| `path` | string | no* | Local path for the repository |
| `credential` | object | no | Authentication credentials |
| `credential.username` | string | | Username |
| `credential.password` | secureString | | Password or token |

*For cloning: `source` is required. For pulling: use `existing` keyword with `path`.

### Outputs

| Property | Type | Description |
|---|---|---|
| `path` | string | Full local path to the repository |

### Examples

**Clone a repository:**

```bicep
resource clone 'Flexor/repo@2026-01-01' = {
  source: 'https://github.com/org/repo.git'
  path: 'output/repo'
}
```

**Pull an existing repository:**

```bicep
resource pull 'Flexor/repo@2026-01-01' existing = {
  path: clone.path
}
```

**Clone with credentials:**

```bicep
@secure()
param token string

resource privateRepo 'Flexor/repo@2026-01-01' = {
  source: 'https://github.com/org/private-repo.git'
  path: 'output/private'
  credential: {
    username: 'git'
    password: token
  }
}
```

---

## Http

Make HTTP requests.

**Type**: `Flexor/http@2026-01-01`

### Properties

| Property | Type | Required | Description |
|---|---|---|---|
| `url` | string | yes | Request URL |
| `method` | string | no | HTTP method (default: `GET`) |
| `headers` | array | no | Request headers `[{ name, value }]` |
| `query` | array | no | Query parameters `[{ name, value }]` |
| `contentType` | string | no | Body MIME type |
| `body` | string | no | Request body |
| `downloadPath` | string | no | Save response to file instead of output |
| `expectedStatusCodes` | int[] | no | Accepted status codes (default: 200) |
| `authorization` | object | no | Auth configuration |
| `authorization.bearerToken` | secureString | | Bearer token |
| `authorization.credential` | object | | Basic auth `{ username, password }` |
| `options` | object | no | HTTP-specific options |
| `options.timeoutSeconds` | int | 300 | Request timeout |
| `options.ignoreSslErrors` | bool | false | Skip SSL certificate validation |
| `options.followRedirects` | bool | true | Follow HTTP redirects |

### Outputs

| Property | Type | Description |
|---|---|---|
| `output` | string | Response body (unless downloadPath is set) |
| `statusCode` | int | HTTP response status code |

### Examples

**GET request:**

```bicep
resource get 'Flexor/http@2026-01-01' existing = {
  url: 'https://api.example.com/status'
}

output statusCode int = get.statusCode
output body string = get.output
```

**POST with JSON body:**

```bicep
var payload = {
  name: 'test'
  value: 42
}

resource post 'Flexor/http@2026-01-01' = {
  url: 'https://api.example.com/data'
  method: 'POST'
  contentType: 'application/json'
  headers: [
    { name: 'accept', value: 'application/json' }
  ]
  body: string(payload)
  expectedStatusCodes: [200, 201]
}

output response object = json(post.output)
```

**Download a file:**

```bicep
resource download 'Flexor/http@2026-01-01' = {
  url: 'https://example.com/artifact.zip'
  downloadPath: 'output/artifact.zip'
}
```

---

## Module

Declare a custom resource type with handler scripts.

**Type**: `Flexor/module@2026-01-01`

See [Custom Modules](modules.md) for detailed documentation.

### Properties

| Property | Type | Required | Description |
|---|---|---|---|
| `type` | string | yes | Custom type name (e.g., `MyOrg/User`) |
| `version` | string | no | Type version |
| `shell` | string | yes | Shell for handler scripts |
| `get` | string | no | Path to Get handler script |
| `createOrUpdate` | string | yes | Path to CreateOrUpdate handler script |
| `delete` | string | no | Path to Delete handler script |
| `options` | object | no | Module options |
| `options.type` | string | `env` | Parameter passing: `env`, `args`, `stdinenv`, `stdinjson` |
| `getOptions` | object | no | Execution overrides for Get |
| `createOrUpdateOptions` | object | no | Execution overrides for CreateOrUpdate |
| `deleteOptions` | object | no | Execution overrides for Delete |

### Outputs

| Property | Type | Description |
|---|---|---|
| `typeName` | string | Fully qualified type name (e.g., `MyOrg/User@v1`) |

---

## Resource

Invoke a previously declared custom module.

**Type**: `Flexor/resource@2026-01-01`

See [Custom Modules](modules.md) for detailed documentation.

### Properties

| Property | Type | Required | Description |
|---|---|---|---|
| `name` | string | yes | Instance name |
| `type` | string | yes | Module type (from `module.typeName`) |
| `parameters` | object | no | Key-value parameters passed to handler scripts |
| `options` | object | no | Execution option overrides |

### Outputs

| Property | Type | Description |
|---|---|---|
| `output` | string | Handler script stdout (JSON auto-detected; multiple JSON objects become an array) |
