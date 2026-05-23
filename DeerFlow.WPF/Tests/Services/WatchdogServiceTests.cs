using DeerFlow.WPF.Services;
using Moq;
using Xunit;

namespace DeerFlow.WPF.Tests.Services;

public class WatchdogServiceTests
{
    private readonly Mock<ILoggerService> _loggerMock;
    private readonly Mock<ITaskWindowManager> _taskManagerMock;
    private readonly WatchdogService _watchdog;

    public WatchdogServiceTests()
    {
        _loggerMock = new Mock<ILoggerService>();
        _taskManagerMock = new Mock<ITaskWindowManager>();
        _watchdog = new WatchdogService(_loggerMock.Object, _taskManagerMock.Object);
    }

    [Fact]
    public void Register_AddsToHealthStatus()
    {
        var process = new System.Diagnostics.Process();
        _watchdog.Register("task-001", process);

        var status = _watchdog.GetHealthStatus();
        Assert.True(status.ContainsKey("task-001"));
        Assert.True(status["task-001"].IsAlive);
    }

    [Fact]
    public void Unregister_RemovesFromHealthStatus()
    {
        var process = new System.Diagnostics.Process();
        _watchdog.Register("task-001", process);
        _watchdog.Unregister("task-001");

        var status = _watchdog.GetHealthStatus();
        Assert.False(status.ContainsKey("task-001"));
    }

    [Fact]
    public void GetHealthStatus_InitiallyEmpty()
    {
        var status = _watchdog.GetHealthStatus();
        Assert.Empty(status);
    }

    [Fact]
    public async Task MultipleRegistrations_AllTracked()
    {
        for (int i = 0; i < 5; i++)
        {
            _watchdog.Register($"task-{i}", new System.Diagnostics.Process());
        }

        var status = _watchdog.GetHealthStatus();
        Assert.Equal(5, status.Count);
        await Task.CompletedTask;
    }
}
