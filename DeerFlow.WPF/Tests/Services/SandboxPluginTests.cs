using DeerFlow.WPF.Services;
using DeerFlow.WPF.Services.Plugins;
using Moq;
using Xunit;

namespace DeerFlow.WPF.Tests.Services;

public class SandboxPluginTests
{
    private readonly Mock<ISandboxManager> _sandboxMock;
    private readonly SandboxPlugin _plugin;

    public SandboxPluginTests()
    {
        _sandboxMock = new Mock<ISandboxManager>();
        _plugin = new SandboxPlugin(_sandboxMock.Object);
    }

    [Fact]
    public async Task CreateSandbox_ReturnsPath()
    {
        const string taskId = "task-001";
        const string expected = @"C:\sandboxes\task-001";
        _sandboxMock.Setup(x => x.CreateSandboxAsync(taskId)).ReturnsAsync(expected);

        var result = await _plugin.CreateSandbox(taskId);

        Assert.Equal(expected, result);
        _sandboxMock.Verify(x => x.CreateSandboxAsync(taskId), Times.Once);
    }

    [Fact]
    public async Task DestroySandbox_ReturnsConfirmation()
    {
        const string taskId = "task-001";
        _sandboxMock.Setup(x => x.DestroySandboxAsync(taskId)).Returns(Task.CompletedTask);

        var result = await _plugin.DestroySandbox(taskId);

        Assert.Contains(taskId, result);
        _sandboxMock.Verify(x => x.DestroySandboxAsync(taskId), Times.Once);
    }

    [Fact]
    public async Task ListFiles_ReturnsFileList()
    {
        const string taskId = "task-001";
        var files = new List<string> { "a.txt", "b.cs", "c.json" };
        _sandboxMock.Setup(x => x.GetSandboxFilesAsync(taskId)).ReturnsAsync(files);

        var result = await _plugin.ListFiles(taskId);

        Assert.Contains("a.txt", result);
        Assert.Contains("b.cs", result);
        Assert.Contains("c.json", result);
        _sandboxMock.Verify(x => x.GetSandboxFilesAsync(taskId), Times.Once);
    }

    [Fact]
    public async Task ListFiles_EmptySandbox_ReturnsEmptyMessage()
    {
        const string taskId = "task-empty";
        _sandboxMock.Setup(x => x.GetSandboxFilesAsync(taskId))
            .ReturnsAsync(new List<string>());

        var result = await _plugin.ListFiles(taskId);

        Assert.Contains("沙箱为空", result);
    }

    [Fact]
    public async Task ExecuteCommand_ReturnsResult()
    {
        const string taskId = "task-001";
        const string command = "echo hello";
        const string expected = "hello";
        _sandboxMock.Setup(x => x.ExecuteInSandboxAsync(taskId, command))
            .ReturnsAsync(expected);

        var result = await _plugin.ExecuteCommand(taskId, command);

        Assert.Equal(expected, result);
        _sandboxMock.Verify(x => x.ExecuteInSandboxAsync(taskId, command), Times.Once);
    }
}
