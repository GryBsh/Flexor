using System.Text.Json;

namespace Flexor.Tests;

/// <summary>
/// Unit tests for the test Helpers class methods (StripLineBreaksInJsonStrings, ResolveJsonValue, AsAny, AsAnyArray)
/// </summary>
public class HelpersUnitTests
{
    #region AsAny

    [Fact]
    public void AsAny_ValidJson_ReturnsSuccess()
    {
        var result = Helpers.AsAny("{\"key\":\"value\"}");
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("value", result.Value!["key"]?.ToString());
    }

    [Fact]
    public void AsAny_InvalidJson_ReturnsFailure()
    {
        var result = Helpers.AsAny("not json");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void AsAny_EmptyObject_ReturnsEmptyAny()
    {
        var result = Helpers.AsAny("{}");
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value!);
    }

    #endregion

    #region AsAnyArray

    [Fact]
    public void AsAnyArray_ValidArray_ReturnsSuccess()
    {
        var result = Helpers.AsAnyArray("[{\"a\":1},{\"b\":2}]");
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value!.Length);
    }

    [Fact]
    public void AsAnyArray_InvalidJson_ReturnsFailure()
    {
        var result = Helpers.AsAnyArray("not an array");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void AsAnyArray_EmptyArray_ReturnsEmpty()
    {
        var result = Helpers.AsAnyArray("[]");
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value!);
    }

    #endregion
}
