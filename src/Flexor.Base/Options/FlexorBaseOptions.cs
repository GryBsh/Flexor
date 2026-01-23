using Azure.Bicep.Types.Concrete;
using Bicep.Local.Extension.Types.Attributes;

namespace Flexor.Options;


/// <summary>
/// Base configuration for Flexor.
/// Extend this class to add version-specific configuration options.
/// </summary>
public class FlexorBaseOptions
{
    const string BicepLocalDefault = ".bicep";
    const string FlexorPathDefault = $"{BicepLocalDefault}/flexor";
    const string FlexorPathEnvVar = "FLEXOR_PATH";



    [TypeProperty("Path to the root of Flexors working space. " +
                  "(Where the logs and psmodules folder are located. " +
                  "Default: FLEXOR_PATH environment variable or .bicep/flexor)",
                  ObjectTypePropertyFlags.None)]
    internal static string? FlexorPathEnvVariable => Environment.GetEnvironmentVariable(FlexorPathEnvVar);

    public string PathRoot
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
                return Path.Combine(PathRoot, "logs");    
            }
            return field;
        }
        set;
    }

    [TypeProperty("Options for Flexor log files", ObjectTypePropertyFlags.None)]
    public FlexorLogOptions LogOptions { get; set; } = new FlexorLogOptions();

}

