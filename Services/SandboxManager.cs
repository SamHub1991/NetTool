using System.IO;

namespace DeerFlow.WPF.Services;

/// <summary>
/// 沙箱管理器接口
/// </summary>
public interface ISandboxManager
{
    /// <summary>创建沙箱工作目录</summary>
    Task<string> CreateSandboxAsync(string taskId);

    /// <summary>销毁沙箱并清理资源</summary>
    Task DestroySandboxAsync(string taskId);

    /// <summary>获取沙箱文件列表</summary>
    Task<List<string>> GetSandboxFilesAsync(string taskId);

    /// <summary>在沙箱内执行命令（受限白名单）</summary>
    Task<string> ExecuteInSandboxAsync(string taskId, string command);
}

/// <summary>
/// 沙箱管理器，基于文件系统实现任务隔离
/// </summary>
public class SandboxManager : ISandboxManager
{
    private readonly ILoggerService _logger;
    private static readonly string SandboxRoot = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "DeerFlow.WPF", "sandboxes");

    private static readonly string[] AllowedCommands =
    {
        "dir", "ls", "type", "cat", "echo", "mkdir", "findstr",
        "python", "node", "git", "tree"
    };

    public SandboxManager(ILoggerService logger)
    {
        _logger = logger;
        Directory.CreateDirectory(SandboxRoot);
    }

    /// <inheritdoc/>
    public async Task<string> CreateSandboxAsync(string taskId)
    {
        var sandboxPath = Path.Combine(SandboxRoot, taskId);
        Directory.CreateDirectory(sandboxPath);

        Directory.CreateDirectory(Path.Combine(sandboxPath, "workspace"));
        Directory.CreateDirectory(Path.Combine(sandboxPath, "temp"));
        Directory.CreateDirectory(Path.Combine(sandboxPath, "output"));

        _logger.Info($"沙箱已创建: {sandboxPath}");

        await Task.CompletedTask;
        return sandboxPath;
    }

    /// <inheritdoc/>
    public async Task DestroySandboxAsync(string taskId)
    {
        var sandboxPath = Path.Combine(SandboxRoot, taskId);

        if (Directory.Exists(sandboxPath))
        {
            try
            {
                Directory.Delete(sandboxPath, true);
                _logger.Info($"沙箱已销毁: {taskId}");
            }
            catch (Exception ex)
            {
                _logger.Error($"销毁沙箱失败: {taskId}", ex);
            }
        }

        await Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<List<string>> GetSandboxFilesAsync(string taskId)
    {
        var sandboxPath = Path.Combine(SandboxRoot, taskId);

        if (!Directory.Exists(sandboxPath))
            return Task.FromResult(new List<string>());

        var files = Directory.GetFiles(sandboxPath, "*.*", SearchOption.AllDirectories)
            .Select(f => Path.GetRelativePath(sandboxPath, f))
            .ToList();

        return Task.FromResult(files);
    }

    /// <inheritdoc/>
    public Task<string> ExecuteInSandboxAsync(string taskId, string command)
    {
        var sandboxPath = Path.Combine(SandboxRoot, taskId);

        if (!Directory.Exists(sandboxPath))
            return Task.FromResult("沙箱不存在");

        var cmdBase = command.Split(' ')[0].ToLower();
        if (!AllowedCommands.Any(c => c.Equals(cmdBase, StringComparison.OrdinalIgnoreCase)))
            return Task.FromResult($"命令 '{cmdBase}' 不在白名单中");

        _logger.Info($"沙箱执行命令: {taskId} -> {command}");
        return Task.FromResult($"Sandbox[{taskId}] > {command}\n结果: 模拟执行成功");
    }
}
