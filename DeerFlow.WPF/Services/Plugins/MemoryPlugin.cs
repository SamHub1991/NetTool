using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace DeerFlow.WPF.Services.Plugins;

/// <summary>
/// 记忆插件，提供对话上下文存储和检索能力
/// </summary>
public sealed class MemoryPlugin
{
    private readonly Dictionary<string, List<string>> _sessions = new();
    private const string DefaultSession = "default";

    /// <summary>
    /// 记录一条重要信息到当前会话记忆
    /// </summary>
    /// <param name="information">要记住的信息</param>
    /// <param name="sessionId">会话标识（可选）</param>
    /// <returns>确认消息</returns>
    [KernelFunction("remember")]
    [Description("将重要信息存入对话记忆，后续对话可引用")]
    public string Remember(
        [Description("要记住的信息内容")]
        string information,
        [Description("会话 ID，默认为 default")]
        string sessionId = DefaultSession)
    {
        if (!_sessions.TryGetValue(sessionId, out var list))
        {
            list = [];
            _sessions[sessionId] = list;
        }

        list.Add(information);

        const int MaxEntries = 20;
        if (list.Count > MaxEntries)
        {
            list.RemoveAt(0);
        }

        return $"已记住: {information}";
    }

    /// <summary>
    /// 检索指定会话的所有记忆
    /// </summary>
    /// <param name="sessionId">会话标识（可选）</param>
    /// <returns>记忆列表文本</returns>
    [KernelFunction("recall")]
    [Description("召回当前会话的所有记忆条目，按时间顺序返回")]
    public string Recall(
        [Description("会话 ID，默认为 default")]
        string sessionId = DefaultSession)
    {
        if (_sessions.TryGetValue(sessionId, out var list) && list.Count > 0)
        {
            return string.Join("\n", list.Select((m, i) => $"{i + 1}. {m}"));
        }

        return "(无记忆)";
    }

    /// <summary>
    /// 清除指定会话的记忆
    /// </summary>
    /// <param name="sessionId">会话标识（可选）</param>
    /// <returns>确认消息</returns>
    [KernelFunction("forget")]
    [Description("清除指定会话的所有记忆")]
    public string Forget(
        [Description("会话 ID，默认为 default")]
        string sessionId = DefaultSession)
    {
        if (_sessions.Remove(sessionId))
        {
            return $"已清除会话 {sessionId} 的所有记忆";
        }

        return $"会话 {sessionId} 无记忆可清除";
    }
}
