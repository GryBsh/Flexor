using System.Runtime.InteropServices;

namespace Flexor;

public static class PathExtensions
{
    public const string PathVariable = "PATH";
    public const char UnixPathSeparator = '/';
    public const char WindowsPathSeparator = '\\';

    /// <summary>
    /// Converts path to the current OS format
    /// </summary>
    /// <param name="path">
    /// The file or directory path to be converted.
    /// </param>
    /// <returns>
    /// A string representing the path formatted for the current operating system.
    /// </returns>
    public static string AsPath(this string path)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return path.Replace(UnixPathSeparator, WindowsPathSeparator);
        }
        else
        {
            return path.Replace(WindowsPathSeparator, UnixPathSeparator);
        }
    }

    /// <summary>
    /// Determines if the given environment variable pair is path-like
    /// </summary>
    /// <remarks>
    /// A path-like variable is one with `path` (any casing) in its name that contains multiple paths separated by the system's path separator
    /// (e.g., ':' on Unix-like systems and ';' on Windows).
    /// </remarks>
    /// <param name="pair">
    /// The environment variable key-value pair to check.
    /// </param>
    /// <returns>
    /// True if the environment variable pair is path-like; otherwise, false.
    /// </returns>
    public static bool IsPathLike(this (string Key, string? Value) pair)
        => pair.Key.Contains(PathVariable, StringComparison.OrdinalIgnoreCase) 
           && (pair.Value?.Contains(Path.PathSeparator) ?? false);

    /// <summary>
    /// Appends new paths to an existing PATH or path-like variable string
    /// </summary>
    /// <param name="existingPaths">
    /// The existing PATH or path-like variable string to which new paths will be appended.
    /// </param>
    /// <param name="newPath">
    /// The new paths to append to the existing PATH or path-like variable string.
    /// </param>
    /// <returns>
    /// A string representing the combined PATH or path-like variable string.
    /// </returns>
    public static string AppendPath(this string? existingPaths, params string[] newPath)
    {
        existingPaths = existingPaths?.Replace('\n', Path.PathSeparator) ?? string.Empty;
        newPath = [.. newPath.Where(p => !string.IsNullOrWhiteSpace(p))];
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            newPath = [.. newPath.Select(p => p.Replace(':', ';'))];
        }
        else
        {
            newPath = [.. newPath.Select(p => p.Replace(';', ':'))];
        }

        if (string.IsNullOrEmpty(existingPaths))
        {
            return string.Join(Path.PathSeparator, newPath);
        }
        
        return string.Join(Path.PathSeparator, [.. newPath, existingPaths]);
    }
}
