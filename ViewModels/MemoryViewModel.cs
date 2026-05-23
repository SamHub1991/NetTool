using DeerFlow.WPF.Core;
using DeerFlow.WPF.Models;
using DeerFlow.WPF.Services;

namespace DeerFlow.WPF.ViewModels;

/// <summary>
/// 记忆管理 ViewModel，支持搜索、筛选和清空记忆
/// </summary>
public class MemoryViewModel : ViewModelBase
{
    private readonly ILoggerService _logger;

    /// <summary>全部记忆数据源（搜索的基准）</summary>
    private readonly List<MemoryItem> _allMemories = new();

    public AsyncObservableCollection<MemoryItem> Memories { get; } = new();

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    private bool _hasNoResults;
    /// <summary>搜索无结果提示</summary>
    public bool HasNoResults
    {
        get => _hasNoResults;
        set => SetProperty(ref _hasNoResults, value);
    }

    public RelayCommand SearchCommand { get; }
    public RelayCommand ClearMemoriesCommand { get; }

    public MemoryViewModel(ILoggerService logger)
    {
        _logger = logger;
        SearchCommand = new RelayCommand(_ => PerformSearch());
        ClearMemoriesCommand = new RelayCommand(_ =>
        {
            _allMemories.Clear();
            Memories.Clear();
            HasNoResults = false;
        });

        _allMemories.Add(new MemoryItem { Content = "用户偏好使用 C# 和 WPF 技术栈", Type = "preference", Importance = 8 });
        _allMemories.Add(new MemoryItem { Content = "项目需要集成 AIO Sandbox 实现任务隔离", Type = "fact", Importance = 9 });
        _allMemories.Add(new MemoryItem { Content = "用户习惯使用 Visual Studio 作为主力 IDE", Type = "preference", Importance = 6 });
        _allMemories.Add(new MemoryItem { Content = "飞书 Webhook 地址格式: https://open.feishu.cn/open-apis/bot/v2/hook/xxx", Type = "fact", Importance = 7 });
        _allMemories.Add(new MemoryItem { Content = "上次对话讨论了 Agent 编排中间件的设计方案", Type = "context", Importance = 5 });

        RefreshDisplay(_allMemories);
    }

    /// <summary>
    /// 执行搜索，支持模糊匹配和精确匹配
    /// </summary>
    private void PerformSearch()
    {
        var keyword = SearchText?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(keyword))
        {
            RefreshDisplay(_allMemories);
            return;
        }

        var results = _allMemories
            .Where(m => FuzzyMatch(m.Content, keyword)
                     || FuzzyMatch(m.Type, keyword)
                     || m.Tags.Any(t => FuzzyMatch(t, keyword)))
            .OrderByDescending(m => m.Importance)
            .ThenByDescending(m => m.CreatedAt)
            .ToList();

        RefreshDisplay(results);
        _logger.Info($"搜索记忆: '{keyword}'，找到 {results.Count} 条结果");
    }

    /// <summary>
    /// 模糊匹配：忽略大小写，支持部分匹配
    /// </summary>
    private static bool FuzzyMatch(string source, string keyword)
    {
        return source.Contains(keyword, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 刷新显示列表
    /// </summary>
    private void RefreshDisplay(List<MemoryItem> items)
    {
        Memories.ReplaceAll(items);
        HasNoResults = items.Count == 0 && _allMemories.Count > 0;
    }
}
