using System.Diagnostics;
using System.IO;
using System.Windows;
using DeerFlow.WPF.Core;

namespace DeerFlow.WPF.ViewModels;

/// <summary>
/// 任务窗口 ViewModel，管理沙箱进程执行、命令行处理、状态更新
/// 符合 MVVM 架构，业务逻辑完全与 UI 解耦
/// </summary>
public class TaskWindowViewModel : ViewModelBase
{
    private string _taskId = string.Empty;
    /// <summary>任务唯一标识</summary>
    public string TaskId
    {
        get => _taskId;
        set => SetProperty(ref _taskId, value);
    }

    private string _sandboxPath = string.Empty;
    /// <summary>沙箱工作目录</summary>
    public string SandboxPath
    {
        get => _sandboxPath;
        set => SetProperty(ref _sandboxPath, value);
    }

    private string _agentProvider = string.Empty;
    /// <summary>智能体提供方</summary>
    public string AgentProvider
    {
        get => _agentProvider;
        set => SetProperty(ref _agentProvider, value);
    }

    private string _inputText = string.Empty;
    /// <summary>输入框文本</summary>
    public string InputText
    {
        get => _inputText;
        set => SetProperty(ref _inputText, value);
    }

    private string _outputText = "DeerFlow Task Sandbox\r\n====================\r\n\r\n沙箱已初始化，等待任务输入...\r\n\r\n";
    /// <summary>输出文本</summary>
    public string OutputText
    {
        get => _outputText;
        set => SetProperty(ref _outputText, value);
    }

    private string _statusText = "状态: 就绪";
    /// <summary>状态文本</summary>
    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }

    private bool _isExecuting;
    /// <summary>是否正在执行</summary>
    public bool IsExecuting
    {
        get => _isExecuting;
        set => SetProperty(ref _isExecuting, value);
    }

    /// <summary>执行命令 — 支持从命令行参数初始化</summary>
    public RelayCommand ExecuteCommand { get; }

    /// <summary>清空输出命令</summary>
    public RelayCommand ClearOutputCommand { get; }

    public TaskWindowViewModel()
    {
        ExecuteCommand = new RelayCommand(_ => ExecuteSandboxCommand(), _ => !IsExecuting && !string.IsNullOrWhiteSpace(InputText));
        ClearOutputCommand = new RelayCommand(_ => OutputText = string.Empty);

        // 从命令行参数解析
        ParseCommandLineArgs();
    }

    /// <summary>
    /// 解析命令行参数：--task-id, --sandbox-path, --agent-provider
    /// </summary>
    private void ParseCommandLineArgs()
    {
        var args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
        {
            switch (args[i])
            {
                case "--task-id":
                    TaskId = args[++i];
                    break;
                case "--sandbox-path":
                    SandboxPath = args[++i];
                    break;
                case "--agent-provider":
                    AgentProvider = args[++i];
                    break;
            }
        }
    }

    /// <summary>
    /// 在沙箱中执行命令
    /// </summary>
    private void ExecuteSandboxCommand()
    {
        var command = InputText?.Trim();
        if (string.IsNullOrWhiteSpace(command)) return;

        AppendOutput($"> {command}\n");

        if (string.IsNullOrEmpty(SandboxPath) || !Directory.Exists(SandboxPath))
        {
            AppendOutput("错误: 沙箱目录未初始化\n");
            StatusText = "状态: 错误";
            return;
        }

        IsExecuting = true;

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c {command}",
                WorkingDirectory = SandboxPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process is null)
            {
                AppendOutput("错误: 无法启动进程\n");
                StatusText = "状态: 错误";
                return;
            }

            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (!string.IsNullOrEmpty(output))
                AppendOutput(output);

            if (!string.IsNullOrEmpty(error))
                AppendOutput($"[错误] {error}");

            StatusText = $"状态: 退出码 {process.ExitCode}";
        }
        catch (Exception ex)
        {
            AppendOutput($"[异常] {ex.Message}\n");
            StatusText = "状态: 异常";
        }
        finally
        {
            IsExecuting = false;
            InputText = string.Empty;
        }
    }

    /// <summary>
    /// 追加文本到输出区域
    /// </summary>
    private void AppendOutput(string text)
    {
        OutputText += text;
    }
}
