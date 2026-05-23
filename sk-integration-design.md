# DeerFlow.WPF × Semantic Kernel 集成设计方案

## 一、当前架构痛诊断

```
┌─────────────────────────────────────────────────┐
│                ChatViewModel                     │
│  ┌──────────────────────┐  ┌────────────────┐  │
│  │  SendMessageAsync()  │  │  裸 HttpClient  │  │
│  │  手动构造 Prompt     │  │  SSE 流式解析   │  │
│  │  无工具调用          │  │  无记忆管理     │  │
│  └──────────────────────┘  └────────────────┘  │
└─────────────────────────────────────────────────┘
                         │
                         ▼
              ┌──────────────────┐
              │    ApiService     │  ← 仅封装 HTTP 调用
              │  - SendChatAsync │
              │  - StreamChat    │
              └──────────────────┘
```

**核心问题**：
| 维度 | 当前状态 | 问题 |
|------|----------|------|
| 工具调用 | 无 | 无法执行代码、查文件、联网搜索 |
| 记忆管理 | 无 | 每次对话无上下文关联 |
| 多模型切换 | 硬编码 | 换模型需改代码 |
| 可观测性 | 手动日志 | 无 Filter 拦截点 |
| 规划能力 | 无 | 复杂任务需手动拆解 |

## 二、目标架构

```
┌────────────────────────────────────────────────────────────┐
│                     DeerFlow.WPF                            │
│  ┌──────────────────────────────────────────────────────┐  │
│  │                    ChatViewModel                      │  │
│  │  kernel.InvokeStreamingAsync(chatFunction, args)     │  │
│  └───────────────────────┬──────────────────────────────┘  │
│                          │                                  │
│                          ▼                                  │
│  ┌──────────────────────────────────────────────────────┐  │
│  │                      Kernel                           │  │
│  │  ┌─────────────┐  ┌───────────────┐  ┌───────────┐  │  │
│  │  │  Plugins    │  │   Filters      │  │  Services │  │  │
│  │  │  ┌───────┐  │  │ ┌───────────┐ │  │  OpenAI   │  │  │
│  │  │  │Tool   │  │  │ │Function   │ │  │  Azure    │  │  │
│  │  │  │Code   │  │  │ │Filter     │ │  │  Ollama   │  │  │
│  │  │  ├───────┤  │  │ ├───────────┤ │  │           │  │  │
│  │  │  │File   │  │  │ │Prompt     │ │  │           │  │  │
│  │  │  │Ops    │  │  │ │Filter     │ │  │           │  │  │
│  │  │  ├───────┤  │  │ ├───────────┤ │  │           │  │  │
│  │  │  │Memory │  │  │ │AutoFunc   │ │  │           │  │  │
│  │  │  │Plugin │  │  │ │Filter     │ │  │           │  │  │
│  │  │  └───────┘  │  │ └───────────┘ │  │           │  │  │
│  │  └─────────────┘  └───────────────┘  └───────────┘  │  │
│  └──────────────────────────────────────────────────────┘  │
└────────────────────────────────────────────────────────────┘
```

## 三、Plugin 设计

### 3.1 SandboxPlugin（沙箱工具）
将现有 `SandboxManager` 封装为 SK Plugin：

```csharp
/// <summary>
/// 沙箱工具插件，提供代码执行和文件操作能力
/// </summary>
public sealed class SandboxPlugin
{
    private readonly SandboxManager _sandbox;

    /// <summary>
    /// 通过 DI 注入 SandboxManager
    /// </summary>
    public SandboxPlugin(SandboxManager sandbox)
    {
        _sandbox = sandbox;
    }

    /// <summary>
    /// 在隔离沙箱中执行一段代码
    /// </summary>
    /// <param name="language">编程语言（python/javascript）</param>
    /// <param name="code">要执行的源代码</param>
    /// <returns>执行结果的标准输出</returns>
    [KernelFunction("execute_code")]
    [Description("在隔离文件系统沙箱中执行代码，返回标准输出")]
    public async Task<string> ExecuteCode(
        [Description("编程语言，支持 python 和 javascript")]
        string language,
        [Description("完整的源代码内容")]
        string code)
    {
        var result = await _sandbox.ExecuteAsync(language, code);
        return result.Output ?? result.Error ?? "(无输出)";
    }

    /// <summary>
    /// 在沙箱中读取文件内容
    /// </summary>
    [KernelFunction("read_file")]
    [Description("从沙箱中读取指定文件的内容")]
    public async Task<string> ReadFile(
        [Description("沙箱内的相对文件路径")]
        string path)
    {
        return await _sandbox.ReadFileAsync(path);
    }

    /// <summary>
    /// 在沙箱中写入文件
    /// </summary>
    [KernelFunction("write_file")]
    [Description("向沙箱中写入文件")]
    public async Task<string> WriteFile(
        [Description("沙箱内的相对文件路径")]
        string path,
        [Description("要写入的文件内容")]
        string content)
    {
        await _sandbox.WriteFileAsync(path, content);
        return $"文件 {path} 写入成功";
    }
}
```

