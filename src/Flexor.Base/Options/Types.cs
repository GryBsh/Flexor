using System.Text.Json;
using System.Text.Json.Nodes;

namespace Flexor.Options;

/// <summary>
/// Types of shells available for script execution
/// </summary>
public enum ShellType : int
{
    Default = 0,
    Bash,
    PowerShell,
    Cmd,
    Python
}

/// <summary>
/// Types of repositories
/// </summary>
public enum RepositoryType : int
{
    Default = 0,
    Git 
}

public enum ModuleParamHandlingType : int
{
    Env = 0,
    Args,
    StdInEnv,
    StdInJson,
}

public record JsonType(bool IsJson, bool IsObject, bool IsArray, bool IsScalar)
{
    public static JsonType Of(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return new(false, false, false, false);

        var result = Result<JsonType>.From(
            () =>
            {
                var node = JsonSerializer.Deserialize<JsonNode>(input);
                return new(
                    node is { },
                    node is JsonObject,
                    node is JsonArray,
                    node is JsonValue
                );
            }
        );
        return result.IsSuccess 
             ? result.Value! 
             : new(false, false, false, false);
    }
}
