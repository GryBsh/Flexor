using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using Flexor.Executor.Scripts;
using Flexor.Options;
using Flexor.Resources;
using Microsoft.Extensions.Logging;

namespace Flexor.Executor;

/// <summary>
/// Provides functionality for executing external commands and scripts. 
/// </summary>
public static partial class ProcessExecutor
{
    public const string StringOutputKey = "";
    public const string ArrayOutputKey = "[]";
    
    #region Start Process
    /// <summary>
    /// Starts a process with the given context.
    /// </summary>
    /// <param name="context">
    /// The context for starting the process.
    /// </param>
    /// <param name="logger">
    /// The logger to use for logging process information.
    /// </param>
    /// <param name="cancellationToken">
    /// The cancellation token to observe while waiting for the process to complete.
    /// </param>
    /// <returns>
    /// A ProcessResult representing the outcome of the process execution.
    /// </returns>
    public static async Task<ProcessResult> StartProcessAsync(
        ProcessStartOptions context,
        ILogger? logger = null,
        CancellationToken cancellationToken = default
    )
    {
       if (context.Command is null)
        {
            throw new ArgumentException("Command must be provided to run a process.");
        }

        var timeout = context.TimeoutSeconds.HasValue 
                      && context.TimeoutSeconds.Value > 0
                    ? TimeSpan.FromSeconds(context.TimeoutSeconds.Value) 
                    : Timeout.InfiniteTimeSpan;
        
        ProcessStartInfo startInfo = CreateProcessStartInfo(
            context.Command.AsPath(),
            context.Args,
            context.WorkingDirectory,
            context.Env ?? [],
            context.RunAsAdmin
        );

        using Process process = new() { StartInfo = startInfo };
        
        process.OutputDataReceived += (_, e) =>
        {
            var data = e.Data ?? string.Empty;
            context.OutputHandler?.Invoke(data);
        };
        process.ErrorDataReceived += (_, e) =>
        {
            var data = e.Data ?? string.Empty;
            context.ErrorHandler?.Invoke(data);
        };
                
        process.Start();        
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        if (context.Input is not null && context.Input.Length > 0)
        {
            foreach (var line in context.Input)
            {
                await process.StandardInput.WriteLineAsync(line);
            }
            process.StandardInput.Close();
        }
        
        await process.WaitForExitAsync(
            CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken,
                timeout == Timeout.InfiniteTimeSpan 
                ? CancellationToken.None 
                : new CancellationTokenSource(timeout).Token
            ).Token
        );

        await Task.Delay(250, cancellationToken); // Ensure all output is flushed
        
        bool shouldKill = process.HasExited is false;
        bool isTimedOut = timeout != Timeout.InfiniteTimeSpan 
                          && process.StartTime.Add(timeout) < DateTime.Now;
        bool wasCancelled = cancellationToken.IsCancellationRequested;
        bool nonZeroExitCode = process.ExitCode != 0;
       
        if (shouldKill) { process.Kill(true); }

        bool success = !( nonZeroExitCode || isTimedOut || shouldKill || wasCancelled ); 
            
        return new ProcessResult(success, process.ExitCode, isTimedOut, wasCancelled, shouldKill);
        
        /// <summary>
        /// Creates a ProcessStartInfo object with the specified parameters.
        /// </summary>
        /// <param name="command">
        /// The command to execute.
        /// </param>
        /// <param name="arguments">
        /// The arguments to pass to the command.
        /// </param>
        /// <param name="workingDirectory">
        /// The working directory for the process.
        /// </param>
        /// <param name="environmentVariables">
        /// The environment variables to set for the process.
        /// </param>
        /// <param name="runAsAdmin">
        /// Whether to run the process with administrative privileges.
        /// </param>
        /// <returns>
        /// A ProcessStartInfo object configured with the specified parameters.
        /// </returns>
        static ProcessStartInfo CreateProcessStartInfo(
            string command, 
            string[] arguments, 
            string? workingDirectory, 
            Dictionary<string, string>? environmentVariables, 
            bool runAsAdmin
        )
        {
            ProcessStartInfo startInfo = new()
            {
                FileName = command,
                Arguments = ConstructArguments(arguments),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                UseShellExecute = false,
                WorkingDirectory = workingDirectory ?? Environment.CurrentDirectory,
            };

            if (runAsAdmin)
            {
                startInfo.Verb = "runas";
            }

            foreach (var env in environmentVariables ?? [])
            {
                if (env.Key is not null && env.Value is not null)
                {
                    startInfo.Environment[env.Key] = env.Value.ToString() ?? string.Empty;
                }
            }

            return startInfo;            
        }

