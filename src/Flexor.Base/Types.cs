using System.Collections;
using Bicep.Local.Extension.Types.Attributes;

namespace Flexor;

// Name/Value base type
public record NamedValue
{ 
    [TypeProperty("The name of the value")]
    public string? Name { get; set; }
    [TypeProperty("The value")]
    public string? Value { get; set; }
}

// HTTP Headers
public record HttpHeader : NamedValue;

// Non-generic dictionary types
public class Any : Dictionary<string, object>, IDictionary;
public class OutputDictionary : Dictionary<string, List<Any>>;
