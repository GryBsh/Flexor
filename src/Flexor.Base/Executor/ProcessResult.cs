namespace Flexor.Executor;

/// <summary>
/// Represents the result of a process execution.
/// </summary>
/// <param name="Success">
/// Indicates whether the process completed successfully.
/// </param>
/// <param name="ExitCode">
/// The exit code returned by the process.
/// </param>
/// <param name="Timeout">
/// Indicates whether the process timed out.
/// </param>
/// <param name="Cancelled">
/// Indicates whether the process was cancelled.
/// </param>
/// <param name="Killed">
/// Indicates whether the process was killed.
/// </param>
public record ProcessResult(bool Success, int ExitCode, bool Timeout, bool Cancelled, bool Killed);
