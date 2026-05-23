using System.Text.Json;
using DeerFlow.WPF.Models;
using DeerFlow.WPF.Services;
using Moq;
using Xunit;

namespace DeerFlow.WPF.Tests.Services;

public class IMPlatformServiceTests
{
    private readonly Mock<IApiService> _apiServiceMock;
    private readonly Mock<ILoggerService> _loggerMock;

    public IMPlatformServiceTests()
    {
        _apiServiceMock = new Mock<IApiService>();
        _loggerMock = new Mock<ILoggerService>();
    }

    [Fact]
    public async Task AddConnection_AddsSuccessfully()
    {
        var service = new IMPlatformService(_apiServiceMock.Object, _loggerMock.Object);
        var connection = new IMConnection
        {
            DisplayName = "测试飞书群",
            PlatformType = PlatformType.Feishu,
            WebhookUrl = "https://open.feishu.cn/open-apis/bot/v2/hook/test",
            IsEnabled = false
        };

        service.AddConnection(connection);
        var connections = service.GetAllConnections();

        Assert.Single(connections);
        Assert.Equal("测试飞书群", connections[0].DisplayName);
        service.Dispose();
    }

    [Fact]
    public async Task RemoveConnection_RemovesSuccessfully()
    {
        var service = new IMPlatformService(_apiServiceMock.Object, _loggerMock.Object);
        var connection = new IMConnection
        {
            DisplayName = "待删除连接",
            PlatformType = PlatformType.WeCom,
            WebhookUrl = "https://qyapi.weixin.qq.com/cgi-bin/webhook/send?key=test"
        };

        service.AddConnection(connection);
        var id = connection.Id;

        service.RemoveConnection(id);
        var connections = service.GetAllConnections();

        Assert.Empty(connections);
        Assert.Null(service.GetConnection(id));
        service.Dispose();
    }

    [Fact]
    public async Task UpdateConnection_UpdatesSuccessfully()
    {
        var service = new IMPlatformService(_apiServiceMock.Object, _loggerMock.Object);
        var connection = new IMConnection
        {
            DisplayName = "原始名称",
            PlatformType = PlatformType.Feishu,
            WebhookUrl = "https://open.feishu.cn/old",
            IsEnabled = false
        };

        service.AddConnection(connection);
        var id = connection.Id;

        connection.DisplayName = "更新后名称";
        connection.WebhookUrl = "https://open.feishu.cn/new";
        service.UpdateConnection(connection);

        var updated = service.GetConnection(id);
        Assert.NotNull(updated);
        Assert.Equal("更新后名称", updated!.DisplayName);
        Assert.Equal("https://open.feishu.cn/new", updated.WebhookUrl);
        service.Dispose();
    }

    [Fact]
    public async Task AddConnection_Disabled_DoesNotConnect()
    {
        var service = new IMPlatformService(_apiServiceMock.Object, _loggerMock.Object);
        var connection = new IMConnection
        {
            DisplayName = "未启用连接",
            PlatformType = PlatformType.Feishu,
            WebhookUrl = "https://open.feishu.cn/test",
            IsEnabled = false
        };

        service.AddConnection(connection);
        var saved = service.GetConnection(connection.Id);

        Assert.NotNull(saved);
        Assert.Equal(ConnectionStatus.Disconnected, saved!.ConnectionStatus);
        service.Dispose();
    }

    [Fact]
    public async Task JsonRoundTrip_PreservesData()
    {
        var connection = new IMConnection
        {
            DisplayName = "序列化测试",
            PlatformType = PlatformType.WeCom,
            WebhookUrl = "https://qyapi.weixin.qq.com/test",
            Token = "test-token-123",
            IsEnabled = true
        };

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var json = JsonSerializer.Serialize(new List<IMConnection> { connection }, options);

        var deserialized = JsonSerializer.Deserialize<List<IMConnection>>(json, options);
        Assert.NotNull(deserialized);
        Assert.Single(deserialized!);
        Assert.Equal("序列化测试", deserialized![0].DisplayName);
        Assert.Equal(PlatformType.WeCom, deserialized[0].PlatformType);
        Assert.Equal("test-token-123", deserialized[0].Token);
    }
}
