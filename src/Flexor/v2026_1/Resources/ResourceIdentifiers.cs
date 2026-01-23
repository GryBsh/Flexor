using Azure.Bicep.Types.Concrete;
using Bicep.Local.Extension.Types.Attributes;
using Flexor.Resources;

namespace Flexor.v2026_1.Resources;

public class ResourceIdentifiers : LoggingResourceIdentifiers, IFlexorResource
{
    [TypeProperty("The resource name", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
    public required string Name { get; set; }
}