        /// <summary>
        /// Constructs a command-line argument string from an array of arguments.
        /// </summary>
        /// <param name="arguments">
        /// The array of command-line arguments.
        /// </param>
        /// <returns>
        /// A string representing the command-line arguments.
        /// </returns>
        static string ConstructArguments(string[] arguments)
        {
            return string.Join(" ", arguments.Select(arg => arg.Contains(' ') ? $"\"{arg}\"" : arg));
        }
    }

    #endregion

    #region StartProcess with Resource
    /// <summary>
    /// Starts a process with the given context and configuration.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the resource to run, must implement IFlexorResource. 
    /// </typeparam>
    /// <param name="resource">
    /// The resource to run.
    /// </param>
    /// <param name="config">
    /// The configuration for the Flexor environment.
    /// </param>
    /// <param name="context">
    /// The context for starting the process.
    /// </param>
    /// <param name="logger">
    /// The logger to use for logging process information.
    /// </param>
    /// <param name="cancellationToken">
    /// The cancellation token to observe while waiting for the process to complete.
    /// </param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the command to run is not provided in the context.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the command invocation fails.
    /// </exception>
    public static async Task<ProcessResult> StartProcessAsync<T>(
        T resource,
        FlexorBaseOptions config,
        ProcessStartOptions context,
        ILogger? logger = null,
        CancellationToken cancellationToken = default
    )   where T : IFlexorResource
    {
        if (context.Command is null)
        {
            throw new ArgumentException("Command must be provided to run a process.");
        }

        var timeout = context.TimeoutSeconds.HasValue 
                      && context.TimeoutSeconds.Value > 0
                    ? TimeSpan.FromSeconds(context.TimeoutSeconds.Value) 
                    : Timeout.InfiniteTimeSpan;

        OutputDictionary? outputs = null;
        if (resource is IOutputResource)
        {
            outputs = [];
        }
        
        var (name, stdOutFile, stdErrFile) = LogFile.BeginOrRollover(
            config,
            resource.Name, 
            resource.AppendLogs || config.LogOptions.Append,
            resource.CleanupLogs || config.LogOptions.DisableRollover
        );

        Action<string> stdOutputHandler 
            = (value) =>
            {
                if (outputs is { })
                {
                    HandleOutput(outputs, name, value);
                }
                if (resource.EnableLogging || config.LogOptions.Enabled)
                    File.AppendAllText(stdOutFile, value + "\n");   
            };

        Action<string> stdErrorHandler 
            = (value) =>
            {
                if (resource.EnableLogging || config.LogOptions.Enabled)
                    File.AppendAllText(stdErrFile, value + "\n");        
            };
        context.Env = MergeEnvironment(
            context,
            config
        );
        //logger?.LogInformation("Starting process. Command: {Command} {Arguments}", context.Command, string.Join(" ", context.Args));

        var result = await StartProcessAsync(
            new ProcessStartOptions
            {
                Command = context.Command,
                Args = context.Args,
                WorkingDirectory = context.WorkingDirectory,
                Env = context.Env,
                Input = context.Input,
                OutputHandler = stdOutputHandler,
                ErrorHandler = stdErrorHandler,
                RunAsAdmin = context.RunAsAdmin,
                ContinueOnFailure = context.ContinueOnFailure,
                TimeoutSeconds = context.TimeoutSeconds
            },
            logger,
            cancellationToken
        );

        var (success, exitcode, _, _, _ ) = result;

        if (!success)
        {
            throw new InvalidOperationException(
                $"Command Invocation Failed.\n"+
                $"Command: {context.Command.AsPath()} {string.Join(" ", context.Args)}\n" +
                $"Exit Code: {exitcode}.\n" +
                $"See logs for more details:\n"+
                $"StdOut: {stdOutFile.AsPath()}\n"+
                $"StdErr: {stdErrFile.AsPath()}"
            );
        }
        
        if (resource is IOutputResource outputResource && outputs is { })
        {
            ApplyOutput(outputResource, name, outputs);
        }

        return result;

        /// <summary>
        /// Handles output data by determining its type and storing it appropriately.
        /// </summary>
        /// <param name="outputs">
        /// The dictionary to store output data.
        /// </param>
        /// <param name="name">
        /// The name of the resource.
        /// </param>
        /// <param name="data">
        /// The output data to handle.
        /// </param>
        /// <param name="processStrings">
        /// Whether to process string outputs.
        /// </param>
        /// <returns></returns>
        static void HandleOutput(OutputDictionary outputs,string name, string data, bool processStrings = true)
        {
            var stepOutputs = outputs.GetValueOrDefault(name) ?? (outputs[name] = []);
            data = data.Trim();

            var (isObject, isArray) = ResolveJsonType(data);
            if (isObject || isArray)
            {
                if (isObject)
                {
                    var output = JsonSerializer.Deserialize<Any>(data)
                                ?? throw new InvalidOperationException("Failed to deserialize output data to dictionary.");
                    stepOutputs.Add(output);
                }
                else if (isArray)
                {
                    var output = JsonSerializer.Deserialize<List<object?>>(data)
                                 ?? throw new InvalidOperationException("Failed to deserialize output data to list.");

                    stepOutputs.Add(new Any() { [ArrayOutputKey] = output });
                }
            }
            else if (processStrings)
            {
                if (stepOutputs.Count == 0)
                {
                    stepOutputs.Add([]);
                }

                if (stepOutputs[0].TryGetValue(StringOutputKey, out var existingText))
                {
                    //remove console characters \x1b[...m
                    existingText = AnsiConsoleCharacters().Replace(existingText?.ToString() ?? string.Empty, string.Empty);
                    
                    stepOutputs[0][StringOutputKey] = existingText + "\n" + data;
                }
                else if (!string.IsNullOrWhiteSpace(data))
                {
                    stepOutputs[0][StringOutputKey] = data;
                }
            }           

            static (bool IsObject, bool IsArray) ResolveJsonType(string input)
            {
                if (string.IsNullOrWhiteSpace(input))
                    return (false, false);

                return (IsJsonObject(input), IsJsonArray(input));
                
                static bool IsJsonObject(string input)
                    => input.StartsWith('{')
                    && input.EndsWith('}');   // Object
            

                static bool IsJsonArray(string input)
                    => input.StartsWith('[')
                    && input.EndsWith(']');  // Array
            }
        };

        /// <summary>
        /// Applies the output data to the resource.
        /// </summary>
        /// <param name="resource">
        /// The resource to which the output will be applied.
        /// </param>
        /// <param name="name">
        /// The name of the resource.
        /// </param>
        /// <param name="outputs">
        /// The dictionary containing output data.
        /// </param>
        /// <returns></returns>
        static void ApplyOutput(IOutputResource resource, string name, OutputDictionary outputs) 
        {
            var stepOutputs = outputs.GetValueOrDefault(name);
            if (stepOutputs is null)
                return;

            List<(Type Type, object? Output)> outputList = [];
            foreach (var outputDict in stepOutputs)
            {
                
                if (outputDict.TryGetValue(ArrayOutputKey, out object? value))
                {
                    outputList.Add((typeof(object[]), value));
                }
                else if (
                    outputDict.Count == 1 && outputDict.ContainsKey(StringOutputKey)
                    && outputDict.TryGetValue(StringOutputKey, out var outputText) 
                    && outputText?.ToString() is string text
                    && !string.IsNullOrEmpty(text)
                )
                {
                    if (outputList.FirstOrDefault().Type == typeof(string))
                    {
                        var existing = outputList[0].Output?.ToString() ?? string.Empty;
                        outputList[0] = (typeof(string), existing + "\n" + text.Trim());
                        continue;
                    }
                    outputList.Add((typeof(string), text.Trim()));
                    
                }
                else if (outputDict.Count == 0)
                {
                    outputList.Add((typeof(string), string.Empty));
                }
                else
                {
                    outputList.Add((typeof(Any), outputDict));
                }
            }

            if (outputList.Count == 1)
            {
                if (outputList[0].Type == typeof(string))
                {
                    resource.Output = outputList[0].Output?.ToString() ?? string.Empty;
                    return;
                }
                resource.Output = JsonSerializer.Serialize(outputList[0].Output);
            }
            else
            {
                resource.Output = JsonSerializer.Serialize<object?[]>([ ..outputList.Select(o => o.Output) ]);
            }
        }
    
    }

    
    #endregion

    #region Run Script

    /// <summary>
    /// Runs a script resource with the given context and configuration.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the resource to run, must implement IFlexorResource and IOutputResource. 
    /// </typeparam>
    /// <param name="resource">
    /// The resource requesting the script execution. 
    /// </param>
    /// <param name="config">
    /// The Flexor environment configuration. 
    /// </param>
    /// <param name="context">
    /// The context for the script run.
    /// </param>
    /// <param name="logger">
    /// An optional logger for logging output and errors.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token to cancel the operation.
    /// </param>
    /// <returns>
    /// <exception cref="ArgumentException">
    /// Thrown when both 'script' and 'contents' are either null or empty, or when both are provided simultaneously.
    /// </exception>
    public static async Task<ProcessResult> RunScriptAsync<T>(
        T resource,
        FlexorBaseOptions? config,
        ScriptRunOptions context,
        ILogger? logger = null,
        CancellationToken cancellationToken = default
    )   where T : IFlexorResource, IOutputResource
    {
        if (context.Path is null && context.Input is null or { Length: 0 })
        {
            throw new ArgumentException("Either 'script' or 'contents' must be provided for script execution.");
        }
        if (context.Path is not null && context.Input is { Length: > 0 })
        {
            throw new ArgumentException("Only one of 'script' or 'contents' can be provided for script execution.");
        }
        
        config ??= new FlexorBaseOptions();
        context.ShellType = ResolveShellType(context.ShellType);
        context.Command = ResolveShellCommand(context.ShellType); 
        context.Args = ResolveShellArguments(context);

        context.Env = MergeEnvironment(context, config);

        if (context.ShellType is ShellType.PowerShell)
        {
            context = AddPowerShell(context, config);
        }

        return await StartProcessAsync(
            resource,
            config,
            context,
            logger,
            cancellationToken
        );
                
        /// <summary>
        /// Resolves the shell type, defaulting based on the operating system if necessary.
        /// </summary>
        /// <param name="shellType">
        /// The type of shell to use for execution.
        /// </param>
        /// <returns>
        /// The resolved shell type.
        /// </returns>
        static ShellType ResolveShellType(ShellType shellType)
        {
            if (shellType is ShellType.Default)
            {
                shellType = Environment.OSVersion.Platform switch
                {
                    _ when OperatingSystem.IsWindows()
                        => ShellType.Cmd,
                    _ => ShellType.Bash
                };
            }

            return shellType;
        }

        /// <summary>
        /// Resolves the shell command based on the shell type.
        /// </summary>
        /// <param name="shellType">
        /// The type of shell to use for execution.
        /// </param>
        /// <returns>
        /// A string representing the shell command.
        /// </returns>
        static string ResolveShellCommand(ShellType shellType)
        {
            return shellType switch
            {
                ShellType.Bash => "bash",
                ShellType.PowerShell => "pwsh",
                ShellType.Cmd => "cmd",
                ShellType.Python => "python",
                _ => throw new ArgumentException("Shell type must be one of Cmd, PowerShell, Bash, or Python.")
            };
        }

        /// <summary>
        /// Resolves the shell arguments based on the shell type and script context.
        /// </summary>
        /// <param name="context">
        /// The script run context containing shell type, script path,
        /// and arguments.
        /// </param>
        /// <returns>
        /// An array of strings representing the resolved shell arguments.
        /// </returns>
        static string[] ResolveShellArguments(ScriptRunOptions context)
        {
            string[] shellArgs = context.ShellType switch
            {
                var cmd when cmd == ShellType.Cmd
                    && context.Path is not null
                    => ["/c", $"\"{context.Path}\""],
                var ps when ps == ShellType.PowerShell
                    && context.Path is not null
                    => ["-File", context.Path],
                var ps when ps == ShellType.PowerShell
                    && context.Path is null
                    && context.Args is { Length: > 0 }
                    => throw new ArgumentException(" You cannot use arguments eith PowerShell literals"),
                var ps when ps == ShellType.PowerShell
                    && context.Path is null
                    => ["-Command", "-"],
                var bash when bash == ShellType.Bash
                    && context.Path is not null
                    => ["-c", "--", context.Path],
                var bash when bash == ShellType.Bash
                    && context.Path is null
                    && context.Args is { Length: > 0 }
                    => ["-s"],
                var bash when bash == ShellType.Bash
                    && context.Path is null
                    => ["-"],
                var python when python == ShellType.Python
                    && context.Path is null
                    && context.Args is { Length: > 0 }
                    => throw new ArgumentException(" You cannot use arguments with Python literals"),
                var python when python == ShellType.Python
                    && context.Path is not null
                    => [context.Path],
                var python when python == ShellType.Python
                    && context.Path is null
                    => ["-"],
                _ => throw new ArgumentException("Shell type must be one of Cmd, PowerShell, Bash, or Python.") 
            };

            return [ 
                ..shellArgs, 
                ..context.Args ?? [] 
            ];
        }

        /// <summary>
        /// Adds PowerShell-specific environment variables to the context.
        /// </summary>
        /// <param name="context">
        /// The script run context to which PowerShell environment variables will be added.
        /// </param>
        /// <param name="config">
        /// The Flexor environment containing configuration settings.
        /// </param>
        /// <returns>
        /// The updated script run context with PowerShell environment variables added.
        /// </returns>
        static ScriptRunOptions AddPowerShell(ScriptRunOptions context, FlexorBaseOptions config)
        {
            context.Env ??= [];
            var psModuleDir = new DirectoryInfo(Path.Combine(config.PathRoot, PwshConstants.PSModulesFolderName));
            var existing = Environment.GetEnvironmentVariable(PwshConstants.PSModulePathEnvVar) ?? string.Empty;
            var value = context.Env.GetValueOrDefault(PwshConstants.PSModulePathEnvVar) ?? string.Empty;

            string[] values = (existing, value, psModuleDir.Exists) switch
            {
                (_, _, true) when !string.IsNullOrWhiteSpace(existing) && !string.IsNullOrWhiteSpace(value)
                    => [existing, value.AsPath(), psModuleDir.FullName.AsPath()],
                (_, _, true) when !string.IsNullOrWhiteSpace(existing)
                    => [existing, psModuleDir.FullName.AsPath()],
                (_, _, true) when !string.IsNullOrWhiteSpace(value)
                    => [value.AsPath(), psModuleDir.FullName.AsPath()],
                (_, _, true)
                    => [psModuleDir.FullName.AsPath()],
                _ => []
            };

            // Special handling for PowerShell module path, never overwrite, always append
            context.Env[PwshConstants.PSModulePathEnvVar] = existing.AppendPath(values);
            return context;
        }
    }

    #endregion

    #region Helpers
    /// <summary>
    /// Merges the environment variables from the process context and the Flexor configuration.
    /// </summary>
    /// <param name="context">
    /// The process start context containing environment variables to merge.
    /// </param>
    /// <param name="config">
    /// The Flexor environment configuration.
    /// </param>
    /// <returns>
    /// A dictionary containing the merged environment variables.
    /// </returns>
    static Dictionary<string, string> MergeEnvironment(
        ProcessStartOptions context, 
        FlexorBaseOptions config
    ) {
        context.Env ??= [];
        var env = new Dictionary<string, string>(context.Env);
        
        foreach (var (key, value) in context.Env)
        {
            var existing = Environment.GetEnvironmentVariable(key);

            // Path-like variables
            if ((key, existing).IsPathLike() || (key, value).IsPathLike())
            {
                // Replace or append to existing path or path-like variable
                env[key] = context.OverwriteEnvPaths
                         ? value.AsPath().AppendPath()
                         : existing.AppendPath(value.AsPath());
            }
            // User-defined appended variables
            else if (
                context.AppendEnvVars is {} vars 
                && vars.Length > 0
                && vars.FirstOrDefault(v => v.Name.Equals(key, StringComparison.OrdinalIgnoreCase)) is {} appendVar
                && !string.IsNullOrWhiteSpace(appendVar.Delimiter)
            ) {              
                string?[] values = existing is {} && !string.IsNullOrWhiteSpace(existing)
                    ? [existing, value]
                    : [value];
                    
                // Append to specified variable with delimiter
                env[key] = string.Join(appendVar.Delimiter, values);
            }
            // Other variables
            else
            {
                // Overwrite other variables
                env[key] = value;
            }
            
        }
        return env;
    }
 
    #endregion

    /// <summary>
    /// Regex to match ANSI console characters
    /// </summary>
    /// <remarks>
    /// Matches escape sequences that start with ESC [ and end with m, containing digits and semicolons in between.
    /// </remarks>
    /// <returns>A Regex object that matches ANSI console characters.</returns>
    [GeneratedRegex(@"\x1b\[[0-9;]*m", RegexOptions.Compiled)]
    private static partial Regex AnsiConsoleCharacters();
}
