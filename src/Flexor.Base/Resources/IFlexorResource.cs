namespace Flexor.Resources;

public interface IFlexorResource
{
    string Name { get; set; }
    bool EnableLogging { get; set; }
    bool CleanupLogs { get; set; }
    bool AppendLogs { get; set; }
}