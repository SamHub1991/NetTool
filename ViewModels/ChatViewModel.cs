using DeerFlow.WPF.Core;
using DeerFlow.WPF.Models;
using DeerFlow.WPF.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace DeerFlow.WPF.ViewModels;

/// <summary>
/// 聊天对话 ViewModel，管理消息流、流式响应和资源生命周期
/// 支持 Semantic Kernel 驱动（优先）和 ApiService 直接调用两种模式
/// </summary>
public class ChatViewModel : ViewModelBase
{
    private readonly IApiService _apiService;
    private readonly ITaskWindowManager _taskManager;
    private readonly ILoggerService _logger;
    private readonly Kernel? _kernel;
    private readonly bool _useSK;
    private CancellationTokenSource? _sendCts;

    public AsyncObservableCollection<ChatMessage> Messages { get; } = new();

    private string _inputText = string.Empty;
    public string InputText
    {
        get => _inputText;
        set
        {
            if (SetProperty(ref _inputText, value))
            {
                SendCommand.RaiseCanExecuteChanged();
            }
        }
    }

    private bool _isSending;
    public bool IsSending
    {
        get => _isSending;
        set
        {
            if (SetProperty(ref _isSending, value))
            {
                SendCommand.RaiseCanExecuteChanged();
                StopGenerationCommand.RaiseCanExecuteChanged();
            }
        }
    }

    private AgentConfig _agentConfig = new();
    public AgentConfig AgentConfig
    {
        get => _agentConfig;
        set => SetProperty(ref _agentConfig, value);
    }

    public RelayCommand SendCommand { get; }
    public RelayCommand ClearChatCommand { get; }
    public RelayCommand StopGenerationCommand { get; }

    public ChatViewModel(IApiService apiService, ITaskWindowManager taskManager, ILoggerService logger)
    {
        _apiService = apiService;
        _taskManager = taskManager;
        _logger = logger;

        // 尝试解析 Kernel，如果存在 AI 服务则启用 SK 模式
        _kernel = App.Services.GetService<Kernel>();
        _useSK = _kernel is not null && _kernel.GetAllServices<IChatCompletionService>().Any();

        SendCommand = new RelayCommand(new Action(async () => await SendMessageAsync()), () => !IsSending && !string.IsNullOrWhiteSpace(InputText));
        ClearChatCommand = new RelayCommand(_ => Messages.Clear());
        StopGenerationCommand = new RelayCommand(new Action(CancelSend), () => IsSending);

        var introMessage = _useSK
            ? "你好，我是 DeerFlow.WPF 智能助手（SK 增强模式）。我可以执行沙箱代码、搜索网络、管理记忆，还能自动调用工具完成复杂任务。有什么需要帮助的？"
            : "你好，我是 DeerFlow.WPF 智能助手。我可以帮你执行任务、分析代码、管理项目。有什么需要帮助的？";

        Messages.Add(new ChatMessage
        {
            Role = "system",
            Content = introMessage,
            Timestamp = DateTime.Now
        });
    }

    /// <summary>
    /// 发送消息并处理流式响应，SK 模式优先，ApiService 模式兜底
    /// </summary>
    private async Task SendMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(InputText) || IsSending)
            return;

        var userMessage = new ChatMessage
        {
            Role = "user",
            Content = InputText
        };

        Messages.Add(userMessage);
        InputText = string.Empty;
        IsSending = true;

        var assistantMessage = new ChatMessage
        {
            Role = "assistant",
            Content = string.Empty,
            IsStreaming = true
        };
        Messages.Add(assistantMessage);

        _sendCts?.Dispose();
        _sendCts = new CancellationTokenSource();

        try
        {
            if (_useSK && _kernel is not null)
            {
                await SendMessageViaSKAsync(userMessage.Content, assistantMessage);
            }
            else
            {
                await SendMessageViaApiAsync(userMessage.Content, assistantMessage);
            }

            assistantMessage.IsStreaming = false;
            _logger.Info("消息回复完成");
        }
        catch (OperationCanceledException)
        {
            assistantMessage.IsStreaming = false;
            assistantMessage.Content = string.IsNullOrEmpty(assistantMessage.Content)
                ? "（生成已取消）"
                : assistantMessage.Content + "\n\n（生成已取消）";
            _logger.Info("消息生成已取消");
        }
        catch (Exception ex)
        {
            assistantMessage.Content = $"请求失败: {ex.Message}";
            assistantMessage.IsStreaming = false;
            _logger.Error("发送消息失败", ex);
        }
        finally
        {
            IsSending = false;
            _sendCts?.Dispose();
            _sendCts = null;
        }
    }

    /// <summary>
    /// 通过 Semantic Kernel 发送消息（支持自动工具调用）
    /// </summary>
    private async Task SendMessageViaSKAsync(string userInput, ChatMessage assistantMessage)
    {
        if (_kernel is null) return;
        var executionSettings = new OpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(autoInvoke: true),
            Temperature = AgentConfig.Temperature,
            MaxTokens = AgentConfig.MaxTokens
        };

        var arguments = new KernelArguments(executionSettings)
        {
            ["input"] = userInput
        };

        var chatFunction = _kernel!.CreateFunctionFromPrompt(
            "{{$input}}",
            executionSettings,
            functionName: "chat",
            description: "鹿流AI聊天");

        var fullResponse = new System.Text.StringBuilder();

        await foreach (var chunk in _kernel.InvokeStreamingAsync<StreamingKernelContent>(
            chatFunction, arguments, _sendCts!.Token))
        {
            fullResponse.Append(chunk.ToString());
            assistantMessage.Content = fullResponse.ToString();
        }

        assistantMessage.Content = fullResponse.ToString();
    }

    /// <summary>
    /// 通过 ApiService 直接发送消息（SK 不可用时的兜底方案）
    /// </summary>
    private async Task SendMessageViaApiAsync(string userInput, ChatMessage assistantMessage)
    {
        await foreach (var chunk in _apiService.SendChatStreamAsync(
            userInput, AgentConfig, null, _sendCts!.Token))
        {
            assistantMessage.Content += chunk;
        }
    }

    /// <summary>
    /// 取消当前生成
    /// </summary>
    private void CancelSend()
    {
        _sendCts?.Cancel();
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _sendCts?.Cancel();
            _sendCts?.Dispose();
            _sendCts = null;
        }

        base.Dispose(disposing);
    }
}
