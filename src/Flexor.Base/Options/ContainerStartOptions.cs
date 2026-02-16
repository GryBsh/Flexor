namespace Flexor.Options;

public record ContainerStartOptions(string? Cli, string[]? Args, string? Image, Dictionary<string, string>? Mounts);
