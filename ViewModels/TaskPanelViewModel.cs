using System.Windows;
using DeerFlow.WPF.Core;
using DeerFlow.WPF.Models;
using DeerFlow.WPF.Services;

namespace DeerFlow.WPF.ViewModels;

/// <summary>
/// 任务面板 ViewModel，管理所有任务窗口的生命周期
/// </summary>
public class TaskPanelViewModel : ViewModelBase
{
    private readonly ITaskWindowManager _taskManager;
    private readonly ILoggerService _logger;

    public AsyncObservableCollection<TaskModel> Tasks { get; } = new();

    private string _newTaskName = string.Empty;
    public string NewTaskName
    {
        get => _newTaskName;
        set => SetProperty(ref _newTaskName, value);
    }

    private string _newTaskDescription = string.Empty;
    public string NewTaskDescription
    {
        get => _newTaskDescription;
        set => SetProperty(ref _newTaskDescription, value);
    }

    public RelayCommand CreateTaskCommand { get; }
    public RelayCommand<TaskModel> CloseTaskCommand { get; }
    public RelayCommand<TaskModel> PauseTaskCommand { get; }
    public RelayCommand<TaskModel> ResumeTaskCommand { get; }

    public TaskPanelViewModel(ITaskWindowManager taskManager, ILoggerService logger)
    {
        _taskManager = taskManager;
        _logger = logger;

        CreateTaskCommand = new RelayCommand(async _ => await CreateTaskAsync());
        CloseTaskCommand = new RelayCommand<TaskModel>(async t => { if (t != null) await CloseTaskAsync(t); });
        PauseTaskCommand = new RelayCommand<TaskModel>(async t => { if (t != null) await _taskManager.PauseTaskAsync(t.Id); });
        ResumeTaskCommand = new RelayCommand<TaskModel>(async t => { if (t != null) await _taskManager.ResumeTaskAsync(t.Id); });

        _taskManager.TaskStatusChanged += OnTaskStatusChanged;
    }

    private async Task CreateTaskAsync()
    {
        if (string.IsNullOrWhiteSpace(NewTaskName))
            return;

        var task = new TaskModel
        {
            Name = NewTaskName,
            Description = NewTaskDescription
        };

        await _taskManager.CreateTaskWindowAsync(task);
        NewTaskName = string.Empty;
        NewTaskDescription = string.Empty;
        RefreshTasks();
    }

    private async Task CloseTaskAsync(TaskModel task)
    {
        await _taskManager.CloseTaskWindowAsync(task.Id);
        RefreshTasks();
    }

    private void RefreshTasks()
    {
        Tasks.ReplaceAll(_taskManager.GetActiveTasks());
    }

    private void OnTaskStatusChanged(TaskModel task)
    {
        Application.Current.Dispatcher.Invoke(RefreshTasks);
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _taskManager.TaskStatusChanged -= OnTaskStatusChanged;
        }

        base.Dispose(disposing);
    }
}