### 3.2 MemoryPlugin（记忆插件）

```csharp
/// <summary>
/// 对话记忆插件，提供会话上下文的存储和检索
/// </summary>
public sealed class MemoryPlugin
{
    private readonly Dictionary<string, List<string>> _sessions = new();

    /// <summary>
    /// 记录一条重要信息到当前会话
    /// </summary>
    [KernelFunction("remember")]
    [Description("将重要信息存入记忆，以便后续对话引用")]
    public string Remember(
        [Description("要记住的信息内容")]
        string information)
    {
        const string DefaultSession = "default";
        if (!_sessions.TryGetValue(DefaultSession, out var list))
        {
            list = [];
            _sessions[DefaultSession] = list;
        }
        list.Add(information);
        return $"已记住: {information}";
    }

    /// <summary>
    /// 检索当前会话的所有记忆
    /// </summary>
    [KernelFunction("recall")]
    [Description("召回当前会话中的所有记忆条目")]
    public string Recall()
    {
        if (_sessions.TryGetValue("default", out var list) && list.Count > 0)
        {
            return string.Join("\n", list.Select((m, i) => $"{i + 1}. {m}"));
        }
        return "(无记忆)";
    }
}
```

### 3.3 WebSearchPlugin（联网搜索插件）

```csharp
/// <summary>
/// 联网搜索插件，提供实时信息获取能力
/// </summary>
public sealed class WebSearchPlugin
{
    private readonly HttpClient _httpClient;

    public WebSearchPlugin(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("WebSearch");
    }

    /// <summary>
    /// 执行一次网络搜索
    /// </summary>
    [KernelFunction("search_web")]
    [Description("联网搜索最新信息，返回前 5 条结果摘要")]
    public async Task<string> SearchWeb(
        [Description("搜索关键词")]
        string query)
    {
        // 使用 DuckDuckGo 的纯文本 API（无需 API Key）
        var url = $"https://lite.duckduckgo.com/lite/?q={Uri.EscapeDataString(query)}";
        var html = await _httpClient.GetStringAsync(url);
        // 简化：提取文本摘要
        return ExtractSummary(html);
    }

    private static string ExtractSummary(string html)
    {
        // 基础文本提取逻辑
        var text = System.Text.RegularExpressions.Regex
            .Replace(html, "<[^>]+>", " ")
            .Replace("&nbsp;", " ");
        text = System.Text.RegularExpressions.Regex
            .Replace(text, "\\s+", " ");
        return text.Length > 3000 ? text[..3000] + "..." : text;
    }
}
```

## 四、Service 注册（App.xaml.cs）

```csharp
/// <summary>
/// 注册 Semantic Kernel 及其插件
/// </summary>
public static void ConfigureSemanticKernel(IServiceCollection services)
{
    // 1. 注册 Kernel
    services.AddKernel();

    // 2. 注册 AI 服务（支持多后端，ServiceSelector 自动选择）
    services.AddOpenAIChatCompletion(
        modelId: "gpt-4o",
        apiKey: GetApiKey("OPENAI_API_KEY"),
        serviceId: "openai"  // 可选的服务标识
    );

    // 备选：Azure OpenAI
    // services.AddAzureOpenAIChatCompletion(
    //     deploymentName: "gpt-4o",
    //     endpoint: "https://your-resource.openai.azure.com/",
    //     apiKey: GetApiKey("AZURE_OPENAI_KEY")
    // );

    // 3. 注册 Plugins（作为 Transient，每次注入可创建新实例）
    services.AddTransient<SandboxPlugin>();
    services.AddSingleton<MemoryPlugin>();
    services.AddTransient<WebSearchPlugin>();

    // 4. 注册 Filters（可观测性拦截点）
    services.AddSingleton<IFunctionInvocationFilter, LoggingFilter>();
    services.AddSingleton<IAutoFunctionInvocationFilter, ToolCallLoggingFilter>();

    // 5. 构建 Kernel 并自动导入 Plugins
    // 方式 A：通过 ConfigureKernel 回调批量导入
    // 方式 B：手动调用 kernel.ImportPluginFromObject()
}
```

## 五、ChatViewModel 改造

### 5.1 改造前（当前）
```csharp
// 当前：直接调用 ApiService
var messages = new List<object> { new { role = "user", content = userInput } };
await foreach (var chunk in _apiService.StreamChatAsync(messages, cancellationToken))
{
    // 手动拼接响应
    fullResponse.Append(chunk.Content);
    OnPropertyChanged(nameof(CurrentResponse));
}
```

