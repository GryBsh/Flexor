using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Flexor.Executor.Scripts;
using Flexor.Options;
using Flexor.Resources;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;

namespace Flexor.Executor;

/// <summary>
/// Provides functionality for executing external commands and scripts. 
/// </summary>
public static partial class ProcessExecutor
{
    

 
    const string ContainerCli = "docker";
    static string[] ContainerCliArgs = [ "run", "--rm", "-i" ];

    const string WorkingPathMount = "/workspace";
    const string FlexorMount = "/.flexor";

    public static void LogContainerOptions(ILogger logger, string location, ProcessStartOptions options)
    {
        /*
        logger.LogInformation(
            "Container @ {} : {} | {} {} {}",
            location,
            options.UseContainer,
            options.UseContainer ? options.ContainerCli ?? "N/A" : "N/A",
            options.UseContainer ? string.Join(" ", options.ContainerCliArgs ?? []) : "N/A",
            options.UseContainer ? string.Join(" ", options.ContainerMounts?.Select(kv => $"{kv.Key}:{kv.Value}") ?? []) : "N/A"
            
        );
        */
    }

    
    #region Start Process
    /// <summary>
    /// Starts a process with the given context.
    /// </summary>
    /// <param name="options">
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
    public static async Task<ProcessResult> RunAsync(
        ProcessStartOptions options,
        ILogger logger,
        CancellationToken cancellationToken = default
    ) 
    {
       if (options.Command is null)
        {
            throw new ArgumentException("Command must be provided to run a process.");
        }

        var timeout = options.TimeoutSeconds.HasValue 
                      && options.TimeoutSeconds.Value > 0
                    ? TimeSpan.FromSeconds(options.TimeoutSeconds.Value) 
                    : Timeout.InfiniteTimeSpan;
        
        var mounts = options.UseContainer ? options.ContainerMounts ?? [] : [];

        mounts[options.WorkingDirectory ?? Environment.CurrentDirectory] = options.WorkingPathMount ?? WorkingPathMount;                     
        mounts[options.FlexorPath ?? Path.Combine(Environment.CurrentDirectory, ".bicep/flexor")] = FlexorMount;

        LogContainerOptions(logger, nameof(RunAsync), options);

        ProcessStartInfo startInfo = CreateProcessStartInfo(
            options.Command.AsPath(),
            options.Args,
            options.WorkingDirectory,
            options.Env ?? [],
            options.RunAsAdmin,
            options.UseContainer,
            new(
                options.UseContainer ? options.ContainerCli : null,
                options.UseContainer ? options.ContainerCliArgs : null,
                options.UseContainer ? options.ContainerImage : null,
                options.UseContainer ? mounts : null
            )
        );

        logger.LogInformation("Executing process. Command: {Command} {Arguments}", startInfo.FileName, string.Join(" ", startInfo.ArgumentList));

        using Process process = new() { StartInfo = startInfo };
        
        process.OutputDataReceived += (_, e) =>
        {
            var data = e.Data ?? string.Empty;
            options.StdOutputHandler?.Invoke(data);
        };
        process.ErrorDataReceived += (_, e) =>
        {
            var data = e.Data ?? string.Empty;
            options.StdErrorHandler?.Invoke(data);
        };
                
        process.Start();        
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        if (options.Input is not null && options.Input.Length > 0)
        {
            foreach (var line in options.Input)
            {
                await process.StandardInput.WriteAsync(
                    RemoveWindowsLineEndings(line) + "\n"
                );
            }
            process.StandardInput.Close();
        }

        if (process.HasExited)
        {
            logger.LogWarning("Process exited immediately after starting. Command: {Command} {Arguments}", options.Command, string.Join(" ", options.Args));
        } 
        else {
            await process.WaitForExitAsync(
                CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken,
                    timeout == Timeout.InfiniteTimeSpan 
                    ? CancellationToken.None 
                    : new CancellationTokenSource(timeout).Token
                ).Token
            );
        }

        await Task.Delay(250, cancellationToken); // Ensure all output is flushed
        
        bool shouldKill = process.HasExited is false;
        bool isTimedOut = timeout != Timeout.InfiniteTimeSpan 
                          && process.StartTime.Add(timeout) < DateTime.Now;
        bool wasCancelled = cancellationToken.IsCancellationRequested;
        bool nonZeroExitCode = process.ExitCode != 0;
       
        if (shouldKill) { process.Kill(true); }

        bool success = !( nonZeroExitCode || isTimedOut || shouldKill || wasCancelled ); 
            
