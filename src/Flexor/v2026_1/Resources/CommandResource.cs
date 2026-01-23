using Azure.Bicep.Types.Concrete;
using Bicep.Local.Extension.Types.Attributes;
using Flexor.Resources;
using Flexor.v2026_1.Options;

namespace Flexor.v2026_1.Resources;

public class CommandResourceIdentifiers : ResourceIdentifiers
{
    [TypeProperty("Path to program/script to execute", ObjectTypePropertyFlags.Required)]
    public string? Command { get; set; }

    [TypeProperty("Parameters to pass to the invocation", ObjectTypePropertyFlags.None)]
    public IEnumerable<string> Args { get; set; } = [];

    [TypeProperty("Environment variables to set for the script", ObjectTypePropertyFlags.None)]
    public Dictionary<string, string> Env { get; set; } = [];

    [TypeProperty("The command innvocation properties", ObjectTypePropertyFlags.None)]
    public CommandResourceOptions Options { get; set; } = new();
}

public class CommandResourceOptions : InvocationResourceOptions;


[ResourceType($"{V2026_1_Constants.Namespace}/command", V2026_1_Constants.ApiVersion)]
public class CommandResource : CommandResourceIdentifiers, IOutputResource
{
    [TypeProperty("Output values from the script step", ObjectTypePropertyFlags.ReadOnly)]
    public string Output { get; set; } = "{}";
}


