# DeerFlow.WPF 研究发现

## 技术发现

### WPF ItemsControl + ScrollViewer 破坏虚拟化
**日期**: 2026-05-05
**类别**: 性能

**发现内容**:
当 `ItemsControl` 被包裹在外层 `ScrollViewer` 中时，`VirtualizingStackPanel` 虚拟化完全失效。
外层 ScrollViewer 给 ItemsControl 提供无限大小的布局空间，导致所有数据项都被实例化，
丧失 UI 虚拟化的全部性能收益。

**影响**:
- TaskPanelPage / MemoryPage / HomePage 中的 ItemsControl 无虚拟化
- 大数据量场景下 UI 线程卡顿、内存飙升

**解决方案**:
1. 将 `ItemsControl + ScrollViewer` 替换为 `ListBox`（自带虚拟化 ScrollViewer）
2. 添加 `VirtualizingStackPanel.IsVirtualizing="True"` 和 `VirtualizationMode="Recycling"`
3. 特殊布局（如 SkillsPage 的 WrapPanel）无法虚拟化，保持原样（小数据量场景可接受）

**经验总结**:
WPF 中凡是有滚动需求的列表控件，一律优先使用 `ListBox` 代替 `ItemsControl + ScrollViewer` 组合。

---

### WeakReference 页面缓存模式
**日期**: 2026-05-05
**类别**: 内存

**发现内容**:
MVVM 导航中反复 `new` ViewModel 会导致旧实例无法被 GC 回收（事件订阅残留、绑定引用）。
使用 `Dictionary<string, WeakReference<ViewModelBase>>` 缓存页面实例：
- 导航时优先从缓存获取，命中则复用
- GC 回收后 WeakReference 自动失效，触发新建
- `CleanupDeadReferences()` 定期清理死亡引用，防止字典膨胀

