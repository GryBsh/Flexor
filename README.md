# Flexor

A Bicep module to flexibly run scripts and commands during deployment.

```yaml
version: 0.1.2026.1
```

## Why?
Why not? There are frequently times I need something quick and easy to help slap disparate parts of a 
deployment together, where the structural parts like parameters and parsing, slicing, and converting data types 
is all handled. So why not Bicep?

## Features

- Run scripts and commands during deployment
- Support for inline scripts and external script files
- Parameterization of scripts with Bicep variables and parameters
- Output handling to capture script results
- Error handling and logging for script execution
- Cross-platform support for Windows and Linux environments
- Support for multiple scripting languages (e.g., PowerShell, Bash, Python) 
  - (They need to be installed already)
