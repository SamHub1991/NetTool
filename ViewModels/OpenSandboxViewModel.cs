using DeerFlow.WPF.Core;
using DeerFlow.WPF.Services;
using Microsoft.Extensions.Logging;
using OpenSandbox.Models;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace DeerFlow.WPF.ViewModels;

/// <summary>
/// OpenSandbox 管理视图模型，提供沙箱创建、销毁、执行和文件操作的 UI 绑定
/// </summary>
public class OpenSandboxViewModel : ViewModelBase
{
    private readonly IOpenSandboxService _sandboxService;
    private readonly ILogger _logger;
    private readonly CancellationTokenSource _pollCts = new();
    private string _apiKey = string.Empty;
    private string _apiDomain = "api.opensandbox.io";
    private string _sandboxImage = "ubuntu";
    private int _timeoutSeconds = 600;
    private string _commandInput = string.Empty;
    private string _outputText = string.Empty;
    private string _statusMessage = "未初始化";
    private bool _isConnected;
    private bool _isCreating;
    private bool _isExecuting;
    private SandboxLanguage _selectedLanguage = SandboxLanguage.Python;

    public string ApiKey
    {
        get => _apiKey;
        set => SetProperty(ref _apiKey, value);
    }

    public string ApiDomain
    {
        get => _apiDomain;
        set => SetProperty(ref _apiDomain, value);
    }

    public string SandboxImage
    {
        get => _sandboxImage;
        set => SetProperty(ref _sandboxImage, value);
    }

    public int TimeoutSeconds
    {
        get => _timeoutSeconds;
        set => SetProperty(ref _timeoutSeconds, value);
    }

    public string CommandInput
    {
        get => _commandInput;
        set => SetProperty(ref _commandInput, value);
    }

