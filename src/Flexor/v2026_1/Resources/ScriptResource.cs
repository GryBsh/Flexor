using Azure.Bicep.Types.Concrete;
using Bicep.Local.Extension.Types.Attributes;
using Flexor.Options;
using Flexor.Resources;
using Flexor.v2026_1.Options;

namespace Flexor.v2026_1.Resources;

public class ScriptResourceProperties : ResourceIdentifiers
{
    [TypeProperty("Shell type to use for execution", ObjectTypePropertyFlags.Required)]
    public ShellType? Shell { get; set; }
    
    [TypeProperty("Script to execute", ObjectTypePropertyFlags.None)]
    public string? Script { get; set; } 
    
    [TypeProperty("Literal contents of the script to execute", ObjectTypePropertyFlags.None)]
    public string? Contents { get; set; }
    
    [TypeProperty("Environment variables to set for the script", ObjectTypePropertyFlags.None)]
    public Dictionary<string, string> Env { get; set; } = [];

    [TypeProperty("Parameters to pass to the invocation", ObjectTypePropertyFlags.None)]
    public IEnumerable<string> Args { get; set; } = [];
    
    [TypeProperty("The script invocation properties", ObjectTypePropertyFlags.None)]
    public ScriptResourceOptions Options { get; set; } = new();
}

public class ScriptResourceOptions : InvocationResourceOptions
{

}



[ResourceType($"{V2026_1_Constants.Namespace}/script", V2026_1_Constants.ApiVersion)]
public class ScriptResource : ScriptResourceProperties, IOutputResource
{
    [TypeProperty("Output values from the script step", ObjectTypePropertyFlags.ReadOnly)]
    public string Output { get; set; } = "{}";

    [TypeProperty("Indicates whether the output string is JSON", ObjectTypePropertyFlags.ReadOnly)]
    public bool IsOutputJson { get; set; } = true;
}



