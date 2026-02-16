using Azure.Bicep.Types.Concrete;
using Bicep.Local.Extension.Types.Attributes;

namespace Flexor.Options;

public class ExecutionOptionsBase
{    
    [TypeProperty("Whether to use a container for execution", ObjectTypePropertyFlags.None)]
    public bool UseContainer { get; set; } = false;
    [TypeProperty("Working directory for the invocation", ObjectTypePropertyFlags.None )]
    public string? WorkingDirectory { get; set; }

    [TypeProperty("Whether to run the invocation with elevated privileges", ObjectTypePropertyFlags.None)]
    public bool? RunAsAdmin { get; set; }

    [TypeProperty("Timeout for the invocation in seconds", ObjectTypePropertyFlags.None)]
    public int? TimeoutSeconds { get; set; }

    [TypeProperty("Whether to continue invocation on failure", ObjectTypePropertyFlags.None)]
    public bool? ContinueOnFailure { get; set; }
    
    [TypeProperty("Environment variable options for the invocation", ObjectTypePropertyFlags.None)]
    public EnvOptions? EnvOptions { get; set; }

    [TypeProperty("Container image to use for execution", ObjectTypePropertyFlags.None)]
    public string? ContainerImage { get; set; }

    [TypeProperty("Container command to use for execution", ObjectTypePropertyFlags.None)]
    public string? ContainerCli { get; set; }

    [TypeProperty("Arguments to use to execute the container", ObjectTypePropertyFlags.None)]
    public string[]? ContainerCliArgs { get; set; }

    [TypeProperty("Mounts to use for the container execution", ObjectTypePropertyFlags.None)]
    public Dictionary<string, string>? ContainerMounts { get; set; }

    public string? WorkingPathMount { get; set; }

    [TypeProperty("Whether to not wait for the process to complete", ObjectTypePropertyFlags.None)]
    public bool NoWait { get; set; } = false;

}
