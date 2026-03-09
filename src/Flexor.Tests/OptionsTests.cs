using Flexor.Options;
using System.Runtime.InteropServices;
using System.Security;

namespace Flexor.Tests;

public class OptionsTests
{
    #region ProcessStartOptions Defaults

    [Fact]
    public void ProcessStartOptions_Defaults()
    {
        var options = new ProcessStartOptions();
        Assert.Null(options.Command);
        Assert.Empty(options.Args);
        Assert.Null(options.Env);
        Assert.Null(options.WorkingDirectory);
        Assert.Empty(options.Input);
        Assert.Null(options.StdOutputHandler);
        Assert.Null(options.StdErrorHandler);
        Assert.False(options.RunAsAdmin);
        Assert.Null(options.TimeoutSeconds);
        Assert.False(options.ContinueOnFailure);
        Assert.False(options.OverwriteEnvPaths);
        Assert.False(options.UseContainer);
        Assert.Null(options.ContainerImage);
        Assert.Null(options.ContainerCli);
        Assert.Null(options.ContainerCliArgs);
        Assert.Null(options.ContainerMounts);
        Assert.Null(options.WorkingPathMount);
        Assert.False(options.NoWait);
        Assert.Empty(options.AppendEnvVars);
        Assert.Null(options.FlexorPath);
        Assert.False(options.CaptureRawOutput);
    }

    #endregion

    #region ScriptRunOptions

    [Fact]
    public void ScriptRunOptions_InheritsProcessStartOptions()
    {
        var options = new ScriptRunOptions
        {
            Command = "bash",
            ShellType = ShellType.Bash,
            Path = "/scripts/test.sh"
        };
        Assert.Equal("bash", options.Command);
        Assert.Equal(ShellType.Bash, options.ShellType);
        Assert.Equal("/scripts/test.sh", options.Path);
    }

    [Fact]
    public void ScriptRunOptions_DefaultShellType()
    {
        var options = new ScriptRunOptions();
        Assert.Equal(ShellType.Default, options.ShellType);
        Assert.Null(options.Path);
    }

    #endregion

    #region ContainerStartOptions

    [Fact]
    public void ContainerStartOptions_RecordValues()
    {
        var mounts = new Dictionary<string, string> { ["/host"] = "/container" };
        var options = new ContainerStartOptions("podman", ["run", "--rm"], "ubuntu:latest", mounts);
        Assert.Equal("podman", options.Cli);
        Assert.Equal(["run", "--rm"], options.Args!);
        Assert.Equal("ubuntu:latest", options.Image);
        Assert.Single(options.Mounts!);
    }

    [Fact]
    public void ContainerStartOptions_NullValues()
    {
        var options = new ContainerStartOptions(null, null, null, null);
        Assert.Null(options.Cli);
        Assert.Null(options.Args);
        Assert.Null(options.Image);
        Assert.Null(options.Mounts);
    }

    #endregion

    #region AppendedVariable

    [Fact]
    public void AppendedVariable_RecordValues()
    {
        var v = new AppendedVariable("CLASSPATH", ":");
        Assert.Equal("CLASSPATH", v.Name);
        Assert.Equal(":", v.Delimiter);
    }

    #endregion

    #region FlexorOptions

    [Fact]
    public void FlexorOptions_DefaultFlexorPath()
    {
        var options = new FlexorOptions();
        Assert.NotNull(options.FlexorPath);
    }

    [Fact]
    public void FlexorOptions_LogPathDerivedFromFlexorPath()
    {
        var options = new FlexorOptions { FlexorPath = "/custom/path" };
        Assert.Contains("logs", options.LogPath);
    }

    [Fact]
    public void FlexorOptions_CustomLogPath()
    {
        var options = new FlexorOptions { LogPath = "/my/logs" };
        Assert.Equal("/my/logs", options.LogPath);
    }

    [Fact]
    public void FlexorOptions_DefaultLogOptions()
    {
        var options = new FlexorOptions();
        Assert.NotNull(options.LogOptions);
        Assert.True(options.LogOptions.Enabled);
        Assert.False(options.EnableTraceLogging);
    }

    #endregion

    #region FlexorLogOptions

    [Fact]
    public void FlexorLogOptions_Defaults()
    {
        var options = new FlexorLogOptions();
        Assert.True(options.Enabled);
        Assert.False(options.DisableRollover);
        Assert.False(options.Append);
        Assert.Equal(".log", options.FilenameExtension);
        Assert.Equal(".", options.FilenameSeparator);
        Assert.Equal(string.Empty, options.FilenameStdOutSegment);
        Assert.Equal("error", options.FilenameStdErrSegment);
        Assert.Equal("yyyyMMddHHmmss", options.FilenameTimestampFormat);
    }

    #endregion

    #region EnvOptions

    [Fact]
    public void EnvOptions_Defaults()
    {
        var options = new EnvOptions();
        Assert.False(options.OverwritePaths);
        Assert.NotNull(options.Append);
        Assert.Empty(options.Append);
    }

    #endregion

    #region ExecutionOptionsBase

    [Fact]
    public void ExecutionOptionsBase_Defaults()
    {
        var options = new ExecutionOptionsBase();
        Assert.False(options.UseContainer);
        Assert.Null(options.WorkingDirectory);
        Assert.Null(options.RunAsAdmin);
        Assert.Null(options.TimeoutSeconds);
        Assert.Null(options.ContinueOnFailure);
        Assert.Null(options.EnvOptions);
        Assert.Null(options.ContainerImage);
        Assert.Null(options.ContainerCli);
        Assert.Null(options.ContainerCliArgs);
        Assert.Null(options.ContainerMounts);
        Assert.Null(options.WorkingPathMount);
        Assert.False(options.NoWait);
    }

    #endregion

    #region Credential

    [Fact]
    public void Credential_StringPassword_ConvertsSecureString()
    {
        var securePassword = new SecureString();
        foreach (char c in "mypassword")
            securePassword.AppendChar(c);
        securePassword.MakeReadOnly();

        var credential = new Credential("user", securePassword);
        Assert.Equal("user", credential.Username);
        Assert.Equal("mypassword", credential.StringPassword);
    }

    [Fact]
    public void Credential_EmptyPassword()
    {
        var securePassword = new SecureString();
        securePassword.MakeReadOnly();

        var credential = new Credential("user", securePassword);
        Assert.Equal(string.Empty, credential.StringPassword);
    }

    #endregion

    #region ShellType Enum

    [Fact]
    public void ShellType_HasExpectedValues()
    {
        Assert.True(Enum.IsDefined(typeof(ShellType), ShellType.Default));
        Assert.True(Enum.IsDefined(typeof(ShellType), ShellType.Bash));
        Assert.True(Enum.IsDefined(typeof(ShellType), ShellType.PowerShell));
        Assert.True(Enum.IsDefined(typeof(ShellType), ShellType.Cmd));
        Assert.True(Enum.IsDefined(typeof(ShellType), ShellType.Python));
    }

    #endregion
}
