using Azure.Bicep.Types.Concrete;
using Bicep.Local.Extension.Types.Attributes;
using Flexor.Resources;
using Flexor.v2026_1.Options;

namespace Flexor.v2026_1.Resources;

public class FlexorResourceIdentifiers : ResourceIdentifiers
{
    [TypeProperty("Resource Type", ObjectTypePropertyFlags.Required | ObjectTypePropertyFlags.Identifier)]
    public string? Type { get; set; }

    [TypeProperty("Parameters to pass to the resource", ObjectTypePropertyFlags.Identifier)]
    public Dictionary<string, string> Parameters { get; set; } = [];

    [TypeProperty("The resource properties", ObjectTypePropertyFlags.Identifier)]
    public FlexorResourceOptions Options { get; set; } = new();
}




[ResourceType( $"{V2026_1_Constants.Namespace}/resource", V2026_1_Constants.ApiVersion)]
public class FlexorResource : FlexorResourceIdentifiers, IOutputResource
{    
    [TypeProperty("Output", ObjectTypePropertyFlags.ReadOnly)]
    public string Output { get; set; } = "{}";
}