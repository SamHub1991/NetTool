using System.Net;
using System.Net.Http;
using DeerFlow.WPF.Services.Plugins;
using Moq;
using Xunit;

namespace DeerFlow.WPF.Tests.Services;

public class WebSearchPluginTests
{
    /// <summary>
    /// 创建带 Fake HttpMessageHandler 的 WebSearchPlugin 实例
    /// </summary>
    private static WebSearchPlugin CreatePlugin(HttpMessageHandler handler)
    {
        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://duckduckgo.com")
        };
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(x => x.CreateClient("WebSearch")).Returns(client);

        return new WebSearchPlugin(factoryMock.Object);
    }

    [Fact]
    public async Task SearchWeb_ReturnsTextSummary()
    {
        var handler = new FakeHttpMessageHandler(_ =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("<html><body><div>搜索结果摘要</div><p>更多内容</p></body></html>")
            }));
        var plugin = CreatePlugin(handler);

        var result = await plugin.SearchWeb("测试搜索词");

        Assert.Contains("搜索结果摘要", result);
        Assert.Contains("更多内容", result);
    }

    [Fact]
    public async Task SearchWeb_TruncatesLongResults()
    {
        var longContent = new string('A', 5000);
        var handler = new FakeHttpMessageHandler(_ =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent($"<html><body><p>{longContent}</p></body></html>")
            }));
        var plugin = CreatePlugin(handler);

        var result = await plugin.SearchWeb("长文本查询");

        Assert.True(result.Length <= 3003, $"实际长度: {result.Length}");
        Assert.EndsWith("...", result);
    }

    [Fact]
    public async Task SearchWeb_NetworkError_ReturnsErrorMessage()
    {
        var handler = new FakeHttpMessageHandler(_ =>
            Task.FromException<HttpResponseMessage>(new HttpRequestException("网络连接失败")));
        var plugin = CreatePlugin(handler);

        var result = await plugin.SearchWeb("error test");

        Assert.Contains("请求失败", result);
    }

    [Fact]
    public async Task SearchWeb_EmptyHtml_DoesNotThrow()
    {
        var handler = new FakeHttpMessageHandler(_ =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(string.Empty)
            }));
        var plugin = CreatePlugin(handler);

        var result = await plugin.SearchWeb("empty test");

        Assert.NotNull(result);
    }

    /// <summary>
    /// 可配置的 Fake HttpMessageHandler，根据请求返回自定义响应
    /// </summary>
    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, Task<HttpResponseMessage>> _handler;

        public FakeHttpMessageHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> handler)
        {
            _handler = handler;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return await _handler(request);
        }
    }
}
