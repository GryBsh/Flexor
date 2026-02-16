# Flexor

A Bicep module to flexibly run scripts and commands during deployment.

```yaml
version: 0.1.2026.1
```

## Why?
Why not? There are frequently times I need something quick and easy to help slap disparate parts of a 
deployment together, where the structural parts like parameters and parsing, slicing, and converting data types 
is all handled. So why not Bicep?

## Requirements
- Bicep v0.40.1 (or later)

## This a work in progress
Assume the composition of resources for unreleased API versions are subject to potential breaking change. Once released, it will remain fixed until superceded or deprecated/removed.

## Features
- Run scripts and commands during deployment
- Support for inline scripts and external script files
- Parameterization of scripts with Bicep variables and parameters
- Output handling to capture script results
- Error handling and logging for script execution
- Cross-platform support for Windows and Linux environments
- Support for multiple scripting languages (e.g., PowerShell, Bash, Python) 
  - (They need to be installed already)


## Resources

### Version: 2026-01-01

#### Run

Runs a program

```bicep
resource cmd 'Flexor/run@2026-01-01' = {
  name: 'some-name'                       // Name of the activity (log name)
  command: 'someexe'                      // The command/executable to run
  args: [                                 // Command line arguments (optional)
    '--arg',
    '-arg','value'
    ...
  ]
  env: {                                  // Environment Variables (optional)
    var: 'value'
    ...
  }
  options: {                              // Optional options (These are the same in Run, Script, and Module resources)
    workingDirectory: '/path/to/'         // A working directory to use other than the current
    runAsAdmin: false                     // Whether to run the command with elevated privleges
    timeoutSeconds: 30                    // A timeout to wait for execution to finish before failing the activity
    continueOnFailure: false              // Whether the command failing is a failure
    env: {                                // Options for how Environment Variables are handled
      overwritePaths: false               // Overwrite PATH and path-like variables instead of appending to them
      append: {                           // Other variables that should be appended to not overwritten, and the delimiter to use
        var: '|' 
      }
    }
    useContainer: false                   // should run in a container                
    containerImage: 'debian:latest'       // the image to use
    containerCli: 'docker'                // the container cli to use to run the container, if you use docker: you can omit this and cli args.
    containerCliArgs: ['run', '--rm', '-t'] // arguments for the container cli
  }
}
```

#### Script

Runs a Script

```bicep
resource script 'Flexor/script@2026-01-01' = {
  name: 'some-name'                       // Name of the activity (log name)
  shell: 'bash'                           // or cmd, powershell, or python
  script: '/path/to/script'               // Path to script file
                                          // OR
  contents: '''                           
# Literal Script Content
  '''                                     // A literal script
  env: { ... }                            // Same as Run
  args [ ... ]                            // Same as Run
  options: { ... }
}
```

#### Repo

Clone or Pull a Repository

```bicep
resource repo 'Flexor/repo@2026-10-01' = {   
  type: 'git'                             // Only git is currently supported, (optional)
  source: 'https://'                      // The repository source
  path: '/path/to/'                       // The local path for the repository
  credential: {                           // Credentials to use for the operation (optional)
    username: '...'
    password: [SecureString]
  }
}
```

#### Http

An HTTP client

```bicep
resource http 'Flexor/http@2026-01-01' = {  
  url: 'https://....'
  authorization: {
    bearerToken: '...'
    credential: { ... }
  }
  query: [                                // Query parameters
    { name: '...', value: ''}
  ]
  method: '...'                           // HTTP method
  headers: [                              // HTTP headers
    { name: '...', value: '...' }
  ],
  contentType: 'some/mimeType'            // Body MIME Type
  body: '...'                             // String body
  options: {
    timeoutSeconds: 300
    ignoreSslErrors: false
    followRedirects: true
  }
}
```

#### Module

Declares a Flexor reource type, that will handle requests for those types.

```bicep
resource 'module' 'Flexor/module@2026-01-01' = {
  type: 'my/rez'
  version: 'v1'
  shell: 'powershell'                  // or bash, cmd, python
  options: {
    type: 'env'                        // or args, how parameters are passed
    args: [ ... ]
    env: { ... } 
    exec: { ... }                      // Same as Run/Script Options
  }
  get: '/path/to/script'               // Path to get script
  getOptions: {
    exec: { ... }                      // option overrides for get
  }
  createOrUpdate: '/path/to/script'    // Path to createOrUpdate script
  createOrUpdateOptions: {
    exec: { ... }                      // option overrides for createOrUpdate
  }
  delete: '/path/to/script'            // Path to delete script
  deleteOptions: { ... }               // option overrides for delete
}
```

#### Resource

Uses a previously decleared resource module

```bicep
resource rez 'Flexor/resource@2026-01-01' = {
  name: 'my-resource-name'
  type: 'my/rez@v1'
  parameters: {                        // Parameters to pass to the resource handler script
    param1: 'value1'
    param2: 'value2'
  }
  options: { 
    exec: { ... }                      // Same as Module exec options
  }
}
```

Parameters are passed as environment variables (`PARAM_{name}`) or args (`--{name} {value}`) depending on the `options.type` setting.

## Known Issues

- PowerShell and Python Scripts do not execute under GitHub Actions
  - I'd like to be able to run the test suite when releases are created, so, I'm looking into this.