namespace DeerFlow.WPF.Models;

/// <summary>
/// 记忆条目模型
/// </summary>
public class MemoryItem
{
    /// <summary>记忆唯一标识</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];

    /// <summary>记忆内容</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>记忆类型（fact/preference/context/skill）</summary>
    public string Type { get; set; } = "fact";

    /// <summary>关联标签</summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>重要性评分 (0-10)</summary>
    public int Importance { get; set; } = 5;

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>最后访问时间</summary>
    public DateTime LastAccessedAt { get; set; } = DateTime.Now;
}
