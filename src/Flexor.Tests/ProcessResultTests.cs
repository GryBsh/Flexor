using Flexor.Executor;

namespace Flexor.Tests;

public class ProcessResultTests
{
    [Fact]
    public void ProcessResult_SuccessRecord_HasCorrectValues()
    {
        var result = new ProcessResult(true, 0, false, false, false);
        Assert.True(result.Success);
        Assert.Equal(0, result.ExitCode);
        Assert.False(result.Timeout);
        Assert.False(result.Cancelled);
        Assert.False(result.Killed);
        Assert.Null(result.RawOutput);
    }

    [Fact]
    public void ProcessResult_FailureRecord_HasCorrectValues()
    {
        var result = new ProcessResult(false, 1, true, false, true);
        Assert.False(result.Success);
        Assert.Equal(1, result.ExitCode);
        Assert.True(result.Timeout);
        Assert.False(result.Cancelled);
        Assert.True(result.Killed);
    }

    [Fact]
    public void ProcessResult_RawOutput_InitOnly()
    {
        var result = new ProcessResult(true, 0, false, false, false)
        {
            RawOutput = "captured output"
        };
        Assert.Equal("captured output", result.RawOutput);
    }

    [Fact]
    public void ProcessResult_Deconstruct_Works()
    {
        var result = new ProcessResult(true, 0, false, true, false);
        var (success, exitCode, timeout, cancelled, killed) = result;
        Assert.True(success);
        Assert.Equal(0, exitCode);
        Assert.False(timeout);
        Assert.True(cancelled);
        Assert.False(killed);
    }

    [Fact]
    public void ProcessResult_RecordEquality_Works()
    {
        var a = new ProcessResult(true, 0, false, false, false);
        var b = new ProcessResult(true, 0, false, false, false);
        Assert.Equal(a, b);
    }

    [Fact]
    public void ProcessResult_RecordInequality_DifferentExitCode()
    {
        var a = new ProcessResult(true, 0, false, false, false);
        var b = new ProcessResult(false, 1, false, false, false);
        Assert.NotEqual(a, b);
    }
}
