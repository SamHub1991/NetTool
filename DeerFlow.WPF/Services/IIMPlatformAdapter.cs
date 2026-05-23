using DeerFlow.WPF.Models;

namespace DeerFlow.WPF.Services;

/// <summary>
/// IM 平台适配器接口，定义统一的消息收发和连接管理能力
/// </summary>
public interface IIMPlatformAdapter : IDisposable
{
    /// <summary>建立平台连接</summary>
    Task<bool> ConnectAsync(IMConnection config);

    /// <summary>断开平台连接</summary>
    Task<bool> DisconnectAsync();

    /// <summary>发送消息至指定接收者</summary>
    Task<bool> SendMessageAsync(string recipientId, string content);

    /// <summary>收到消息事件</summary>
    event EventHandler<IMIncomingMessageEventArgs>? OnMessageReceived;

    /// <summary>平台名称标识</summary>
    string PlatformName { get; }
}
