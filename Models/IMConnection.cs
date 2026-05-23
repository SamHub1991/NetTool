namespace DeerFlow.WPF.Models;

/// <summary>
/// IM 平台类型枚举
/// </summary>
public enum PlatformType
{
    /// <summary>飞书</summary>
    Feishu,

    /// <summary>企业微信</summary>
    WeCom
}

/// <summary>
/// IM 连接状态枚举
/// </summary>
public enum ConnectionStatus
{
    /// <summary>已断开</summary>
    Disconnected,

    /// <summary>连接中</summary>
    Connecting,

    /// <summary>已连接</summary>
    Connected,

    /// <summary>连接失败</summary>
    Failed
}

/// <summary>
/// IM 连接配置模型
/// </summary>
public class IMConnection
{
    /// <summary>连接唯一标识</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];

    /// <summary>平台类型</summary>
    public PlatformType PlatformType { get; set; } = PlatformType.Feishu;

    /// <summary>用户可读名称</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>平台 Webhook 地址</summary>
    public string WebhookUrl { get; set; } = string.Empty;

    /// <summary>验证令牌</summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>是否启用</summary>
    public bool IsEnabled { get; set; }

    /// <summary>连接状态</summary>
    public ConnectionStatus ConnectionStatus { get; set; } = ConnectionStatus.Disconnected;

    /// <summary>最近活动时间</summary>
    public DateTime? LastActiveTime { get; set; }

    /// <summary>已处理消息数</summary>
    public int TotalMessagesProcessed { get; set; }
}

/// <summary>
/// IM 收消息事件参数
/// </summary>
public class IMIncomingMessageEventArgs : EventArgs
{
    /// <summary>连接ID</summary>
    public string ConnectionId { get; set; } = string.Empty;

    /// <summary>消息内容</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>发送者ID</summary>
    public string SenderId { get; set; } = string.Empty;

    /// <summary>消息时间戳</summary>
    public DateTime Timestamp { get; set; } = DateTime.Now;
}
