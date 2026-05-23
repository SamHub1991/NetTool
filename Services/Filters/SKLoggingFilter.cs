using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace DeerFlow.WPF.Services.Filters;

/// <summary>
/// SK 函数调用日志过滤器，记录每次函数调用的耗时和结果
/// </summary>
public sealed class SKLoggingFilter : IFunctionInvocationFilter
{
    private readonly Microsoft.Extensions.Logging.ILogger _logger;

    public SKLoggingFilter(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger("SemanticKernel");
    }

    public async Task OnFunctionInvocationAsync(
        FunctionInvocationContext context,
        Func<FunctionInvocationContext, Task> next)
    {
        var sw = Stopwatch.StartNew();
        var fnName = context.Function.PluginName is not null
            ? $"{context.Function.PluginName}.{context.Function.Name}"
            : context.Function.Name;

        try
        {
            await next(context);
            sw.Stop();
            _logger.LogDebug("[SK] 完成: {Function} ({Elapsed}ms)", fnName, sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SK] 失败: {Function}", fnName);
            throw;
        }
    }
}
