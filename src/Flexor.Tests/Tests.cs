using Microsoft.Extensions.Logging.Abstractions;

namespace Flexor.Tests;

public class UnitTest1
{

    static string ListAssets()
    {
        // Match Pester's Get-AssetList: Get-ChildItem -Path "assets" | join with \n
        var entries = Directory.GetFileSystemEntries("./assets")
                               .Select(Path.GetFullPath)
                               .OrderBy(e => e)
                               .ToArray();
        return string.Join("\n", entries);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task CommandTest()
    {
        Helpers.SetWorkingDirectory();
        Helpers.WriteBicepParamsFile("commands.test.bicep", "test.bicepparam", []);
        var results = await Helpers.RunBicepLocalDeploy("Command","test.bicepparam", NullLogger.Instance);

        Assert.NotNull(results);
        Assert.Equal(0, results["bicepOutputLength"]);
        var expectedAssets = ListAssets().Split('\n').OrderBy(x => x).ToArray();
        var actualAssets = ((string)results["stringOutput"]!).Split('\n').OrderBy(x => x).ToArray();
        Assert.Equal(expectedAssets, actualAssets);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GitTest()
    {
        Helpers.SetWorkingDirectory();
        Helpers.WriteBicepParamsFile("git.test.bicep", "test.bicepparam", []);
        var results = await Helpers.RunBicepLocalDeploy("Git","test.bicepparam", NullLogger.Instance);
        Assert.NotNull(results);
        var outputPath = new DirectoryInfo("./output/Flexor/.git").FullName+Path.DirectorySeparatorChar;
        Assert.Equal(results["clonePath"], outputPath);
        Assert.Equal(results["pullPath"], outputPath);
    }

    [Fact]
    [Trait("Category", "Integration")]
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
    [Trait("Category", "Integration")]
    public async Task ScriptTest()
    {
        Helpers.SetWorkingDirectory();
        Helpers.WriteBicepParamsFile("scripts.test.bicep", "test.bicepparam", []);
        var results = await Helpers.RunBicepLocalDeploy("Script","test.bicepparam", NullLogger.Instance);
        Assert.NotNull(results);
        Assert.Equal(results["scriptLiteralOutput"], "Hello from script literal!");
        Assert.Equal(results["pwshResult"], "{\"Works\":true,\"EnvVar\":\"Set from Bicep\"}");
        Assert.Equal(results["pwshWorks"], true);

        Assert.Equal(results["bashResult"], "{\"Works\":true}");
        Assert.Equal(results["bashWorks"], true);

        Assert.Equal(results["pythonResult"], "{\"Works\":true,\"EnvVar\":\"Set from Bicep\"}");
        Assert.Equal(results["pythonWorks"], true);

        Assert.Equal(results["pythonFileResult"], "Works!");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task ContainerTest()
    {
        Helpers.SetWorkingDirectory();
        Helpers.WriteBicepParamsFile("containers.test.bicep", "test.bicepparam", []);
        var results = await Helpers.RunBicepLocalDeploy("Container","test.bicepparam", NullLogger.Instance);
        Assert.NotNull(results);
        Assert.Equal("Hello from script literal!", ((string)results["scriptLiteralOutput"]!).Trim());
        Assert.Equal("{\"Works\":true,\"EnvVar\":\"Set from Bicep\"}", ((string)results["pwshResult"]!).Trim());
        Assert.Equal(results["pwshWorks"], true);

        Assert.Equal("{\"Works\":true}", ((string)results["bashResult"]!).Trim());
        Assert.Equal(results["bashWorks"], true);

        Assert.Equal("{\"Works\":true,\"EnvVar\":\"Set from Bicep\"}", ((string)results["pythonResult"]!).Trim());
        Assert.Equal(results["pythonWorks"], true);

        Assert.Equal("Works!", ((string)results["pythonFileResult"]!).Trim());
    }

    [Fact]
    [Trait("Category", "Integration")]
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