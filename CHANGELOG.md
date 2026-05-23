# Changelog

All notable changes to DeerFlow.WPF will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [1.0.0] — 2026-05-05

### Added — 新增功能

#### 核心框架
- 实现 `ViewModelBase` 基类，提供 `INotifyPropertyChanged` + `IDisposable` 双重能力
- 实现 `RelayCommand`（泛型 / 非泛型双版本），支持 `async` 委托转换
- 实现 `AsyncObservableCollection<T>`，支持批量更新通知抑制和加载状态跟踪

#### 数据模型
- 新增 `TaskModel`、`AgentConfig`、`ChatMessage`、`MemoryItem`、`SkillItem` 五大数据模型
- 新增 `IMConnection` 模型和 `PlatformType`/`ConnectionStatus` 枚举

#### 服务层
- **ApiService**: 实现 SSE 流式聊天 (`SendChatStreamAsync`)，支持 `CancellationToken` 全链路传递
- **TaskWindowManager**: 多窗口进程隔离，CPU/内存 2 秒级实时监控
- **SandboxManager**: 文件系统沙箱 + 命令白名单（10 个命令）
- **WatchdogService**: 进程存活 3 秒轮询 + 30 秒心跳超时 + 最多 3 次自动重启
- **LoggerService**: Serilog 封装，10 MB 文件轮转，200 条内存缓存
- **MemorySnapshotService**: 内存快照采集 + 差异对比 + 50 MB 泄漏检测阈值
- **IMPlatformService**: IM 连接生命周期管理 + JSON 持久化 + 消息路由至 AI 回复

#### IM 平台适配器
- 新增 `IIMPlatformAdapter` 接口和 `IMAdapterBase` 抽象基类
- 实现 **飞书适配器** (`FeishuAdapter`): HEAD 验证连接 + 互动卡片消息格式
- 实现 **企业微信适配器** (`WeComAdapter`): URL 格式校验 + Markdown 消息格式

#### ViewModels
- 实现 `MainViewModel`: 窗口导航 + WeakReference 页面缓存 + 状态栏管理
- 实现 `HomeViewModel`: 欢迎页 + 快速操作入口 + 最近任务
- 实现 `ChatViewModel`: 消息流管理 + SSE 流式接收 + 取消生成 + 停止按钮
- 实现 `TaskPanelViewModel`: 任务 CRUD + 进程管理 + 实时监控刷新
- 实现 `AgentOrchestrationViewModel`: 智能体工作流配置 + 中间件链展示
- 实现 `MemoryViewModel`: 记忆库搜索 + 列表展示
- 实现 `SkillsViewModel`: 技能卡片 + 安装/卸载
- 实现 `SettingsViewModel`: 模型配置 + 功能开关 + 保存/重置
- 实现 `IMSettingsViewModel`: IM 连接 CRUD + 测试连接 + 启用/禁用

#### Views
- 实现 `MainWindow` (WindowChrome 无边框): 自定义标题栏 + 侧边栏 + ContentControl 页面切换 + 状态栏
- 实现 `TitleBarControl`: 最小化/最大化/关闭按钮 + WindowChrome.IsHitTestVisibleInChrome
- 实现 `SidebarControl`: 8 项图标导航菜单（含新增的 IM 接入）
- 实现 `StatusBarControl`: 任务计数 + 内存占用文本
- 实现 `HomePage`: 欢迎横幅 + 快速操作卡片 + 功能亮点
- 实现 `ChatPage`: 消息列表 + VirtualizingStackPanel 虚拟化 + 文本输入
- 实现 `TaskPanelPage`: 任务创建表单 + 任务列表（ListBox 替代 ItemsControl+ScrollViewer）
- 实现 `AgentOrchestrationPage`: 工作流表单
- 实现 `MemoryPage`: 搜索框 + 记忆列表
- 实现 `SkillsPage`: 技能卡片 WrapPanel 网格
- 实现 `SettingsPage`: 配置表单
- 实现 `IMSettingsPage`: 左右两栏（连接列表 + 编辑表单）+ 状态色灯
- 实现 `TaskWindow`: 独立沙箱窗口 + 命令输入

