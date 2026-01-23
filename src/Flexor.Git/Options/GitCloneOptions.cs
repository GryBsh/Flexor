namespace Flexor.Git.Options;

public record GitCloneOptions
{
    public string Branch { get; init; } = "main";
    public bool IsBare { get; init; } = false;
    public bool RecurseSubmodules { get; init; } = true;
    public bool Checkout { get; init; } = true;    
}
