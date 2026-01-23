using Bicep.Local.Extension.Host.Handlers;
using Flexor.Executor;
using Flexor.Options;
using Flexor.v2026_1.Options;
using Flexor.v2026_1.Resources;
using Microsoft.Extensions.Logging;

namespace Flexor.v2026_1.Handlers;

public class ScriptResourceHandler(ILogger<ScriptResourceHandler> logger) : TypedResourceHandler<ScriptResource, ResourceIdentifiers, FlexorOptions>
{

    protected override ResourceIdentifiers GetIdentifiers(ScriptResource properties) 
        => new()
        {
            Name = properties.Name
        };

    protected override async Task<ResourceResponse> Preview(ResourceRequest request, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return GetResponse(request);
    }

    protected override async Task<ResourceResponse> CreateOrUpdate(ResourceRequest request, CancellationToken cancellationToken)
    {
        var script = request.Properties;
        script.Shell ??= ShellType.Default;

        _ = await ProcessExecutor.RunScriptAsync(
            script,
            request.Config,
            new()
            {
                ShellType = script.Shell.Value,
                Path = script.Script,
                Args = [.. script.Args ],
                Env = script.Env,
                WorkingDirectory = script.Options.WorkingDirectory,
                RunAsAdmin = script.Options.RunAsAdmin ?? false,
                TimeoutSeconds = script.Options.TimeoutSeconds ?? 0,
                ContinueOnFailure = script.Options.ContinueOnFailure ?? false,
                Input = script.Contents is {} 
                    ? [ script.Contents ] 
                    : []
            },
            logger,
            cancellationToken: cancellationToken
        );
        return GetResponse(request);
    }

    
}