#### 主题
- 新增 `DarkTheme.xaml`: Trae Solo 暗色主题 (#1E1E24/#25252C/#2A2A33 背景 + #6C5CE7 强调色)
- 统一定义 TextBox / PrimaryButton / SecondaryButton 全局样式

#### 测试
- 新增 10 个测试类，共 40 个测试用例
- 覆盖范围: ViewModelBase (4) / RelayCommand (5) / SandboxManager (5) / LoggerService (4) / TaskWindowManager (4) / WatchdogService (4) / ApiService (2) / IMPlatformService (5) / FeishuAdapter (3) / WeComAdapter (4)
- 集成测试 1 个 (TaskWindowManager 多窗口并发，`Category=Integration`)

#### CI/CD
- 新增 GitHub Actions 工作流 (`.github/workflows/build.yml`): push/PR 自动编译 + 运行测试 + 上传产出

#### 文档
- 新增 `可行性研究报告.md` (6 章)
- 新增 `开发文档.md` (架构/布局/API/扩展)
- 新增 `用户手册.md` (安装/界面/功能/FAQ)
- 新增 `Installers/DeerFlow.WPF.wxs` (WiX MSI 打包)
- 新增 `Product-Spec.md` (14 章完整 PRD)
- 新增 `README.md` (项目主页)
- 新增 `task_plan.md` / `progress.md` / `findings.md` (项目管理)
- 新增 `CHANGELOG.md` (本文档)

### Changed — 修改

- `ViewModelBase`: 由纯 `INotifyPropertyChanged` 升级为同时实现 `IDisposable`
- `MainViewModel`: 页面导航由每次 `new` 改为 `WeakReference` 缓存模式
- `AsyncObservableCollection`: `AddRange`/`ReplaceAll` 由多次通知改为批量抑制后单次通知
- `ChatViewModel`: 新增 `StopGenerationCommand` 和 `CancellationTokenSource` 支持
- `TaskPanelViewModel`: 新增 `IDisposable` 取消事件订阅
- `ApiService`: 新增 `CancellationToken` 参数传递至 `SendAsync` 和 `ReadLineAsync`
- 多个 XAML 页面: `ItemsControl + ScrollViewer` 替换为 `ListBox`，启用 `VirtualizingStackPanel`
- `App.xaml.cs`: 新增 `WatchdogService` / `MemorySnapshotService` / `IMPlatformService` / `IMSettingsViewModel` DI 注册
- `MainWindow.xaml`: 新增 `IMSettingsPage` DataTemplate 映射
- `MainViewModel.InitMenu()`: 新增 "IM接入" 菜单项（排在"设置"之前）

### Fixed — 修复

- **CS0017**: 多入口点冲突 → 添加 `<StartupObject>DeerFlow.WPF.App</StartupObject>`
- **CS0104**: `TaskStatus` 歧义 → 使用 `using TaskStatus = DeerFlow.WPF.Models.TaskStatus;`
- **CS1593**: 异步 lambda 委托重载歧义 → 使用 `new Action(async () => ...)` 显式转换
- **CA2024**: `EndOfStream` 在 async 中警告 → 改为 `while(true) + null check`
- **NU1510**: `System.Net.Http.Json` 重复包 → 移除包引用，使用框架自带
- **CS0579**: obj 缓存重复程序集特性 → 添加 `GenerateAssemblyInfo=false` + 清理 obj
- **CS0266**: `double * int → long` 隐式转换 → 使用 `(long)(...)` 显式转换
- **CS0246**: `EnumeratorCancellation` 命名空间缺失 → 添加 `using System.Runtime.CompilerServices;`
- **文件锁冲突**: `SaveToDiskAsync` 和 `Dispose` 竞争 → 测试重构为独立 Service 实例
- **Mock 回调死锁**: `ReadAsStringAsync().Result` → 使用 `FakeHttpMessageHandler` 替代

### Security — 安全

- Token/ApiKey 不记录到日志
- 日志消息内容截断至 100 字符
- 沙箱命令白名单限制
- HttpClient `BaseAddress` 不从用户输入直接设置

---

## [1.1.0] — 2026-05-06

### Added — 新增功能

#### Semantic Kernel 集成
- 添加 `Microsoft.SemanticKernel` v1.52.0 + `Microsoft.SemanticKernel.Connectors.OpenAI` NuGet 包
- 实现 `SandboxPlugin`: 将 SandboxManager 封装为 4 个 SK 工具函数（创建/销毁沙箱、列出文件、执行命令）
- 实现 `MemoryPlugin`: 3 个记忆工具函数（记住/召回/遗忘），支持多会话隔离 + 20 条上限
- 实现 `WebSearchPlugin`: DuckDuckGo 联网搜索 + HTML→纯文本摘要提取 + 超时/错误处理
- 实现 `SKLoggingFilter` (IFunctionInvocationFilter): 函数调用耗时日志，中间件模式
- 实现 `ToolCallLoggingFilter` (IAutoFunctionInvocationFilter): 自动工具调用拦截日志

#### ChatViewModel SK 增强
- 新增 `SendMessageViaSKAsync()`: Kernel 驱动的流式聊天 + `FunctionChoiceBehavior.Auto()` 自动工具调用
- 智能回退机制: 当 `OPENAI_API_KEY` 未设置时自动降级为 `ApiService` 模式，不影响现有用户
- 环境变量驱动配置: `OPENAI_API_KEY` / `OPENAI_BASE_URL` / `OPENAI_MODEL` 零代码切换 AI 后端

### Changed — 修改

- `App.xaml.cs`: `ConfigureServices()` 新增 Kernel / OpenAI Connector / Plugins / Filters 的完整 DI 注册
- `DeerFlow.WPF.csproj`: `Microsoft.Extensions.DependencyInjection` 8.0.0 → 8.0.1，`Microsoft.Extensions.Http` 8.0.0 → 8.0.1

### Stats — 统计

- 总测试数: 40 → **41** (新增 1 个)
- 新增文件: 5 个 (3 Plugin + 2 Filter)
- 修改文件: 3 个 (csproj / App.xaml.cs / PageViewModels.cs)
- 新增代码: ~370 行

---

## [Unreleased]

### Planned — 已规划暂未实现

- SK Plugin 单元测试套件
- 多模型切换 UI（Settings 页面 Kernel 后端配置界面）
- 本地 LLM 推理引擎集成
- UI 自动化测试框架
- i18n 国际化支持
- System Prompt 编辑器
- 实时语音对话集成
- 端到端 (E2E) 测试套件
- Avalonia/MAUI 跨平台迁移可行性评估
