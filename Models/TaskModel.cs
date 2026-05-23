using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DeerFlow.WPF.Models;

/// <summary>
/// 任务窗口模型，实现 INotifyPropertyChanged 以支持状态实时更新
/// </summary>
public class TaskModel : INotifyPropertyChanged
{
    /// <summary>任务唯一标识</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];

    private string _name = string.Empty;
    /// <summary>任务名称</summary>
    public string Name
    {
        get => _name;
        set { _name = value; OnPropertyChanged(); }
    }

    private string _description = string.Empty;
    /// <summary>任务描述</summary>
    public string Description
    {
        get => _description;
        set { _description = value; OnPropertyChanged(); }
    }

    private TaskStatus _status = TaskStatus.Pending;
    /// <summary>当前状态</summary>
    public TaskStatus Status
    {
        get => _status;
        set { _status = value; OnPropertyChanged(); }
    }

    /// <summary>使用的智能体配置</summary>
    public AgentConfig AgentConfig { get; set; } = new();

    private int _processId;
    /// <summary>窗口进程ID</summary>
    public int ProcessId
    {
        get => _processId;
        set { _processId = value; OnPropertyChanged(); }
    }

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    private double _memoryUsageMB;
    /// <summary>内存占用 (MB)</summary>
    public double MemoryUsageMB
    {
        get => _memoryUsageMB;
        set { _memoryUsageMB = value; OnPropertyChanged(); }
    }

    private double _cpuUsagePercent;
    /// <summary>CPU占用百分比</summary>
    public double CpuUsagePercent
    {
        get => _cpuUsagePercent;
        set { _cpuUsagePercent = value; OnPropertyChanged(); }
    }

    private bool _isSandboxed = true;
    /// <summary>是否使用沙箱隔离</summary>
    public bool IsSandboxed
    {
        get => _isSandboxed;
        set { _isSandboxed = value; OnPropertyChanged(); }
    }

    private string _sandboxPath = string.Empty;
    /// <summary>沙箱工作目录</summary>
    public string SandboxPath
    {
        get => _sandboxPath;
        set { _sandboxPath = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// 触发属性变更通知
    /// </summary>
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
