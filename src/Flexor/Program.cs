using Bicep.Local.Extension.Host.Extensions;
using Bicep.Local.Extension.Types;
using Flexor.Git;
using Flexor.v2026_1.Handlers;
using Flexor.v2026_1.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;


var builder = WebApplication.CreateBuilder();

builder.Services.AddTransient<IGitClient, GitClient>();

builder.AddBicepExtensionHost(args);

#if WINDOWS
builder.Logging.AddEventLog(c =>
{
    c.LogName = "Flexor";    
    c.SourceName = "Flexor.Extension";
});
#endif

builder.Services
       .AddBicepExtension()
       .WithExtensionInfo(
            name: "Flexor",
            version: "0.1.2026.1",
            isSingleton: true
       )
       .WithTypeProvider<TypeProvider>()
       .WithTypeDefinitionBuilder<TypeDefinitionBuilder>()
       .WithTypeAssembly<Program>()
       .WithConfigurationType<FlexorOptions>()
       .WithResourceHandler<CommandResourceHandler>()
       .WithResourceHandler<RepositoryResourceHandler>()
       .WithResourceHandler<HttpResourceHandler>()
       .WithResourceHandler<ScriptResourceHandler>()
       .WithResourceHandler<FlexorModuleResourceHandler>()
       .WithResourceHandler<FlexorResourceHandler>();

var app = builder.Build();

await app.MapBicepExtension()
         .RunAsync();