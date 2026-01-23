using Bicep.Local.Extension.Host.Handlers;
using Flexor.Executor;
using Flexor.v2026_1.Options;
using Flexor.v2026_1.Resources;

namespace Flexor.v2026_1.Handlers;

public class CommandResourceHandler : TypedResourceHandler<CommandResource, CommandResourceIdentifiers, FlexorOptions>
{
    protected override CommandResourceIdentifiers GetIdentifiers(CommandResource properties) 
        => new()
        {
            Name = properties.Name
        };

    protected override Task<ResourceResponse> Preview(ResourceRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(GetResponse(request));    
    }
    
    protected override async Task<ResourceResponse> CreateOrUpdate(ResourceRequest request, CancellationToken cancellationToken)
    {
        var cmd = request.Properties;
        if (string.IsNullOrWhiteSpace(cmd.Command))
        {
            throw new ArgumentException("The 'Command' property must be provided for the exec step.", nameof(request));
        }

        var result = await ProcessExecutor.StartProcessAsync(
            resource: cmd,
            config: request.Config,
            new()
            {
                Command = cmd.Command,
                Args = [.. cmd.Args],
                Env = cmd.Env,
                WorkingDirectory = cmd.Options.WorkingDirectory,
                RunAsAdmin = cmd.Options.RunAsAdmin ?? false,
                TimeoutSeconds = cmd.Options.TimeoutSeconds ?? 0,
                ContinueOnFailure = cmd.Options.ContinueOnFailure ?? false
            },
            cancellationToken: cancellationToken
        );

        if (!result.Success)
        {
            throw new InvalidOperationException($"Command '{cmd.Command}' failed with exit code {result.ExitCode}.");
        }

        return GetResponse(request);

       
    }

}
