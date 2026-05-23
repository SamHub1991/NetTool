using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace DeerFlow.WPF.Models;

/// <summary>
/// 自我反思记录 - 任务执行后的反思和总结
/// </summary>
public class SelfReflectionItem
{
    /// <summary>
    /// 任务唯一标识
    /// </summary>
    public string TaskId { get; set; } = string.Empty;

    /// <summary>
    /// 任务描述
    /// </summary>
    public string TaskDescription { get; set; } = string.Empty;

    /// <summary>
    /// 任务类型（聊天/代码执行/文件操作/网络搜索）
    /// </summary>
    public string TaskType { get; set; } = string.Empty;

    /// <summary>
    /// 执行结果（成功/失败/部分成功）
    /// </summary>
    public string Outcome { get; set; } = string.Empty;

    /// <summary>
    /// 成功率评分（0-1）
    /// </summary>
    public double SuccessRate { get; set; }

    /// <summary>
    /// 使用的工具列表
    /// </summary>
    public List<string> ToolsUsed { get; set; } = new();

    /// <summary>
    /// 执行耗时（毫秒）
    /// </summary>
    public long ExecutionTimeMs { get; set; }

    /// <summary>
    /// 遇到的问题列表
    /// </summary>
    public List<string> Challenges { get; set; } = new();

    /// <summary>
    /// 解决方案列表
    /// </summary>
    public List<string> Solutions { get; set; } = new();

    /// <summary>
    /// 提取的最佳实践
    /// </summary>
    public List<string> BestPractices { get; set; } = new();

    /// <summary>
    /// 需要改进的点
    /// </summary>
    public List<string> Improvements { get; set; } = new();

    /// <summary>
    /// 反思时间戳
    /// </summary>
    public DateTime ReflectionTimestamp { get; set; } = DateTime.Now;

    /// <summary>
    /// 是否已转化为模式
    /// </summary>
    public bool IsConvertedToPattern { get; set; }
}

/// <summary>
/// 模式条目 - 从历史经验中提炼的可复用模式
/// </summary>
public class PatternItem
{
    /// <summary>
    /// 模式唯一标识（Guid）
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// 模式名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 模式描述
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 模式类别（问题解决/工具使用/流程优化/配置调优）
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// 适用场景描述
    /// </summary>
    public string ApplicableScenarios { get; set; } = string.Empty;

    /// <summary>
    /// 模式内容（结构化步骤）
    /// </summary>
    public List<string> Steps { get; set; } = new();

    /// <summary>
    /// 前置条件
    /// </summary>
    public List<string> Preconditions { get; set; } = new();

    /// <summary>
    /// 预期结果
    /// </summary>
    public List<string> ExpectedOutcomes { get; set; } = new();

    /// <summary>
    /// 关联的经验反思 ID 列表
    /// </summary>
    public List<string> SourceReflectionIds { get; set; } = new();

    /// <summary>
    /// 模式评分（基于使用次数和成功率）
    /// </summary>
    public double Score { get; set; } = 1.0;

    /// <summary>
    /// 使用次数
    /// </summary>
    public int UsageCount { get; set; }

    /// <summary>
    /// 成功次数
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// 最后使用时间
    /// </summary>
    public DateTime? LastUsedAt { get; set; }

    /// <summary>
    /// 模式状态（草稿/已验证/已归档）
    /// </summary>
    public string Status { get; set; } = "Draft";
}

/// <summary>
/// 经验记忆条目 - 改进后的记忆系统
/// </summary>
public class ExperienceMemoryItem : MemoryItem
{
    /// <summary>
    /// 经验类型（成功/失败/教训/洞察）
    /// </summary>
    public string ExperienceType { get; set; } = string.Empty;

    /// <summary>
    /// 关联的任务 ID
    /// </summary>
    public string RelatedTaskId { get; set; } = string.Empty;

    /// <summary>
    /// 经验摘要（一句话总结）
    /// </summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// 详细经验内容
    /// </summary>
    public string DetailedContent { get; set; } = string.Empty;

    /// <summary>
    /// 可操作的建议
    /// </summary>
    public List<string> ActionableTips { get; set; } = new();

    /// <summary>
    /// 置信度（0-1，基于验证次数）
    /// </summary>
    public double Confidence { get; set; } = 1.0;

    /// <summary>
    /// 验证次数
    /// </summary>
    public int ValidationCount { get; set; }

    /// <summary>
    /// 是否已应用于后续任务
    /// </summary>
    public bool IsApplied { get; set; }
}

/// <summary>
/// 演化策略 - 系统自我优化的策略配置
/// </summary>
public class EvolutionStrategy
{
    /// <summary>
    /// 策略唯一标识
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// 策略名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 策略描述
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 优化目标（性能/准确性/用户体验）
    /// </summary>
    public string OptimizationTarget { get; set; } = string.Empty;

    /// <summary>
    /// 当前参数配置（JSON）
    /// </summary>
    public string CurrentConfig { get; set; } = string.Empty;

    /// <summary>
    /// 候选参数配置（JSON）
    /// </summary>
    public string CandidateConfig { get; set; } = string.Empty;

    /// <summary>
    /// A/B 测试分组（A 组/B 组）
    /// </summary>
    public string ABTestGroup { get; set; } = "A";

    /// <summary>
    /// 策略效果指标（JSON）
    /// </summary>
    public Dictionary<string, double> Metrics { get; set; } = new();

    /// <summary>
    /// 策略状态（实验中/已采纳/已废弃）
    /// </summary>
    public string Status { get; set; } = "Experimenting";

    /// <summary>
    /// 实验开始时间
    /// </summary>
    public DateTime ExperimentStartDate { get; set; } = DateTime.Now;

    /// <summary>
    /// 实验结束时间
    /// </summary>
    public DateTime? ExperimentEndDate { get; set; }

    /// <summary>
    /// 实验结果总结
    /// </summary>
    public string ExperimentSummary { get; set; } = string.Empty;
}

/// <summary>
/// 实验项目 - 用于 A/B 测试的实验配置
/// </summary>
public class Experiment
{
    /// <summary>
    /// 实验名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 实验类型（参数调整/策略优化/配置变更）
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// 实验评分（0-1）
    /// </summary>
    public double Score { get; set; }

    /// <summary>
    /// 实验状态（进行中/已完成/已采纳/已废弃）
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// 实验开始时间
    /// </summary>
    public DateTime StartTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 实验结束时间
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// 实验描述
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 实验指标列表
    /// </summary>
    public Dictionary<string, double> Metrics { get; set; } = new();
}

/// <summary>
/// 用户反馈条目 - 点赞/点踩反馈
/// </summary>
public class UserFeedback
{
    /// <summary>
    /// 反馈唯一标识
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// 关联的任务/对话 ID
    /// </summary>
    public string TaskId { get; set; } = string.Empty;

    /// <summary>
    /// 反馈类型（Like/Dislike）
    /// </summary>
    public string FeedbackType { get; set; } = string.Empty;

    /// <summary>
    /// 反馈评分（1-5）
    /// </summary>
    public int Rating { get; set; } = 5;

    /// <summary>
    /// 反馈文本内容
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// 反馈时间戳
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// 是否已处理
    /// </summary>
    public bool IsProcessed { get; set; }

    /// <summary>
    /// 关联的模式 ID（如果 feedback 针对某个模式）
    /// </summary>
    public string? RelatedPatternId { get; set; }
}
