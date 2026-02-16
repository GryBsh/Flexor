using Azure.Bicep.Types.Concrete;
using Bicep.Local.Extension.Types.Attributes;
using Microsoft.Extensions.Configuration;

namespace Flexor.Options;


/// <summary>
/// Base configuration for Flexor.
/// Extend this class to add version-specific configuration options.
/// </summary>
public class FlexorOptions
{
    const string BicepLocalDefault = ".bicep";
    const string FlexorPathDefault = $"{BicepLocalDefault}/flexor";
    const string FlexorPathEnvVar = "FLEXOR_PATH";

    const string FlexorConfigFilename = "flexorconfig.json";


    [TypeProperty("Path to the root of Flexors working space. " +
                  "(Where the logs and psmodules folder are located. " +
                  "Default: FLEXOR_PATH environment variable or .bicep/flexor)",
                  ObjectTypePropertyFlags.None)]
    internal static string? FlexorPathEnvVariable => Environment.GetEnvironmentVariable(FlexorPathEnvVar);

    public string FlexorPath
    {
        get => field switch
        {
            not null => field,
            _ when FlexorPathEnvVariable is not null => FlexorPathEnvVariable,
            _ => FlexorPathDefault
        };
        set;
    }

    [TypeProperty("Path to store Flexor logs", ObjectTypePropertyFlags.None)]
    public string LogPath
    {
        get
        {
            if (field is null)
            {
                return Path.Combine(FlexorPath, "logs");    
            }
            return field;
        }
        set;
    }

    [TypeProperty("Enable or disable trace logging. Default: false", ObjectTypePropertyFlags.None)]
    public bool EnableTraceLogging { get; set; } = false;

    [TypeProperty("Options for Flexor log files", ObjectTypePropertyFlags.None)]
    public FlexorLogOptions LogOptions { get; set; } = new FlexorLogOptions();

    internal static TOptions GetConfigFileOptions<TOptions>()
        where TOptions : FlexorOptions, new()
    {
        var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile(FlexorConfigFilename, optional: true, reloadOnChange: true)
                    .Build();

        var options = new TOptions();
        config.Bind(options);
        return options;
    }

}

