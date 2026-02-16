using System.Text.Json;
using Bicep.Local.Extension.Host.Handlers;
using Flexor.Executor;
using Flexor.Options;
using Flexor.v2026_1.Options;
using Flexor.v2026_1.Resources;
using Microsoft.Extensions.Logging;

namespace Flexor.v2026_1.Handlers;

public class RunResourceHandler(ILogger<RunResourceHandler> logger) : TypedResourceHandler<RunResource, RunResourceIdentifiers, FlexorV2026_01_01_Options>()
{
    protected override RunResourceIdentifiers GetIdentifiers(RunResource properties) 
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
        
        var result = await ProcessExecutor.RunAsync(
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
                ContinueOnFailure = cmd.Options.ContinueOnFailure ?? false,
                UseContainer = cmd.Options.UseContainer,
                ContainerImage = cmd.Options.ContainerImage,
                ContainerCli = cmd.Options.ContainerCli,
                ContainerCliArgs = cmd.Options.ContainerCliArgs,
                ContainerMounts = cmd.Options.ContainerMounts,
                WorkingPathMount = cmd.Options.WorkingPathMount,
                FlexorPath = request.Config.FlexorPath,
                NoWait = cmd.Options.NoWait
            },
            logger,
            cancellationToken: cancellationToken
        );

        if (!result.Success)
        {
            throw new InvalidOperationException($"Command '{cmd.Command}' failed with exit code {result.ExitCode}.");
        }

        return GetResponse(request);

       
    }



}