### 5.2 改造后（SK 驱动）
```csharp
/// <summary>
/// 通过 Semantic Kernel 发送消息并流式接收响应
/// </summary>
public async Task SendMessageWithSKAsync(string userInput)
{
    var kernel = App.Services.GetRequiredService<Kernel>();

    // 激活自动工具调用（替代旧版 Planner）
    var executionSettings = new OpenAIPromptExecutionSettings
    {
        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(
            autoInvoke: true,  // 自动执行工具，无需用户确认
            maximumAutoInvokeAttempts: 3  // 最多尝试 3 轮
        ),
        Temperature = 0.7
    };

    var arguments = new KernelArguments(executionSettings)
    {
        ["input"] = userInput
    };

    // 构造 Chat Function
    var chatFunction = kernel.CreateFunctionFromPrompt(
        "{{$input}}",
        executionSettings,
        functionName: "chat",
        description: "鹿流AI聊天"
    );

    var fullResponse = new StringBuilder();

    // 流式调用
    await foreach (var chunk in kernel.InvokeStreamingAsync<StreamingKernelContent>(
        chatFunction, arguments, _cancellationTokenSource.Token))
    {
        fullResponse.Append(chunk.ToString());
        CurrentResponse = fullResponse.ToString();  // 实时更新 UI
    }

    // 可在 AutoFunctionInvocationFilter 中捕获工具调用事件
}
```

## 六、Filter 拦截点设计

### 6.1 可观测性 Filter
```csharp
/// <summary>
/// 记录函数调用日志的 Filter
/// </summary>
public sealed class LoggingFilter : IFunctionInvocationFilter
{
    private readonly LoggerService _logger;

    public LoggingFilter(LoggerService logger)
    {
        _logger = logger;
    }

    public async Task OnFunctionInvocationAsync(
        FunctionInvocationContext context,
        Func<FunctionInvocationContext, Task> next)
    {
        var sw = Stopwatch.StartNew();
        _logger.Info($"[SK] 调用: {context.Function.PluginName}.{context.Function.Name}");

        try
        {
            await next(context);
            sw.Stop();
            _logger.Info($"[SK] 完成: {context.Function.Name} ({sw.ElapsedMilliseconds}ms)");
        }
        catch (Exception ex)
        {
            _logger.Error($"[SK] 失败: {context.Function.Name} - {ex.Message}");
            throw;
        }
    }
}
```

### 6.2 工具调用日志 Filter
```csharp
/// <summary>
/// 记录自动工具调用过程的 Filter
/// </summary>
public sealed class ToolCallLoggingFilter : IAutoFunctionInvocationFilter
{
    public async Task OnAutoFunctionInvocationAsync(
        AutoFunctionInvocationContext context,
        Func<AutoFunctionInvocationContext, Task> next)
    {
        // 记录工具调用开始
        var functionName = context.Function?.Name ?? "unknown";
        Debug.WriteLine($"[ToolCall] 开始: {functionName}");

        await next(context);

        // 记录工具调用结果
        Debug.WriteLine($"[ToolCall] 完成: {functionName}");
    }
}
```

## 七、迁移策略

### 阶段 1：并行运行（1-2 天）
- 保留现有 `ApiService`，新增 `SKChatService` 包装 Kernel
- 通过配置开关切换新旧实现
- 确保功能回退能力

### 阶段 2：Plugin 迁移（2-3 天）
- 将 `SandboxManager` 封装为 `SandboxPlugin`
- 新增 `MemoryPlugin`、`WebSearchPlugin`
- 编写对应单元测试

### 阶段 3：全面切换（1-2 天）
- ChatViewModel 迁移至 SK 调用
- 启用 `FunctionChoiceBehavior.Auto()`
- 移除旧 `ApiService`

### 阶段 4：优化增强
- 集成 TextMemory/Vectors 持久化记忆
- 添加多模型切换 UI
- Process Framework 实现复杂工作流

## 八、NuGet 包依赖

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.SemanticKernel" Version="1.52.0" />
  <PackageReference Include="Microsoft.SemanticKernel.Connectors.OpenAI" Version="1.52.0" />
</ItemGroup>
```

`Microsoft.SemanticKernel` 元包包含：
- `Microsoft.SemanticKernel.Core` — Kernel + Plugins + Filters
- `Microsoft.SemanticKernel.Abstractions` — 接口和基类

## 九、成功标准

| 指标 | 目标 | 验证方式 |
|------|------|----------|
| 插件数量 | ≥ 3 个 Plugin（Sandbox/Memory/WebSearch） | 单元测试 |
| 工具调用成功率 | ≥ 90% | 集成测试 |
| 迁移后测试通过率 | 100%（保持 40 个现有测试） | `dotnet test` |
| 流式响应延迟 | ≤ 现有方案 + 100ms | 性能测试 |
| 代码可维护性 | ViewModel 行数减少 ≥ 30% | 静态分析 |
