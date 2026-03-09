# Configuration

## Extension Configuration

The Flexor extension is configured in your Bicep file's `extension` declaration:

```bicep
extension flexor with {
  flexorPath: '.bicep/flexor'
  logOptions: { ... }
  enableTraceLogging: false
}
```

### Top-Level Options

| Property | Type | Default | Description |
|---|---|---|---|
| `flexorPath` | string | `.bicep/flexor` or `$FLEXOR_PATH` | Root directory for Flexor's working files (logs, PowerShell modules) |
| `logPath` | string | `{flexorPath}/logs` | Directory for log files |
| `enableTraceLogging` | bool | false | Enable verbose trace-level logging |
| `logOptions` | object | see below | Log file configuration |

### Log Options

| Property | Type | Default | Description |
|---|---|---|---|
| `enabled` | bool | true | Enable logging for all resources |
| `append` | bool | false | Append to existing log files instead of rolling over |
| `disableRollover` | bool | false | Delete old logs instead of archiving with timestamps |
| `filenameExtension` | string | `.log` | Log file extension |
| `filenameSeparator` | string | `.` | Separator in log filenames |
| `filenameStdOutSegment` | string | (empty) | Segment identifier for stdout log files |
| `filenameStdErrSegment` | string | `error` | Segment identifier for stderr log files |
| `filenameTimestampFormat` | string | `yyyyMMddHHmmss` | Timestamp format for rolled-over log files |

### Log File Naming

Log files are created per resource using the resource's `name`:

```
{logPath}/{name}.log                    # stdout log
{logPath}/{name}.error.log              # stderr log
```

When a log file already exists:
- **Append mode** (`append: true`): Content is appended to the existing file
- **Rollover mode** (default): The existing file is renamed with a timestamp suffix
- **Cleanup mode** (`disableRollover: true`): The existing file is deleted

## Bicep Configuration

The `bicepconfig.json` file in your project root registers the extension:

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

## Environment Variables

| Variable | Description |
|---|---|
| `FLEXOR_PATH` | Override the default Flexor working directory |

## Per-Resource Logging

Individual resources can control their own logging behavior:

```bicep
resource verbose 'Flexor/script@2026-01-01' = {
  name: 'verbose-task'
  enableLogging: true              // Override global log setting
  cleanupLogs: true                // Delete logs after execution
  appendLogs: false                // Don't append to existing logs
  shell: 'Bash'
  contents: 'echo "logged"'
}
```

## Environment Variable Handling

### Path-Like Variables

Variables with `PATH` in their name (case-insensitive) that contain path separators (`;` on Windows, `:` on Unix) are treated specially. By default, new values are **appended** to the existing system value:

```bicep
resource task 'Flexor/run@2026-01-01' = {
  name: 'with-path'
  command: 'myapp'
  env: {
    MY_PATH_VAR: '/new/path:/another/path'    // Appended to existing value
  }
}
```

To **overwrite** instead of append:

```bicep
options: {
  env: {
    overwritePaths: true
  }
}
```

### Appended Variables

For non-path variables that should be appended with a custom delimiter:

```bicep
options: {
  env: {
    append: {
      CLASSPATH: ':'                // Append CLASSPATH values with : separator
      FLAGS: ' '                    // Append FLAGS values with space separator
    }
  }
}
```

## Build Configuration

The `build.ps1` script compiles Flexor for multiple platforms:

```powershell
./build.ps1                        # Build for all platforms
```

Target runtimes:
- `win-x64`, `win-arm64`
- `linux-x64`, `linux-arm64`
- `osx-arm64`

Output is placed in `.bicep/bin/flexor/{runtime}/` as a self-contained, trimmed, single-file executable.

## flexorconfig.json

An optional `flexorconfig.json` file in the working directory can provide default configuration values. This file is loaded via `Microsoft.Extensions.Configuration` and bound to the `FlexorOptions` class:

```json
{
  "flexorPath": ".bicep/flexor",
  "enableTraceLogging": true,
  "logOptions": {
    "enabled": true,
    "append": false
  }
}
```
