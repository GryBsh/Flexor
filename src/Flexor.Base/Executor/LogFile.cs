using Flexor.Options;

namespace Flexor.Executor;

public static class LogFile
{
    public static (string Name, string StdOut, string StdErr) BeginOrRollover(FlexorOptions config, string? name, bool append, bool cleanup)
    {
        if (!Directory.Exists(config.LogPath))
            Directory.CreateDirectory(config.LogPath);

        name = $"{name ?? "current"}";
        
        var stdOutFile = BeginOrRollover(config, name, config.LogOptions.FilenameStdOutSegment, append, cleanup);
        var stdErrFile = BeginOrRollover(config, name, config.LogOptions.FilenameStdErrSegment, append, cleanup);

        return (name, stdOutFile, stdErrFile);
    }
    
    public static string BeginOrRollover(FlexorOptions config, string name, string? type, bool append, bool cleanup)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Log name cannot be null or whitespace.", nameof(name));

        var filename = LogName(config, name, type);
        var log = new FileInfo(filename);
        if (log.Exists)
        {
            if (append)
            {
                return filename;
            }
            var ts = log.LastWriteTime.ToString(config.LogOptions.FilenameTimestampFormat);
            type = string.IsNullOrWhiteSpace(type) ? ts : $"{type}{config.LogOptions.FilenameSeparator}{ts}";
            if (cleanup) log.Delete();
            else log.MoveTo(LogName(config, name, type), true);
        }

        return filename;

        static string LogName(FlexorOptions config, string name, string? type = null)
        {
            return $"{config.LogPath}/{name}" + (string.IsNullOrWhiteSpace(type) ? string.Empty : $"{config.LogOptions.FilenameSeparator}{type}") + config.LogOptions.FilenameExtension;
        }

        
    }
}
