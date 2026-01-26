using Flexor.Git.Options;
using Flexor.Options;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;

namespace Flexor.Git;


public class GitClient : IGitClient
{
    public string CloneRespository(
        GitCloneOptions? options, 
        string repositoryUrl, 
        string? localPath = null,
        Credential? credential = null)
    {
        localPath ??= Path.GetFileNameWithoutExtension(repositoryUrl);
        var defaultCloneOptions = new CloneOptions();
        var cloneOptions = new CloneOptions()
        {
            BranchName = options?.Branch ?? defaultCloneOptions.BranchName,
            IsBare = options?.IsBare ?? defaultCloneOptions.IsBare,
            RecurseSubmodules = options?.RecurseSubmodules ?? defaultCloneOptions.RecurseSubmodules,
            Checkout = options?.Checkout ?? defaultCloneOptions.Checkout,
        };
        CredentialProvider(credential, cloneOptions.FetchOptions);
        try{
            return Repository.Clone(repositoryUrl, localPath, cloneOptions);    
        }
        catch(Exception ex)
        {
            throw new InvalidOperationException($"Failed to clone the git repository.\n{ex.Message}\n{ex.StackTrace}", ex);
        }
    }

    public bool ShouldPull(string? path)
    {
        if (path is null || !Directory.Exists(path))
        {
            return false;
        }

        var repositoryPath = Repository.Discover(path);
        return repositoryPath is not null;
    }

    public void PullRepository(string repositoryPath, Credential? credential = null)
    {
        try
        {
            repositoryPath = Repository.Discover(repositoryPath)
                             ?? throw new InvalidOperationException("Could not find a git repository at the specified path.");
            var repository = new Repository(repositoryPath);
            
            var sig = repository.Config.BuildSignature(DateTimeOffset.Now);
            
            var options = new PullOptions();
            CredentialProvider(credential, options.FetchOptions);
            var result = Commands.Pull(
                repository, 
                sig, 
                options
            );

            if (result.Status == MergeStatus.Conflicts)
            {
                throw new InvalidOperationException("Pull resulted in conflicts.");
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to pull the git repository.\n{ex.Message}\n{ex.StackTrace}", ex);
        }
    }

    private static void CredentialProvider(Credential? credential, FetchOptions options)
    {
        if (credential is not null)
        {
            options.CredentialsProvider = (url, user, types) =>
            {        
                    return new UsernamePasswordCredentials
                    {
                        Username = credential.Username,
                        Password = credential.StringPassword
                    };
            };
        }
    }
}
