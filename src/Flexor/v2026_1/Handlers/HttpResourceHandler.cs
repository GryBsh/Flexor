using Bicep.Local.Extension.Host.Handlers;
using Flexor.v2026_1.Options;
using Flexor.v2026_1.Resources;
using System.Net;
using System.Text;

namespace Flexor.v2026_1.Handlers;

public class HttpResourceHandler : TypedResourceHandler<HttpResource, HttpResourceProperties, FlexorOptions>
{
    protected override HttpResourceProperties GetIdentifiers(HttpResource properties) 
        => new()
        {
            Name = properties.Name
        };
    protected override async Task<ResourceResponse> Preview(ResourceRequest request, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return GetResponse(request);
    }

    protected override Task<ResourceResponse> Get(ReferenceRequest request, CancellationToken cancellationToken)
    {
        var resource = new HttpResource{
            Name = request.Identifiers.Name,
            Url = request.Identifiers.Url,
            Headers = request.Identifiers.Headers,
            TimeoutSeconds = request.Identifiers.TimeoutSeconds
        };

        return InvokeHttpRequest(
            resource, 
            request.Config,
            cancellationToken
        );
    }

    protected override async Task<ResourceResponse> CreateOrUpdate(ResourceRequest request, CancellationToken cancellationToken)
    {
        return await InvokeHttpRequest(
            request.Properties, 
            request.Config,
            cancellationToken
        );
    }


    protected async Task<ResourceResponse> InvokeHttpRequest(
        HttpResource resource, 
        FlexorOptions config,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(resource.Url))
        {
            throw new ArgumentException("The 'Url' property must be provided for the http step.");
        }


        var handler = new HttpClientHandler()
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            ServerCertificateCustomValidationCallback = resource.IgnoreSslErrors ? HttpClientHandler.DangerousAcceptAnyServerCertificateValidator : null,
            AllowAutoRedirect = resource.FollowRedirects,
            Credentials = resource.Credentials is not null 
                ? new NetworkCredential(
                    resource.Credentials.Username, 
                    resource.Credentials.StringPassword
                  ) 
                : null            
        };
        
        var httpClient = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(resource.TimeoutSeconds),   
            DefaultRequestHeaders =
            {
                Authorization = resource.Credentials is not null 
                    ? new System.Net.Http.Headers.AuthenticationHeaderValue(
                        "Basic", 
                        Convert.ToBase64String(
                            Encoding.UTF8.GetBytes(
                                $"{resource.Credentials.Username}:{resource.Credentials.StringPassword}"
                            )
                        )
                      ) 
                    : null
            }
        };

        foreach (var header in resource.Headers)
        {
            if (string.IsNullOrWhiteSpace(header.Name)) continue;
            httpClient.DefaultRequestHeaders.Add(header.Name!, header.Value);
        }

        HttpRequestMessage httpRequest = new(
            new HttpMethod(resource.Method),
            resource.Url
        );

        if (resource.Body != null)
        {
            if (resource.ContentType != null)
            {
                httpRequest.Content = new StringContent(
                    resource.Body, 
                    Encoding.UTF8, 
                    resource.ContentType
                );
            }
            else
            {
                httpRequest.Content = new StringContent(resource.Body);
            }
        }
        var response = await httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();
        resource.StatusCode = (int)response.StatusCode;
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        resource.Output = responseBody;
        return GetResponse(new() { 
            Type = HttpResource.ResourceType, 
            Properties = resource, 
            Config = config, 
            ApiVersion = ApiVersion 
        });
    }
}
