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
            FetchOptions =
            {
                CredentialsProvider = CredentialProvider(credential, defaultCloneOptions.FetchOptions)
            }
        };

        return Repository.Clone(repositoryUrl, localPath, cloneOptions);
    }

    public void PullRepository(string repositoryPath, Credential? credential = null)
    {
        var repository = new Repository(repositoryPath);
        var sig = repository.Config.BuildSignature(DateTimeOffset.Now);
        var options = new PullOptions()
        {
            FetchOptions =
            {
                CredentialsProvider = CredentialProvider(credential, new FetchOptions())
            }
        };
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

    private static CredentialsHandler CredentialProvider(Credential? credential, FetchOptions defaultOptions) =>
        (url, user, types) =>
        {            
            if (credential is not null)
            {
                return new UsernamePasswordCredentials
                {
                    Username = credential.Username,
                    Password = credential.StringPassword
                };
            }
            else 
            {
                return defaultOptions.CredentialsProvider(url, user, types);
            }
        };
}