        return new ProcessResult(success, process.ExitCode, isTimedOut, wasCancelled, shouldKill);
        
        static string RemoveWindowsLineEndings(string input)
            => input.Replace("\r\n", "\n").Replace("\r", "\n");
        
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
            string fileName, 
            string[] arguments, 
            string? workingDirectory, 
            Dictionary<string, string>? environmentVariables, 
            bool runAsAdmin, 
            bool useContainer,
            ContainerStartOptions? csInfo = null
        )
        {
            ProcessStartInfo startInfo = new()
            {
                FileName = useContainer 
                            ? csInfo?.Cli ?? ContainerCli 
                            : fileName,
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
            if (useContainer)
            {
                foreach (var arg in csInfo?.Args ?? ContainerCliArgs)
                {
                    startInfo.ArgumentList.Add(arg);
                }
            }

            foreach (var env in environmentVariables ?? [])
            {
                if (env.Key is not null && env.Value is not null)
                {
                    if (useContainer)
                    {
                        // For containerized execution, prefix environment variables with -e
                        startInfo.ArgumentList.Add("-e");
                        startInfo.ArgumentList.Add($"{env.Key}={env.Value}");        
                    }
                    else { startInfo.Environment[env.Key] = env.Value.ToString() ?? string.Empty; }
                }
            }

            if (useContainer && csInfo?.Mounts is not null)
            {
                foreach (var mount in csInfo.Mounts)
                {
                    startInfo.ArgumentList.Add("-v");
                    startInfo.ArgumentList.Add($"{mount.Key}:{mount.Value.AsUnixPath()}");
                }
            }
            
            if (useContainer)
            {
                if (csInfo?.Image is null)
                {
                    throw new ArgumentException("Container image must be provided when using containerized execution.");
                }
                startInfo.ArgumentList.Add(csInfo.Image);
                startInfo.ArgumentList.Add(fileName);
            }
            
            foreach (var arg in arguments)
            {
                startInfo.ArgumentList.Add(arg);
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
        //static string ConstructArguments(string[] arguments)
        //{
        //    return string.Join(" ", arguments.Select(arg => arg.Contains(' ') ? $"\"{arg}\"" : arg));
        //}


    }

    #endregion

    #region Run with Resource
    /// <summary>
    /// Starts a process with the given context and configuration.
    /// </summary>
    /// <typeparam name="TResource">
    /// The type of the resource to run, must implement IFlexorResource. 
    /// </typeparam>
    /// <param name="resource">
    /// The resource to run.
    /// </param>
    /// <param name="config">
    /// The configuration for the Flexor environment.
    /// </param>
    /// <param name="options">
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
    public static async Task<ProcessResult> RunAsync<TResource>(
        TResource resource,
        FlexorOptions config,
        ProcessStartOptions options,
        ILogger logger,
        CancellationToken cancellationToken = default
    )   where TResource : IFlexorResource
    {
        if (options.Command is null)
        {
            throw new ArgumentException("Command must be provided to run a process.");
        }

        var timeout = options.TimeoutSeconds.HasValue 
                      && options.TimeoutSeconds.Value > 0
                    ? TimeSpan.FromSeconds(options.TimeoutSeconds.Value) 
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

        var originalOutputandler = options.StdOutputHandler;
        var originalErrorHandler = options.StdErrorHandler;

        options.StdOutputHandler
            = (value) =>
            {
                if (outputs is { })
                {
                    ParseOutput(outputs, name, value);
                }
                if (originalOutputandler is { } handler)
                {
                    handler(value);
                }
                if (resource.EnableLogging || config.LogOptions.Enabled)
                {
                    WriteLog(stdOutFile, value);
                }
            };

        options.StdErrorHandler 
            = (value) =>
            {
                if (originalErrorHandler is { } handler)
                {
                    handler(value);
                }
                if (resource.EnableLogging || config.LogOptions.Enabled)
                {
                    WriteLog(stdErrFile, value);
                }
            };
        
        if (!options.UseContainer)
        {
            options.Env = MergeEnvironment(
                options,
                config
            );
        }
        
        LogContainerOptions(logger, nameof(RunAsync)+"WithResource", options);

        var result = await RunAsync(
            options,
            logger,
            cancellationToken
        );

        var (success, exitcode, _, _, _ ) = result;

        if (!success)
        {
            throw new InvalidOperationException(
                $"Command Invocation Failed.\n"+
                $"Command: {options.Command.AsPath()} {string.Join(" ", options.Args)}\n" +
                $"Exit Code: {exitcode}.\n" +
                $"See logs for more details:\n"+
                $"StdOut: {stdOutFile.AsPath()}\n"+
                $"StdErr: {stdErrFile.AsPath()}"
            );
        }
        
        if (resource is IOutputResource outputResource && outputs is { })
        {
            outputResource.Output = StrigifyOutput(name, outputs);
        }

        return result;

        static void WriteLog(string file, string line)
        {
            File.AppendAllLines(file, [AnsiConsoleCharacters().Replace(line, string.Empty)]);
        }
    }

    #endregion

    #region Output Handling
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
    public static void ParseOutput(OutputDictionary outputs, string name, string data, bool processStrings = true)
    {
        var current = outputs.GetValueOrDefault(name) ?? (outputs[name] = []);
        data = data.Replace("\r", string.Empty)
                   .Replace("\n", string.Empty);

        var (_, isObject, isArray, _) = JsonType.Of(data.Trim());
        if (isObject || isArray)
        {
            if (isObject)
            {
                var output = JsonSerializer.Deserialize<Any>(data)
                            ?? throw new InvalidOperationException("Failed to deserialize output data to dictionary.");
                current.Add(output);
            }
            else if (isArray)
            {
                var output = JsonSerializer.Deserialize<List<object?>>(data)
                             ?? throw new InvalidOperationException("Failed to deserialize output data to list.");

                current.Add(new Any() { [OutputDictionary.ArrayOutputKey] = output });
            }
        }
        else if (processStrings)
        {
            if (current.Count == 0)
            {
                current.Add([]);
            }

            if (current[0].TryGetValue(OutputDictionary.StringOutputKey, out var existingText))
            {
                //remove console characters \x1b[...m
                existingText = AnsiConsoleCharacters().Replace(existingText?.ToString() ?? string.Empty, string.Empty);

                current[0][OutputDictionary.StringOutputKey] = existingText + "\n" + data;
            }
            else if (!string.IsNullOrWhiteSpace(data))
            {
                current[0][OutputDictionary.StringOutputKey] = data;
            }
        }
    }

    

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
    public static object? ResolveOutput(string name, OutputDictionary outputs)
    {
        var stepOutputs = outputs.GetValueOrDefault(name);
        if (stepOutputs is null)
            return null;

        List<(Type Type, object? Output)> outputList = [];
        foreach (var output in stepOutputs)
        {
            if (output.TryGetValue(OutputDictionary.ArrayOutputKey, out object? value))
            {
                outputList.Add((typeof(object[]), value));
            }
            else if (
                output.Count == 1 && output.ContainsKey(OutputDictionary.StringOutputKey)
                && output.TryGetValue(OutputDictionary.StringOutputKey, out var outputText)
                && outputText is string text
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
            else if (output.Count == 0)
            {
                outputList.Add((typeof(string), string.Empty));
            }
            else
            {
                outputList.Add((typeof(Any), output));
            }
        }

        if (outputList.Count == 1)
        {
            var singleOutput = outputList[0].Output;
            // If the output is a string, try to parse it as JSON
            if (singleOutput is string strOutput && !string.IsNullOrWhiteSpace(strOutput))
            {
                var (_, isObject, isArray, _) = JsonType.Of(strOutput.Trim());
                if (isObject)
                {
                    return JsonSerializer.Deserialize<Any>(strOutput);
                }
                else if (isArray)
                {
                    return JsonSerializer.Deserialize<List<object?>>(strOutput);
                }
            }
            return singleOutput;
        }
        return outputList.Select(o => o.Output).ToArray();
    }

    public static string StrigifyOutput(string name, OutputDictionary outputs)
    {
        var output = ResolveOutput(name, outputs);
        if (output is null) { return "{}"; }
        
        if (output is string strOutput)
        {
            return strOutput;
        }
        else if (output is IEnumerable<object> arrOutput)
        {
            var allStrings = arrOutput.All(o => o is string);
            if (allStrings)
            {
                var strOutputs = string.Join("\n", arrOutput.Select(o => o?.ToString()));
                var newOutputs = new OutputDictionary();
                ParseOutput(newOutputs, name, strOutputs, processStrings: false);
                var newOutput = ResolveOutput(name, newOutputs);
                if (newOutput is { })
                {
                    return StrigifyOutput(name, newOutputs);
                }
            }
            return JsonSerializer.Serialize(arrOutput);
        }
        else
        {
            return JsonSerializer.Serialize(output);
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
    /// <param name="options">
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
        FlexorOptions? config,
        ScriptRunOptions options,
        ILogger logger,
        CancellationToken cancellationToken = default
    )   where T : IFlexorResource, IOutputResource
    {
        if (options.Path is null && options.Input is null or { Length: 0 })
        {
            throw new ArgumentException("Either 'script' or 'contents' must be provided for script execution.");
        }
        if (options.Path is not null && options.Input is { Length: > 0 })
        {
            throw new ArgumentException("Only one of 'script' or 'contents' can be provided for script execution.");
        }
        
        config ??= new FlexorOptions();
        options.ShellType = ResolveShellType(options.ShellType);
        options.Command = ResolveShellCommand(options.ShellType); 
        options.Args = ResolveShellArguments(options);

        if (!options.UseContainer)
        {
            options.Env = MergeEnvironment(options, config);
        }

        if (options.ShellType is ShellType.PowerShell)
        {
            options = AddPowerShell(options, config);
        }

        LogContainerOptions(logger, nameof(RunScriptAsync), options);
        
        return await RunAsync(
            resource,
            config,
            options,
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
        static string[] ResolveShellArguments(ScriptRunOptions options)
        {
            string[] shellArgs = options.ShellType switch
            {
                var cmd when cmd == ShellType.Cmd
                    && options.Path is not null
                    => ["/c", $"\"{options.Path}\""],
                var ps when ps == ShellType.PowerShell
                    && options.Path is not null
                    => ["-File", options.Path],
                var ps when ps == ShellType.PowerShell
                    && options.Path is null
                    && options.Args is { Length: > 0 }
                    => throw new ArgumentException(" You cannot use arguments eith PowerShell literals"),
                var ps when ps == ShellType.PowerShell
                    && options.Path is null
                    => ["-Command", "-"],
                var bash when bash == ShellType.Bash
                    && options.Path is not null
                    => ["-c", "--", options.Path],
                var bash when bash == ShellType.Bash
                    && options.Path is null
                    && options.Args is { Length: > 0 }
                    => ["-s", "--"],
                var bash when bash == ShellType.Bash
                    && options.Path is null
                    => ["-s"],
                var python when python == ShellType.Python
                    && options.Path is null
                    && options.Args is { Length: > 0 }
                    => throw new ArgumentException(" You cannot use arguments with Python literals"),
                var python when python == ShellType.Python
                    && options.Path is not null
                    => [options.Path],
                var python when python == ShellType.Python
                    && options.Path is null
                    => ["-"],
                _ => throw new ArgumentException("Shell type must be one of Cmd, PowerShell, Bash, or Python.") 
            };

            return [ 
                ..shellArgs, 
                ..options.Args ?? [] 
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
        static ScriptRunOptions AddPowerShell(ScriptRunOptions options, FlexorOptions config)
        {
            options.Env ??= [];
            if (options.UseContainer)
            {
                options.Env[PwshConstants.PSModulePathEnvVar] 
                    = Path.Combine(
                        FlexorMount,
                        PwshConstants.PSModulesFolderName
                      ).AsUnixPath();

                return options;
            }

            var psModuleDir = new DirectoryInfo(Path.Combine(config.FlexorPath, PwshConstants.PSModulesFolderName));
            var existing = Environment.GetEnvironmentVariable(PwshConstants.PSModulePathEnvVar) ?? string.Empty;
            var value = options.Env.GetValueOrDefault(PwshConstants.PSModulePathEnvVar) ?? string.Empty;

            var psModulePath = options.UseContainer
                ? Path.Combine(
                    FlexorMount,
                    PwshConstants.PSModulesFolderName
                  ).AsUnixPath()
                : psModuleDir.FullName.AsPath();

            string[] values = psModuleDir.Exists switch
            {
                true when !string.IsNullOrWhiteSpace(existing) && !string.IsNullOrWhiteSpace(value)
                    => [existing, value.AsPath(), psModulePath],
                true when !string.IsNullOrWhiteSpace(existing)
                    => [existing, psModulePath],
                true when !string.IsNullOrWhiteSpace(value)
                    => [value.AsPath(), psModulePath],
                true
                    => [psModulePath],
                _ => []
            };

            // Special handling for PowerShell module path, never overwrite, always append
            options.Env[PwshConstants.PSModulePathEnvVar] = existing.AppendPath(values);

            return options;
        }
    }

    #endregion

    #region Environment

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
        FlexorOptions config
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
