using Azure.Bicep.Types.Concrete;
using Bicep.Local.Extension.Types.Attributes;

namespace Flexor.Options;

public class EnvOptions
{
    [TypeProperty("Whether to overwrite existing PATH or path-like environment variables instead of appending to them", ObjectTypePropertyFlags.None)]
    public bool OverwritePaths { get; set; }
    [TypeProperty("Environment variables to which values will be appended instead of overwritten", ObjectTypePropertyFlags.None)]
    public Dictionary<string, string> Append { get; set; } = new();
}
