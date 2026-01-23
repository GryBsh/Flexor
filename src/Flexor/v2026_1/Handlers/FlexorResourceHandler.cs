using Bicep.Local.Extension.Host.Handlers;
using Flexor.Executor;
using Flexor.v2026_1.Options;
using Flexor.v2026_1.Resources;
using Microsoft.Extensions.Logging;

namespace Flexor.v2026_1.Handlers;

public class FlexorResourceHandler(ILogger<FlexorResourceHandler> logger) : TypedResourceHandler<FlexorResource, FlexorResourceIdentifiers, FlexorOptions>
{
    protected override FlexorResourceIdentifiers GetIdentifiers(FlexorResource resource) 
        => new()
        {
            Name = resource.Name,
            Options = resource.Options
        };

    protected override async Task<ResourceResponse> Preview(ResourceRequest request, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return GetResponse(request);
    }

    protected override async Task<ResourceResponse> Get(ReferenceRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Identifiers.Type))
        {
            throw new ArgumentException("The 'Type' property must be provided for the custom module step.");
        }

        if (!FlexorModuleResourceHandler.DefinedModules.TryGetValue(request.Identifiers.Type, out var moduleDefinition))
        {
            throw new ArgumentException($"No module definition found for type '{request.Identifiers.Type}'.");
        }

        var env = ApplyResourceEnvironment(request.Identifiers.Parameters, moduleDefinition);

        var resource = new FlexorResource
        {
            Name = request.Identifiers.Name,
            Options = request.Identifiers.Options,
            EnableLogging = request.Identifiers.EnableLogging,
            CleanupLogs = request.Identifiers.CleanupLogs,
            AppendLogs = request.Identifiers.AppendLogs
        };

        string[] args = [
            ..moduleDefinition.DeleteOptions?.Args ?? [],
            ..request.Identifiers.Options?.Args ?? []
        ];

        await ProcessExecutor.RunScriptAsync(
            resource,
            request.Config,
            new()
            {
                ShellType = moduleDefinition.ShellType,
                Path = moduleDefinition.Get,
                Args = args,
                Env = env,
                WorkingDirectory = resource.Options.WorkingDirectory ?? moduleDefinition.GetOptions?.WorkingDirectory,
                RunAsAdmin = resource.Options.RunAsAdmin ?? moduleDefinition.GetOptions?.RunAsAdmin is true,
                TimeoutSeconds = resource.Options.TimeoutSeconds ?? moduleDefinition.GetOptions?.TimeoutSeconds,
                ContinueOnFailure = resource.Options.ContinueOnFailure ?? moduleDefinition.GetOptions?.ContinueOnFailure is true,
                Input = []
            },
            logger,
            cancellationToken
        );

        return GetResponse(request, resource);
    }

    protected override async Task<ResourceResponse> CreateOrUpdate(ResourceRequest request, CancellationToken cancellationToken)
    {
        var resource = request.Properties;
        if (string.IsNullOrWhiteSpace(resource.Type))
        {
            throw new ArgumentException("The 'Type' property must be provided for the custom module step.");
        }

        if (!FlexorModuleResourceHandler.DefinedModules.TryGetValue(resource.Type, out var moduleDefinition))
        {
            throw new ArgumentException($"No module definition found for type '{resource.Type}'.");
        }

        var env = ApplyResourceEnvironment(resource.Parameters, moduleDefinition);

        await ProcessExecutor.RunScriptAsync(
            resource,
            request.Config,
            new()
            {
                ShellType = (Flexor.Options.ShellType)(int)moduleDefinition.ShellType,
                Path = moduleDefinition.CreateOrUpdate,
                Args = [],
                Env = env,
                WorkingDirectory = resource.Options.WorkingDirectory ?? moduleDefinition.CreateOrUpdateOptions?.WorkingDirectory,
                RunAsAdmin = resource.Options.RunAsAdmin ?? moduleDefinition.CreateOrUpdateOptions?.RunAsAdmin is true,
                TimeoutSeconds = resource.Options.TimeoutSeconds ?? moduleDefinition.CreateOrUpdateOptions?.TimeoutSeconds,
                ContinueOnFailure = resource.Options.ContinueOnFailure ?? moduleDefinition.CreateOrUpdateOptions?.ContinueOnFailure is true,
                Input = []
            },
            logger,
            cancellationToken
        );

        return GetResponse(request);
    }

    static Dictionary<string, string> ApplyResourceEnvironment(
        Dictionary<string, string>? parameters, 
        DefinedModuleOptions moduleDefinition
    )
    {
        var result = new Dictionary<string, string>();
        
        foreach (var e in moduleDefinition.Env)
        {
            result[e.Key] = e.Value;
        }

        if (parameters is not null)
        {
            foreach (var param in parameters)
            {
                result[$"PARAM_{param.Key}"] = param.Value;
            }
        }
        
        return result;
    }

}