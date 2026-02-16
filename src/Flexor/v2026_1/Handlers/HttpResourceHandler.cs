using Bicep.Local.Extension.Host.Handlers;
using Flexor.v2026_1.Options;
using Flexor.v2026_1.Resources;
using System.Net;
using System.Text;

namespace Flexor.v2026_1.Handlers;

public class HttpResourceHandler : TypedResourceHandler<HttpResource, HttpResourceIdentifiers, FlexorV2026_01_01_Options>
{
    protected override HttpResourceIdentifiers GetIdentifiers(HttpResource properties) 
        => new()
        {
            Url = properties.Url,
            Headers = properties.Headers,
            Query = properties.Query,
            ExpectedStatusCodes = properties.ExpectedStatusCodes,
            Options = properties.Options,
            Authorization = properties.Authorization
        };
    protected override async Task<ResourceResponse> Preview(ResourceRequest request, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return GetResponse(request);
    }

    protected override Task<ResourceResponse> Get(ReferenceRequest request, CancellationToken cancellationToken)
    {
        var resource = new HttpResource{
            Url = request.Identifiers.Url,
            Headers = request.Identifiers.Headers,
            Query = request.Identifiers.Query,
            ExpectedStatusCodes = request.Identifiers.ExpectedStatusCodes,
            Options = request.Identifiers.Options,
            Authorization = request.Identifiers.Authorization
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
        FlexorV2026_01_01_Options config,
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
            ServerCertificateCustomValidationCallback = resource.Options.IgnoreSslErrors ? HttpClientHandler.DangerousAcceptAnyServerCertificateValidator : null,
            AllowAutoRedirect = resource.Options.FollowRedirects,
            Credentials = resource.Authorization?.Credential is not null 
                ? new NetworkCredential(
                    resource.Authorization.Credential.Username, 
                    resource.Authorization.Credential.StringPassword
                  ) 
                : null            
        };
        
        var httpClient = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(resource.Options.TimeoutSeconds),   
            DefaultRequestHeaders =
            {
                Authorization = 
                    resource.Authorization?.BearerToken is not null
                    ? new System.Net.Http.Headers.AuthenticationHeaderValue(
                        "Bearer", 
                        resource.Authorization.StringBearerToken
                    ) 
                    : resource.Authorization?.Credential is not null 
                    ? new System.Net.Http.Headers.AuthenticationHeaderValue(
                        "Basic", 
                        Convert.ToBase64String(
                            Encoding.UTF8.GetBytes(
                                $"{resource.Authorization.Credential.Username}:{resource.Authorization.Credential.StringPassword}"
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

        var url = new UriBuilder(resource.Url!);
        if (resource.Query.Length > 0)
        {
            var query = System.Web.HttpUtility.ParseQueryString(url.Query);
            foreach (var param in resource.Query.Where(q => !string.IsNullOrWhiteSpace(q.Name)))
            {
                query[param.Name!] = param.Value;
            }
            url.Query = query.ToString() ?? string.Empty;
        }

        HttpRequestMessage httpRequest = new(
            new HttpMethod(resource.Method) ,
            url.ToString()
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
                httpRequest.Content = new StringContent(
                    resource.Body, 
                    Encoding.UTF8
                );
            }
        }
        var response = await httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();
        resource.StatusCode = (int)response.StatusCode;

        if (!resource.ExpectedStatusCodes.Contains(resource.StatusCode))
        {
            throw new HttpRequestException(
                $"Unexpected status code {resource.StatusCode} received from {resource.Url}.\n" +
                $"Expected status codes: {string.Join(", ", resource.ExpectedStatusCodes)}"
            );
        }

        if (!string.IsNullOrWhiteSpace(resource.DownloadPath))
        {
            var responseBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            await File.WriteAllBytesAsync(resource.DownloadPath, responseBytes, cancellationToken);
        }
        else {
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            resource.Output = responseBody;
        }
        
        return GetResponse(new() { 
            Type = HttpResource.ResourceType, 
            Properties = resource, 
            Config = config, 
            ApiVersion = ApiVersion 
        });
    }
}
