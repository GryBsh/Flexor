using Azure.Bicep.Types.Concrete;
using Bicep.Local.Extension.Types.Attributes;

namespace Flexor.v2026_1.Options;

public class HttpOptions
{
    [TypeProperty("Timeout for the HTTP request in seconds", ObjectTypePropertyFlags.None | ObjectTypePropertyFlags.Identifier)]
    public int TimeoutSeconds { get; set; } = 300;

    [TypeProperty("Indicates whether to ignore SSL certificate errors", ObjectTypePropertyFlags.None | ObjectTypePropertyFlags.Identifier)]
    public bool IgnoreSslErrors { get; set; } = false;

    [TypeProperty("Indicates whether to follow HTTP redirects", ObjectTypePropertyFlags.None | ObjectTypePropertyFlags.Identifier)]
    public bool FollowRedirects { get; set; } = true;

}

