using System.Net;
using System.Net.Http;

namespace DeerFlow.WPF.Tests.Services;

/// <summary>
/// 可捕获请求内容的假 HttpMessageHandler，用于适配器单元测试
/// </summary>
public class FakeHttpMessageHandler : HttpMessageHandler
{
    /// <summary>最近一次捕获的请求</summary>
    public HttpRequestMessage? LastRequest { get; set; }

    private readonly HttpStatusCode _statusCode;

    public FakeHttpMessageHandler(HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        _statusCode = statusCode;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastRequest = request;
        return Task.FromResult(new HttpResponseMessage(_statusCode));
    }
}
