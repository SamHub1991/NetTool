using DeerFlow.WPF.Core;
using DeerFlow.WPF.Models;
using DeerFlow.WPF.Services;

namespace DeerFlow.WPF.ViewModels;

/// <summary>
/// 技能商店 ViewModel，管理技能列表和安装/卸载操作
/// </summary>
public class SkillsViewModel : ViewModelBase
{
    private readonly ILoggerService _logger;

    public AsyncObservableCollection<SkillItem> Skills { get; } = new();

    public RelayCommand RefreshSkillsCommand { get; }

    /// <summary>安装技能命令</summary>
    public RelayCommand<SkillItem> InstallSkillCommand { get; }

    public SkillsViewModel(ILoggerService logger)
    {
        _logger = logger;
        RefreshSkillsCommand = new RelayCommand(_ => LoadSkills());
        InstallSkillCommand = new RelayCommand<SkillItem>(async item =>
        {
            if (item != null) await InstallSkillAsync(item);
        }, item => item != null && !item.IsInstalled);

        LoadSkills();
    }

    private void LoadSkills()
    {
        Skills.Clear();
        Skills.AddRange(new[]
        {
            new SkillItem { Name = "深度研究", Description = "执行Web搜索并生成结构化研究报告", IsInstalled = true },
            new SkillItem { Name = "代码审查", Description = "分析代码质量并提供优化建议", IsInstalled = true },
            new SkillItem { Name = "前端设计", Description = "生成前端UI设计稿和样式建议", IsInstalled = false },
            new SkillItem { Name = "数据分析", Description = "执行数据统计分析并生成可视化", IsInstalled = false },
            new SkillItem { Name = "小说工坊", Description = "协助创作小说和故事内容", IsInstalled = false }
        });
    }

    /// <summary>
    /// 执行技能安装流程
    /// </summary>
    private async Task InstallSkillAsync(SkillItem skill)
    {
        _logger.Info($"开始安装技能: {skill.Name}");

        try
        {
            await Task.Delay(500);

            skill.IsInstalled = true;
            _logger.Info($"技能安装成功: {skill.Name}");
        }
        catch (Exception ex)
        {
            _logger.Error($"技能安装失败: {skill.Name}", ex);
        }
    }
}
