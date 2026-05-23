using DeerFlow.WPF.Core;
using DeerFlow.WPF.Services;

namespace DeerFlow.WPF.ViewModels;

/// <summary>
/// 智能体编排 ViewModel，管理子智能体编排
/// </summary>
public class AgentOrchestrationViewModel : ViewModelBase
{
    private readonly ITaskWindowManager _taskManager;
    private readonly ILoggerService _logger;

    private string _workflowName = string.Empty;
    public string WorkflowName
    {
        get => _workflowName;
        set => SetProperty(ref _workflowName, value);
    }

    private string _workflowSteps = string.Empty;
    public string WorkflowSteps
    {
        get => _workflowSteps;
        set => SetProperty(ref _workflowSteps, value);
    }

    private int _maxSubAgents = 3;
    public int MaxSubAgents
    {
        get => _maxSubAgents;
        set => SetProperty(ref _maxSubAgents, value);
    }

    public RelayCommand CreateWorkflowCommand { get; }
    public RelayCommand StartWorkflowCommand { get; }
    public RelayCommand IncreaseMaxSubAgentsCommand { get; }
    public RelayCommand DecreaseMaxSubAgentsCommand { get; }

    /// <summary>最大子智能体数的最小值</summary>
    private const int MIN_SUB_AGENTS = 1;

    /// <summary>最大子智能体数的最大值</summary>
    private const int MAX_SUB_AGENTS = 20;

    public AgentOrchestrationViewModel(ITaskWindowManager taskManager, ILoggerService logger)
    {
        _taskManager = taskManager;
        _logger = logger;

        CreateWorkflowCommand = new RelayCommand(_ => _logger.Info($"创建编排: {WorkflowName}"));
        StartWorkflowCommand = new RelayCommand(_ => _logger.Info("启动编排工作流"));
        IncreaseMaxSubAgentsCommand = new RelayCommand(_ =>
        {
            if (MaxSubAgents < MAX_SUB_AGENTS)
                MaxSubAgents++;
        });
        DecreaseMaxSubAgentsCommand = new RelayCommand(_ =>
        {
            if (MaxSubAgents > MIN_SUB_AGENTS)
                MaxSubAgents--;
        });
    }
}
