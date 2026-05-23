using DeerFlow.WPF.Services;
using Xunit;

namespace DeerFlow.WPF.Tests.Services;

public class LoggerServiceTests : IDisposable
{
    private readonly LoggerService _logger;

    public LoggerServiceTests()
    {
        _logger = new LoggerService();
    }

    [Fact]
    public void Info_AddsToRecentLogs()
    {
        _logger.Info("测试消息");
        var logs = _logger.GetRecentLogs(10);

        Assert.Contains(logs, l => l.Contains("测试消息"));
    }

    [Fact]
    public void Error_LogsException()
    {
        var ex = new InvalidOperationException("测试异常");
        _logger.Error("错误发生", ex);

        var logs = _logger.GetRecentLogs(10);
        Assert.Contains(logs, l => l.Contains("错误发生") && l.Contains("测试异常"));
    }

    [Fact]
    public void Warn_AddsCorrectLevel()
    {
        _logger.Warn("警告消息");
        var logs = _logger.GetRecentLogs(10);

        Assert.Contains(logs, l => l.Contains("[WARN]") && l.Contains("警告消息"));
    }

    [Fact]
    public void GetRecentLogs_RespectsCount()
    {
        for (int i = 0; i < 10; i++)
            _logger.Info($"消息{i}");

        var logs = _logger.GetRecentLogs(5);
        Assert.Equal(5, logs.Count);
    }

    public void Dispose()
    {
    }
}
