using Flexor.Executor;
using Flexor.Options;
using Flexor.Resources;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

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

        var resource = new DummyOutputResource();
        var outputs = new OutputDictionary();
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
                StdOutputHandler = (line) =>
                {
                    ProcessExecutor.ParseOutput(outputs, name, line);                    
                },                                                                          
            },  
            logger,
            cancellation
        );
        if (result.Success is false)
        {
            Console.WriteLine(JsonSerializer.Serialize(outputs, new JsonSerializerOptions { WriteIndented = true }));
        }
        Assert.True(result.Success, $"Process failed with exit code {result.ExitCode}");
        var output = ProcessExecutor.ResolveOutput(name, outputs);
        
        if (output is Any any)
        {
            return any;
        }
        
        throw new Exception($"Output is not of type Any. Actual type: {output?.GetType().FullName ?? "null"}");
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
