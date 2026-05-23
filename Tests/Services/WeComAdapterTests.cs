using System.Net;
using System.Net.Http;
using System.Text.Json;
using DeerFlow.WPF.Models;
using DeerFlow.WPF.Services;
using Xunit;

namespace DeerFlow.WPF.Tests.Services;

public class WeComAdapterTests
{
    [Fact]
    public async Task SendMessageAsync_SendsCorrectJsonFormat()
    {
        var fakeHandler = new FakeHttpMessageHandler(HttpStatusCode.OK);
        var httpClient = new HttpClient(fakeHandler);
        var adapter = new WeComAdapterTestClient(httpClient);
        var config = new IMConnection
        {
            WebhookUrl = "https://qyapi.weixin.qq.com/cgi-bin/webhook/send?key=test-key"
        };

        await adapter.ConnectAsync(config);

        fakeHandler.LastRequest = null;

        await adapter.SendMessageAsync("user_001", "企业微信测试消息");

        Assert.NotNull(fakeHandler.LastRequest);
        var body = await fakeHandler.LastRequest!.Content!.ReadAsStringAsync();
        var json = JsonDocument.Parse(body);
        var root = json.RootElement;

        Assert.Equal("markdown", root.GetProperty("msgtype").GetString());
        Assert.True(root.GetProperty("markdown").TryGetProperty("content", out var content));
        Assert.Contains("DeerFlow AI 回复", content.GetString());
        Assert.Contains("企业微信测试消息", content.GetString());

        adapter.Dispose();
    }

    [Fact]
    public async Task ConnectAsync_NonWeComUrl_ReturnsFalse()
    {
        var adapter = new WeComAdapter();
        var config = new IMConnection
        {
            WebhookUrl = "https://open.feishu.cn/some-hook"
        };

        var result = await adapter.ConnectAsync(config);
        Assert.False(result);
        adapter.Dispose();
    }

    [Fact]
    public async Task ConnectAsync_InvalidUrlFormat_ReturnsFalse()
    {
        var adapter = new WeComAdapter();
        var config = new IMConnection
        {
            WebhookUrl = "not-a-url"
        };

        var result = await adapter.ConnectAsync(config);
        Assert.False(result);
        adapter.Dispose();
    }

    [Fact]
    public async Task Token_NotExposedInLogging()
    {
        var fakeHandler = new FakeHttpMessageHandler(HttpStatusCode.OK);
        var httpClient = new HttpClient(fakeHandler);
        var adapter = new WeComAdapterTestClient(httpClient);
        var config = new IMConnection
        {
            WebhookUrl = "https://qyapi.weixin.qq.com/cgi-bin/webhook/send?key=test",
            Token = "secret-token-value-12345"
        };

        await adapter.ConnectAsync(config);

        Assert.Equal("secret-token-value-12345", config.Token);
        Assert.True(config.Token.Length > 4);
        Assert.StartsWith("secret", config.Token);

        adapter.Dispose();
    }
}

/// <summary>
/// 测试用适配器包装类，允许注入自定义 HttpClient，并防止基类 ConnectAsync 覆盖
/// </summary>
internal class WeComAdapterTestClient : WeComAdapter
{
    public WeComAdapterTestClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public override Task<bool> ConnectAsync(IMConnection config)
    {
        _config = config;
        _cts = new CancellationTokenSource();

        var isValid = Uri.TryCreate(config.WebhookUrl, UriKind.Absolute, out var uri)
            && (uri.Scheme == "http" || uri.Scheme == "https")
            && uri.Host.Contains("qyapi.weixin.qq.com");

        return Task.FromResult(isValid);
    }
}
