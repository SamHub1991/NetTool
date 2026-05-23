using System.Windows;
using System.Windows.Controls;
using DeerFlow.WPF.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DeerFlow.WPF.Views.Controls;

public partial class DocumentPreviewControl : UserControl
{
    private readonly IDocumentGenerationService? _documentService;
    private string _currentContent = string.Empty;
    private string _documentType = "Health";

    public DocumentPreviewControl()
    {
        InitializeComponent();
        _documentService = App.Services.GetService<IDocumentGenerationService>();
    }

    private void DocumentTypeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DocumentTypeSelector.SelectedItem is ComboBoxItem item && item.Tag is string tag)
        {
            _documentType = tag;
        }
    }

    private async void GenerateButton_Click(object sender, RoutedEventArgs e)
    {
        if (_documentService is null)
        {
            StatusText.Text = "错误：文档生成服务未初始化";
            return;
        }

        try
        {
            StatusText.Text = "正在生成文档...";
            GenerateButton.IsEnabled = false;

            string content;

            switch (_documentType)
            {
                case "Health":
                    var stats = CollectSystemStatistics();
                    content = await _documentService.GenerateSystemHealthReportAsync(stats);
                    break;

                case "Patterns":
                    var patterns = CollectPatterns();
                    content = await _documentService.GeneratePatternDocumentationAsync(patterns);
                    break;

                case "Experiences":
                    var experiences = CollectExperiences();
                    content = await _documentService.GenerateExperienceDocumentationAsync(experiences);
                    break;

                case "Reflections":
                    var reflections = CollectReflections();
                    var endDate = DateTime.Now;
                    var startDate = endDate.AddDays(-30);
                    content = await _documentService.GenerateReflectionReportAsync(
                        reflections, startDate, endDate);
                    break;

                default:
                    content = "Unknown document type";
                    break;
            }

            _currentContent = content;
            DocumentContent.Text = content;
            WordCountText.Text = $"{content.Length} 字";
            StatusText.Text = "✓ 生成完成";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"✗ 生成失败：{ex.Message}";
        }
        finally
        {
            GenerateButton.IsEnabled = true;
        }
    }

    private async void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        if (_documentService is null || string.IsNullOrEmpty(_currentContent))
        {
            StatusText.Text = "错误：无内容可导出";
            return;
        }

        try
        {
            StatusText.Text = "正在导出...";
            ExportButton.IsEnabled = false;

            var fileName = $"{_documentType}_{DateTime.Now:yyyyMMdd_HHmmss}.md";
            var filePath = await _documentService.ExportDocumentAsync(_currentContent, fileName);

            if (!string.IsNullOrEmpty(filePath))
            {
                StatusText.Text = $"✓ 已导出到：{filePath}";
            }
            else
            {
                StatusText.Text = "✗ 导出失败";
            }
        }
        catch (Exception ex)
        {
            StatusText.Text = $"✗ 导出失败：{ex.Message}";
        }
        finally
        {
            ExportButton.IsEnabled = true;
        }
    }

    private Dictionary<string, object> CollectSystemStatistics()
    {
        // 从各服务收集统计数据
        var stats = new Dictionary<string, object>();

        try
        {
            var patternMiner = App.Services.GetService<IPatternMiner>();
            if (patternMiner is not null)
            {
                var patternStats = patternMiner.GetPatternStatistics();
                foreach (var kvp in patternStats)
                {
                    stats[$"Patterns.{kvp.Key}"] = kvp.Value;
                }
            }

            var experienceStore = App.Services.GetService<IExperienceMemoryStore>();
            if (experienceStore is not null)
            {
                var expStats = experienceStore.GetStatistics();
                foreach (var kvp in expStats)
                {
                    stats[$"Experiences.{kvp.Key}"] = kvp.Value;
                }
            }

            var autoEvolver = App.Services.GetService<IAutoEvolver>();
            if (autoEvolver is not null)
            {
                var expStats = autoEvolver.GetExperimentStatistics();
                foreach (var kvp in expStats)
                {
                    stats[$"Experiments.{kvp.Key}"] = kvp.Value;
                }
            }

            var feedbackService = App.Services.GetService<IFeedbackService>();
            if (feedbackService is not null)
            {
                var fbStats = feedbackService.GetFeedbackStatistics();
                foreach (var kvp in fbStats)
                {
                    stats[$"Feedback.{kvp.Key}"] = kvp.Value;
                }
            }
        }
        catch (Exception ex)
        {
            StatusText.Text = $"警告：统计数据收集部分失败 - {ex.Message}";
        }

        return stats;
    }

    private List<Models.PatternItem> CollectPatterns()
    {
        try
        {
            var patternMiner = App.Services.GetService<IPatternMiner>();
            return patternMiner?.GetPatterns(limit: 100).ToList() ?? new List<Models.PatternItem>();
        }
        catch
        {
            return new List<Models.PatternItem>();
        }
    }

    private List<Models.ExperienceMemoryItem> CollectExperiences()
    {
        try
        {
            var experienceStore = App.Services.GetService<IExperienceMemoryStore>();
            return experienceStore?.GetAllExperiences(limit: 100).ToList() ?? new List<Models.ExperienceMemoryItem>();
        }
        catch
        {
            return new List<Models.ExperienceMemoryItem>();
        }
    }

    private List<Models.SelfReflectionItem> CollectReflections()
    {
        try
        {
            var reflectionService = App.Services.GetService<ISelfReflectionService>();
            return reflectionService?.GetReflections(limit: 200).ToList() ?? new List<Models.SelfReflectionItem>();
        }
        catch
        {
            return new List<Models.SelfReflectionItem>();
        }
    }
}
