namespace Flexor.Options;

/// <summary>
/// Types of shells available for script execution
/// </summary>
public enum ShellType : int
{
    Default = 0,
    Bash,
    PowerShell,
    Cmd,
    Python
}

/// <summary>
/// Types of repositories
/// </summary>
public enum RepositoryType : int
{
    Default = 0,
    Git 
}

/// <summary>
/// Specifies the archive formats supported for compression and extraction operations.
/// </summary>
public enum ArchiveType : int
{
    Zip = 0,
    Tar,
    TarGz,
    //TarBz2,
    //TarXz
}