using System.Text.Json;
using Bicep.Local.Extension.Host.Handlers;
using Flexor.Executor;
using Flexor.Options;
using Flexor.v2026_1.Options;
using Flexor.v2026_1.Resources;
using Microsoft.Extensions.Logging;

namespace Flexor.v2026_1.Handlers;

public class FlexorResourceHandler(ILogger<FlexorResourceHandler> logger) : TypedResourceHandler<FlexorResource, FlexorResourceIdentifiers, FlexorV2026_01_01_Options>
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

        var (env, args, input) = ResolveStartParameters(
            request.Identifiers.Parameters, 
            request.Identifiers.Options.Exec, 
            moduleDefinition
        );
        var resource = new FlexorResource
        {
            Name = request.Identifiers.Name,
            Options = request.Identifiers.Options,
            EnableLogging = request.Identifiers.EnableLogging,
            CleanupLogs = request.Identifiers.CleanupLogs,
            AppendLogs = request.Identifiers.AppendLogs
        };

        await ProcessExecutor.RunScriptAsync(
            resource,
            request.Config,
            new()
            {
                ShellType = moduleDefinition.ShellType,
                Path = moduleDefinition.Get,
                Args = args,
                Env = env,
                WorkingDirectory = resource.Options.Exec.WorkingDirectory ?? moduleDefinition.GetOptions?.Exec.WorkingDirectory,
                RunAsAdmin = resource.Options.Exec.RunAsAdmin ?? moduleDefinition.GetOptions?.Exec.RunAsAdmin is true,
                TimeoutSeconds = resource.Options.Exec.TimeoutSeconds ?? moduleDefinition.GetOptions?.Exec.TimeoutSeconds,
                ContinueOnFailure = resource.Options.Exec.ContinueOnFailure ?? moduleDefinition.GetOptions?.Exec.ContinueOnFailure is true,
                Input = input,
                UseContainer = resource.Options.Exec.UseContainer || moduleDefinition.GetOptions?.Exec.UseContainer is true,
                ContainerImage = resource.Options.Exec.ContainerImage ?? moduleDefinition.GetOptions?.Exec.ContainerImage,
                ContainerCli = resource.Options.Exec.ContainerCli ?? moduleDefinition.GetOptions?.Exec.ContainerCli,
                ContainerCliArgs = resource.Options.Exec.ContainerCliArgs ?? moduleDefinition.GetOptions?.Exec.ContainerCliArgs,
                ContainerMounts = resource.Options.Exec.ContainerMounts ?? moduleDefinition.GetOptions?.Exec.ContainerMounts,
                WorkingPathMount = resource.Options.Exec.WorkingPathMount ?? moduleDefinition.GetOptions?.Exec.WorkingPathMount,
                FlexorPath = request.Config.FlexorPath,
                NoWait = resource.Options.Exec.NoWait
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

       var (env, args, input) = ResolveStartParameters(
            request.Properties.Parameters, 
            request.Properties.Options.Exec, 
            moduleDefinition
        );

        await ProcessExecutor.RunScriptAsync(
            resource,
            request.Config,
            new()
            {
                ShellType = (ShellType)(int)moduleDefinition.ShellType,
                Path = moduleDefinition.CreateOrUpdate,
                Args = args,
                Env = env,
                WorkingDirectory = resource.Options.Exec.WorkingDirectory ?? moduleDefinition.CreateOrUpdateOptions?.Exec.WorkingDirectory,
                RunAsAdmin = resource.Options.Exec.RunAsAdmin ?? moduleDefinition.CreateOrUpdateOptions?.Exec.RunAsAdmin is true,
                TimeoutSeconds = resource.Options.Exec.TimeoutSeconds ?? moduleDefinition.CreateOrUpdateOptions?.Exec.TimeoutSeconds,
                ContinueOnFailure = resource.Options.Exec.ContinueOnFailure ?? moduleDefinition.CreateOrUpdateOptions?.Exec.ContinueOnFailure is true,
                Input = input,
                UseContainer = resource.Options.Exec.UseContainer || moduleDefinition.CreateOrUpdateOptions?.Exec.UseContainer is true,
                ContainerImage = resource.Options.Exec.ContainerImage ?? moduleDefinition.CreateOrUpdateOptions?.Exec.ContainerImage,
                ContainerCli = resource.Options.Exec.ContainerCli ?? moduleDefinition.CreateOrUpdateOptions?.Exec.ContainerCli,
                ContainerCliArgs = resource.Options.Exec.ContainerCliArgs ?? moduleDefinition.CreateOrUpdateOptions?.Exec.ContainerCliArgs
            },
            logger,
            cancellationToken
        );

        return GetResponse(request);
    }

    protected override async Task<ResourceResponse> Delete(ReferenceRequest request, CancellationToken cancellationToken)
    {

        if (string.IsNullOrWhiteSpace(request.Identifiers.Type))
        {
            throw new ArgumentException("The 'Type' property must be provided for the custom module step.");
        }

        if (!FlexorModuleResourceHandler.DefinedModules.TryGetValue(request.Identifiers.Type, out var moduleDefinition))
        {
            throw new ArgumentException($"No module definition found for type '{request.Identifiers.Type}'.");
        }

        var (env, args, input) = ResolveStartParameters(
            request.Identifiers.Parameters,
            request.Identifiers.Options.Exec, 
            moduleDefinition
        );

        var resource = new FlexorResource
        {
            Name = request.Identifiers.Name,
            Options = request.Identifiers.Options,
            EnableLogging = request.Identifiers.EnableLogging,
            CleanupLogs = request.Identifiers.CleanupLogs,
            AppendLogs = request.Identifiers.AppendLogs
        };

        await ProcessExecutor.RunScriptAsync(
            resource,
            request.Config,
            new()
            {
                ShellType = moduleDefinition.ShellType,
                Path = moduleDefinition.Delete,
                Args = args,
                Env = env,
                WorkingDirectory = resource.Options.Exec.WorkingDirectory ?? moduleDefinition.DeleteOptions?.Exec.WorkingDirectory,
                RunAsAdmin = resource.Options.Exec.RunAsAdmin ?? moduleDefinition.DeleteOptions?.Exec.RunAsAdmin is true,
                TimeoutSeconds = resource.Options.Exec.TimeoutSeconds ?? moduleDefinition.DeleteOptions?.Exec.TimeoutSeconds,
                ContinueOnFailure = resource.Options.Exec.ContinueOnFailure ?? moduleDefinition.DeleteOptions?.Exec.ContinueOnFailure is true,
                Input = input,
                UseContainer = resource.Options.Exec.UseContainer || moduleDefinition.DeleteOptions?.Exec.UseContainer is true,
                ContainerImage = resource.Options.Exec.ContainerImage ?? moduleDefinition.DeleteOptions?.Exec.ContainerImage,
                ContainerCli = resource.Options.Exec.ContainerCli ?? moduleDefinition.DeleteOptions?.Exec.ContainerCli,
                ContainerCliArgs = resource.Options.Exec.ContainerCliArgs ?? moduleDefinition.DeleteOptions?.Exec.ContainerCliArgs,
                ContainerMounts = resource.Options.Exec.ContainerMounts ?? moduleDefinition.DeleteOptions?.Exec.ContainerMounts
            },
            logger,
            cancellationToken
        );

        return GetResponse(request, resource);
    }

    static (Dictionary<string,string>, string[], string[] ) ResolveStartParameters(
        Dictionary<string, string> parameters,
        ExecutionOptionsBase options,
        DefinedModule? moduleDefinition)
    {
        var env = moduleDefinition?.Options.Type is ModuleParamHandlingType.Env
             ? ApplyResourceEnvironment(parameters, moduleDefinition)
             : moduleDefinition?.Options.Env ?? new Dictionary<string, string>();
        var args = moduleDefinition?.Options.Type is ModuleParamHandlingType.Args
            ? ApplyResourceArguments(parameters, moduleDefinition)
            : moduleDefinition?.Options.Args ?? [];
        string? containerCmd = null;
        string[]? containerArgs = null;
        if (options.UseContainer is true)
        {
            containerCmd = options.ContainerCli;
            var image = options.ContainerImage;
            containerArgs = [ ..options.ContainerCliArgs ?? [], image ?? string.Empty ];
        }

        string[] input = moduleDefinition?.Options.Type switch
        {
            ModuleParamHandlingType.StdInEnv => [ParameterEnv(parameters)],
            ModuleParamHandlingType.StdInJson => [JsonSerializer.Serialize(parameters)],
            _ => []
        };
        return (env, args, input);
    }

    static Dictionary<string, string> ApplyResourceEnvironment(
        Dictionary<string, string>? parameters, 
        DefinedModule moduleDefinition
    )
    {
        var result = new Dictionary<string, string>();
        
        foreach (var e in moduleDefinition.Options.Env)
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

    static string[] ApplyResourceArguments(
        Dictionary<string, string>? parameters, 
        DefinedModule moduleDefinition
    )
    {
        var args = new List<string>();
        
        foreach (var arg in moduleDefinition.Options.Args)
        {
            args.Add(arg);
        }
        if (parameters is not null)
        {
            foreach (var param in parameters)
            {
                args.Add($"--{param.Key}");
                args.Add(param.Value);
            }
        }
        return [..args];
    }

    static string ParameterEnv(Dictionary<string, string> parameters)
    {
        return string.Join(
            '\n',
            parameters.Select(
                p => $"{p.Key}=\"{p.Value.Replace("\"", "\\\"")}\""
            )
        );
        
    }

}