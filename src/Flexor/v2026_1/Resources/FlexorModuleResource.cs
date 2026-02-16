using Azure.Bicep.Types.Concrete;
using Bicep.Local.Extension.Types.Attributes;
using Flexor.Options;
using Flexor.v2026_1.Options;

namespace Flexor.v2026_1.Resources;

public class FlexorModuleIdentifiers
{
    [TypeProperty("Resource Type", ObjectTypePropertyFlags.Required | ObjectTypePropertyFlags.Identifier)]
    public string? Type { get; set; }
    
    [TypeProperty("API Version", ObjectTypePropertyFlags.Identifier)]
    public string? Version { get; set; }
}
public class FlexorModuleProperties : FlexorModuleIdentifiers
{

    
    [TypeProperty("Shell type to use for execution", ObjectTypePropertyFlags.Required)]
    public ShellType? Shell { get; set; }

    [TypeProperty("The module invocation options", ObjectTypePropertyFlags.None)]
    public FlexorModuleOptions Options { get; set; } = new();

    [TypeProperty("Path to Get handler script", ObjectTypePropertyFlags.None)]
    public string? Get { get; set; }
    
    [TypeProperty("The Get handler invocation options", ObjectTypePropertyFlags.None)]
    public FlexorResourceOptions GetOptions { get; set; } = new();
    
    [TypeProperty("Path to CreateOrUpdate handler script", ObjectTypePropertyFlags.Required)]
    public string? CreateOrUpdate { get; set; }
    
    [TypeProperty("The CreateOrUpdate handler invocation options", ObjectTypePropertyFlags.None)]
    public FlexorResourceOptions CreateOrUpdateOptions { get; set; } = new();

    [TypeProperty("Path to Delete handler script", ObjectTypePropertyFlags.None)]
    public string? Delete { get; set; }   

    [TypeProperty("The Delete handler invocation options", ObjectTypePropertyFlags.None)]
    public FlexorResourceOptions DeleteOptions { get; set; } = new();
}

[ResourceType($"{V2026_1_Constants.Namespace}/module", V2026_1_Constants.ApiVersion)]
public class FlexorModuleResource : FlexorModuleProperties
{

    [TypeProperty("The type name of the module", ObjectTypePropertyFlags.ReadOnly)]
    public string? TypeName { get; set; }
}
