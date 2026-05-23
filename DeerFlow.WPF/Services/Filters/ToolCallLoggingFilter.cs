using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace DeerFlow.WPF.Services.Filters;

/// <summary>
/// 自动工具调用日志过滤器，记录 AI 自动选择并执行工具的过程
/// </summary>
public sealed class ToolCallLoggingFilter : IAutoFunctionInvocationFilter
{
    private readonly Microsoft.Extensions.Logging.ILogger _logger;

    public ToolCallLoggingFilter(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger("SemanticKernel.ToolCall");
    }

    public async Task OnAutoFunctionInvocationAsync(
        AutoFunctionInvocationContext context,
        Func<AutoFunctionInvocationContext, Task> next)
    {
        var fnName = context.Function?.Name ?? "unknown";

        _logger.LogDebug("[ToolCall] 开始调用工具: {Function}", fnName);

        var sw = Stopwatch.StartNew();
        await next(context);

        _logger.LogDebug(
            "[ToolCall] 工具执行完成: {Function} ({Elapsed}ms)",
            fnName, sw.ElapsedMilliseconds);
    }
}
