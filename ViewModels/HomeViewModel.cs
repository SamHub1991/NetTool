using System.Collections.ObjectModel;
using DeerFlow.WPF.Core;
using DeerFlow.WPF.Models;
using DeerFlow.WPF.Services;

namespace DeerFlow.WPF.ViewModels;

/// <summary>
/// 首页 ViewModel，展示应用概览和快速操作入口
/// </summary>
public class HomeViewModel : ViewModelBase
{
    private readonly ITaskWindowManager _taskManager;
    private readonly ILoggerService _logger;
    private readonly Action<string>? _navigateTo;
    private readonly Action? _createTask;

    private string _welcomeMessage = "欢迎使用 DeerFlow.WPF";
    public string WelcomeMessage
    {
        get => _welcomeMessage;
        set => SetProperty(ref _welcomeMessage, value);
    }

    private string _quickStatus = "系统就绪";
    public string QuickStatus
    {
        get => _quickStatus;
        set => SetProperty(ref _quickStatus, value);
    }

    public ObservableCollection<TaskModel> RecentTasks { get; } = new();

    public RelayCommand NewChatCommand { get; }
    public RelayCommand NewTaskCommand { get; }
    public RelayCommand OpenSkillsCommand { get; }

    public HomeViewModel(
        ITaskWindowManager taskManager,
        ILoggerService logger,
        Action<string>? navigateTo = null,
        Action? createTask = null)
    {
        _taskManager = taskManager;
        _logger = logger;
        _navigateTo = navigateTo;
        _createTask = createTask;

        NewChatCommand = new RelayCommand(_ => _navigateTo?.Invoke("Chat"));
        NewTaskCommand = new RelayCommand(_ => _createTask?.Invoke());
        OpenSkillsCommand = new RelayCommand(_ => _navigateTo?.Invoke("Skills"));

        RefreshRecentTasks();
    }

    private void RefreshRecentTasks()
    {
        RecentTasks.Clear();
        foreach (var task in _taskManager.GetActiveTasks().Take(5))
        {
            RecentTasks.Add(task);
        }
        QuickStatus = $"活跃任务: {RecentTasks.Count} 个";
    }
}
