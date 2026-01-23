using Azure.Bicep.Types.Concrete;
using Bicep.Local.Extension.Types.Attributes;
using Flexor.Options;

namespace Flexor.v2026_1.Options;

public class InvocationResourceOptions
{    
    [TypeProperty("Working directory for the invocation", ObjectTypePropertyFlags.None )]
    public string? WorkingDirectory { get; set; }

    [TypeProperty("Whether to run the invocation with elevated privileges", ObjectTypePropertyFlags.None)]
    public bool? RunAsAdmin { get; set; }

    [TypeProperty("Timeout for the invocation in seconds", ObjectTypePropertyFlags.None)]
    public int? TimeoutSeconds { get; set; }

    [TypeProperty("Whether to continue invocation on failure", ObjectTypePropertyFlags.None)]
    public bool? ContinueOnFailure { get; set; }
    [TypeProperty("Whether to overwrite existing PATH or path-like environment variables instead of appending to them", ObjectTypePropertyFlags.None)]
    public bool OverwriteEnvPaths { get; set; } = false;
    [TypeProperty("List of environment variables to which values will be appended instead of overwritten", ObjectTypePropertyFlags.None)]
    public List<AppendedVariable> AppendedEnvVariables { get; set; } = []; 
}



