namespace DeerFlow.WPF.Models;

/// <summary>
/// 任务状态枚举
/// </summary>
public enum TaskStatus
{
    /// <summary>等待中</summary>
    Pending,

    /// <summary>运行中</summary>
    Running,

    /// <summary>已完成</summary>
    Completed,

    /// <summary>失败</summary>
    Failed,

    /// <summary>已暂停</summary>
    Paused
}
