using Flexor.Executor;
using Flexor.Options;
using Flexor.Resources;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace Flexor.Tests;

internal class Helpers
{
    internal record DummyOutputResource : IOutputResource
    {
        public string Output { get; set; } = "{}";
    }

    public static void SetWorkingDirectory()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "../../../../../tests");
        var fullPath = new DirectoryInfo(path).FullName;
        Environment.CurrentDirectory = fullPath;
    }

    public static void WriteBicepParamsFile(string template, string filePath, Dictionary<string, object> parameters)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"using '{template}'");
        foreach (var param in parameters)
        {
            sb.AppendLine($"param {param.Key} = {System.Text.Json.JsonSerializer.Serialize(param.Value)}");
        }
        File.WriteAllText(filePath, sb.ToString());
    }

    public static async Task<Any> RunBicepLocalDeploy(string name, string paramsFile, ILogger logger, CancellationToken cancellation = default)
    {
        Console.WriteLine($"Current working directory: {Environment.CurrentDirectory}");
        Console.WriteLine($"Running bicep local deploy with params file: {paramsFile}");

        var result = await ProcessExecutor.RunAsync(
            new ProcessStartOptions
            {
                Command = "bicep",
                Args = [
                    "local-deploy",
                    paramsFile,
                    "--format",
                    "json"
                ],
                CaptureRawOutput = true
            },
            logger,
            cancellation
        );

        if (result.Success is false)
        {
            Console.WriteLine(result.RawOutput);
        }
        Assert.True(result.Success, $"Process failed with exit code {result.ExitCode}");

        var fullOutput = StripLineBreaksInJsonStrings(result.RawOutput ?? "");

        var doc = JsonNode.Parse(fullOutput);
        var outputs = doc?["outputs"]?.AsObject()
            ?? throw new Exception($"No 'outputs' property found in bicep output:\n{fullOutput}");

        var any = new Any();
        foreach (var prop in outputs)
        {
            any[prop.Key] = ResolveJsonValue(prop.Value);
        }
        return any;
    }

    static readonly Regex AnsiEscape = new(@"\x1b\[[\?]?[0-9;]*[A-Za-z]", RegexOptions.Compiled);

    static object? ResolveJsonValue(JsonNode? node) => node switch
    {
        JsonValue v when v.TryGetValue<bool>(out var b) => b,
        JsonValue v when v.TryGetValue<int>(out var i) => i,
        JsonValue v when v.TryGetValue<long>(out var l) => l,
        JsonValue v when v.TryGetValue<double>(out var d) => d,
        JsonValue v when v.TryGetValue<string>(out var s) => AnsiEscape.Replace(s, ""),
        JsonArray a => a.Select(ResolveJsonValue).ToArray(),
        JsonObject o => o.ToDictionary(p => p.Key, p => ResolveJsonValue(p.Value)),
        null => null,
        _ => node.ToJsonString()
    };

    /// <summary>
    /// Bicep CLI wraps long JSON string values at a column boundary, inserting
    /// literal CR/LF bytes inside the string. Strip them so System.Text.Json
    /// can parse the output.
    /// </summary>
    static string StripLineBreaksInJsonStrings(string json)
    {
        var sb = new StringBuilder(json.Length);
        bool inString = false;
        for (int i = 0; i < json.Length; i++)
        {
            char c = json[i];
            if (c == '"' && !IsEscaped(json, i))
            {
                inString = !inString;
                sb.Append(c);
            }
            else if (inString && (c == '\r' || c == '\n'))
            {
                // Drop unwanted line-wrap bytes
            }
            else
            {
                sb.Append(c);
            }
        }
        return sb.ToString();

        static bool IsEscaped(string s, int index)
        {
            int backslashes = 0;
            for (int i = index - 1; i >= 0 && s[i] == '\\'; i--)
                backslashes++;
            return backslashes % 2 == 1;
        }
    }

    public static Result<Any?> AsAny(string json)
    {
        return Result<Any>.From(() => JsonSerializer.Deserialize<Any>(json));
    }

    public static Result<Any[]?> AsAnyArray(string json)
    {
        return Result<Any[]>.From(() => JsonSerializer.Deserialize<Any[]>(json));
    }
}
