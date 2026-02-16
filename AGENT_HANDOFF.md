# Flexor Project - Agent Handoff Document

**Last Updated:** February 16, 2026  
**Project Version:** 0.1.2026.1  
**Status:** Work in Progress

## Project Overview

**Flexor** is a Bicep Local Extension that enables flexible execution of scripts and commands during Azure Bicep deployments. It provides a framework for integrating disparate deployment components by offering parameterized script execution, output handling, error management, and cross-platform support.

### Key Capabilities
- Execute scripts and commands (PowerShell, Bash, Python) during Bicep deployment
- Support inline scripts and external script files
- Parameterized script execution with Bicep variables
- Environment variable management with append/overwrite options
- Container-based execution support
- Git repository cloning/pulling during deployment
- HTTP resource operations
- Cross-platform support (Windows, Linux, macOS)
- Comprehensive logging and error handling

## Architecture

### Overall Design
Flexor is implemented as an **Azure Bicep Local Extension** built with ASP.NET Core. It follows a handler pattern where each resource type (Run, Script, Repo, Http, etc.) has a corresponding resource handler that implements the `IResourceHandler` interface.

### Core Projects

#### 1. **Flexor (Main Extension)**
- **Path:** `src/Flexor/`
- **Purpose:** Main extension host and resource handlers
- **Technology:** ASP.NET Core, Azure.Bicep.Local.Extension v0.40.2
- **Target Framework:** .NET 10.0
- **Entry Point:** `Program.cs` - Configures the Bicep extension with all handlers

**Key Handlers:**
- `RunResourceHandler` - Executes programs with arguments and environment variables
- `ScriptResourceHandler` - Runs shell/scripting language scripts
- `RepositoryResourceHandler` - Git clone/pull operations
- `HttpResourceHandler` - HTTP resource operations
- `FlexorModuleResourceHandler` - Module resource handling
- `FlexorResourceHandler` - Generic Flexor resource handling

#### 2. **Flexor.Base**
- **Path:** `src/Flexor.Base/`
- **Purpose:** Shared base classes, options, and utilities
- **Key Components:**
  - `Options/` - Configuration classes for various execution contexts
    - `FlexorOptions` - Global configuration
    - `ExecutionOptionsBase` - Base class for execution options
    - `ProcessStartOptions` - Process execution configuration
    - `ScriptRunOptions` - Script execution configuration
    - `ContainerStartOptions` - Container execution configuration
    - `EnvOptions` - Environment variable handling
    - `Credential` - Credential management
  - `Executor/` - Execution engine
    - `ProcessExecutor` - Executes processes and captures output
    - `ProcessResult` - Result container with exit code and output
    - `LogFile` - Logging utilities
  - `Resources/` - Resource interfaces
    - `IFlexorResource` - Base resource interface
    - `IOutputResource` - Output capture interface
  - `Types.cs` - Type definitions
  - `Result.cs` - Result/error handling
  - `PathExtensions.cs` - Path utilities

#### 3. **Flexor.Git**
- **Path:** `src/Flexor.Git/`
- **Purpose:** Git operations (clone, pull)
- **Key Component:** `IGitClient` interface and implementation

#### 4. **Flexor.Tests**
- **Path:** `src/Flexor.Tests/`
- **Purpose:** Unit tests for core functionality
- **Contains:** Test cases for handlers, execution, and resource management

### API Version
- **Current:** `2026-01-01`
- **Namespace:** `Flexor/` (in Bicep)
- **Resource Types:**
  - `Flexor/run@2026-01-01`
  - `Flexor/script@2026-01-01`
  - `Flexor/repo@2026-01-01`
  - `Flexor/http@2026-01-01`

## Development Environment

### Requirements
- .NET 10.0 SDK
- PowerShell 7+ (for build scripts)
- Bicep v0.40.1 or later
- Visual Studio 2022 (recommended) or VS Code with C# extension

### Project Structure
```
Flexor/
├── .bicep/                 # Build output (generated)
├── scripts/                # Build helper scripts
│   ├── Build-Extension.ps1
│   ├── Get-Bicep.ps1
│   └── Run-Tests.ps1
├── src/
│   ├── assets/            # Bicep config and readme
│   ├── Flexor/            # Main extension
│   ├── Flexor.Base/       # Base/shared classes
│   ├── Flexor.Git/        # Git client
│   └── Flexor.Tests/      # Unit tests
├── tests/                 # Bicep test files
│   ├── *.test.bicep      # Resource type tests
│   ├── test.bicepparam   # Test parameters
│   └── tests.pester.ps1  # PowerShell test runner
├── build.ps1             # Main build script
├── test.ps1              # Test runner script
└── Flexor.sln            # Solution file
```

## Build & Deployment

### Build Process
```powershell
# Full build (default)
.\build.ps1

# With packaging
.\build.ps1 -Package

# Custom SDK version
.\build.ps1 -Sdk net10.0

# Specific runtimes
.\build.ps1 -Runtimes win-x64, linux-x64
```

**Build Outputs:**
- `.bicep/bin/` - Compiled extension binaries for each runtime
- `.bicep/` - Bicep artifacts and configuration
- `flexor-template.zip` - Packaged template (with -Package flag)

### Testing
```powershell
# Run all tests
.\test.ps1

# Tests validate:
# - Command execution
# - Script handling
# - Git operations
# - HTTP operations
# - Module resources
# - Container operations
```

