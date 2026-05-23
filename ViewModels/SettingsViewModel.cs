using System.IO;
using DeerFlow.WPF.Core;
using DeerFlow.WPF.Services;

namespace DeerFlow.WPF.ViewModels;

/// <summary>
/// 设置页面 ViewModel，管理系统配置的加载、保存和重置
/// </summary>
public class SettingsViewModel : ViewModelBase
{
    private readonly ILoggerService _logger;
    private readonly string _settingsPath;

    private const string SETTINGS_FILENAME = "settings.json";

    private string _modelProvider = "openai";
    public string ModelProvider
    {
        get => _modelProvider;
        set => SetProperty(ref _modelProvider, value);
    }

    private string _apiBaseUrl = "http://localhost:11434/v1";
    public string ApiBaseUrl
    {
        get => _apiBaseUrl;
        set => SetProperty(ref _apiBaseUrl, value);
    }

    private string _apiKey = string.Empty;
    public string ApiKey
    {
        get => _apiKey;
        set
        {
            if (SetProperty(ref _apiKey, value))
            {
                OnPropertyChanged(nameof(ApiKeyStatusText));
            }
        }
    }

    private bool _enableLocalInference = true;
    public bool EnableLocalInference
    {
        get => _enableLocalInference;
        set => SetProperty(ref _enableLocalInference, value);
    }

    private bool _enableSandbox = true;
    public bool EnableSandbox
    {
        get => _enableSandbox;
        set => SetProperty(ref _enableSandbox, value);
    }

    private bool _autoSaveMemory = true;
    public bool AutoSaveMemory
    {
        get => _autoSaveMemory;
        set => SetProperty(ref _autoSaveMemory, value);
    }

    private bool _isApiKeyVisible;
    /// <summary>API Key 是否以明文显示</summary>
    public bool IsApiKeyVisible
    {
        get => _isApiKeyVisible;
        set
        {
            if (SetProperty(ref _isApiKeyVisible, value))
            {
                OnPropertyChanged(nameof(IsApiKeyHidden));
            }
        }
    }

    /// <summary>API Key 是否以密码遮蔽（与 IsApiKeyVisible 互斥）</summary>
    public bool IsApiKeyHidden => !_isApiKeyVisible;

    /// <summary>API Key 状态描述文本，用于概览面板显示</summary>
    public string ApiKeyStatusText => string.IsNullOrWhiteSpace(ApiKey) ? "未配置（使用本地推理）" : "已配置";

    public RelayCommand SaveSettingsCommand { get; }
    public RelayCommand ResetSettingsCommand { get; }

    /// <summary>切换 API Key 可见性命令</summary>
    public RelayCommand ToggleApiKeyVisibilityCommand { get; }

    public SettingsViewModel(ILoggerService logger)
    {
        _logger = logger;
        _settingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DeerFlow.WPF", SETTINGS_FILENAME);

        SaveSettingsCommand = new RelayCommand(_ => SaveSettings());
        ResetSettingsCommand = new RelayCommand(_ => ResetSettings());
        ToggleApiKeyVisibilityCommand = new RelayCommand(_ => IsApiKeyVisible = !IsApiKeyVisible);

        LoadSettings();
    }

    /// <summary>
    /// 从磁盘加载设置
    /// </summary>
    private void LoadSettings()
    {
        try
        {
            if (!File.Exists(_settingsPath))
                return;

            var json = File.ReadAllText(_settingsPath);
            var doc = System.Text.Json.JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("ModelProvider", out var mp))
                ModelProvider = mp.GetString() ?? "openai";
            if (root.TryGetProperty("ApiBaseUrl", out var abu))
                ApiBaseUrl = abu.GetString() ?? "http://localhost:11434/v1";
            if (root.TryGetProperty("ApiKey", out var ak))
                ApiKey = ak.GetString() ?? string.Empty;
            if (root.TryGetProperty("EnableLocalInference", out var eli))
                EnableLocalInference = eli.GetBoolean();
            if (root.TryGetProperty("EnableSandbox", out var es))
                EnableSandbox = es.GetBoolean();
            if (root.TryGetProperty("AutoSaveMemory", out var asm))
                AutoSaveMemory = asm.GetBoolean();

            _logger.Info("设置已从磁盘加载");
        }
        catch (Exception ex)
        {
            _logger.Error("加载设置失败", ex);
        }
    }

    /// <summary>
    /// 保存设置到磁盘
    /// </summary>
    private void SaveSettings()
    {
        try
        {
            var dir = Path.GetDirectoryName(_settingsPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var settings = new
            {
                ModelProvider,
                ApiBaseUrl,
                ApiKey,
                EnableLocalInference,
                EnableSandbox,
                AutoSaveMemory
            };

            var json = System.Text.Json.JsonSerializer.Serialize(settings, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(_settingsPath, json);
            _logger.Info("设置已保存到磁盘");
        }
        catch (Exception ex)
        {
            _logger.Error("保存设置失败", ex);
        }
    }

    /// <summary>
    /// 重置设置为默认值
    /// </summary>
    private void ResetSettings()
    {
        ModelProvider = "openai";
        ApiBaseUrl = "http://localhost:11434/v1";
        ApiKey = string.Empty;
        EnableLocalInference = true;
        EnableSandbox = true;
        AutoSaveMemory = true;
        _logger.Info("设置已重置为默认值");
    }
}
