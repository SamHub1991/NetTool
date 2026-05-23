using System.Collections.Concurrent;
using DeerFlow.WPF.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Data.Sqlite;

namespace DeerFlow.WPF.Services;

/// <summary>
/// 经验记忆存储服务接口
/// </summary>
public interface IExperienceMemoryStore
{
    /// <summary>
    /// 保存经验记忆
    /// </summary>
    Task<string> SaveExperienceAsync(
        ExperienceMemoryItem experience,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 检索相关经验
    /// </summary>
    IEnumerable<ExperienceMemoryItem> RetrieveExperiences(
        string query,
        int limit = 10);

    /// <summary>
    /// 获取经验记忆
    /// </summary>
    ExperienceMemoryItem? GetExperience(string id);

    /// <summary>
    /// 更新经验置信度
    /// </summary>
    void UpdateConfidence(string id, bool positive);

    /// <summary>
    /// 获取经验统计
    /// </summary>
    Dictionary<string, object> GetStatistics();
}

/// <summary>
/// 经验记忆存储服务 - 持久化存储经验和模式
/// </summary>
public class ExperienceMemoryStore : IExperienceMemoryStore
{
    private readonly string _connectionString;
    private readonly ILogger _logger;
    private readonly ConcurrentDictionary<string, ExperienceMemoryItem> _cache = new();
    private const string DbFileName = "experiences.db";

    public ExperienceMemoryStore(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger("ExperienceMemory");
        _connectionString = $"Data Source={Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DbFileName)}";
        InitializeDatabase();
    }

    /// <inheritdoc/>
    public async Task<string> SaveExperienceAsync(
        ExperienceMemoryItem experience,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Experiences (
                    Id, Content, Type, Tags, Importance, 
                    ExperienceType, RelatedTaskId, Summary, 
                    DetailedContent, Confidence, ValidationCount, 
                    CreatedAt
                ) VALUES (
                    @Id, @Content, @Type, @Tags, @Importance,
                    @ExperienceType, @RelatedTaskId, @Summary,
                    @DetailedContent, @Confidence, @ValidationCount,
                    @CreatedAt
                )";

            command.Parameters.AddWithValue("@Id", experience.Id);
            command.Parameters.AddWithValue("@Content", experience.Content);
            command.Parameters.AddWithValue("@Type", experience.Type ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Tags", string.Join(",", experience.Tags));
            command.Parameters.AddWithValue("@Importance", experience.Importance);
            command.Parameters.AddWithValue("@ExperienceType", experience.ExperienceType);
            command.Parameters.AddWithValue("@RelatedTaskId", experience.RelatedTaskId);
            command.Parameters.AddWithValue("@Summary", experience.Summary);
            command.Parameters.AddWithValue("@DetailedContent", experience.DetailedContent);
            command.Parameters.AddWithValue("@Confidence", experience.Confidence);
            command.Parameters.AddWithValue("@ValidationCount", experience.ValidationCount);
            command.Parameters.AddWithValue("@CreatedAt", experience.CreatedAt);

            await command.ExecuteNonQueryAsync(cancellationToken);

            _cache.TryAdd(experience.Id, experience);
            _logger.LogInformation(
                "[ExperienceMemory] 保存经验：{Id} - {Summary}",
                experience.Id, experience.Summary);

            return experience.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ExperienceMemory] 保存经验失败：{Id}", experience.Id);
            throw;
        }
    }

    /// <inheritdoc/>
    public IEnumerable<ExperienceMemoryItem> RetrieveExperiences(
        string query,
        int limit = 10)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT * FROM Experiences 
                WHERE Content LIKE @Query 
                   OR Summary LIKE @Query 
                   OR DetailedContent LIKE @Query
                ORDER BY Importance DESC, Confidence DESC, LastAccessedAt DESC
                LIMIT @Limit";

            command.Parameters.AddWithValue("@Query", $"%{query}%");
            command.Parameters.AddWithValue("@Limit", limit);

            using var reader = command.ExecuteReader();
            var experiences = new List<ExperienceMemoryItem>();

            while (reader.Read())
            {
                var experience = ReadExperience(reader);
                experiences.Add(experience);

                // 更新最后访问时间
                experience.LastAccessedAt = DateTime.Now;
                _cache.AddOrUpdate(experience.Id, experience, (_, _) => experience);
            }

            _logger.LogInformation(
                "[ExperienceMemory] 检索到{Count}条相关经验",
                experiences.Count);

