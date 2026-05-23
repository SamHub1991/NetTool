using System.Net.Http;
using System.Text;
using System.Text.Json;
using DeerFlow.WPF.Models;

namespace DeerFlow.WPF.Services;

/// <summary>
/// 企业微信平台适配器，实现企业微信群机器人回调协议
/// </summary>
public class WeComAdapter : IMAdapterBase
{
    public override string PlatformName => "企业微信";

    /// <inheritdoc/>
    public override async Task<bool> ConnectAsync(IMConnection config)
    {
        _config = config;
        _cts = new CancellationTokenSource();

        try
        {
            var isValid = Uri.TryCreate(config.WebhookUrl, UriKind.Absolute, out var uri)
                && (uri.Scheme == "http" || uri.Scheme == "https")
                && uri.Host.Contains("qyapi.weixin.qq.com");

            if (!isValid)
                return false;

            _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            var request = new HttpRequestMessage(HttpMethod.Head, config.WebhookUrl);
            var response = await _httpClient.SendAsync(request, _cts.Token);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public override async Task<bool> SendMessageAsync(string recipientId, string content)
    {
        if (_httpClient == null || _config == null)
            return false;

        var body = new
        {
            msgtype = "markdown",
            markdown = new
            {
                content = $"## DeerFlow AI 回复\n\n{TruncateContent(content)}"
            }
        };

        var json = JsonSerializer.Serialize(body);
        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync(_config.WebhookUrl, httpContent, _cts?.Token ?? default);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 截断过长消息，企业微信有 4096 字符限制
    /// </summary>
    private static string TruncateContent(string content)
    {
        const int MAX_CHARS = 3900;
        if (content.Length <= MAX_CHARS)
            return content;

        return content[..MAX_CHARS] + "\n\n...(内容已截断)";
    }
}
