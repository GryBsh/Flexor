namespace Flexor.Options;

/// <summary>
/// Context for starting a process
/// </summary>
public class ProcessStartOptions
{
    /// <summary>
    /// The command to execute
    /// </summary>
    public string? Command { get; set; }
    /// <summary>
    /// Arguments to pass to the command
    /// </summary>
    public string[] Args { get; set; } = [];
    /// <summary>
    /// Environment variables to set for the process
    /// </summary>
    public Dictionary<string, string>? Env { get; set; }
    /// <summary>
    /// The working directory for the process
    /// </summary>
    public string? WorkingDirectory { get; set; }
    /// <summary>
    /// Input lines to pass to the process's standard input
    /// </summary>
    public string[] Input { get; set; } = [];
    /// <summary>
    /// Handler for standard output lines
    /// </summary>
    public Action<string>? OutputHandler { get; set; }
    /// <summary>
    /// Handler for standard error lines
    /// </summary>
    public Action<string>? ErrorHandler { get; set; }
    /// <summary>
    /// Indicates whether to run the process with elevated (administrator) privileges
    /// </summary>
    public bool RunAsAdmin { get; set; } = false;
    /// <summary>
    /// Timeout in seconds for the process execution
    /// </summary>
    public int? TimeoutSeconds { get; set; }
    /// <summary>
    /// Indicates whether to continue execution on failure
    /// </summary>
    public bool ContinueOnFailure { get; set; } = false;
    /// <summary>
    /// Indicates whether to overwrite existing PATH or path-like environment variables instead of appending to them
    /// </summary>
    public bool OverwriteEnvPaths { get; set; } = false;
    /// <summary>
    /// Environment variables to which values will be appended
    /// </summary>
    public AppendedVariable[] AppendEnvVars { get; set; } = [];

}