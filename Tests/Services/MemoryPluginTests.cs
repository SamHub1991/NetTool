using DeerFlow.WPF.Services.Plugins;
using Xunit;

namespace DeerFlow.WPF.Tests.Services;

public class MemoryPluginTests
{
    private readonly MemoryPlugin _plugin = new();

    [Fact]
    public void Remember_ReturnsConfirmation()
    {
        var result = _plugin.Remember("用户喜欢Python编程");

        Assert.Contains("已记住", result);
        Assert.Contains("Python", result);
    }

    [Fact]
    public void Recall_ReturnsAllEntries()
    {
        _plugin.Remember("第一条信息");
        _plugin.Remember("第二条信息");
        _plugin.Remember("第三条信息");

        var result = _plugin.Recall();

        Assert.Contains("第一条信息", result);
        Assert.Contains("第二条信息", result);
        Assert.Contains("第三条信息", result);
    }

    [Fact]
    public void Recall_EmptySession_ReturnsEmptyMessage()
    {
        var result = _plugin.Recall("empty-session");

        Assert.Contains("无记忆", result);
    }

    [Fact]
    public void Recall_DefaultSessionEmpty_ReturnsEmptyMessage()
    {
        var plugin = new MemoryPlugin();

        var result = plugin.Recall();

        Assert.Contains("无记忆", result);
    }

    [Fact]
    public void Forget_ClearsSessionMemory()
    {
        const string sessionId = "session-a";
        _plugin.Remember("data", sessionId);

        var result = _plugin.Forget(sessionId);

        Assert.Contains("已清除", result);
        Assert.Contains("无记忆", _plugin.Recall(sessionId));
    }

    [Fact]
    public void Memory_EnforcesMaxEntriesLimit()
    {
        for (var i = 0; i < 25; i++)
        {
            _plugin.Remember($"信息 {i}", "limit-test");
        }

        var result = _plugin.Recall("limit-test");

        // 最多保留 20 条，因此最旧的信息不应该存在
        Assert.DoesNotContain("信息 0", result);
        Assert.Contains("信息 24", result);
    }

    [Fact]
    public void MultipleSessions_AreIndependent()
    {
        _plugin.Remember("会话A-信息", "session-a");
        _plugin.Remember("会话B-信息", "session-b");

        Assert.Contains("会话A-信息", _plugin.Recall("session-a"));
        Assert.Contains("会话B-信息", _plugin.Recall("session-b"));
        Assert.DoesNotContain("会话B-信息", _plugin.Recall("session-a"));
    }
}
