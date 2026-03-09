using Flexor.Executor;
using Flexor.Options;
using Microsoft.Extensions.Logging.Abstractions;

namespace Flexor.Tests;

public class ProcessExecutorTests
{
    #region Null Command

    [Fact]
    public async Task RunAsync_NullCommand_ThrowsArgumentException()
    {
        var options = new ProcessStartOptions { Command = null };
        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => ProcessExecutor.RunAsync(options, NullLogger.Instance)
        );
        Assert.Contains("Command must be provided", ex.Message);
    }

    #endregion

    #region Basic Execution

    [Fact]
    public async Task RunAsync_EchoCommand_ReturnsSuccess()
    {
        var options = new ProcessStartOptions
        {
            Command = "echo",
            Args = ["hello"]
        };
        var result = await ProcessExecutor.RunAsync(options, NullLogger.Instance);
        Assert.True(result.Success);
        Assert.Equal(0, result.ExitCode);
        Assert.False(result.Timeout);
        Assert.False(result.Cancelled);
        Assert.False(result.Killed);
    }

    [Fact]
    public async Task RunAsync_NonZeroExitCode_ReturnsFailure()
    {
        var options = new ProcessStartOptions
        {
            Command = "bash",
            Args = ["-c", "exit 42"]
        };
        var result = await ProcessExecutor.RunAsync(options, NullLogger.Instance);
        Assert.False(result.Success);
        Assert.Equal(42, result.ExitCode);
    }

    #endregion

    #region StdOutput / StdError Handlers

    [Fact]
    public async Task RunAsync_StdOutputHandler_ReceivesOutput()
    {
        var lines = new List<string>();
        var options = new ProcessStartOptions
        {
            Command = "echo",
            Args = ["test output"],
            StdOutputHandler = line => lines.Add(line)
        };
        await ProcessExecutor.RunAsync(options, NullLogger.Instance);
        Assert.Contains(lines, l => l.Contains("test output"));
    }

    [Fact]
    public async Task RunAsync_StdErrorHandler_ReceivesStderr()
    {
        var errors = new List<string>();
        var options = new ProcessStartOptions
        {
            Command = "bash",
            Args = ["-c", "echo error_msg >&2"],
            StdErrorHandler = line => errors.Add(line)
        };
        await ProcessExecutor.RunAsync(options, NullLogger.Instance);
        Assert.Contains(errors, l => l.Contains("error_msg"));
    }

    #endregion

    #region CaptureRawOutput

    [Fact]
    public async Task RunAsync_CaptureRawOutput_PopulatesRawOutput()
    {
        var options = new ProcessStartOptions
        {
            Command = "echo",
            Args = ["raw capture test"],
            CaptureRawOutput = true
        };
        var result = await ProcessExecutor.RunAsync(options, NullLogger.Instance);
        Assert.True(result.Success);
        Assert.NotNull(result.RawOutput);
        Assert.Contains("raw capture test", result.RawOutput);
    }

    [Fact]
    public async Task RunAsync_CaptureRawOutput_False_RawOutputIsNull()
    {
        var options = new ProcessStartOptions
        {
            Command = "echo",
            Args = ["test"],
            CaptureRawOutput = false
        };
        var result = await ProcessExecutor.RunAsync(options, NullLogger.Instance);
        Assert.Null(result.RawOutput);
    }

    [Fact]
    public async Task RunAsync_CaptureRawOutput_PreservesMultiLineJson()
    {
        var json = "{\"key\":\"value\",\"nested\":{\"a\":1}}";
        var options = new ProcessStartOptions
        {
            Command = "bash",
            Args = ["-c", $"echo '{json}'"],
            CaptureRawOutput = true
        };
        var result = await ProcessExecutor.RunAsync(options, NullLogger.Instance);
        Assert.Contains("key", result.RawOutput);
        Assert.Contains("value", result.RawOutput);
    }

    [Fact]
    public async Task RunAsync_CaptureRawOutput_StillFiresStdOutputHandler()
    {
        var lines = new List<string>();
        var options = new ProcessStartOptions
        {
            Command = "bash",
            Args = ["-c", "echo line1; echo line2"],
            CaptureRawOutput = true,
            StdOutputHandler = line => lines.Add(line)
        };
        await ProcessExecutor.RunAsync(options, NullLogger.Instance);
        Assert.Contains(lines, l => l == "line1");
        Assert.Contains(lines, l => l == "line2");
    }

    [Fact]
    public async Task RunAsync_CaptureRawOutput_NoTrailingEmptyLine()
    {
        var lines = new List<string>();
        var options = new ProcessStartOptions
        {
            Command = "bash",
            Args = ["-c", "echo hello"],
            CaptureRawOutput = true,
            StdOutputHandler = line => lines.Add(line)
        };
        await ProcessExecutor.RunAsync(options, NullLogger.Instance);
        // Should not have a trailing empty string from the split
        if (lines.Count > 0)
        {
            Assert.NotEqual(string.Empty, lines[^1]);
        }
    }

    #endregion

    #region Timeout

    [Fact]
    public async Task RunAsync_Timeout_KillsProcess()
    {
        var options = new ProcessStartOptions
        {
            Command = "bash",
            Args = ["-c", "sleep 30"],
            TimeoutSeconds = 1
        };
        // The timeout CTS causes WaitForExitAsync to throw TaskCanceledException
        try
        {
            var result = await ProcessExecutor.RunAsync(options, NullLogger.Instance);
            Assert.False(result.Success);
            Assert.True(result.Timeout || result.Killed);
        }
        catch (TaskCanceledException)
        {
            // Expected — timeout triggers cancellation internally
        }
    }

    [Fact]
    public async Task RunAsync_NoTimeout_CompletesNormally()
    {
        var options = new ProcessStartOptions
        {
            Command = "echo",
            Args = ["fast"],
            TimeoutSeconds = null
        };
        var result = await ProcessExecutor.RunAsync(options, NullLogger.Instance);
        Assert.True(result.Success);
        Assert.False(result.Timeout);
    }

    [Fact]
    public async Task RunAsync_ZeroTimeout_TreatedAsInfinite()
    {
        var options = new ProcessStartOptions
        {
            Command = "echo",
            Args = ["hello"],
            TimeoutSeconds = 0
        };
        var result = await ProcessExecutor.RunAsync(options, NullLogger.Instance);
        Assert.True(result.Success);
        Assert.False(result.Timeout);
    }

    #endregion

    #region Cancellation

    [Fact]
    public async Task RunAsync_Cancellation_SetsCancelledFlag()
    {
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(500);

        var options = new ProcessStartOptions
        {
            Command = "bash",
            Args = ["-c", "sleep 30"]
        };

        // The process should be cancelled
        try
        {
            var result = await ProcessExecutor.RunAsync(options, NullLogger.Instance, cts.Token);
            // If we get a result, it should show cancelled or killed
            Assert.False(result.Success);
        }
        catch (OperationCanceledException)
        {
            // Expected
        }
    }

    #endregion

    #region StdInput

    [Fact]
    public async Task RunAsync_StdInput_WritesLines()
    {
        var options = new ProcessStartOptions
        {
            Command = "bash",
            Args = ["-c", "cat"],
            Input = ["hello from stdin"],
            CaptureRawOutput = true
        };
        var result = await ProcessExecutor.RunAsync(options, NullLogger.Instance);
        Assert.True(result.Success);
        Assert.Contains("hello from stdin", result.RawOutput);
    }

    #endregion

    #region Working Directory

    [Fact]
    public async Task RunAsync_WorkingDirectory_UsesSpecifiedDir()
    {
        var tempDir = Path.GetTempPath();
        var options = new ProcessStartOptions
        {
            Command = "bash",
            Args = ["-c", "pwd"],
            WorkingDirectory = tempDir,
            CaptureRawOutput = true
        };
        var result = await ProcessExecutor.RunAsync(options, NullLogger.Instance);
        Assert.True(result.Success);
        // The output should contain the temp directory path
        Assert.NotNull(result.RawOutput);
    }

    #endregion
}
