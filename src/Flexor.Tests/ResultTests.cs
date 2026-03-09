namespace Flexor.Tests;

public class ResultTests
{
    [Fact]
    public void Success_SetsIsSuccessTrue()
    {
        var result = Result<int>.Success(42);
        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void Failure_SetsIsSuccessFalse()
    {
        var result = Result<int>.Failure("something went wrong");
        Assert.False(result.IsSuccess);
        Assert.Equal(default, result.Value);
        Assert.Equal("something went wrong", result.ErrorMessage);
    }

    [Fact]
    public void From_SuccessfulFunc_ReturnsSuccess()
    {
        var result = Result<string>.From(() => "hello");
        Assert.True(result.IsSuccess);
        Assert.Equal("hello", result.Value);
    }

    [Fact]
    public void From_ThrowingFunc_ReturnsFailure()
    {
        var result = Result<string>.From(() => throw new InvalidOperationException("test error"));
        Assert.False(result.IsSuccess);
        Assert.Equal("test error", result.ErrorMessage);
    }

    [Fact]
    public void From_NullResult_ReturnsSuccessWithNull()
    {
        var result = Result<string>.From(() => null);
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
    }

    [Fact]
    public void Success_NullValue_IsValid()
    {
        var result = Result<object>.Success(null);
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
    }
}
