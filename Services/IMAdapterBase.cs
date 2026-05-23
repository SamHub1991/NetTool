using System.Net.Http;
using DeerFlow.WPF.Models;

namespace DeerFlow.WPF.Services;

/// <summary>
/// IM 平台适配器抽象基类，封装 HttpClient 和 CancellationTokenSource 的管理
/// </summary>
public abstract class IMAdapterBase : IIMPlatformAdapter, IDisposable
{
    protected HttpClient? _httpClient;
    protected CancellationTokenSource? _cts;
    protected IMConnection? _config;
    private bool _disposed;

    public abstract string PlatformName { get; }

    public event EventHandler<IMIncomingMessageEventArgs>? OnMessageReceived;

    /// <inheritdoc/>
    public abstract Task<bool> ConnectAsync(IMConnection config);

    /// <inheritdoc/>
    public abstract Task<bool> SendMessageAsync(string recipientId, string content);

    /// <inheritdoc/>
    public virtual Task<bool> DisconnectAsync()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        _httpClient?.Dispose();
        _httpClient = null;
        return Task.FromResult(true);
    }

    /// <summary>
    /// 触发消息接收事件
    /// </summary>
    protected void RaiseMessageReceived(IMIncomingMessageEventArgs args)
    {
        OnMessageReceived?.Invoke(this, args);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _cts?.Cancel();
        _cts?.Dispose();
        _httpClient?.Dispose();
    }
}
