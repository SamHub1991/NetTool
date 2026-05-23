using System.Collections.Concurrent;
using DeerFlow.WPF.Models;
using Microsoft.Extensions.Logging;

namespace DeerFlow.WPF.Services;

/// <summary>
/// 用户反馈服务接口
/// </summary>
public interface IFeedbackService
{
    /// <summary>
    /// 提交反馈
    /// </summary>
    Task<string> SubmitFeedbackAsync(
        string taskId,
        string feedbackType,
        int rating,
        string? comment = null,
        string? relatedPatternId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取任务的反馈
    /// </summary>
    List<UserFeedback> GetFeedbackForTask(string taskId);

    /// <summary>
    /// 获取所有反馈
    /// </summary>
    List<UserFeedback> GetAllFeedback(int limit = 100);

    /// <summary>
    /// 获取反馈统计
    /// </summary>
    Dictionary<string, object> GetFeedbackStatistics();

    /// <summary>
    /// 分析负面反馈
    /// </summary>
    List<UserFeedback> AnalyzeNegativeFeedback();

    /// <summary>
    /// 标记反馈为已处理
    /// </summary>
    void MarkFeedbackAsProcessed(string feedbackId);
}

/// <summary>
/// 用户反馈服务 - 收集和分析用户反馈
/// </summary>
public class FeedbackService : IFeedbackService
{
    private readonly ILogger _logger;
    private readonly ConcurrentDictionary<string, UserFeedback> _feedbackStore = new();
    private readonly object _lock = new();

    public FeedbackService(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger("FeedbackService");
    }

    /// <inheritdoc/>
    public Task<string> SubmitFeedbackAsync(
        string taskId,
        string feedbackType,
        int rating,
        string? comment = null,
        string? relatedPatternId = null,
        CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            try
            {
                var feedback = new UserFeedback
                {
                    TaskId = taskId,
                    FeedbackType = feedbackType,
                    Rating = rating,
                    Comment = comment,
                    RelatedPatternId = relatedPatternId,
                    CreatedAt = DateTime.Now,
                    IsProcessed = false
                };

                _feedbackStore.TryAdd(feedback.Id, feedback);

                _logger.LogInformation(
                    "[FeedbackService] 收到反馈：{Type}, 评分：{Rating}, 任务：{TaskId}",
                    feedbackType, rating, taskId);

                // 如果是负面反馈，记录告警
                if (feedbackType == "Dislike" || rating <= 2)
                {
                    _logger.LogWarning(
                        "[FeedbackService] 收到负面反馈：{Comment}",
                        comment ?? "无评论");
                }

                return feedback.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[FeedbackService] 提交反馈失败");
                return string.Empty;
            }
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public List<UserFeedback> GetFeedbackForTask(string taskId)
    {
        return _feedbackStore.Values
            .Where(f => f.TaskId == taskId)
            .OrderByDescending(f => f.CreatedAt)
            .ToList();
    }

    /// <inheritdoc/>
    public List<UserFeedback> GetAllFeedback(int limit = 100)
    {
        return _feedbackStore.Values
            .OrderByDescending(f => f.CreatedAt)
            .Take(limit)
            .ToList();
    }

    /// <inheritdoc/>
    public Dictionary<string, object> GetFeedbackStatistics()
    {
        var feedbackList = _feedbackStore.Values.ToList();
        var total = feedbackList.Count;

        return new Dictionary<string, object>
        {
            ["TotalFeedback"] = total,
            ["LikeCount"] = feedbackList.Count(f => f.FeedbackType == "Like"),
            ["DislikeCount"] = feedbackList.Count(f => f.FeedbackType == "Dislike"),
            ["AverageRating"] = total > 0 ? feedbackList.Average(f => f.Rating) : 0,
            ["ProcessedCount"] = feedbackList.Count(f => f.IsProcessed),
            ["UnprocessedCount"] = feedbackList.Count(f => !f.IsProcessed),
            ["SatisfactionRate"] = total > 0
                ? (double)feedbackList.Count(f => f.FeedbackType == "Like" || f.Rating >= 4) / total
                : 0
        };
    }

    /// <inheritdoc/>
    public List<UserFeedback> AnalyzeNegativeFeedback()
    {
        return _feedbackStore.Values
            .Where(f => f.FeedbackType == "Dislike" || f.Rating <= 2)
            .OrderByDescending(f => f.CreatedAt)
            .Take(20)
            .ToList();
    }

    /// <inheritdoc/>
    public void MarkFeedbackAsProcessed(string feedbackId)
    {
        if (_feedbackStore.TryGetValue(feedbackId, out var feedback))
        {
            feedback.IsProcessed = true;
            _logger.LogInformation("[FeedbackService] 反馈已标记为已处理：{FeedbackId}", feedbackId);
        }
    }
}
