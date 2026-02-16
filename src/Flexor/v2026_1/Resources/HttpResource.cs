using System.Runtime.InteropServices;
using System.Security;
using Azure.Bicep.Types.Concrete;
using Bicep.Local.Extension.Types.Attributes;
using Flexor.Options;
using Flexor.v2026_1.Options;

namespace Flexor.v2026_1.Resources;

public class HttpResourceIdentifiers
{
    [TypeProperty("URL to send the HTTP request to", ObjectTypePropertyFlags.Required | ObjectTypePropertyFlags.Identifier)]
    public string? Url { get; set; }

    [TypeProperty("Headers to include in the HTTP request", ObjectTypePropertyFlags.None | ObjectTypePropertyFlags.Identifier )]
    public HttpHeader[] Headers { get; set; } = [];

    [TypeProperty("Query parameters to include in the HTTP request", ObjectTypePropertyFlags.None | ObjectTypePropertyFlags.Identifier)]
    public HttpQuery[] Query { get; set; } = [];

    [TypeProperty("Expected HTTP status codes indicating success", ObjectTypePropertyFlags.None | ObjectTypePropertyFlags.Identifier)]
    public int[] ExpectedStatusCodes { get; set; } = [200]; 

    [TypeProperty("Options for the HTTP request", ObjectTypePropertyFlags.None | ObjectTypePropertyFlags.Identifier)]
    public HttpOptions Options { get; set; } = new();
    
    [TypeProperty("Authorization options for the HTTP request", ObjectTypePropertyFlags.None | ObjectTypePropertyFlags.Identifier)]
    public HttpAuthorizationOptions? Authorization { get; set; }

}

public class HttpResourceProperties : HttpResourceIdentifiers
{


    [TypeProperty("HTTP method to use for the request", ObjectTypePropertyFlags.Required)]
    public string Method { get; set; } = "GET";

    

    [TypeProperty("Content type for the HTTP request", ObjectTypePropertyFlags.None)]
    public string? ContentType { get; set; }

    

    [TypeProperty("Body content for the HTTP request", ObjectTypePropertyFlags.None)]
    public string? Body { get; set; }


    [TypeProperty("Path to download the response content to (if applicable)", ObjectTypePropertyFlags.None)]
    public string? DownloadPath { get; set; }



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