    public string OutputText
    {
        get => _outputText;
        private set => SetProperty(ref _outputText, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public bool IsConnected
    {
        get => _isConnected;
        private set => SetProperty(ref _isConnected, value);
    }

    public bool IsCreating
    {
        get => _isCreating;
        private set => SetProperty(ref _isCreating, value);
    }

    public bool IsExecuting
    {
        get => _isExecuting;
        private set => SetProperty(ref _isExecuting, value);
    }

    public SandboxLanguage SelectedLanguage
    {
        get => _selectedLanguage;
        set => SetProperty(ref _selectedLanguage, value);
    }

    public SandboxLanguage[] Languages => (SandboxLanguage[])System.Enum.GetValues(typeof(SandboxLanguage));

    public RelayCommand ConfigureCommand { get; }
    public RelayCommand CreateCommand { get; }
    public RelayCommand DestroyCommand { get; }
    public RelayCommand ExecuteCommand { get; }
    public RelayCommand ClearOutputCommand { get; }
    public RelayCommand RenewCommand { get; }

    public OpenSandboxViewModel(IOpenSandboxService sandboxService, ILoggerFactory loggerFactory)
    {
        _sandboxService = sandboxService;
        _logger = loggerFactory.CreateLogger("OpenSandbox.UI");

        ConfigureCommand = new RelayCommand(async () => await ConfigureAsync());
        CreateCommand = new RelayCommand(async () => await CreateAsync());
        DestroyCommand = new RelayCommand(async () => await DestroyAsync());
        ExecuteCommand = new RelayCommand(async () => await ExecuteAsync());
        ClearOutputCommand = new RelayCommand(() => OutputText = string.Empty);
        RenewCommand = new RelayCommand(async () => await RenewAsync());

        // 监听服务状态变化
        Task.Run(async () =>
        {
            while (!_pollCts.IsCancellationRequested)
            {
                try
                {
                    var status = _sandboxService.Status;
                    var connected = _sandboxService.IsConnected;
                    var sandboxId = _sandboxService.SandboxId;

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        StatusMessage = status;
                        IsConnected = connected;
                    });

                    await Task.Delay(1000, _pollCts.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        });
    }

    /// <summary>
    /// 配置 OpenSandbox 连接
    /// </summary>
    private async Task ConfigureAsync()
    {
        try
        {
            StatusMessage = "配置中...";
            await _sandboxService.ConfigureAsync(ApiKey, ApiDomain);
            StatusMessage = "已配置";
            _logger.LogInformation("OpenSandbox 连接已配置: {Domain}", ApiDomain);
        }
        catch (Exception ex)
        {
            StatusMessage = $"配置失败: {ex.Message}";
            _logger.LogError(ex, "OpenSandbox 配置失败");
            MessageBox.Show($"配置失败:\n{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// 创建沙箱实例
    /// </summary>
    private async Task CreateAsync()
    {
        try
        {
            IsCreating = true;
            StatusMessage = "创建沙箱中...";
            AppendOutput("正在创建沙箱实例...\n");

            await _sandboxService.CreateSandboxAsync(SandboxImage, TimeoutSeconds);

            AppendOutput($"沙箱已创建: ID={_sandboxService.SandboxId}\n");
            AppendOutput("状态: 运行中\n");
            _logger.LogInformation("沙箱创建成功: {Id}", _sandboxService.SandboxId);
        }
        catch (Exception ex)
        {
            AppendOutput($"创建失败: {ex.Message}\n");
            StatusMessage = $"创建失败: {ex.Message}";
            _logger.LogError(ex, "沙箱创建失败");
            MessageBox.Show($"创建沙箱失败:\n{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsCreating = false;
        }
    }

    /// <summary>
    /// 销毁当前沙箱
    /// </summary>
    private async Task DestroyAsync()
    {
        if (!_sandboxService.IsConnected)
        {
            MessageBox.Show("当前没有运行中的沙箱", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var result = MessageBox.Show("确定要销毁当前沙箱吗？\n此操作不可逆。", "确认", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            AppendOutput("正在销毁沙箱...\n");
            await _sandboxService.DestroyAsync();
            AppendOutput("沙箱已销毁\n");
            _logger.LogInformation("沙箱已销毁");
        }
        catch (Exception ex)
        {
            AppendOutput($"销毁失败: {ex.Message}\n");
            _logger.LogError(ex, "沙箱销毁失败");
        }
    }

    /// <summary>
    /// 执行命令或代码
    /// </summary>
    private async Task ExecuteAsync()
    {
        if (!_sandboxService.IsConnected)
        {
            MessageBox.Show("请先创建沙箱", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var command = CommandInput.Trim();
        if (string.IsNullOrEmpty(command))
        {
            return;
        }

        try
        {
            IsExecuting = true;
            AppendOutput($"> [{SelectedLanguage}] {command}\n");

            var outputBuffer = new System.Text.StringBuilder();
            var errorBuffer = new System.Text.StringBuilder();

            await _sandboxService.ExecuteCodeAsync(
                command,
                SelectedLanguage,
                output =>
                {
                    outputBuffer.AppendLine(output);
                    AppendOutput(output);
                },
                error =>
                {
                    errorBuffer.AppendLine(error);
                    AppendOutput($"[错误] {error}");
                });

            AppendOutput("\n--- 执行完成 ---\n");
            _logger.LogInformation("代码执行完成: {Language}", SelectedLanguage);
        }
        catch (Exception ex)
        {
            AppendOutput($"\n执行失败: {ex.Message}\n");
            _logger.LogError(ex, "代码执行失败");
        }
        finally
        {
            IsExecuting = false;
            CommandInput = string.Empty;
        }
    }

    /// <summary>
    /// 沙箱续期
    /// </summary>
    private async Task RenewAsync()
    {
        try
        {
            await _sandboxService.RenewAsync(3600);
            AppendOutput("沙箱已续期: +3600s\n");
            _logger.LogInformation("沙箱续期成功");
        }
        catch (Exception ex)
        {
            AppendOutput($"续期失败: {ex.Message}\n");
            _logger.LogError(ex, "沙箱续期失败");
        }
    }

    /// <summary>
    /// 追加文本到输出区域
    /// </summary>
    private void AppendOutput(string text)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            OutputText += text;
        });
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _pollCts.Cancel();
            _pollCts.Dispose();
            _sandboxService.Dispose();
        }

        base.Dispose(disposing);
    }
}