### Supported Runtimes
- `osx-arm64` - macOS ARM64
- `linux-x64` - Linux x86-64
- `linux-arm64` - Linux ARM64
- `win-x64` - Windows x86-64
- `win-arm64` - Windows ARM64

## Key Design Patterns

### 1. Handler Pattern
Each resource type is handled by a dedicated handler implementing `IResourceHandler`:
```csharp
public interface IResourceHandler
{
    string ResourceType { get; }
    Task<ResourceResult> HandleAsync(ResourceRequest request);
}
```

### 2. Options Pattern
Configuration is managed through option classes with nested structures for granular control:
```csharp
var options = new ScriptRunOptions
{
    WorkingDirectory = "/path",
    TimeoutSeconds = 30,
    ContinueOnFailure = false,
    Environment = new EnvOptions { OverwritePaths = false }
};
```

### 3. Process Execution
Centralized through `ProcessExecutor` which:
- Captures stdout/stderr
- Manages environment variables
- Enforces timeouts
- Handles elevated privileges (Windows)
- Supports container execution

## Important Implementation Details

### Environment Variable Handling
- **Append Mode (Default):** New variables are added to existing PATH and similar
- **Overwrite Mode:** Existing PATH-like variables are replaced
- **Custom Append:** Specific variables can use custom delimiters

### Error Handling
- `continueOnFailure` option allows deployments to succeed even if scripts fail
- All errors are logged with full context (command, args, environment)
- Exit codes are captured and available in outputs

### Logging
- Windows: Event Log (`Flexor` source)
- Linux/macOS: Standard output/error
- Log file support in `Flexor.Base.Executor.LogFile`

### Container Support
- Requires Docker or compatible container CLI
- Configurable image, CLI, and CLI arguments
- Useful for isolated script execution

## Configuration

### Bicep Configuration
- **File:** `src/assets/bicepconfig.json`
- Controls Bicep compilation settings
- Included in build output

### Global Options
Configured via `FlexorV2026_01_01_Options` class in `Program.cs`

## Testing Strategy

### Unit Tests (Flexor.Tests)
- Resource handler tests
- Option validation
- Process execution tests

### Integration Tests (tests/*)
- Bicep files for each resource type:
  - `commands.test.bicep`
  - `containers.test.bicep`
  - `git.test.bicep`
  - `http.test.bicep`
  - `modules.test.bicep`
  - `scripts.test.bicep`
- Pester test runner: `tests.pester.ps1`
- Test assets in `tests/assets/`

## Current State & Status

### Completed Features
✅ Core execution framework (Run, Script handlers)
✅ Git integration
✅ HTTP operations
✅ Environment variable management
✅ Container support
✅ Cross-platform binary building
✅ Error handling and logging
✅ Process timeout support
✅ Elevated privilege execution (Windows)

### Known Limitations
- Work in progress project (breaking changes possible before 1.0)
- Assumes required scripting runtimes are pre-installed
- Container execution requires container CLI installed

### Version History
- **0.1.2026.1** - Current version
  - Initial feature-complete release
  - API version 2026-01-01

## Common Tasks

### Adding a New Resource Type
1. Create handler in `src/Flexor/v2026_1/Handlers/`
2. Implement `IResourceHandler` interface
3. Create corresponding options class in `Flexor.Base`
4. Register handler in `Program.cs` with `.WithResourceHandler<NewHandler>()`
5. Add Bicep type definitions
6. Create unit tests
7. Add integration test `.test.bicep` file

### Modifying Execution Options
1. Edit relevant options class in `src/Flexor.Base/Options/`
2. Update handler to use new options
3. Update Bicep type definitions
4. Add tests

### Updating Bicep Configuration
1. Edit `src/assets/bicepconfig.json`
2. Run `.\build.ps1` to incorporate changes

## Dependencies

### NuGet Packages
- `Azure.Bicep.Local.Extension` v0.40.2 - Bicep extension framework

### System Requirements
- PowerShell Core 7+
- Bicep v0.40.1+
- .NET 10.0 Runtime (for execution)

## Build Outputs Location
- **Compiled Binaries:** `.bicep/bin/`
- **Bicep Artifacts:** `.bicep/`
- **Packaged Template:** `flexor-template.zip` (with -Package)

## Additional Notes

### Code Style
- Implicit usings enabled (`using` directives simplified)
- Nullable reference types enabled
- C# latest features enabled

### Build Configuration
- Single-file self-contained deployments
- Native library inclusion for self-extraction
- Invariant globalization enabled

### Testing Notes
- Tests can be run via Pester PowerShell tests
- Integration tests use actual Bicep compilation
- Manual testing important for container operations

## Quick Commands Reference

```powershell
# Build only
.\build.ps1

# Build and package
.\build.ps1 -Package

# Run tests
.\test.ps1

# Get Bicep CLI
.\scripts/Get-Bicep.ps1

# Build Extension directly
.\scripts/Build-Extension.ps1 -Extension src/Flexor -OutputPath .bicep/bin

# Run Pester tests
.\tests/tests.pester.ps1
```

## Contact & Handoff Notes

This document provides a complete overview of:
- Project purpose and capabilities
- Architecture and component relationships
- Build and test processes
- Development patterns and conventions
- Current state and known limitations

For questions about specific functionality, refer to the README.md for user documentation or examine the handler implementations for technical details.

---
**Next Agent:** Review this document, then examine specific handlers or options classes as needed for your assigned work.
