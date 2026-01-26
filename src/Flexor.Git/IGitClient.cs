using Flexor.Git.Options;
using Flexor.Options;
using LibGit2Sharp;

namespace Flexor.Git;

public interface IGitClient
{
    string CloneRespository(
        GitCloneOptions? options, 
        string repositoryUrl, 
        string? localPath = null,
        Credential? credentials = null
    );

    bool ShouldPull(string? path);

    void PullRepository(
        string repositoryPath,
        Credential? credential = null
    );
}

