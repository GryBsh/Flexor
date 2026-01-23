using Bicep.Local.Extension.Host.Handlers;
using Flexor.Git;
using Flexor.Options;
using Flexor.v2026_1.Options;
using Flexor.v2026_1.Resources;
using LibGit2Sharp;

namespace Flexor.v2026_1.Handlers;


public class RepositoryResourceHandler(IGitClient gitClient) : TypedResourceHandler<RepositoryResource, RepositoryResourceIdentifiers, FlexorOptions>
{
    protected override RepositoryResourceIdentifiers GetIdentifiers(RepositoryResource properties) 
        => new()
        {
            Name = properties.Name
        };

    protected override FlexorOptions GetConfig(string configJson)
    {
        return base.GetConfig(configJson);
    }
    protected override async Task<ResourceResponse> Preview(ResourceRequest request, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return GetResponse(request);
    }

    protected override async Task<ResourceResponse> Get(ReferenceRequest request, CancellationToken cancellationToken)
    {
        request.Identifiers.Type ??= RepositoryType.Default;
        if (request.Identifiers.Type is RepositoryType.Default or RepositoryType.Git)
        {
            gitClient.PullRepository(
                request.Identifiers.Path!, // path is non-null here
                request.Identifiers.Credential
            );
        }
        else
        {
            throw new NotSupportedException($"Repository type '{request.Identifiers.Type}' is not supported.");
        }
        await Task.CompletedTask;
        var response = new RepositoryResource
        {
            Name = request.Identifiers.Name,
            Type = request.Identifiers.Type,
            Source = request.Identifiers.Source,
            Path = request.Identifiers.Path
        }; 
        return GetResponse(request, response);
    }

    protected override async Task<ResourceResponse> CreateOrUpdate(ResourceRequest request, CancellationToken cancellationToken)
    {
        request.Properties.Type ??= RepositoryType.Default;
        if (request.Properties.Type is RepositoryType.Default or RepositoryType.Git)
        {
            HandleGitRepo(request.Properties);
        }
        else
        {
            throw new NotSupportedException($"Repository type '{request.Properties.Type}' is not supported.");
        }
        await Task.CompletedTask;
        return GetResponse(request);
    }

    private void HandleGitRepo(RepositoryResourceIdentifiers request)
    {
        var path = request.Path switch
        {
            string p => p,
            _ when request.Source is { } src => Path.GetFileNameWithoutExtension(src),
            _ => throw new InvalidOperationException(
                "Either a valid existing git repository path must be provided to pull from, or a source URL must be provided to clone from."
            )
        };

        var pull = path is { }                                       // If a path is provided, we may need to pull
                   && Directory.Exists(path)                         // if the directory exists
                   && Repository.Discover(path) is { } repository;   // and it's a git repository

        var clone = !pull && request.Source is { };

        if (pull)
        {
            gitClient.PullRepository(
                request.Path!, // path is non-null here
                request.Credential
            );
        }
        else if (clone)
        {
            request.Path = gitClient.CloneRespository(
                null,
                request.Source!, // source is non-null here
                request.Path,
                request.Credential
            );
        }
        else
        {
            throw new ArgumentException("Either a valid existing git repository path must be provided to pull from, or a source URL must be provided to clone from.");
        }

    }

}
