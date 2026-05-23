# DeerFlow.WPF 任务计划

## 目标
开发一款功能完整的 Windows 桌面 AI 超级智能体应用，集成 AIO Sandbox 多窗口任务隔离，仿 Trae Solo 设计风格，具备智能体编排、长期记忆、技能管理等核心能力。

## 范围
- **包含**: WPF MVVM 架构、暗色主题 UI、聊天对话、任务面板、智能体编排、记忆库、技能商店、设置、AIO Sandbox 进程隔离、单元测试、安装包
- **不包含**: macOS/Linux 跨平台、移动端、Web 版本、后端推理服务部署

---

## 阶段规划

### 阶段 1: 核心框架搭建 [✅ completed]
**目标**: 完成 MVVM 基础设施、主窗口、全部页面和服务的代码实现及编译

**任务列表**:
- [x] DI 容器配置 (App.xaml.cs)
- [x] ViewModelBase / RelayCommand / AsyncObservableCollection
- [x] 全部数据模型 (TaskModel, ChatMessage, MemoryItem, SkillItem, AgentConfig)
- [x] ApiService (SSE 流式响应)
- [x] TaskWindowManager (进程隔离 + 资源监控)
- [x] SandboxManager (文件系统隔离 + 命令白名单)
- [x] LoggerService (文件轮转日志)
- [x] MainWindow + WindowChrome 自定义标题栏
- [x] SidebarControl (图标导航) + StatusBarControl
- [x] HomePage / ChatPage / TaskPanelPage / AgentOrchPage / MemoryPage / SkillsPage / SettingsPage
- [x] TaskWindow (独立沙箱窗口)
- [x] DarkTheme.xaml (Trae Solo 暗色主题)
- [x] 编译通过 (0 错误 0 警告)

**输出物**:
- 38 个源文件
- 可编译的 .dll (bin/Debug/net8.0-windows/DeerFlow.WPF.dll)
- 可行性研究报告.md

---

### 阶段 2: 质量验证与异常处理 [✅ completed]
**目标**: 编写单元测试、多窗口并发测试、实现崩溃恢复机制

**任务列表**:
- [x] 单元测试: ViewModelBase 属性变更通知 (4 tests)
- [x] 单元测试: RelayCommand 执行逻辑 (5 tests)
- [x] 单元测试: SandboxManager 沙箱创建/销毁 (5 tests)
- [x] 单元测试: LoggerService 日志写入 (4 tests)
- [x] 单元测试: ApiService Mock 响应 (2 tests)
- [x] 多窗口并发测试 (创建 10+ 任务窗口 - Integration 分类)
- [x] 异常崩溃自动恢复机制 (Watchdog 3秒检查 + 30秒心跳 + 3次重启上限)
- [x] 窗口间通信安全验证

**输出物**:
- Tests/ 目录下 7 个测试类 (28 测试用例)
- WatchdogService.cs
- 28/28 测试通过 (0 失败 0 跳过)

---

### 阶段 3: 性能优化 [✅ completed]
**目标**: 排查内存泄漏、优化 GC 策略、降低资源占用

**任务列表**:
- [x] WeakReference 模式防止 ViewModel 泄漏（页面导航缓存）
- [x] Dispose 模式实现 IDisposable（ViewModelBase + 全部 ViewModel）
- [x] 大型集合虚拟化 (UI Virtualization — ListBox + 容器回收)
- [x] 后台任务 CancellationToken 传递（ApiService / ChatViewModel）
- [x] 内存快照对比分析（MemorySnapshotService）

**输出物**:
- ViewModelBase 实现 IDisposable
- MainViewModel WeakReference 页面缓存
- AsyncObservableCollection 通知抑制批量更新
- ChatViewModel / ChatPage 支持取消生成
- MemorySnapshotService 快照+诊断
- 6 个 View 文件添加 VirtualizingStackPanel

---

### 阶段 4: 文档与发布 [✅ completed]
**目标**: 编写开发文档、用户手册、打包安装程序

**任务列表**:
- [x] 开发文档 (架构说明、API 参考、扩展指南)
- [x] 用户手册 (安装步骤、功能说明、常见问题)
- [x] MSI/WiX 安装包配置
- [x] 图标与应用清单
- [x] 版本发布说明 (CHANGELOG)
- [x] CI/CD Pipeline (GitHub Actions 自动构建+测试)

**输出物**:
- 开发文档.md
- 用户手册.md
- Installer/DeerFlow.WPF.wxs
- .github/workflows/build.yml (push/PR 自动构建+测试+产出)

---

## 里程碑

| 里程碑 | 预计日期 | 状态 |
|--------|----------|------|
| Phase 1 核心框架完成 | 2026-05-04 | ✅ |
| Phase 2 质量验证完成 | 2026-05-05 | ✅ |
| Phase 3 性能优化完成 | 2026-05-06 | ✅ |
| Phase 4 文档发布完成 | 2026-05-07 | ✅ |
| v1.0 正式发布 | 2026-05-08 | ✅ |

## 风险与依赖

### 风险
- **Windows 环境差异**: 不同 Win10/Win11 版本的系统 API 差异 → 在多个虚拟机环境中测试
- **AIO Sandbox 兼容性**: 沙箱在不同安全策略下的表现 → 提供降级方案（无沙箱模式）
- **第三方 API 变动**: OpenAI 等 API 格式变化 → 适配器模式隔离外部依赖

### 依赖
- .NET 8.0 SDK: 开发环境已满足
- Visual Studio 2022: 可选，dotnet CLI 可替代
- WiX Toolset: 用于 MSI 打包
