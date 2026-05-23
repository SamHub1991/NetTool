using System;
using System.Collections.Generic;
using System.Linq;
using DeerFlow.WPF.Models;
using DeerFlow.WPF.Services;
using DeerFlow.WPF.Services.Plugins;
using Moq;
using Xunit;

namespace DeerFlow.WPF.Tests.Services;

public class SelfImprovementPluginTests
{
    private readonly Mock<ISelfReflectionService> _reflectionMock;
    private readonly Mock<IPatternMiner> _patternMinerMock;
    private readonly Mock<IAutoEvolver> _evolverMock;
    private readonly Mock<IExperienceMemoryStore> _experienceMock;
    private readonly SelfImprovementPlugin _plugin;

    public SelfImprovementPluginTests()
    {
        _reflectionMock = new Mock<ISelfReflectionService>();
        _patternMinerMock = new Mock<IPatternMiner>();
        _evolverMock = new Mock<IAutoEvolver>();
        _experienceMock = new Mock<IExperienceMemoryStore>();

        _plugin = new SelfImprovementPlugin(
            _reflectionMock.Object,
            _patternMinerMock.Object,
            _evolverMock.Object,
            _experienceMock.Object);
    }

    [Fact]
    public async Task ReflectOnRecentTasks_ReturnsSummary_WithReflections()
    {
        // Arrange
        var reflections = new List<SelfReflectionItem>
        {
            new SelfReflectionItem
            {
                TaskId = "1",
                TaskDescription = "任务 1",
                TaskType = "code",
                Outcome = "Success",
                SuccessRate = 0.9,
                ExecutionTimeMs = 1000
            },
            new SelfReflectionItem
            {
                TaskId = "2",
                TaskDescription = "任务 2",
                TaskType = "code",
                Outcome = "Success",
                SuccessRate = 0.85,
                ExecutionTimeMs = 1500
            }
        };

        _reflectionMock
            .Setup(x => x.GetReflections(It.IsAny<string>(), 10))
            .Returns(reflections);

        // Act
        var result = await _plugin.ReflectOnRecentTasks("code", 10);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("任务数量", result);
        Assert.Contains("2", result);
    }

    [Fact]
    public async Task ReflectOnRecentTasks_ReturnsEmptyMessage_WhenNoReflections()
    {
        // Arrange
        _reflectionMock
            .Setup(x => x.GetReflections(It.IsAny<string>(), 10))
            .Returns(new List<SelfReflectionItem>());

        // Act
        var result = await _plugin.ReflectOnRecentTasks();

        // Assert
        Assert.NotNull(result);
        Assert.Contains("暂无反思记录", result);
    }

    [Fact]
    public async Task MinePatterns_ReturnsPatterns_WhenFound()
    {
        // Arrange
        var patterns = new List<PatternItem>
        {
            new PatternItem
            {
                Name = "测试模式",
                Category = "代码执行",
                Score = 0.9,
                UsageCount = 10
            }
        };

        _patternMinerMock
            .Setup(x => x.MinePatternsAsync(3, 0.7, default))
            .ReturnsAsync(patterns);

        // Act
        var result = await _plugin.MinePatterns(3, 0.7);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("新挖掘模式", result);
        Assert.Contains("测试模式", result);
    }

    [Fact]
    public async Task MinePatterns_ReturnsEmptyMessage_WhenNoPatterns()
    {
        // Arrange
        _patternMinerMock
            .Setup(x => x.MinePatternsAsync(It.IsAny<int>(), It.IsAny<double>(), default))
            .ReturnsAsync(new List<PatternItem>());

        // Act
        var result = await _plugin.MinePatterns();

        // Assert
        Assert.NotNull(result);
        Assert.Contains("未挖掘到新模式", result);
    }

