namespace Flexor.Options;

/// <summary>
/// Defines an environment variable to which any set value will be appended
/// </summary>
/// <param name="Name">
/// The name of the environment variable.
/// </param>
/// <param name="Delimiter">
/// The delimiter used to separate appended values.
/// </param>
public record AppendedVariable(string Name, string Delimiter);