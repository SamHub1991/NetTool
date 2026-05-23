namespace SystemToolkit.Core.Models;

public class ProcessInfo
{
    public int Id { get; set; }
    public string ProcessName { get; set; } = string.Empty;
    public string? Path { get; set; }
    public TimeSpan TotalProcessorTime { get; set; }
    public long WorkingSet64 { get; set; }
    public long PeakWorkingSet64 { get; set; }
    public long VirtualMemorySize64 { get; set; }
    public int ThreadCount { get; set; }
    public int HandleCount { get; set; }
    public DateTime StartTime { get; set; }
    public string? PriorityClass { get; set; }
}

public class ProcessResourceUsage
{
    public int ProcessId { get; set; }
    public string ProcessName { get; set; } = string.Empty;
    public double CpuPercentage { get; set; }
    public long MemoryUsage { get; set; }
    public long DiskReadBytes { get; set; }
    public long DiskWriteBytes { get; set; }
    public long NetworkSentBytes { get; set; }
    public long NetworkReceivedBytes { get; set; }
}
