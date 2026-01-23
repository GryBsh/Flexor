using Azure.Bicep.Types.Concrete;
using Bicep.Local.Extension.Types.Attributes;

namespace Flexor.Options;

/// <summary>
/// Options for Flexor log files. 
/// </summary>
public class FlexorLogOptions
{
    [TypeProperty("Enable or disable logging for Flexor operations", ObjectTypePropertyFlags.None)]
    public bool Enabled { get; set; } = true;
    [TypeProperty("Disable rolling over existing logs", ObjectTypePropertyFlags.None)]
    public bool DisableRollover { get; set; } = false;
    [TypeProperty("Append to existing logs instead of overwriting", ObjectTypePropertyFlags.None)]
    public bool Append { get; set; } = false;
    [TypeProperty("Extension for log file names", ObjectTypePropertyFlags.None)]
    public string FilenameExtension { get; set; } = ".log";
    [TypeProperty("Separator used in log file names", ObjectTypePropertyFlags.None)]
    public string FilenameSeparator { get; set; } = ".";
    [TypeProperty("Segment identifier for standard output log files", ObjectTypePropertyFlags.None)]
    public string FilenameStdOutSegment { get; set; } = string.Empty;
    [TypeProperty("Segment identifier for standard error log files", ObjectTypePropertyFlags.None)]
    public string FilenameStdErrSegment { get; set; } = "error";
    [TypeProperty("Timestamp format for log file rollover", ObjectTypePropertyFlags.None)]
    public string FilenameTimestampFormat { get; set; } = "yyyyMMddHHmmss";
    
}

