using System.Diagnostics;
using System.IO;
using DeerFlow.WPF.Models;
using TaskStatus = DeerFlow.WPF.Models.TaskStatus;

namespace DeerFlow.WPF.Services;

/// <summary>
/// 看门狗服务接口，负责进程异常检测和自动恢复
/// </summary>
public interface IWatchdogService
{
    /// <summary>注册需要监控的进程</summary>
    void Register(string taskId, Process process);

    /// <summary>取消注册</summary>
    void Unregister(string taskId);

    /// <summary>获取所有已注册进程的健康状态</summary>
    Dictionary<string, ProcessHealth> GetHealthStatus();

    /// <summary>启动监控循环</summary>
    void StartMonitoring();

    /// <summary>停止监控循环</summary>
    void StopMonitoring();

    /// <summary>进程崩溃事件</summary>
    event Action<TaskModel>? ProcessCrashed;
}

/// <summary>
/// 进程健康状态
/// </summary>
public class ProcessHealth
{
    public string TaskId { get; set; } = string.Empty;
    public bool IsAlive { get; set; }
    public int CrashCount { get; set; }
    public DateTime? LastCrash { get; set; }
    public DateTime? LastHeartbeat { get; set; }
}

/// <summary>
/// 看门狗服务实现，监控任务进程并实现崩溃自动恢复
/// </summary>
public class WatchdogService : IWatchdogService, IDisposable
{
    private readonly ILoggerService _logger;
    private readonly ITaskWindowManager _taskManager;
    private readonly Dictionary<string, WatchEntry> _entries = new();
    private CancellationTokenSource? _cts;
    private bool _disposed;

    private const int CHECK_INTERVAL_MS = 3000;
    private const int MAX_RESTART_ATTEMPTS = 3;
    private const int RESTART_COOLDOWN_MS = 5000;
    private const int HEARTBEAT_TIMEOUT_MS = 30000;

    public event Action<TaskModel>? ProcessCrashed;

    public WatchdogService(ILoggerService logger, ITaskWindowManager taskManager)
    {
        _logger = logger;
        _taskManager = taskManager;
    }

    /// <inheritdoc/>
    public void Register(string taskId, Process process)
    {
        lock (_entries)
        {
            _entries[taskId] = new WatchEntry
            {
                Process = process,
                Health = new ProcessHealth
                {
                    TaskId = taskId,
                    IsAlive = true,
                    LastHeartbeat = DateTime.Now
                }
            };
            _logger.Info($"看门狗已注册: {taskId}");
        }
    }

    /// <inheritdoc/>
    public void Unregister(string taskId)
    {
        lock (_entries)
        {
            if (_entries.TryGetValue(taskId, out var entry))
            {
                entry.Process?.Dispose();
                _entries.Remove(taskId);
                _logger.Info($"看门狗已注销: {taskId}");
            }
        }
    }

    /// <inheritdoc/>
    public Dictionary<string, ProcessHealth> GetHealthStatus()
    {
        lock (_entries)
        {
            return _entries.ToDictionary(kv => kv.Key, kv => kv.Value.Health);
        }
    }

    /// <inheritdoc/>
    public void StartMonitoring()
    {
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        Task.Run(async () =>
        {
            _logger.Info("看门狗监控已启动");

            while (!token.IsCancellationRequested)
            {
                await Task.Delay(CHECK_INTERVAL_MS, token);
                CheckAllProcesses();
            }
        }, token);

        Task.Run(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(HEARTBEAT_TIMEOUT_MS, token);
                CheckHeartbeats();
            }
        }, token);
    }

    /// <inheritdoc/>
    public void StopMonitoring()
    {
        _cts?.Cancel();
        _logger.Info("看门狗监控已停止");
    }

    /// <summary>
    /// 检查所有注册进程的存活状态
    /// </summary>
    private void CheckAllProcesses()
    {
        lock (_entries)
        {
            foreach (var (taskId, entry) in _entries.ToList())
            {
                try
                {
                    if (entry.Process == null)
                        continue;

                    entry.Process.Refresh();

                    if (entry.Process.HasExited)
                    {
                        HandleProcessCrash(taskId, entry);
                    }
                    else
                    {
                        entry.Health.IsAlive = true;
                        entry.Health.LastHeartbeat = DateTime.Now;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Warn($"检查进程失败: {taskId} - {ex.Message}");
                }
            }
        }
    }

    /// <summary>
    /// 心跳超时检测
    /// </summary>
    private void CheckHeartbeats()
    {
        lock (_entries)
        {
            foreach (var (taskId, entry) in _entries.ToList())
            {
                if (entry.Health.LastHeartbeat.HasValue &&
                    (DateTime.Now - entry.Health.LastHeartbeat.Value).TotalMilliseconds > HEARTBEAT_TIMEOUT_MS &&
                    entry.Health.IsAlive)
                {
                    _logger.Warn($"进程心跳超时: {taskId}");
                    HandleProcessCrash(taskId, entry);
                }
            }
        }
    }

    /// <summary>
    /// 处理进程崩溃，尝试自动恢复
    /// </summary>
    private void HandleProcessCrash(string taskId, WatchEntry entry)
    {
        entry.Health.IsAlive = false;
        entry.Health.CrashCount++;
        entry.Health.LastCrash = DateTime.Now;

        _logger.Error($"进程崩溃: {taskId} (第 {entry.Health.CrashCount} 次)");

        var task = new TaskModel { Id = taskId, Status = TaskStatus.Failed };
        ProcessCrashed?.Invoke(task);

        if (entry.Health.CrashCount <= MAX_RESTART_ATTEMPTS)
        {
            _logger.Info($"尝试重启进程: {taskId} (第 {entry.Health.CrashCount}/{MAX_RESTART_ATTEMPTS} 次)");

            Task.Run(async () =>
            {
                await Task.Delay(RESTART_COOLDOWN_MS * entry.Health.CrashCount);

                try
                {
                    entry.Process?.Dispose();

                    entry.Process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "DeerFlow.WPF.exe",
                            Arguments = $"--task-id {taskId} --sandbox-path {GetSandboxPath(taskId)}",
                            UseShellExecute = false,
                            CreateNoWindow = false
                        }
                    };

                    entry.Process.Start();
                    entry.Health.IsAlive = true;
                    entry.Health.LastHeartbeat = DateTime.Now;

                    _logger.Info($"进程已重启: {taskId}");
                }
                catch (Exception ex)
                {
                    _logger.Error($"重启进程失败: {taskId}", ex);
                    entry.Health.CrashCount = MAX_RESTART_ATTEMPTS + 1;
                }
            });
        }
        else
        {
            _logger.Error($"进程重启次数超限: {taskId}，不再尝试恢复");
            Unregister(taskId);
        }
    }

    private static string GetSandboxPath(string taskId)
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DeerFlow.WPF", "sandboxes", taskId);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _cts?.Cancel();
        _cts?.Dispose();

        lock (_entries)
        {
            foreach (var entry in _entries.Values)
                entry.Process?.Dispose();

            _entries.Clear();
        }
    }

    /// <summary>
    /// 监控条目
    /// </summary>
    private class WatchEntry
    {
        public Process? Process { get; set; }
        public ProcessHealth Health { get; set; } = new();
    }
}
