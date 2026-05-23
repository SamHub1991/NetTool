using System;
using System.Threading;
using System.Threading.Tasks;
using DeerFlow.WPF.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DeerFlow.WPF.Tests.Services;

/// <summary>
/// OpenSandboxService 单元测试
/// 由于 OpenSandbox SDK 依赖于真实的外部 API，这些测试主要验证服务接口和状态管理逻辑
/// </summary>
public class OpenSandboxServiceTests
{
    private readonly Mock<ILoggerFactory> _loggerFactoryMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly IOpenSandboxService _sandboxService;

    public OpenSandboxServiceTests()
    {
        _loggerMock = new Mock<ILogger>();
        _loggerFactoryMock = new Mock<ILoggerFactory>();
        _loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>()))
            .Returns(_loggerMock.Object);

        _sandboxService = new OpenSandboxService(_loggerFactoryMock.Object);
    }

    [Fact]
    public void InitialStatus_ShouldBe_NotConnected()
    {
        // 验证初始状态
        Assert.False(_sandboxService.IsConnected);
        Assert.Equal("未连接", _sandboxService.Status);
        Assert.Equal(string.Empty, _sandboxService.SandboxId);
    }

    [Fact]
    public async Task ConfigureAsync_ShouldUpdateStatus()
    {
        // 配置服务
        await _sandboxService.ConfigureAsync("test-api-key", "api.opensandbox.io");

        // 验证配置后的状态
        Assert.Equal("已配置", _sandboxService.Status);
        Assert.False(_sandboxService.IsConnected); // 配置但未创建沙箱
    }

    [Fact]
    public async Task CreateSandboxAsync_WithoutConfigure_ShouldThrowException()
    {
        // 未配置直接创建应抛出异常
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sandboxService.CreateSandboxAsync("ubuntu", 600));

        Assert.Contains("请先调用 ConfigureAsync 配置连接", ex.Message);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WithoutSandbox_ShouldThrowException()
    {
        // 未创建沙箱执行命令应抛出异常
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sandboxService.ExecuteCommandAsync("echo test"));

        Assert.Contains("沙箱未创建", ex.Message);
    }

    [Fact]
    public async Task ExecuteCodeAsync_WithoutSandbox_ShouldThrowException()
    {
        // 未创建沙箱执行代码应抛出异常
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sandboxService.ExecuteCodeAsync("print('hello')", SandboxLanguage.Python));

        Assert.Contains("沙箱未创建", ex.Message);
    }

    [Fact]
    public async Task ReadFileAsync_WithoutSandbox_ShouldThrowException()
    {
        // 未创建沙箱读取文件应抛出异常
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sandboxService.ReadFileAsync("/test.txt"));

        Assert.Contains("沙箱未创建", ex.Message);
    }

    [Fact]
    public async Task WriteFileAsync_WithoutSandbox_ShouldThrowException()
    {
        // 未创建沙箱写入文件应抛出异常
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sandboxService.WriteFileAsync("/test.txt", "content"));

        Assert.Contains("沙箱未创建", ex.Message);
    }

    [Fact]
    public async Task GetInfoAsync_WithoutSandbox_ShouldThrowException()
    {
        // 未创建沙箱获取信息应抛出异常
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sandboxService.GetInfoAsync());

        Assert.Contains("沙箱未创建", ex.Message);
    }

    [Fact]
    public async Task RenewAsync_WithoutSandbox_ShouldThrowException()
    {
        // 未创建沙箱续期应抛出异常
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sandboxService.RenewAsync(3600));

        Assert.Contains("沙箱未创建", ex.Message);
    }

    [Fact]
    public void Dispose_ShouldNotThrowException()
    {
        // 验证在未初始化状态下 Dispose 不抛出异常
        _sandboxService.Dispose();
        Assert.False(_sandboxService.IsConnected);
    }

    [Fact]
    public async Task DestroyAsync_WithoutSandbox_ShouldNotThrowException()
    {
        // 验证销毁不存在的沙箱不抛出异常
        await _sandboxService.DestroyAsync();
        Assert.Equal("未连接", _sandboxService.Status);
    }

    [Fact]
    public void SandboxLanguage_Enum_ShouldHaveAllExpectedValues()
    {
        // 验证 SandboxLanguage 枚举包含所有预期值
        var languages = Enum.GetValues<SandboxLanguage>();

        Assert.Contains(SandboxLanguage.Python, languages);
        Assert.Contains(SandboxLanguage.JavaScript, languages);
        Assert.Contains(SandboxLanguage.TypeScript, languages);
        Assert.Contains(SandboxLanguage.Go, languages);
        Assert.Contains(SandboxLanguage.Java, languages);
        Assert.Contains(SandboxLanguage.Bash, languages);

        Assert.Equal(6, languages.Length);
    }
}
