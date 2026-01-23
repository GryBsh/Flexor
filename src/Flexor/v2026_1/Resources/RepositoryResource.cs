using Azure.Bicep.Types.Concrete;
using Bicep.Local.Extension.Types.Attributes;
using Flexor.Options;
using LibGit2Sharp;

namespace Flexor.v2026_1.Resources;

public class RepositoryResourceIdentifiers : ResourceIdentifiers
{
    [TypeProperty("Repository Type", ObjectTypePropertyFlags.None)]
    public RepositoryType? Type { get; set; } = null;
    [TypeProperty("Repostitory Source")]
    public string? Source { get; set; }
    [TypeProperty("Repository Local Path", ObjectTypePropertyFlags.Identifier)]
    public string? Path { get; set; }

    //[TypeProperty("Options for cloning the Git repository", ObjectTypePropertyFlags.None)]
    //public CloneOptions CloneOptions { get; set; } = new();

    //[TypeProperty("Options for pulling updates from the Git repository", ObjectTypePropertyFlags.None)]
    //public PullOptions PullOptions { get; set; } = new();

    [TypeProperty("Credentials for accessing the Git repository", ObjectTypePropertyFlags.None)]
    public Credential? Credential { get; set; }
}

[ResourceType(RepositoryResource.ResourceType, V2026_1_Constants.ApiVersion)]
public class RepositoryResource : RepositoryResourceIdentifiers
{
    internal const string ResourceType = $"{V2026_1_Constants.Namespace}/repo";
}
    