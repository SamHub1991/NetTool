using System.IO;
using DeerFlow.WPF.Models;
using DeerFlow.WPF.Services;
using Moq;
using Xunit;
using TaskStatus = DeerFlow.WPF.Models.TaskStatus;

namespace DeerFlow.WPF.Tests.Services;

public class TaskWindowManagerTests
{
    private readonly Mock<ISandboxManager> _sandboxMock;
    private readonly Mock<ILoggerService> _loggerMock;
    private readonly TaskWindowManager _manager;

    public TaskWindowManagerTests()
    {
        _sandboxMock = new Mock<ISandboxManager>();
        _loggerMock = new Mock<ILoggerService>();
        _manager = new TaskWindowManager(_sandboxMock.Object, _loggerMock.Object);
    }

    [Fact]
    public void InitialState_NoActiveTasks()
    {
        var tasks = _manager.GetActiveTasks();
        Assert.Empty(tasks);
    }

    [Fact]
    public async Task PauseResume_NonExistentTask_ReturnsFalse()
    {
        var result = await _manager.PauseTaskAsync("non-existent");
        Assert.False(result);

        var resumeResult = await _manager.ResumeTaskAsync("non-existent");
        Assert.False(resumeResult);
    }

    [Fact]
    public async Task CloseTask_NonExistent_ReturnsFalse()
    {
        var result = await _manager.CloseTaskWindowAsync("non-existent");
        Assert.False(result);
    }

    [Fact]
    public async Task GetTaskStatus_NonExistent_ReturnsFailed()
    {
        var status = await _manager.GetTaskStatusAsync("non-existent");
        Assert.Equal(TaskStatus.Failed, status.Status);
    }
}

/// <summary>
/// 并发压力测试（仅在开发机上手动运行）
/// </summary>
[Trait("Category", "Integration")]
public class TaskWindowManagerIntegrationTests
{
    private readonly Mock<ISandboxManager> _sandboxMock;
    private readonly Mock<ILoggerService> _loggerMock;
    private readonly TaskWindowManager _manager;

    public TaskWindowManagerIntegrationTests()
    {
        _sandboxMock = new Mock<ISandboxManager>();
        _sandboxMock.Setup(s => s.CreateSandboxAsync(It.IsAny<string>()))
            .ReturnsAsync((string id) => Path.Combine(Path.GetTempPath(), "test_sandbox", id));

        _loggerMock = new Mock<ILoggerService>();
        _manager = new TaskWindowManager(_sandboxMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ConcurrentTaskCreation_HandlesMultipleRequests()
    {
        const int taskCount = 5;
        var tasks = new Task[taskCount];
        var exceptions = new List<Exception>();

        for (int i = 0; i < taskCount; i++)
        {
            var idx = i;
            tasks[i] = Task.Run(async () =>
            {
                try
                {
                    var task = new TaskModel
                    {
                        Name = $"并发任务-{idx}",
                        IsSandboxed = false
                    };
                    await _manager.CreateTaskWindowAsync(task);
                }
                catch (Exception ex)
                {
                    lock (exceptions)
                    {
                        exceptions.Add(ex);
                    }
                }
            });
        }

        await Task.WhenAll(tasks);
        Assert.Empty(exceptions);
    }
}

