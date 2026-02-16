using Azure.Bicep.Types.Concrete;
using Bicep.Local.Extension.Types.Attributes;
using Flexor.Options;

namespace Flexor.v2026_1.Options;

public class FlexorModuleOptionsBase
{
    [TypeProperty("Execution options for the script", ObjectTypePropertyFlags.None)]
    public ExecutionOptions Exec { get; set; } = new();
}

public class FlexorModuleOptions : FlexorModuleOptionsBase
{
    [TypeProperty("How to handle module parameters", ObjectTypePropertyFlags.None)]
    public ModuleParamHandlingType Type { get; set; } = ModuleParamHandlingType.Env;

    [TypeProperty("Environment variables to set during execution", ObjectTypePropertyFlags.None)]
    public Dictionary<string, string> Env { get; set; } = [];

    [TypeProperty("Arguments to pass to the script during execution", ObjectTypePropertyFlags.None)]
    public string[] Args { get; set; } = [];
}

public class FlexorResourceOptions : FlexorModuleOptionsBase
{
    
}