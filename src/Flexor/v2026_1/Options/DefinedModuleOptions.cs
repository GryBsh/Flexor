using Flexor.Options;

namespace Flexor.v2026_1.Options;



public record DefinedModuleOptions
{
    public required string Type { get; set; }
    public required string Version { get; set; }
    public ShellType ShellType { get; set; }
    public Dictionary<string, string> Env { get; set; } = new();
    public string? Get { get; set; }
    public FlexorResourceOptions? GetOptions { get; set; } = new();
    public string? CreateOrUpdate { get; set; }
    public FlexorResourceOptions? CreateOrUpdateOptions { get; set; } = new();
    public string? Delete { get; set; }
    public FlexorResourceOptions? DeleteOptions { get; set; } = new();
}