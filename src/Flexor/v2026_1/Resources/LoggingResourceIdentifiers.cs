using Azure.Bicep.Types.Concrete;
using Bicep.Local.Extension.Types.Attributes;

namespace Flexor.v2026_1.Resources;

public class LoggingResourceIdentifiers
{
    [TypeProperty("Whether to log to output streams (out,err) to disk", ObjectTypePropertyFlags.Identifier)]
    public bool EnableLogging { get; set; } = true;

    [TypeProperty("Cleanup logs from previous execution(s) before execution", ObjectTypePropertyFlags.Identifier)]
    public bool CleanupLogs { get; set; } = false;
    
    [TypeProperty("Append logs to existing log files instead of overwriting", ObjectTypePropertyFlags.Identifier)]
    public bool AppendLogs {get; set;} = false;
}
