using Flexor.Options;

namespace Flexor.Tests;

public class JsonTypeTests
{
    [Fact]
    public void Of_JsonObject_DetectsObject()
    {
        var (_, isObject, isArray, _) = JsonType.Of("{\"key\":\"value\"}");
        Assert.True(isObject);
        Assert.False(isArray);
    }

    [Fact]
    public void Of_JsonArray_DetectsArray()
    {
        var (_, isObject, isArray, _) = JsonType.Of("[1,2,3]");
        Assert.False(isObject);
        Assert.True(isArray);
    }

    [Fact]
    public void Of_PlainString_DetectsNeither()
    {
        var (_, isObject, isArray, _) = JsonType.Of("hello world");
        Assert.False(isObject);
        Assert.False(isArray);
    }

    [Fact]
    public void Of_EmptyString_DetectsNeither()
    {
        var (_, isObject, isArray, _) = JsonType.Of("");
        Assert.False(isObject);
        Assert.False(isArray);
    }

    [Fact]
    public void Of_EmptyObject_DetectsObject()
    {
        var (_, isObject, isArray, _) = JsonType.Of("{}");
        Assert.True(isObject);
        Assert.False(isArray);
    }

    [Fact]
    public void Of_EmptyArray_DetectsArray()
    {
        var (_, isObject, isArray, _) = JsonType.Of("[]");
        Assert.False(isObject);
        Assert.True(isArray);
    }

    [Fact]
    public void Of_NestedJson_DetectsObject()
    {
        var (_, isObject, _, _) = JsonType.Of("{\"nested\":{\"deep\":true}}");
        Assert.True(isObject);
    }

    [Fact]
    public void Of_WhitespaceAroundJson_DetectsObject()
    {
        var (_, isObject, _, _) = JsonType.Of("  { \"key\": 1 }  ");
        Assert.True(isObject);
    }

    [Fact]
    public void Of_InvalidJson_DetectsNeither()
    {
        var result = JsonType.Of("{not valid json}");
        Assert.False(result.IsObject);
        Assert.False(result.IsArray);
        Assert.False(result.IsJson);
    }

    [Fact]
    public void Of_Number_IsScalar()
    {
        var result = JsonType.Of("42");
        Assert.True(result.IsJson);
        Assert.True(result.IsScalar);
        Assert.False(result.IsObject);
        Assert.False(result.IsArray);
    }

    [Fact]
    public void Of_BooleanTrue_IsScalar()
    {
        var result = JsonType.Of("true");
        Assert.True(result.IsJson);
        Assert.True(result.IsScalar);
        Assert.False(result.IsObject);
        Assert.False(result.IsArray);
    }

    [Fact]
    public void Of_NullLiteral_NotDetectedAsJson()
    {
        // JsonSerializer.Deserialize<JsonNode>("null") returns null,
        // so node is { } evaluates to false
        var result = JsonType.Of("null");
        Assert.False(result.IsJson);
    }
}