            return experiences;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ExperienceMemory] 检索经验失败");
            return Enumerable.Empty<ExperienceMemoryItem>();
        }
    }

    /// <inheritdoc/>
    public ExperienceMemoryItem? GetExperience(string id)
    {
        if (_cache.TryGetValue(id, out var cached))
        {
            return cached;
        }

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Experiences WHERE Id = @Id";
            command.Parameters.AddWithValue("@Id", id);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                var experience = ReadExperience(reader);
                _cache.TryAdd(id, experience);
                return experience;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ExperienceMemory] 获取经验失败：{Id}", id);
        }

        return null;
    }

    /// <inheritdoc/>
    public void UpdateConfidence(string id, bool positive)
    {
        if (_cache.TryGetValue(id, out var experience))
        {
            if (positive)
            {
                experience.ValidationCount++;
                experience.Confidence = Math.Min(1.0, experience.ValidationCount / 10.0);
            }
            else
            {
                experience.Confidence = Math.Max(0.0, experience.Confidence - 0.1);
            }

            _cache.AddOrUpdate(id, experience, (_, _) => experience);

            _logger.LogInformation(
                "[ExperienceMemory] 更新置信度：{Id} - {Confidence}",
                id, experience.Confidence);
        }
    }

    /// <inheritdoc/>
    public Dictionary<string, object> GetStatistics()
    {
        return new Dictionary<string, object>
        {
            ["TotalExperiences"] = _cache.Count,
            ["ByType"] = _cache.Values
                .GroupBy(e => e.ExperienceType)
                .ToDictionary(g => g.Key, g => g.Count()),
            ["AverageConfidence"] = _cache.Count > 0
                ? _cache.Values.Average(e => e.Confidence)
                : 0,
            ["HighConfidenceCount"] = _cache.Count(e => e.Confidence >= 0.8),
            ["AppliedCount"] = _cache.Count(e => e.IsApplied)
        };
    }

    #region Private Methods

    private void InitializeDatabase()
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Experiences (
                    Id TEXT PRIMARY KEY,
                    Content TEXT NOT NULL,
                    Type TEXT,
                    Tags TEXT,
                    Importance REAL DEFAULT 0.5,
                    ExperienceType TEXT NOT NULL,
                    RelatedTaskId TEXT,
                    Summary TEXT,
                    DetailedContent TEXT,
                    Confidence REAL DEFAULT 1.0,
                    ValidationCount INTEGER DEFAULT 0,
                    IsApplied INTEGER DEFAULT 0,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    LastAccessedAt DATETIME
                )";

            command.ExecuteNonQuery();

            // 创建索引
            using var indexCommand = connection.CreateCommand();
            indexCommand.CommandText = @"
                CREATE INDEX IF NOT EXISTS IX_Experiences_Type 
                ON Experiences (ExperienceType)";
            indexCommand.ExecuteNonQuery();

            using var indexCommand2 = connection.CreateCommand();
            indexCommand2.CommandText = @"
                CREATE INDEX IF NOT EXISTS IX_Experiences_Confidence 
                ON Experiences (Confidence DESC)";
            indexCommand2.ExecuteNonQuery();

            _logger.LogInformation("[ExperienceMemory] 数据库初始化完成");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ExperienceMemory] 数据库初始化失败");
            throw;
        }
    }

    private static ExperienceMemoryItem ReadExperience(SqliteDataReader reader)
    {
        return new ExperienceMemoryItem
        {
            Id = reader.GetString(reader.GetOrdinal("Id")),
            Content = reader.GetString(reader.GetOrdinal("Content")),
            Type = reader.IsDBNull(reader.GetOrdinal("Type"))
                ? null
                : reader.GetString(reader.GetOrdinal("Type")),
            Tags = reader.GetString(reader.GetOrdinal("Tags")).Split(',').ToList(),
            Importance = reader.GetDouble(reader.GetOrdinal("Importance")),
            ExperienceType = reader.GetString(reader.GetOrdinal("ExperienceType")),
            RelatedTaskId = reader.GetString(reader.GetOrdinal("RelatedTaskId")),
            Summary = reader.GetString(reader.GetOrdinal("Summary")),
            DetailedContent = reader.GetString(reader.GetOrdinal("DetailedContent")),
            Confidence = reader.GetDouble(reader.GetOrdinal("Confidence")),
            ValidationCount = reader.GetInt32(reader.GetOrdinal("ValidationCount")),
            IsApplied = reader.GetInt32(reader.GetOrdinal("IsApplied")) == 1,
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
            LastAccessedAt = reader.IsDBNull(reader.GetOrdinal("LastAccessedAt"))
                ? null
                : reader.GetDateTime(reader.GetOrdinal("LastAccessedAt"))
        };
    }

    #endregion
}
