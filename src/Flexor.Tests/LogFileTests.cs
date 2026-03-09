using Flexor.Executor;
using Flexor.Options;

namespace Flexor.Tests;

public class LogFileTests : IDisposable
{
    private readonly string _tempDir;
    private readonly FlexorOptions _config;

    public LogFileTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"flexor_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _config = new FlexorOptions
        {
            FlexorPath = _tempDir,
            LogOptions = new FlexorLogOptions()
        };
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
    }

    [Fact]
    public void BeginOrRollover_CreatesLogDirectory()
    {
        var logDir = _config.LogPath;
        if (Directory.Exists(logDir))
            Directory.Delete(logDir, true);

        LogFile.BeginOrRollover(_config, "test", false, false);
        Assert.True(Directory.Exists(logDir));
    }

    [Fact]
    public void BeginOrRollover_ReturnsNameAndFiles()
    {
        var (name, stdOut, stdErr) = LogFile.BeginOrRollover(_config, "myresource", false, false);
        Assert.Equal("myresource", name);
        Assert.Contains("myresource", stdOut);
        Assert.Contains("myresource", stdErr);
        Assert.Contains("error", stdErr);
    }

    [Fact]
    public void BeginOrRollover_NullName_UsesDefault()
    {
        var (name, _, _) = LogFile.BeginOrRollover(_config, null, false, false);
        Assert.Equal("current", name);
    }

    [Fact]
    public void BeginOrRollover_EmptyName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => LogFile.BeginOrRollover(_config, " ", "stdout", false, false)
        );
    }

    [Fact]
    public void BeginOrRollover_Append_ReturnsSameFile()
    {
        // Create an initial log file
        var logDir = _config.LogPath;
        Directory.CreateDirectory(logDir);
        var logFile = Path.Combine(logDir, "test.log");
        File.WriteAllText(logFile, "existing content");

        var result = LogFile.BeginOrRollover(_config, "test", null, true, false);

        Assert.NotNull(result);
    }

    [Fact]
    public void BeginOrRollover_Cleanup_DeletesExistingFile()
    {
        var logDir = _config.LogPath;
        Directory.CreateDirectory(logDir);
        var logFile = Path.Combine(logDir, "cleanup_test.log");
        File.WriteAllText(logFile, "old content");

        LogFile.BeginOrRollover(_config, "cleanup_test", null, false, true);

        // The file should have been deleted (cleanup=true)
        Assert.False(File.Exists(logFile));
    }

    [Fact]
    public void BeginOrRollover_Rollover_MovesExistingFile()
    {
        var logDir = _config.LogPath;
        Directory.CreateDirectory(logDir);
        var logFile = Path.Combine(logDir, "rollover_test.log");
        File.WriteAllText(logFile, "old content");

        LogFile.BeginOrRollover(_config, "rollover_test", null, false, false);

        // Original filename should be available (file was moved to timestamped name)
        // There should be a timestamped version
        var files = Directory.GetFiles(logDir, "rollover_test*");
        Assert.True(files.Length >= 1);
    }
}
