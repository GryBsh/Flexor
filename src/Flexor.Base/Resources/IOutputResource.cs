using System.Text.Json.Serialization;

namespace Flexor.Resources;

/// <summary>
/// Indicates that a resource produces an output string.
/// </summary>
public interface IOutputResource
{
    [JsonPropertyName("output")]
    public string Output { get; set; }
}