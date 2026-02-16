using Bicep.Local.Extension.Host.Handlers;
using Flexor.Options;
using Flexor.v2026_1.Options;
using Flexor.v2026_1.Resources;

namespace Flexor.v2026_1.Handlers;

public class FlexorModuleResourceHandler : TypedResourceHandler<FlexorModuleResource, FlexorModuleIdentifiers, FlexorV2026_01_01_Options>
{
    internal static Dictionary<string, DefinedModule> DefinedModules { get; } = [];

    protected override FlexorModuleIdentifiers GetIdentifiers(FlexorModuleResource properties) 
        => new()
        {
            Type = properties.Type,
            Version = properties.Version
        };

    protected override async Task<ResourceResponse> Preview(ResourceRequest request, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return GetResponse(request);
    }

    protected override Task<ResourceResponse> CreateOrUpdate(ResourceRequest request, CancellationToken cancellationToken)
    {
        var module = request.Properties;
        var moduleKey = module.Version switch
        {
            var v when string.IsNullOrWhiteSpace(v) => $"{module.Type}",
            _ => $"{module.Type}@{module.Version}"
        };

        if (module.Get is {} && File.Exists(module.Get) is false)
        {
            throw new FileNotFoundException($"The specified 'get' script file was not found: {module.Get}");
        }
        if (module.CreateOrUpdate is {} && File.Exists(module.CreateOrUpdate) is false)
        {
            throw new FileNotFoundException($"The specified 'createOrUpdate' script file was not found: {module.CreateOrUpdate}");
        }
        if (module.Delete is {} && File.Exists(module.Delete) is false)
        {
            throw new FileNotFoundException($"The specified 'delete' script file was not found: {module.Delete}");
        }

        DefinedModules[moduleKey] = 
            new DefinedModule
            {
                Type = module.Type ?? string.Empty,
                Version = module.Version ?? string.Empty,
                ShellType = module.Shell ?? ShellType.Default,
                Options = module.Options ?? new FlexorModuleOptions(),
                Get = module.Get ?? string.Empty,
                GetOptions = module.GetOptions,
                CreateOrUpdate = module.CreateOrUpdate ?? string.Empty,
                CreateOrUpdateOptions = module.CreateOrUpdateOptions,
                Delete = module.Delete ?? string.Empty,
                DeleteOptions = module.DeleteOptions
            };

        request.Properties.TypeName = moduleKey;

        return Task.FromResult(GetResponse(request));
    }
}
