using Bicep.Local.Extension.Host.Handlers;
using Flexor.Options;
using Flexor.v2026_1.Options;
using Flexor.v2026_1.Resources;

namespace Flexor.v2026_1.Handlers;

public class FlexorModuleResourceHandler : TypedResourceHandler<FlexorModuleResource, ResourceIdentifiers, FlexorOptions>
{
    internal static Dictionary<string, DefinedModuleOptions> DefinedModules { get; } = [];

    protected override ResourceIdentifiers GetIdentifiers(FlexorModuleResource properties) 
        => new()
        {
            Name = properties.Name
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

        DefinedModules[moduleKey] = 
            new DefinedModuleOptions
            {
                Type = module.Type ?? string.Empty,
                Version = module.Version ?? string.Empty,
                ShellType = module.Shell ?? ShellType.Default,
                Env = module.Options.Env ?? [],
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
