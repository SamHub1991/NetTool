using System.IO;
using System.Text.Json;
using DeerFlow.WPF.Models;

namespace DeerFlow.WPF.Services;

/// <summary>
/// IM 平台服务接口
/// </summary>
public interface IIMPlatformService
{
    /// <summary>初始化服务，加载持久化配置并自动连接已启用的连接</summary>
    Task InitializeAsync();

    /// <summary>获取所有 IM 连接</summary>
    List<IMConnection> GetAllConnections();

    /// <summary>添加新连接</summary>
    void AddConnection(IMConnection connection);

    /// <summary>更新已有连接</summary>
    void UpdateConnection(IMConnection connection);

    /// <summary>删除连接</summary>
    void RemoveConnection(string id);

    /// <summary>测试连接</summary>
    Task<bool> TestConnectionAsync(string id);

    /// <summary>根据 ID 查找连接</summary>
    IMConnection? GetConnection(string id);

    /// <summary>持久化所有连接到磁盘</summary>
    Task SaveToDiskAsync();
}

/// <summary>
/// IM 平台服务实现，管理所有 IM 连接的生命周期、持久化和消息路由
/// </summary>
public class IMPlatformService : IIMPlatformService, IDisposable
{
    private readonly IApiService _apiService;
    private readonly ILoggerService _logger;
    private readonly Dictionary<string, IMConnection> _connections = new();
    private readonly Dictionary<string, IIMPlatformAdapter> _adapters = new();
    private readonly string _storagePath;
    private bool _disposed;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public IMPlatformService(IApiService apiService, ILoggerService logger)
    {
        _apiService = apiService;
        _logger = logger;
        _storagePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DeerFlow.WPF",
            "im_connections.json");
    }

    /// <inheritdoc/>
    public async Task InitializeAsync()
    {
        _logger.Info("初始化 IM 平台服务...");

        var loaded = await LoadFromDiskAsync();
        foreach (var conn in loaded)
        {
            _connections[conn.Id] = conn;
            if (conn.IsEnabled)
            {
                await ConnectAdapterAsync(conn);
            }
        }

        _logger.Info($"IM 平台服务初始化完成，共 {_connections.Count} 个连接");
    }

    /// <inheritdoc/>
    public List<IMConnection> GetAllConnections()
    {
        return _connections.Values.ToList();
    }

    /// <inheritdoc/>
    public void AddConnection(IMConnection connection)
    {
        if (string.IsNullOrEmpty(connection.Id))
        {
            connection.Id = Guid.NewGuid().ToString("N")[..8];
        }

        _connections[connection.Id] = connection;
        _logger.Info($"添加 IM 连接: {connection.DisplayName} ({connection.PlatformType})");

        if (connection.IsEnabled)
        {
            _ = ConnectAdapterAsync(connection);
        }

        _ = SaveToDiskAsync();
    }

    /// <inheritdoc/>
    public void UpdateConnection(IMConnection connection)
    {
        if (!_connections.ContainsKey(connection.Id))
            return;

        _connections[connection.Id] = connection;
        _logger.Info($"更新 IM 连接: {connection.DisplayName}");

        _ = DisconnectAdapterAsync(connection.Id);

        if (connection.IsEnabled)
        {
            _ = ConnectAdapterAsync(connection);
        }

        _ = SaveToDiskAsync();
    }

    /// <inheritdoc/>
    public void RemoveConnection(string id)
    {
        if (!_connections.TryGetValue(id, out var conn))
            return;

        _logger.Info($"删除 IM 连接: {conn.DisplayName}");
        _ = DisconnectAdapterAsync(id);
        _connections.Remove(id);
        _ = SaveToDiskAsync();
    }

    /// <inheritdoc/>
    public async Task<bool> TestConnectionAsync(string id)
    {
        if (!_connections.TryGetValue(id, out var conn))
            return false;

        var adapter = CreateAdapter(conn.PlatformType);
        try
        {
            var result = await adapter.ConnectAsync(conn);
            await adapter.DisconnectAsync();
            adapter.Dispose();
            return result;
        }
        catch (Exception ex)
        {
            _logger.Error($"测试连接失败: {conn.DisplayName}", ex);
            adapter.Dispose();
            return false;
        }
    }

    /// <inheritdoc/>
    public IMConnection? GetConnection(string id)
    {
        return _connections.GetValueOrDefault(id);
    }

    /// <inheritdoc/>
    public async Task SaveToDiskAsync()
    {
        try
        {
            var dir = Path.GetDirectoryName(_storagePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var json = JsonSerializer.Serialize(_connections.Values.ToList(), _jsonOptions);
            await File.WriteAllTextAsync(_storagePath, json);
        }
        catch (Exception ex)
        {
            _logger.Error("保存 IM 连接配置失败", ex);
        }
    }

    /// <summary>
    /// 创建适配器并建立连接，注册消息接收事件
    /// </summary>
    private async Task ConnectAdapterAsync(IMConnection connection)
    {
        connection.ConnectionStatus = ConnectionStatus.Connecting;
        var adapter = CreateAdapter(connection.PlatformType);

        adapter.OnMessageReceived += (_, args) =>
        {
            _ = HandleIncomingMessageAsync(connection, args);
        };

        var success = await adapter.ConnectAsync(connection);
        connection.ConnectionStatus = success ? ConnectionStatus.Connected : ConnectionStatus.Failed;
        _adapters[connection.Id] = adapter;

        _logger.Info($"IM 连接 [{connection.DisplayName}]: {(success ? "已连接" : "连接失败")}");
    }

    /// <summary>
    /// 断开指定连接的适配器
    /// </summary>
    private async Task DisconnectAdapterAsync(string id)
    {
        if (_adapters.TryGetValue(id, out var adapter))
        {
            await adapter.DisconnectAsync();
            adapter.Dispose();
            _adapters.Remove(id);

            if (_connections.TryGetValue(id, out var conn))
            {
                conn.ConnectionStatus = ConnectionStatus.Disconnected;
            }
        }
    }

    /// <summary>
    /// 处理收到的 IM 消息，路由至 AI 回复流程
    /// </summary>
    private async Task HandleIncomingMessageAsync(IMConnection connection, IMIncomingMessageEventArgs args)
    {
        _logger.Info($"收到 IM 消息 [{connection.DisplayName}]: {MaskContent(args.Content)}");
        connection.LastActiveTime = DateTime.Now;
        connection.TotalMessagesProcessed++;

        try
        {
            var agentConfig = new AgentConfig();
            var fullReply = string.Empty;

            await foreach (var chunk in _apiService.SendChatStreamAsync(args.Content, agentConfig))
            {
                fullReply += chunk;
            }

            if (_adapters.TryGetValue(connection.Id, out var adapter))
            {
                await adapter.SendMessageAsync(args.SenderId, fullReply);
                connection.TotalMessagesProcessed++;
                _logger.Info($"AI 回复已发送至 [{connection.DisplayName}]");
            }
        }
        catch (Exception ex)
        {
            _logger.Error($"处理 IM 消息失败 [{connection.DisplayName}]", ex);
        }
    }

    /// <summary>
    /// 根据平台类型创建对应适配器
    /// </summary>
    private static IIMPlatformAdapter CreateAdapter(PlatformType type)
    {
        return type switch
        {
            PlatformType.Feishu => new FeishuAdapter(),
            PlatformType.WeCom => new WeComAdapter(),
            _ => new FeishuAdapter()
        };
    }

    /// <summary>
    /// 脱敏消息内容，避免在日志中记录完整敏感信息
    /// </summary>
    private static string MaskContent(string content)
    {
        const int MAX_LOG_LENGTH = 100;
        return content.Length <= MAX_LOG_LENGTH ? content : content[..MAX_LOG_LENGTH] + "...";
    }

    /// <summary>
    /// 从磁盘加载 IM 连接配置
    /// </summary>
    private async Task<List<IMConnection>> LoadFromDiskAsync()
    {
        try
        {
            if (!File.Exists(_storagePath))
                return new List<IMConnection>();

            var json = await File.ReadAllTextAsync(_storagePath);
            return JsonSerializer.Deserialize<List<IMConnection>>(json, _jsonOptions) ?? new List<IMConnection>();
        }
        catch (Exception ex)
        {
            _logger.Error("加载 IM 连接配置失败", ex);
            return new List<IMConnection>();
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        foreach (var (_, adapter) in _adapters)
        {
            try
            {
                adapter.DisconnectAsync().GetAwaiter().GetResult();
                adapter.Dispose();
            }
            catch { }
        }

        _adapters.Clear();
        SaveToDiskAsync().GetAwaiter().GetResult();
    }
}
