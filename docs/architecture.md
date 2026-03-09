# Architecture

## Overview

Flexor is a .NET 10.0 application that implements the Azure Bicep Local Extension protocol. It runs as a gRPC server that Bicep CLI communicates with during `bicep local-deploy` operations.

```
bicep local-deploy ──gRPC──> Flexor Extension ──> ProcessExecutor ──> OS Process
                                                 ──> GitClient     ──> libgit2
                                                 ──> HttpClient    ──> HTTP
```

## Project Structure

```
src/
├── Flexor/                     # Main extension host
│   ├── Program.cs              # Entry point, service registration
│   └── v2026_1/                # API version 2026-01-01
│       ├── Resources/          # Resource type definitions (data models)
│       ├── Handlers/           # Resource handlers (business logic)
│       └── Options/            # Version-specific configuration
├── Flexor.Base/                # Shared infrastructure library
│   ├── Executor/               # Process execution engine
│   │   ├── ProcessExecutor.cs  # Core execution logic
│   │   ├── ProcessResult.cs    # Execution result record
│   │   ├── LogFile.cs          # Log file management
│   │   └── Scripts/            # Shell-specific constants
│   ├── Options/                # Shared option types
│   │   ├── ProcessStartOptions.cs
│   │   ├── ScriptRunOptions.cs
│   │   ├── Types.cs            # Enums and JsonType
│   │   └── ...
│   ├── Resources/              # Resource interfaces
│   │   ├── IFlexorResource.cs
│   │   └── IOutputResource.cs
│   ├── PathExtensions.cs       # Cross-platform path utilities
│   └── Result.cs               # Result<T> monad
├── Flexor.Git/                 # Git integration (libgit2)
│   ├── GitClient.cs
│   └── IGitClient.cs
└── Flexor.Tests/               # Test project
    ├── Tests.cs                # Integration tests (bicep local-deploy)
    ├── Helpers.cs              # Test infrastructure
    └── *Tests.cs               # Unit tests
```

## Key Components

### ProcessExecutor

The central execution engine in `Flexor.Base`. All script and command execution flows through `ProcessExecutor`:

- **`RunAsync(ProcessStartOptions, ILogger)`** - Executes a raw process
- **`RunAsync<TResource>(TResource, FlexorOptions, ProcessStartOptions, ILogger)`** - Executes with resource context (output parsing, logging)
- **`RunScriptAsync<T>(T, FlexorOptions, ScriptRunOptions, ILogger)`** - Executes a script with shell resolution

#### Execution Flow

1. Resolve shell type and arguments (for scripts)
2. Create `ProcessStartInfo` with command, args, env vars, working directory
3. If container execution: wrap command in `docker run` invocation
4. Start the process
5. Capture stdout (line-by-line via `BeginOutputReadLine` or raw via `ReadToEndAsync`)
6. Capture stderr via `BeginErrorReadLine`
7. Write stdin lines if provided
8. Wait for exit with timeout and cancellation support
9. Parse output (JSON detection, string accumulation)
10. Write logs, set resource output

#### CaptureRawOutput Mode

When `CaptureRawOutput = true`, stdout is read as a single string via `ReadToEndAsync` instead of line-by-line events. This preserves multi-line JSON documents that `BeginOutputReadLine` would split. The raw output is stored in `ProcessResult.RawOutput`.

If a `StdOutputHandler` is also set, it fires post-process (batched) by splitting the raw output on newlines, unlike the streaming behavior of the non-raw path.

### Resource Handlers

Each resource type has a handler class that extends `TypedResourceHandler<TResource, TIdentifiers, TConfig>` from the Azure.Bicep.Local.Extension SDK:

| Handler | Resource Type | Operations |
|---|---|---|
| `RunResourceHandler` | `Flexor/run` | CreateOrUpdate, Preview |
| `ScriptResourceHandler` | `Flexor/script` | CreateOrUpdate, Preview |
| `RepositoryResourceHandler` | `Flexor/repo` | CreateOrUpdate, Get, Preview |
| `HttpResourceHandler` | `Flexor/http` | CreateOrUpdate, Get, Preview |
| `FlexorModuleResourceHandler` | `Flexor/module` | CreateOrUpdate, Preview |
| `FlexorResourceHandler` | `Flexor/resource` | CreateOrUpdate, Get, Delete, Preview |

