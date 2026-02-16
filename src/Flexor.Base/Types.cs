using Bicep.Local.Extension.Types.Attributes;
using Flexor.Options;
using System.Collections;
using System.Text.Json.Nodes;

namespace Flexor;

// Name/Value base type
public record NamedValue
{ 
    [TypeProperty("The name of this item")]
    public string? Name { get; set; }
    [TypeProperty("The value of this item")]
    public string? Value { get; set; }
}

// HTTP Headers
public record HttpHeader : NamedValue;

public record HttpQuery : NamedValue;

// Non-generic dictionary types

/// <summary>
/// A dictionary that represents a JavaScript `any` type object.
/// </summary>
public class Any : Dictionary<string, object?>, IDictionary;

public class OutputDictionary : Dictionary<string, List<Any>>
{
    public const string StringOutputKey = "";
    public const string ArrayOutputKey = "[]";
}