    [Fact]
    public void RecommendPattern_ReturnsRecommendation_WhenPatternExists()
    {
        // Arrange
        var pattern = new PatternItem
        {
            Name = "推荐模式",
            Category = "代码执行",
            Description = "这是推荐模式",
            Score = 0.95,
            SuccessCount = 20,
            Steps = new List<string> { "步骤 1", "步骤 2" },
            ExpectedOutcomes = new List<string> { "结果 1" }
        };

        _patternMinerMock
            .Setup(x => x.RecommendPattern(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(pattern);

        // Act
        var result = _plugin.RecommendPattern("帮我执行代码", "code");

        // Assert
        Assert.NotNull(result);
        Assert.Contains("推荐模式", result);
        Assert.Contains("代码执行", result);
    }

    [Fact]
    public void RecommendPattern_ReturnsNullMessage_WhenNoPattern()
    {
        // Arrange
        _patternMinerMock
            .Setup(x => x.RecommendPattern(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((PatternItem)null);

        // Act
        var result = _plugin.RecommendPattern("随便什么任务");

        // Assert
        Assert.NotNull(result);
        Assert.Contains("暂无推荐模式", result);
    }

    [Fact]
    public void RetrieveExperiences_ReturnsExperiences_WhenFound()
    {
        // Arrange
        var experiences = new List<ExperienceMemoryItem>
        {
            new ExperienceMemoryItem
            {
                Id = "exp-1",
                ExperienceType = "Success",
                Summary = "成功经验",
                DetailedContent = "详细内容",
                Confidence = 0.9,
                ValidationCount = 5,
                ActionableTips = new List<string> { "建议 1" }
            }
        };

        _experienceMock
            .Setup(x => x.RetrieveExperiences("测试", 5))
            .Returns(experiences);

        // Act
        var result = _plugin.RetrieveExperiences("测试", 5);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("相关经验", result);
        Assert.Contains("成功经验", result);
    }

    [Fact]
    public void RetrieveExperiences_ReturnsEmptyMessage_WhenNotFound()
    {
        // Arrange
        _experienceMock
            .Setup(x => x.RetrieveExperiences(It.IsAny<string>(), It.IsAny<int>()))
            .Returns(new List<ExperienceMemoryItem>());

        // Act
        var result = _plugin.RetrieveExperiences("不存在的关键词");

        // Assert
        Assert.NotNull(result);
        Assert.Contains("未找到相关经验", result);
    }

    [Fact]
    public async Task RecordFeedback_SavesReflection_WhenCalled()
    {
        // Arrange
        const string taskId = "task-001";
        _reflectionMock
            .Setup(x => x.ReflectOnTaskAsync(
                taskId,
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<List<string>>(),
                It.IsAny<long>(),
                default))
            .ReturnsAsync(new SelfReflectionItem { TaskId = taskId });

        // Act
        var result = await _plugin.RecordFeedback(
            taskId,
            "测试任务",
            "code",
            true,
            "sandbox,memory",
            1000,
            "这是一个经验教训");

        // Assert
        Assert.NotNull(result);
        Assert.Contains("反馈已记录", result);
        _reflectionMock.Verify(
            x => x.ReflectOnTaskAsync(taskId, It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<bool>(), It.IsAny<List<string>>(), It.IsAny<long>(), default),
            Times.Once);
    }

    [Fact]
    public void GetSystemHealth_ReturnsStatistics()
    {
        // Arrange
        _patternMinerMock
            .Setup(x => x.GetPatternStatistics())
            .Returns(new Dictionary<string, object>
            {
                ["TotalPatterns"] = 10,
                ["VerifiedPatterns"] = 8,
                ["AverageScore"] = 0.85
            });

        _evolverMock
            .Setup(x => x.GetExperimentStatistics())
            .Returns(new Dictionary<string, object>
            {
                ["TotalExperiments"] = 5,
                ["ExperimentingCount"] = 2,
                ["AdoptedCount"] = 3,
                ["SuccessRate"] = 0.6
            });

        _experienceMock
            .Setup(x => x.GetStatistics())
            .Returns(new Dictionary<string, object>
            {
                ["TotalExperiences"] = 50,
                ["HighConfidenceCount"] = 30,
                ["AverageConfidence"] = 0.75
            });

        // Act
        var result = _plugin.GetSystemHealth();

        // Assert
        Assert.NotNull(result);
        Assert.Contains("自我改进系统健康状态", result);
        Assert.Contains("模式总数：10", result);
        Assert.Contains("总实验数：5", result);
        Assert.Contains("总经验数：50", result);
    }

    [Fact]
    public async Task StartOptimizationExperiment_ReturnsExperimentId()
    {
        // Arrange
        const string experimentId = "EXP-001";
        _evolverMock
            .Setup(x => x.StartExperimentAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                default))
            .ReturnsAsync(experimentId);

        // Act
        var result = await _plugin.StartOptimizationExperiment(
            "测试实验",
            "Target",
            "current",
            "candidate");

        // Assert
        Assert.NotNull(result);
        Assert.Contains(experimentId, result);
    }

    [Fact]
    public void RecordExperimentMetric_DoesNotThrow()
    {
        // Arrange
        const string experimentId = "EXP-001";
        const string metricName = "SuccessRate";
        const double value = 0.9;

        _evolverMock
            .Setup(x => x.RecordMetric(experimentId, metricName, value))
            .Verifiable();

        // Act & Assert - Should not throw
        _plugin.RecordExperimentMetric(experimentId, metricName, value);
        _evolverMock.Verify(
            x => x.RecordMetric(experimentId, metricName, value),
            Times.Once);
    }
}