Handlers translate Bicep resource declarations into `ProcessStartOptions` and delegate to `ProcessExecutor`.

### Output Pipeline

Process output flows through several stages:

1. **Capture** - Raw bytes or line-by-line from process stdout (or single read via `CaptureRawOutput`)
2. **ParseOutput** - Classifies each line as JSON object, JSON array, or plain string
3. **ResolveOutput** - Aggregates parsed outputs into a final value
4. **StrigifyOutput** - Serializes the resolved output back to a string for the Bicep resource's `output` property

#### ParseOutput

Each line of stdout is classified independently:

- **JSON object** (`{...}`) — deserialized and stored as a separate `Any` dictionary entry
- **JSON array** (`[...]`) — deserialized and stored with a special `ArrayOutputKey` wrapper
- **Plain string** — accumulated into the most recent string entry (if one exists), otherwise a new string entry is created. Consecutive strings merge into one multiline string. Whitespace-only lines are skipped.

Strings never merge into JSON entries. This ensures JSON objects remain structurally intact regardless of surrounding text output.

#### ResolveOutput

Aggregates the parsed entries into a final return value:

- Each string entry is checked for valid JSON and re-parsed if possible (handles multiline JSON that arrived line-by-line)
- Empty entries are skipped
- **0 entries** → `null`
- **1 entry** → the value directly (string, object, or array)
- **2+ entries** → an `object?[]` array

This produces 7 distinct output behaviors:

| Scenario | Result |
|---|---|
| One or more plain strings | Single multiline string |
| One single-line JSON object | JSON object |
| Multiple single-line JSON objects | Array of objects |
| Multiline JSON (pretty-printed) | JSON object (re-parsed from accumulated string) |
| Multiple multiline JSON blocks | Array of objects |
| Mixed JSON types (objects + arrays) | Array |
| Mix of JSON and strings | Array (each string block and JSON object is one item) |

### GitClient

Wraps [LibGit2Sharp](https://github.com/libgit2/libgit2sharp) for git operations:

- `CloneRepository` - Clone with branch, bare, submodule, and checkout options
- `PullRepository` - Pull with automatic signature generation
- `ShouldPull` - Check if a path contains a discoverable git repository

Credential support uses `UsernamePasswordCredentials` via the `CredentialsProvider` callback.

### Path Extensions

Cross-platform path utilities:

- `AsPath()` - Convert to current OS format
- `AsUnixPath()` / `AsWindowsPath()` - Explicit conversion
- `IsPathLike()` - Detect PATH-like environment variables
- `AppendPath()` - Merge paths with correct separators

## Dependencies

| Package | Purpose |
|---|---|
| Azure.Bicep.Local.Extension | Bicep extension host SDK (gRPC protocol) |
| LibGit2Sharp | Native git operations |
| xUnit | Test framework |

## Extension Registration

The entry point (`Program.cs`) registers all components with the Bicep extension host:

```csharp
builder.Services
    .AddBicepExtension()
    .WithExtensionInfo(name: "Flexor", version: "0.2.2026.1", isSingleton: true)
    .WithResourceHandler<RunResourceHandler>()
    .WithResourceHandler<ScriptResourceHandler>()
    .WithResourceHandler<RepositoryResourceHandler>()
    .WithResourceHandler<HttpResourceHandler>()
    .WithResourceHandler<FlexorModuleResourceHandler>()
    .WithResourceHandler<FlexorResourceHandler>();
```

The extension runs as a singleton, meaning one instance handles all resource operations within a deployment.

## Testing Strategy

### Integration Tests

The 6 integration tests in `Tests.cs` invoke `bicep local-deploy` against `.test.bicep` files. These test the full pipeline from Bicep parsing through gRPC to process execution and output capture.

### Unit Tests

The 121 unit tests cover individual components in isolation:

- `ProcessExecutorTests` - Process execution, timeout, cancellation, raw output
- `OutputParsingTests` - JSON/string parsing, ANSI stripping, output resolution, 7 output behavior cases
- `PathExtensionsTests` - Cross-platform path conversion
- `JsonTypeTests` - JSON type detection
- `ResultTests` - Result monad
- `ProcessResultTests` - Record behavior
- `OptionsTests` - Configuration defaults
- `LogFileTests` - Log file management
- `HelpersUnitTests` - Test helper utilities
- `TypeTests` - Any/OutputDictionary types
