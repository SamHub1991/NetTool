using System.Diagnostics;

namespace DeerFlow.WPF.Services;

/// <summary>
/// 内存快照服务接口，用于性能诊断和内存泄漏检测
/// </summary>
public interface IMemorySnapshotService
{
    /// <summary>采集当前内存快照</summary>
    MemorySnapshot TakeSnapshot(string label);

    /// <summary>对比两个快照，生成差异报告</summary>
    MemoryDiffReport Compare(MemorySnapshot before, MemorySnapshot after);

    /// <summary>强制执行一次完整的垃圾回收</summary>
    void ForceGcCollect();
}

/// <summary>
/// 内存快照数据结构
/// </summary>
public class MemorySnapshot
{
    public string Label { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public long WorkingSetBytes { get; set; }
    public long PrivateMemoryBytes { get; set; }
    public long ManagedMemoryBytes { get; set; }
    public int Gen0Collections { get; set; }
    public int Gen1Collections { get; set; }
    public int Gen2Collections { get; set; }
    public int TotalThreads { get; set; }
    public long HandleCount { get; set; }
}

/// <summary>
/// 内存差异报告
/// </summary>
public class MemoryDiffReport
{
    public string BeforeLabel { get; set; } = string.Empty;
    public string AfterLabel { get; set; } = string.Empty;
    public TimeSpan Elapsed { get; set; }
    public long WorkingSetDeltaBytes { get; set; }
    public long PrivateMemoryDeltaBytes { get; set; }
    public long ManagedMemoryDeltaBytes { get; set; }
    public int Gen0Delta { get; set; }
    public int Gen1Delta { get; set; }
    public int Gen2Delta { get; set; }
    public int ThreadDelta { get; set; }
    public long HandleDelta { get; set; }
    public bool PotentialLeak { get; set; }
    public string Summary { get; set; } = string.Empty;
}

/// <summary>
/// 内存快照服务实现，提供运行时内存诊断能力
/// </summary>
public class MemorySnapshotService : IMemorySnapshotService
{
    private readonly ILoggerService _logger;
    private readonly Process _currentProcess;

    private const double LEAK_THRESHOLD_MB = 50;
    private static readonly long LEAK_THRESHOLD_BYTES = (long)(LEAK_THRESHOLD_MB * 1024 * 1024);

    public MemorySnapshotService(ILoggerService logger)
    {
        _logger = logger;
        _currentProcess = Process.GetCurrentProcess();
    }

    /// <inheritdoc/>
    public MemorySnapshot TakeSnapshot(string label)
    {
        _currentProcess.Refresh();

        var snapshot = new MemorySnapshot
        {
            Label = label,
            Timestamp = DateTime.Now,
            WorkingSetBytes = _currentProcess.WorkingSet64,
            PrivateMemoryBytes = _currentProcess.PrivateMemorySize64,
            ManagedMemoryBytes = GC.GetTotalMemory(false),
            Gen0Collections = GC.CollectionCount(0),
            Gen1Collections = GC.CollectionCount(1),
            Gen2Collections = GC.CollectionCount(2),
            TotalThreads = _currentProcess.Threads.Count,
            HandleCount = _currentProcess.HandleCount
        };

        _logger.Info($"内存快照 [{label}]: 工作集={snapshot.WorkingSetBytes / 1048576.0:F1}MB, " +
                     $"托管堆={snapshot.ManagedMemoryBytes / 1048576.0:F1}MB, " +
                     $"GC=(G0:{snapshot.Gen0Collections}/G1:{snapshot.Gen1Collections}/G2:{snapshot.Gen2Collections})");

        return snapshot;
    }

    /// <inheritdoc/>
    public MemoryDiffReport Compare(MemorySnapshot before, MemorySnapshot after)
    {
        var report = new MemoryDiffReport
        {
            BeforeLabel = before.Label,
            AfterLabel = after.Label,
            Elapsed = after.Timestamp - before.Timestamp,
            WorkingSetDeltaBytes = after.WorkingSetBytes - before.WorkingSetBytes,
            PrivateMemoryDeltaBytes = after.PrivateMemoryBytes - before.PrivateMemoryBytes,
            ManagedMemoryDeltaBytes = after.ManagedMemoryBytes - before.ManagedMemoryBytes,
            Gen0Delta = after.Gen0Collections - before.Gen0Collections,
            Gen1Delta = after.Gen1Collections - before.Gen1Collections,
            Gen2Delta = after.Gen2Collections - before.Gen2Collections,
            ThreadDelta = after.TotalThreads - before.TotalThreads,
            HandleDelta = after.HandleCount - before.HandleCount
        };

        report.PotentialLeak = report.ManagedMemoryDeltaBytes > LEAK_THRESHOLD_BYTES;
        report.Summary = BuildSummary(report);

        if (report.PotentialLeak)
        {
            _logger.Warn($"⚠ 潜在内存泄漏: {report.Summary}");
        }
        else
        {
            _logger.Info($"内存对比: {report.Summary}");
        }

        return report;
    }

    /// <inheritdoc/>
    public void ForceGcCollect()
    {
        _logger.Info("强制执行 GC 回收...");

        GC.Collect(2, GCCollectionMode.Forced, true, true);
        GC.WaitForPendingFinalizers();
        GC.Collect(2, GCCollectionMode.Forced, true, true);

        var managedMem = GC.GetTotalMemory(false);
        _logger.Info($"GC 回收完成, 托管堆: {managedMem / 1048576.0:F1}MB");
    }

    /// <summary>
    /// 构建差异报告摘要
    /// </summary>
    private static string BuildSummary(MemoryDiffReport report)
    {
        return $"工作集 Δ{FormatBytes(report.WorkingSetDeltaBytes)}, " +
               $"托管堆 Δ{FormatBytes(report.ManagedMemoryDeltaBytes)}, " +
               $"线程 Δ{report.ThreadDelta}, " +
               $"句柄 Δ{report.HandleDelta}, " +
               $"GC(ΔG0:{report.Gen0Delta}/ΔG1:{report.Gen1Delta}/ΔG2:{report.Gen2Delta}), " +
               $"耗时 {report.Elapsed.TotalSeconds:F1}s" +
               (report.PotentialLeak ? " ⚠ 疑似泄漏" : "");
    }

    /// <summary>
    /// 格式化字节数为可读字符串
    /// </summary>
    private static string FormatBytes(long bytes)
    {
        if (Math.Abs(bytes) >= 1048576)
            return $"{bytes / 1048576.0:+#.##;-#.##}MB";
        if (Math.Abs(bytes) >= 1024)
            return $"{bytes / 1024.0:+#.#;-#.#}KB";
        return $"{bytes:+0;-0}B";
    }
}
