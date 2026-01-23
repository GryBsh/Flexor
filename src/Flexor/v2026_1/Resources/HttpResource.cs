using System.Runtime.InteropServices;
using System.Security;
using Azure.Bicep.Types.Concrete;
using Bicep.Local.Extension.Types.Attributes;
using Flexor.Options;

namespace Flexor.v2026_1.Resources;

public class HttpResourceProperties : ResourceIdentifiers
{
    [TypeProperty("URL to send the HTTP request to", ObjectTypePropertyFlags.Required | ObjectTypePropertyFlags.Identifier)]
    public string? Url { get; set; }

    [TypeProperty("HTTP method to use for the request", ObjectTypePropertyFlags.Required)]
    public string Method { get; set; } = "GET";

    [TypeProperty("Headers to include in the HTTP request", ObjectTypePropertyFlags.None | ObjectTypePropertyFlags.Identifier )]
    public HttpHeader[] Headers { get; set; } = [];

    [TypeProperty("Content type for the HTTP request", ObjectTypePropertyFlags.None)]
    public string? ContentType { get; set; }

    [TypeProperty("Query parameters to include in the HTTP request", ObjectTypePropertyFlags.None | ObjectTypePropertyFlags.Identifier)]
    public Dictionary<string, string>[] Query { get; set; } = [];

    [TypeProperty("Body content for the HTTP request", ObjectTypePropertyFlags.None)]
    public string? Body { get; set; }

    [TypeProperty("Timeout for the HTTP request in seconds", ObjectTypePropertyFlags.None | ObjectTypePropertyFlags.Identifier)]
    public int TimeoutSeconds { get; set; } = 300;

    [TypeProperty("Indicates whether to ignore SSL certificate errors", ObjectTypePropertyFlags.None | ObjectTypePropertyFlags.Identifier)]
    public bool IgnoreSslErrors { get; set; } = false;

    [TypeProperty("Indicates whether to follow HTTP redirects", ObjectTypePropertyFlags.None | ObjectTypePropertyFlags.Identifier)]
    public bool FollowRedirects { get; set; } = true;

    [TypeProperty("Expected HTTP status codes indicating success", ObjectTypePropertyFlags.None | ObjectTypePropertyFlags.Identifier)]
    public int[] ExpectedStatusCodes { get; set; } = [200]; 

    [TypeProperty("A token used for `Bearer` authentication in the HTTP request", ObjectTypePropertyFlags.None)]
    public SecureString? BearerToken { get; set; }

    [TypeProperty("Credentials used for `Basic` authentication in the HTTP request", ObjectTypePropertyFlags.None)]
    public Credential? Credentials { get; set; }

    internal string BearerTokenUnsecure 
        => BearerToken is null 
         ? string.Empty 
         : Marshal.PtrToStringUni(Marshal.SecureStringToGlobalAllocUnicode(BearerToken)) 
           ?? string.Empty;
}

[ResourceType(ResourceType, V2026_1_Constants.ApiVersion)]
public class HttpResource : HttpResourceProperties
{
    internal const string ResourceType = $"{V2026_1_Constants.Namespace}/http";
    [TypeProperty("Output from the HTTP step", ObjectTypePropertyFlags.ReadOnly)]
    public string Output { get; set; } = "{}";

    [TypeProperty("HTTP status code from the response", ObjectTypePropertyFlags.ReadOnly)]
    public int StatusCode { get; set; }

}