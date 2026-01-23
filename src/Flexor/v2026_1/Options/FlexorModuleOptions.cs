using Azure.Bicep.Types.Concrete;
using Bicep.Local.Extension.Types.Attributes;

namespace Flexor.v2026_1.Options;

public class FlexorModuleOptions : InvocationResourceOptions
{
    [TypeProperty("Environment variables to set for the script", ObjectTypePropertyFlags.None)]
    public Dictionary<string, string> Env { get; set; } = [];
}

public class FlexorResourceOptions : FlexorModuleOptions
{
    [TypeProperty("Parameters to pass to the invocation", ObjectTypePropertyFlags.None)]
    public IEnumerable<string> Args { get; set; } = [];

}