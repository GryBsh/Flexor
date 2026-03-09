using System.Runtime.InteropServices;

namespace Flexor.Tests;

public class PathExtensionsTests
{
    #region AsPath

    [Fact]
    public void AsPath_ConvertsToCurrentOsFormat()
    {
        var path = "some/path/to/file";
        var result = path.AsPath();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Assert.Equal("some\\path\\to\\file", result);
        }
        else
        {
            Assert.Equal("some/path/to/file", result);
        }
    }

    #endregion

    #region AsUnixPath

    [Fact]
    public void AsUnixPath_ConvertsBackslashesToForwardSlashes()
    {
        Assert.Equal("some/path/to/file", "some\\path\\to\\file".AsUnixPath());
    }

    [Fact]
    public void AsUnixPath_LeavesForwardSlashesAlone()
    {
        Assert.Equal("some/path", "some/path".AsUnixPath());
    }

    #endregion

    #region AsWindowsPath

    [Fact]
    public void AsWindowsPath_ConvertsForwardSlashesToBackslashes()
    {
        Assert.Equal("some\\path\\to\\file", "some/path/to/file".AsWindowsPath());
    }

    [Fact]
    public void AsWindowsPath_LeavesBackslashesAlone()
    {
        Assert.Equal("some\\path", "some\\path".AsWindowsPath());
    }

    #endregion

    #region IsPathLike

    [Fact]
    public void IsPathLike_PathVariable_WithSeparator_ReturnsTrue()
    {
        var sep = Path.PathSeparator;
        var pair = ("MY_PATH_VAR", $"/usr/bin{sep}/usr/local/bin");
        Assert.True(pair.IsPathLike());
    }

    [Fact]
    public void IsPathLike_PathVariable_NoSeparator_ReturnsFalse()
    {
        var pair = ("MY_PATH_VAR", "/usr/bin");
        Assert.False(pair.IsPathLike());
    }

    [Fact]
    public void IsPathLike_NonPathVariable_ReturnsFalse()
    {
        var sep = Path.PathSeparator;
        var pair = ("MY_VAR", $"value1{sep}value2");
        Assert.False(pair.IsPathLike());
    }

    [Fact]
    public void IsPathLike_CaseInsensitive()
    {
        var sep = Path.PathSeparator;
        var pair = ("custom_PATH_dir", $"/a{sep}/b");
        Assert.True(pair.IsPathLike());
    }

    [Fact]
    public void IsPathLike_NullValue_ReturnsFalse()
    {
        var pair = ("PATH", (string?)null);
        Assert.False(pair.IsPathLike());
    }

    #endregion

    #region AppendPath

    [Fact]
    public void AppendPath_NullExisting_ReturnsNewPaths()
    {
        string? existing = null;
        var result = existing.AppendPath("/new/path");
        Assert.Contains("/new/path", result);
    }

    [Fact]
    public void AppendPath_EmptyExisting_ReturnsNewPaths()
    {
        var result = "".AppendPath("/first", "/second");
        Assert.Contains("/first", result);
        Assert.Contains("/second", result);
    }

    [Fact]
    public void AppendPath_WithExisting_PrependNewPaths()
    {
        var sep = Path.PathSeparator;
        var result = "/existing".AppendPath("/new");
        // New paths are prepended
        Assert.StartsWith("/new", result);
        Assert.Contains($"{sep}/existing", result);
    }

    [Fact]
    public void AppendPath_FiltersEmptyPaths()
    {
        var result = "".AppendPath("/valid", "", "  ", "/also_valid");
        Assert.DoesNotContain($"{Path.PathSeparator}{Path.PathSeparator}", result);
    }

    [Fact]
    public void AppendPath_NewlinesConvertedToPathSeparator()
    {
        var sep = Path.PathSeparator;
        var result = $"/a\n/b".AppendPath();
        Assert.Contains(sep.ToString(), result);
        Assert.DoesNotContain("\n", result);
    }

    [Fact]
    public void AppendPath_CrossPlatformSeparatorConversion()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // On Windows, colons in new paths should become semicolons
            var result = "".AppendPath("/usr/bin:/usr/local/bin");
            Assert.DoesNotContain(":", result);
        }
        else
        {
            // On Unix, semicolons should become colons
            var result = "".AppendPath("C:\\path1;C:\\path2");
            Assert.DoesNotContain(";", result);
        }
    }

    #endregion
}
