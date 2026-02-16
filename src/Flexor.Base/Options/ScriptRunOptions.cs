namespace Flexor.Options;

/// <summary>
/// Context for running a script
/// </summary>
public class ScriptRunOptions : ProcessStartOptions
{   
    /// <summary>
    /// The shell type to use for running the script
    /// </summary>
    public ShellType ShellType { get; set; }
    /// <summary>
    /// The path to the script file to execute
    /// </summary>
    public string? Path { get; set; }
}
