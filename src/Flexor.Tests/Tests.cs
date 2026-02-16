using Microsoft.Extensions.Logging.Abstractions;

namespace Flexor.Tests;

public class UnitTest1
{

    static string[] ListAssets() 
        => Directory.GetFiles(
            "./tests/assets", 
            "*", 
            new EnumerationOptions { RecurseSubdirectories = true }
        );

    [Fact]
    public async Task CommandTest()
    {
        Helpers.SetWorkingDirectory();
        Helpers.WriteBicepParamsFile("commands.test.bicep", "test.bicepparam", []);
        var results = await Helpers.RunBicepLocalDeploy("Command","test.bicepparam", NullLogger.Instance);

        Assert.NotNull(results);
        Assert.Equal(results["bicepOutputLength"], 0);
        Assert.Equal(results["stringOutput"], ListAssets());
    }

    [Fact]
    public async Task GitTest()
    {
        Helpers.SetWorkingDirectory();
        Helpers.WriteBicepParamsFile("commands.test.bicep", "test.bicepparam", []);
        var results = await Helpers.RunBicepLocalDeploy("Git","test.bicepparam", NullLogger.Instance);
        Assert.NotNull(results);
        var outputPath = new DirectoryInfo("./output/Flexor/.git").FullName+Path.DirectorySeparatorChar;
        Assert.Equal(results["clonePath"], outputPath);
        Assert.Equal(results["pullPath"], outputPath);
    }

    [Fact]
    public async Task HttpTest()
    {
        Helpers.SetWorkingDirectory();
        Helpers.WriteBicepParamsFile("http.test.bicep", "test.bicepparam", []);
        var results = await Helpers.RunBicepLocalDeploy("Http","test.bicepparam", NullLogger.Instance);
        Assert.NotNull(results);
        Assert.Equal(results["getStatusCode"], 200);
        Assert.Equal(results["postStatusCode"], 200);
    }

    [Fact]
    public async Task ScriptTest()
    {
        Helpers.SetWorkingDirectory();
        Helpers.WriteBicepParamsFile("scripts.test.bicep", "test.bicepparam", []);
        var results = await Helpers.RunBicepLocalDeploy("Script","test.bicepparam", NullLogger.Instance);
        Assert.NotNull(results);
        Assert.Equal(results["scriptLiteralOutput"], "Hello from script literal!");
        Assert.Equal(results["pwshResult"], "{\"Works\":true,\"EnvVar\":\"Set from Bicep\"}");
        Assert.Equal(results["pwshWorks"], true);

        Assert.Equal(results["bashResult"], "{\"Works\":true,\"EnvVar\":\"Set from Bicep\"}");
        Assert.Equal(results["bashWorks"], true);

        Assert.Equal(results["pythonResult"], "{\"Works\":true,\"EnvVar\":\"Set from Bicep\"}");
        Assert.Equal(results["pythonWorks"], true);

        Assert.Equal(results["puthFileResusult"], "Works!");
    }

    [Fact]
    public async Task ContainerTest()
    {
        Helpers.SetWorkingDirectory();
        Helpers.WriteBicepParamsFile("containers.test.bicep", "test.bicepparam", []);
        var results = await Helpers.RunBicepLocalDeploy("Container","test.bicepparam", NullLogger.Instance);
        Assert.NotNull(results);
        Assert.Equal(results["scriptLiteralOutput"], "Hello from script literal!\n");
        Assert.Equal(results["pwshResult"], "{\"Works\":true,\"EnvVar\":\"Set from Bicep\"}\n");
        Assert.Equal(results["pwshWorks"], true);

        Assert.Equal(results["bashResult"], "{\"Works\":true,\"EnvVar\":\"Set from Bicep\"}\n");
        Assert.Equal(results["bashWorks"], true);

        Assert.Equal(results["pythonResult"], "{\"Works\":true,\"EnvVar\":\"Set from Bicep\"}\n");
        Assert.Equal(results["pythonWorks"], true);

        Assert.Equal(results["puthFileResusult"], "Works!\n");
    }

    [Fact]
    public async Task ModulesTest()
    {
        Helpers.SetWorkingDirectory();
        Helpers.WriteBicepParamsFile("modules.test.bicep", "test.bicepparam", []);
        var results = await Helpers.RunBicepLocalDeploy("Modules","test.bicepparam", NullLogger.Instance);
        Assert.NotNull(results);
        Assert.Equal(results["createdUsername"], "tuser");
        Assert.Equal(results["createdEmail"], "tuser@org.local");
        Assert.Equal(results["createdName"], "Test User");

        Assert.Equal(results["existingUsername"], "euser");
        Assert.Equal(results["existingEmail"], "euser@org.local");
        Assert.Equal(results["existingName"], "Existing User");
    }

}