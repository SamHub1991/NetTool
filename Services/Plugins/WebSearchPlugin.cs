using System.ComponentModel;
using System.Net.Http;
using System.Text.RegularExpressions;
using Microsoft.SemanticKernel;

namespace DeerFlow.WPF.Services.Plugins;

/// <summary>
/// 联网搜索插件，提供实时信息检索能力
/// </summary>
public sealed partial class WebSearchPlugin
{
    private readonly HttpClient _httpClient;
    private const int MaxResultLength = 3000;

    public WebSearchPlugin(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("WebSearch");
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
            "DeerFlow.WPF/1.0");
        _httpClient.Timeout = TimeSpan.FromSeconds(15);
    }

    /// <summary>
    /// 执行联网搜索，返回纯文本摘要
    /// </summary>
    /// <param name="query">搜索关键词</param>
    /// <returns>搜索结果摘要</returns>
    [KernelFunction("search_web")]
    [Description("使用 DuckDuckGo 进行联网搜索，返回文本摘要。当需要查找实时信息、最新资讯或不确定的知识时调用。")]
    public async Task<string> SearchWeb(
        [Description("搜索关键词，越具体越好")]
        string query)
    {
        try
        {
            var encodedQuery = Uri.EscapeDataString(query);
            var url = $"https://html.duckduckgo.com/html/?q={encodedQuery}";
            var html = await _httpClient.GetStringAsync(url);
            return ExtractText(html);
        }
        catch (TaskCanceledException)
        {
            return "搜索超时，请稍后重试";
        }
        catch (HttpRequestException ex)
        {
            return $"搜索请求失败: {ex.Message}";
        }
    }

    /// <summary>
    /// 从 HTML 中提取纯文本摘要
    /// </summary>
    private static string ExtractText(string html)
    {
        var text = HtmlTagPattern().Replace(html, " ");
        text = WhitespacePattern().Replace(text, " ");
        text = text.Trim();

        if (text.Length > MaxResultLength)
        {
            text = text[..MaxResultLength] + "...";
        }

        return text;
    }

    [GeneratedRegex("<[^>]+>")]
    private static partial Regex HtmlTagPattern();

    [GeneratedRegex("\\s+")]
    private static partial Regex WhitespacePattern();
}
