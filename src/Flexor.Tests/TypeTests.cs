namespace Flexor.Tests;

public class TypeTests
{
    #region Any

    [Fact]
    public void Any_IsDictionary()
    {
        var any = new Any();
        any["key"] = "value";
        Assert.Single(any);
        Assert.Equal("value", any["key"]);
    }

    [Fact]
    public void Any_SupportsNullValues()
    {
        var any = new Any { ["nullable"] = null };
        Assert.Null(any["nullable"]);
    }

    [Fact]
    public void Any_SupportsNestedTypes()
    {
        var inner = new Any { ["nested"] = true };
        var outer = new Any { ["child"] = inner };
        Assert.IsType<Any>(outer["child"]);
    }

    [Fact]
    public void Any_SupportsMixedTypes()
    {
        var any = new Any
        {
            ["string"] = "hello",
            ["int"] = 42,
            ["bool"] = true,
            ["null"] = null,
            ["array"] = new[] { 1, 2, 3 }
        };
        Assert.Equal(5, any.Count);
    }

    #endregion

    #region OutputDictionary

    [Fact]
    public void OutputDictionary_Constants()
    {
        Assert.Equal("", OutputDictionary.StringOutputKey);
        Assert.Equal("[]", OutputDictionary.ArrayOutputKey);
    }

    [Fact]
    public void OutputDictionary_StoresListsOfAny()
    {
        var dict = new OutputDictionary();
        dict["step1"] = [new Any { ["result"] = "ok" }];
        Assert.Single(dict);
        Assert.Single(dict["step1"]);
    }

    #endregion
}
