using System.Net;
using System.Net.Http;
using System.Text.Json;
using DeerFlow.WPF.Models;
using DeerFlow.WPF.Services;
using Xunit;

namespace DeerFlow.WPF.Tests.Services;

public class FeishuAdapterTests
{
    [Fact]
    public async Task SendMessageAsync_SendsCorrectJsonFormat()
    {
        var fakeHandler = new FakeHttpMessageHandler(HttpStatusCode.OK);
        var httpClient = new HttpClient(fakeHandler);
        var adapter = new FeishuAdapterTestClient(httpClient);
        var config = new IMConnection
        {
            WebhookUrl = "https://open.feishu.cn/open-apis/bot/v2/hook/test-key"
        };

        await adapter.ConnectAsync(config);

        fakeHandler.LastRequest = null;

        await adapter.SendMessageAsync("user_001", "Hello from test");

        Assert.NotNull(fakeHandler.LastRequest);
        var body = await fakeHandler.LastRequest!.Content!.ReadAsStringAsync();
        var json = JsonDocument.Parse(body);
        var root = json.RootElement;

        Assert.Equal("interactive", root.GetProperty("msg_type").GetString());
        Assert.True(root.GetProperty("card").TryGetProperty("header", out _));
        Assert.True(root.GetProperty("card").TryGetProperty("elements", out var elements));
        Assert.Equal(1, elements.GetArrayLength());
        Assert.Equal("markdown", elements[0].GetProperty("tag").GetString());

        adapter.Dispose();
    }

    [Fact]
    public async Task ConnectAsync_InvalidUrl_ReturnsFalse()
    {
        var adapter = new FeishuAdapter();
        var config = new IMConnection
        {
            WebhookUrl = "not-a-valid-url"
        };

        var result = await adapter.ConnectAsync(config);
        Assert.False(result);
        adapter.Dispose();
    }

    [Fact]
    public async Task SendMessageAsync_WithoutConnection_ReturnsFalse()
    {
        var fakeHandler = new FakeHttpMessageHandler(HttpStatusCode.OK);
        var httpClient = new HttpClient(fakeHandler);
        var adapter = new FeishuAdapterTestClient(httpClient);

        var result = await adapter.SendMessageAsync("user_001", "Test");
        Assert.False(result);
        adapter.Dispose();
    }
}

/// <summary>
/// 测试用适配器包装类，允许注入自定义 HttpClient，并防止基类 ConnectAsync 覆盖
/// </summary>
internal class FeishuAdapterTestClient : FeishuAdapter
{
    public FeishuAdapterTestClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public override Task<bool> ConnectAsync(IMConnection config)
    {
        _config = config;
        _cts = new CancellationTokenSource();
        return Task.FromResult(true);
    }
}