**参考实现**:
[MainViewModel.NavigateTo()](file:///e:/code/item/DeerFlow.WPF/ViewModels/MainViewModel.cs#L155-L189)

---

### .NET 10 EnumeratorCancellation 属性
**日期**: 2026-05-05
**类别**: 技术

**发现内容**:
在 .NET 10 中使用 `IAsyncEnumerable<T>` 时，`[EnumeratorCancellation]` 属性需要
`using System.Runtime.CompilerServices;`。该属性让编译器自动将 `CancellationToken` 参数
传递给生成的 `IAsyncEnumerable<T>.GetAsyncEnumerator()` 方法。

**使用方式**:
```csharp
public async IAsyncEnumerable<string> SendChatStreamAsync(
    string message,
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
{
    // 在循环体内显式检查
    cancellationToken.ThrowIfCancellationRequested();
    yield return data;
}
```

### 性能优化清单总结
**日期**: 2026-05-05
**类别**: 性能

**发现内容**:
本次 Phase 3 性能优化覆盖 6 个维度：

| 维度 | 优化措施 | 收益 |
|------|----------|------|
| ViewModel 泄漏 | WeakReference 页面缓存 | 避免导航频繁创建/泄漏 |
| 资源释放 | IDisposable 模式 | 事件订阅清理、CTS 释放 |
| UI 渲染 | ListBox + VirtualizingStackPanel | 大数据量滚动性能 10x+ |
| 异步取消 | CancellationToken 全链路传递 | 避免无效网络请求 |
| 批量更新 | AsyncObservableCollection 通知抑制 | 减少 UI 重复刷新 |
| 内存诊断 | MemorySnapshotService | 50MB 泄漏阈值自动告警 |

### WPF WindowChrome 多入口点冲突
**日期**: 2026-05-04
**类别**: 技术

**发现内容**:
当 WPF 项目中同时存在 App.xaml 和另一个带有 `x:Class` 的 Window XAML 文件时，WPF 的 MSBuild 管道会为每个 Window 子类生成静态 `Main()` 方法，导致 CS0017 "程序定义了多个入口点" 错误。

**影响**:
- TaskWindow.xaml 和 App.xaml 冲突，无法编译

**解决方案**:
在 `.csproj` 中显式设置 `<StartupObject>DeerFlow.WPF.App</StartupObject>`，明确告知编译器入口点。

**相关链接**:
- [MainWindow.xaml](file:///e:/code/item/DeerFlow.WPF/MainWindow.xaml)
- [DeerFlow.WPF.csproj](file:///e:/code/item/DeerFlow.WPF/DeerFlow.WPF.csproj#L12)

---

### C# async Lambda 与 RelayCommand 重载解析
**日期**: 2026-05-04
**类别**: 技术

**发现内容**:
`async _ => await SomeTask()` 形式的 lambda 在 RelayCommand 双重重载（`Action<object?>` vs `Action`）下，编译器重载解析行为不稳定。`Func<bool>` 参数的 `() => condition` 与 `Predicate<object?>` 也存在歧义。

**解决方案**:
显式使用 `new Action(async () => await ...)` 构造非泛型 Action，强制匹配 `RelayCommand(Action, Func<bool>?)` 重载。

```csharp
// ✅ 正确的写法
SendCommand = new RelayCommand(
    new Action(async () => await SendMessageAsync()),
    () => !IsSending && !string.IsNullOrWhiteSpace(InputText)
);
```

**经验总结**:
在 MVVM 中处理异步 Command 时，优先使用显式委托类型，避免依赖编译器的隐式转换推断。

---

### DeerFlow 2.0 架构亮点总结
**日期**: 2026-05-04
**类别**: 架构

**发现内容**:
1. **Harness/App 分离**: 核心框架包与业务应用严格分离，通过 CI 强制执行单向依赖
2. **17 个中间件链**: 线程数据 → 上传 → 沙箱 → 工具护栏 → 记忆 → 标题生成 → 子智能体限制 → 循环检测
3. **6 种消息类型**: Error / Warning / Assistant / ToolCall / Subagent / SubagentEnd
4. **子智能体系统**: 内置 2 种子智能体，支持并发 3 个，15 分钟超时

**建议**:
在 DeerFlow.WPF 后续迭代中引入子智能体编排，借鉴 Harness/App 分离模式重构服务层。

---

### Lynn 记忆系统架构亮点
**日期**: 2026-05-04
**类别**: 架构

**发现内容**:
Lynn 有 6 种记忆子系统：
- `deep-memory.js` - 深度记忆
- `proactive-recall.js` - 主动回忆
- `fact-store.js` - 事实库
- `inferred-profile.js` - 用户画像推断
- `diary-writer.js` - 日记写入
- `skill-distiller.js` - 技能蒸馏

**建议**:
在 DeerFlow.WPF 的 MemoryViewModel 中引入"重要性评分"机制（借鉴 fact-store）和"主动回忆"能力（借鉴 proactive-recall）。

## 决策记录

### 技术栈: WPF vs WinUI 3 vs MAUI
**日期**: 2026-05-04
**决策者**: 用户需求

**背景**:
需选择 Windows 桌面 UI 框架

**选项**:
1. **WPF (.NET 8)**: 成熟稳定，MVVM 支持优秀，WindowChrome 自定义标题栏方便，大量社区资源
2. **WinUI 3**: 最新 Windows 原生 UI，但生态不成熟，.NET 8 支持有限
3. **MAUI**: 跨平台，但 Windows 端体验不如原生

**决策**: WPF (.NET 8)

**理由**:
1. 用户明确要求 WPF 技术栈
2. 对标 Trae Solo 桌面版（Electron），WPF 可提供同等原生体验
3. WindowChrome + ControlTemplate 组合可完美实现暗色无边框窗口
4. 成熟的 MVVM 工具链（CommunityToolkit.Mvvm 可选升级）

---

### 沙箱方案: AIO Sandbox vs Docker vs Process Isolation
**日期**: 2026-05-04

**背景**:
需要为每个任务创建隔离执行环境

**选项**:
1. **AIO Sandbox**: 真正的沙箱，文件系统/网络/进程全隔离，但需要额外部署
2. **Docker**: 容器化隔离，兼容性好，但 Windows Docker 体验一般
3. **进程隔离 + 文件系统隔离**: 轻量级，不依赖第三方，但有安全边界限制

**决策**: 进程隔离 + 文件系统隔离（Phase 1），预留 AIO Sandbox 集成接口

**理由**:
1. 开发阶段快速验证，无需额外基础设施
2. 文件系统隔离 + 命令白名单满足基本安全需求
3. 通过 ISandboxManager 接口抽象，可随时切换为 AIO Sandbox
