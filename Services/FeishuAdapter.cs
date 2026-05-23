using System.Net.Http;
using System.Text;
using System.Text.Json;
using DeerFlow.WPF.Models;

namespace DeerFlow.WPF.Services;

/// <summary>
/// 飞书平台适配器，实现飞书自定义机器人 Webhook 协议
/// </summary>
public class FeishuAdapter : IMAdapterBase
{
    public override string PlatformName => "飞书";

    /// <inheritdoc/>
    public override async Task<bool> ConnectAsync(IMConnection config)
    {
        _config = config;
        _cts = new CancellationTokenSource();

        try
        {
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
            msg_type = "interactive",
            card = new
            {
                header = new
                {
                    title = new { tag = "plain_text", content = "DeerFlow AI 回复" },
                    template = "blue"
                },
                elements = new[]
                {
                    new
                    {
                        tag = "markdown",
                        content = TruncateContent(content)
                    }
                }
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
    /// 截断过长消息，飞书卡片有字符数限制
    /// </summary>
    private static string TruncateContent(string content)
    {
        const int MAX_CHARS = 3000;
        if (content.Length <= MAX_CHARS)
            return content;

        return content[..MAX_CHARS] + "\n\n...(内容已截断)";
    }
}
