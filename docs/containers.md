# Container Execution

Flexor can run scripts and commands inside Docker containers (or any OCI-compatible runtime). This is useful for isolated environments, specific tool versions, or cross-platform execution.

## Enabling Container Execution

Set `useContainer: true` in the `options` block and specify a `containerImage`:

```bicep
resource script 'Flexor/script@2026-01-01' = {
  name: 'containerized'
  shell: 'Bash'
  contents: 'echo "Running inside a container"'
  options: {
    useContainer: true
    containerImage: 'debian:latest'
  }
}
```

## How It Works

When container execution is enabled, Flexor:

1. Translates the command into a `docker run` invocation (or your configured CLI)
2. Automatically mounts the working directory to `/workspace` inside the container
3. Automatically mounts the Flexor working directory to `/.flexor`
4. Passes environment variables via `-e` flags
5. Passes volume mounts via `-v` flags
6. Runs with `--rm` and `-i` flags by default

The resulting command looks like:

```
docker run --rm -i -e VAR=value -v /host/path:/workspace -v /flexor/path:/.flexor image:tag command args...
```

## Container Options

| Property | Type | Default | Description |
|---|---|---|---|
| `useContainer` | bool | false | Enable container execution |
| `containerImage` | string | required | Docker image to use |
| `containerCli` | string | `docker` | Container runtime CLI (e.g., `podman`) |
| `containerCliArgs` | string[] | `['run', '--rm', '-i']` | CLI arguments before the image name |
| `containerMounts` | object | auto | Additional host:container volume mounts |
| `workingPathMount` | string | `/workspace` | Container path for the working directory mount |

## Examples

### PowerShell in Azure PowerShell Container

```bicep
resource pwsh 'Flexor/script@2026-01-01' = {
  name: 'azure-pwsh'
  shell: 'PowerShell'
  script: '/workspace/scripts/deploy.ps1'
  env: {
    AZURE_SUBSCRIPTION: subscriptionId
  }
  options: {
    useContainer: true
    containerImage: 'mcr.microsoft.com/azure-powershell:latest'
  }
}
```

Note: Script paths must reference the container filesystem. The working directory is mounted at `/workspace` by default, so a local file at `scripts/deploy.ps1` becomes `/workspace/scripts/deploy.ps1` inside the container.

### Python in a Specific Version

```bicep
resource py 'Flexor/script@2026-01-01' = {
  name: 'python-task'
  shell: 'Python'
  contents: '''
import sys, json
print(json.dumps({"version": sys.version}))
  '''
  options: {
    useContainer: true
    containerImage: 'python:3.11-slim'
  }
}
```

### Bash in Debian

```bicep
resource bash 'Flexor/script@2026-01-01' = {
  name: 'bash-task'
  shell: 'Bash'
  script: '/workspace/scripts/build.sh'
  options: {
    useContainer: true
    containerImage: 'debian:latest'
  }
}
```

### Custom Volume Mounts

```bicep
resource task 'Flexor/script@2026-01-01' = {
  name: 'mounted-task'
  shell: 'Bash'
  contents: 'ls /data && ls /config'
  options: {
    useContainer: true
    containerImage: 'debian:latest'
    containerMounts: {
      '/host/data': '/data'
      '/host/config': '/config'
    }
  }
}
```

### Using Podman Instead of Docker

```bicep
resource task 'Flexor/run@2026-01-01' = {
  name: 'podman-task'
  command: 'echo'
  args: ['hello']
  options: {
    useContainer: true
    containerImage: 'alpine:latest'
    containerCli: 'podman'
    containerCliArgs: ['run', '--rm', '-i']
  }
}
```

### Commands in Containers

The `Flexor/run` resource also supports container execution:

```bicep
resource cmd 'Flexor/run@2026-01-01' = {
  name: 'container-cmd'
  command: 'pwsh'
  args: [
    '-c'
    '&{ dir /workspace/assets | % { $_.FullName } }'
  ]
  options: {
    useContainer: true
    containerImage: 'mcr.microsoft.com/azure-powershell:latest'
  }
}
```
