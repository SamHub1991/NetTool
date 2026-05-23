# 结构化学习与发展计划

## 目标
通过系统性研读高质量 .NET/AI 开源项目，持续提升 DeerFlow.WPF 产品的架构质量和技术深度，
将业界最佳实践融入后续迭代（v1.1 → v2.0）。

## 范围

### 包含
- 2 个核心开源项目的深度代码研读（架构层 → 实现层 → 测试层）
- 从每个项目中提取 ≥ 5 项可落地的技术决策或实现模式
- 将学习成果以 Spec / 代码重构 / 技术文档三种形式整合入 DeerFlow.WPF
- 按节奏执行审查与调整（每 2 周一次）

### 不包含
- 泛泛的项目浏览（必须产出具体代码和文档变更）
- 偏离 DeerFlow.WPF 技术路线的纯学术研究
- AI / 大模型底层训练相关（聚焦应用层和工程化）

---

## 项目选择

### 项目 1: Semantic Kernel (microsoft/semantic-kernel) ⭐ 22k+
| 维度 | 说明 |
|------|------|
| **定位** | 微软官方 LLM 编排 SDK，.NET 生态最成熟的 AI 编排框架 |
| **核心概念** | Kernel / Plugin / Planner / Memory / Agent |
| **与 DeerFlow.WPF 关系** | 对标 DeerFlow.WPF 的智能体编排模块（AgentOrchestration），替换自研编排层 |
| **研读重点** | Plugin 注册与发现机制、Plan 多步骤执行、MemoryStore 接口设计、Function Calling 流程 |
| **文档质量** | 丰富的官方文档 + CookBook 示例 + API 参考 |
| **社区活跃度** | 高 (Discussions / Issues / PRs 频繁) |

### 项目 2: Avalonia UI (AvaloniaUI/Avalonia) ⭐ 26k+
| 维度 | 说明 |
|------|------|
| **定位** | .NET 跨平台 UI 框架，WPF 风格 XAML，支持 Windows/macOS/Linux/Android/iOS/Web |
| **核心概念** | StyledProperty / TemplatedControl / ReactiveUI / CompiledBinding / AOT |
| **与 DeerFlow.WPF 关系** | 为 v2.0 跨平台迁移提供技术可行性评估和架构参考 |
| **研读重点** | 跨平台渲染管线、样式系统与 WPF 差异、AOT 编译配置、ControlTheme vs Style |
| **文档质量** | 详尽文档 + 迁移指南 (WPF→Avalonia) + API 参考 |
| **社区活跃度** | 极高 (GitHub 最活跃的 .NET UI 项目) |

---

## 阶段规划

### 阶段 1: Semantic Kernel 深层研读 [in_progress]
**目标**: 掌握 LLM 编排最佳实践，评估是否替换 DeerFlow.WPF 自研编排层
**时间**: 第 1~2 周 | **状态**: in_progress

**任务列表**:
- [ ] 克隆仓库，本地构建，运行所有示例项目
- [ ] 深入分析 `Kernel` 构造和 `IServiceProvider` 集成模式
- [ ] 研读 `KernelPlugin` 注册机制和 OpenAPI/Type 两种导入策略
- [ ] 分析 `FunctionCallingStepwisePlanner` 多步推理流程
- [ ] 研究 `ISemanticTextMemory` 接口以及现有 Connector 实现
- [ ] 提取 ≥ 5 项可应用于 DeerFlow.WPF 的模式，记录到 learning_findings.md

**输出物**:
- `learning_findings.md` 新增 "Semantic Kernel 技术发现" 章节
- 技术选型决策记录：自研编排层 vs 集成 SK
- 如决策集成 → 创建 `.trae/specs/sk-integration/` Spec 文档

**预估耗时**: 总约 16 小时
```
┌─────────────────────────────────────────────────┐
│ 第 1 周                                          │
│   周一: 环境搭建 + 示例运行 (2h)                  │
│   周三: Kernel/Plugin 源码研读 (3h)              │
│   周五: Planner/FunctionCalling 源码 (3h)         │
│                                                  │
│ 第 2 周                                          │
│   周一: Memory/Connector 接口研读 (2h)            │
│   周三: 提取模式 + 编写 findings (3h)             │
│   周五: 技术决策 + 整合路线图 (3h)                │
└─────────────────────────────────────────────────┘
```

---

### 阶段 2: Avalonia UI 跨平台迁移评估 [pending]
**目标**: 评估 WPF → Avalonia 迁移的可行性和工作量
**时间**: 第 3~4 周 | **状态**: pending

**任务列表**:
- [ ] 搭建 Avalonia 原型项目，移植 MainWindow 核心布局
- [ ] 测试 DarkTheme.xaml 样式在 Avalonia 中的兼容性
- [ ] 评估 WindowChrome → 跨平台窗口管理方案
- [ ] 分析 MVVM 层（ViewModelBase / RelayCommand）可复用度
- [ ] 测试 VirtualizingStackPanel ↔ Avalonia 虚拟化方案
- [ ] 评估 AOT 发布和打包工具链（替代 WiX）
- [ ] 提取 ≥ 5 项关键差异和迁移路径，记录到 learning_findings.md

**输出物**:
- `learning_findings.md` 新增 "Avalonia 跨平台迁移评估" 章节
- 迁移可行性分析：WPF 特性 ↔ Avalonia 特性 对照表
- 原型代码仓库（如条件允许）
- 如决策迁移 → 创建 `migration_roadmap.md`

**预估耗时**: 总约 18 小时
```
┌─────────────────────────────────────────────────┐
│ 第 3 周                                          │
│   周一: 环境搭建 + WPF→Avalonia 官方指南通读 (2h)│
│   周三: 原型项目搭建 + XAML/主题移植 (4h)         │
│   周五: MVVM 层兼容性评估 (3h)                    │
│                                                  │
│ 第 4 周                                          │
│   周一: AOT/打包工具链评估 (2h)                   │
│   周三: 差异对照表 + 迁移路径编写 (3h)             │
│   周五: 整理 findings + 决策建议 (4h)              │
└─────────────────────────────────────────────────┘
```

---

## 里程碑

| 里程碑 | 日期 | 状态 |
|--------|------|------|
| SK 研读完成 + 技术发现文档 | 第 2 周末 (2026-05-16) | 🔄 |
| SK 集成决策 + Spec 创建 | 第 2 周末 | 🔄 |
| Avalonia 原型搭建 | 第 3 周中 (2026-05-20) | ⏳ |
| Avalonia 差异对照表 + 迁移路径 | 第 4 周末 (2026-05-23) | ⏳ |
| 第 1 次审查会议 (2 周复盘) | 2026-05-19 | ⏳ |
| 第 2 次审查会议 (4 周复盘) | 2026-05-26 | ⏳ |

---

## 知识整合里程碑

### 从 Semantic Kernel → DeerFlow.WPF 整合目标

| SK 概念 | 预期整合方式 | 整合标志 |
|---------|-------------|----------|
| `Kernel` + `IServiceProvider` DI 模式 | 重构 `ApiService` 为 `Kernel` 实例管理 | ApiService 增加 KernelProvider |
| `KernelPlugin` 注册与发现 | 技能商店引入 Plugin 元数据注册 | SkillsViewModel 绑定 Plugin 列表 |
| `FunctionCallingStepwisePlanner` | AgentOrchestration 引入多步推理 | OrchestrationViewModel 支持 Plan 执行 |
| `ISemanticTextMemory` Connector 模式 | MemoryPage 从自研模型切换到标准接口 | MemoryItem 实现 IMemoryEntry |
| `OpenAI/AsyncHTTP` SSE 处理 | 替换自研 SSE 解析器为标准实现 | ApiService.SendChatStreamAsync 改用 SK |

### 从 Avalonia → DeerFlow.WPF 整合目标

| Avalonia 概念 | 预期整合方式 | 整合标志 |
|---------------|-------------|----------|
| `StyledProperty` 系统 | 评估 Theme ResourceDictionary 兼容性 | DarkTheme 双框架兼容 |
| `CompiledBinding` | 评估替换 XAML 反射绑定 | DataTemplate 性能基准测试 |
| 跨平台渲染管线 | 抽象 `IWindowManager` 接口 | 除 WPF 外增加 Avalonia 后端 |
| AOT 发布 | 评估 DeerFlow.WPF 的 AOT 可行性 | .csproj 添加 PublishAot 配置 |
| `ControlTheme` | 评估统一控件样式注册方案 | Themes/ 目录重构 |

---

## 可量化的进度指标

### 学习成果指标

| 指标 | 测量方法 | 阶段 1 目标 | 阶段 2 目标 |
|------|----------|------------|------------|
| 源码研读深度 | 关键文件数 + 笔记条目 | SK ≥ 15 个关键文件 | Avalonia ≥ 12 个关键文件 |
| 技术发现输出 | findings.md 条目数 | ≥ 5 条 SK 发现 | ≥ 5 条 Avalonia 发现 |
| 可落地模式提取 | 整合到 DeerFlow.WPF 的 Spec/代码数 | ≥ 3 项代码变更 | ≥ 3 项架构评估 |
| 原型验证 | 可运行的 Demo 项目数 | ≥ 1 个 SK Plugin Demo | ≥ 1 个 Avalonia 原型窗口 |
| 决策记录 | findings.md 决策条目数 | ≥ 1 项技术决策 | ≥ 1 项迁移决策 |

### 开发进展指标

| 指标 | 测量方法 | 合并至 DeerFlow.WPF 的验收标准 |
|------|----------|------------------------------|
| ApiService 重构 | Kernel 集成后的代码 diff 行数 | 编译通过 + 现有 40 测试不退化 |
| AgentOrchestration | 新增 Plan 执行能力 | 3 个以上新测试覆盖 Plan 路径 |
| Memory ISemanticTextMemory | 新接口实现 | MemoryViewModel 绑定 Connector 列表 |
| 跨平台窗口管理器 | IWindowManager 接口 | 2 个后端实现 (WPF + Avalonia) |
| AOT 构建 | dotnet publish 成功 | 发布文件大小 < 原始 200% |

---

## 审查会议安排

### 第 1 次审查 (2 周末 — 2026-05-19)
**议程**:
- [ ] Semantic Kernel 研读完成情况
- [ ] 检查 findings.md ≥ 5 条 SK 发现
- [ ] 评估 ApiService + AgentOrchestration 整合可行性
- [ ] 决定 SK 集成是否创建 Spec
- [ ] 调整阶段 2 (Avalonia) 的时间分配

### 第 2 次审查 (4 周末 — 2026-05-26)
**议程**:
- [ ] Avalonia 迁移评估完成情况
- [ ] 检查 findings.md ≥ 5 条 Avalonia 发现
- [ ] 评估跨平台迁移整体可行性
- [ ] 更新 DeerFlow.WPF v2.0 路线图
- [ ] 总结学习规划整体执行效果
- [ ] 选择下一批学习项目（可选）

**审查输出**:
- 每次审查后更新 `learning_progress.md`
- 产生的技术决策追加到 `findings.md` 决策记录区
- 调整 `learning_plan.md` 中的里程碑日期和时间分配

---

## 风险与依赖

### 风险

| 风险 | 描述 | 应对措施 |
|------|------|----------|
| **Semantic Kernel API 不稳定** | SK 仍在快速迭代，v1.x API 可能有 Breaking Changes | 固定研读版本，记录发现时标注版本号 |
| **Avalonia 迁移成本被低估** | WPF→Avalonia 可能需要在 XAML 和控件层做大量重写 | 只建立原型 + 差异评估，不承诺完整迁移 |
| **学习时间被 DeerFlow.WPF 开发挤占** | 用户可能优先推进功能而非学习 | 每周至少保留 2 个固定时间槽（每次 ≥ 2h），不可挤占 |
| **学习产出难以量化** | "理解"难以用代码体现 | 强制要求每次学习必须有代码或文档产出物 |

### 依赖

| 依赖 | 描述 |
|------|------|
| **Semantic Kernel 仓库** | 需要 GitHub 访问，当前网络环境已验证可用 |
| **Avalonia 文档和模板** | `dotnet new avalonia.app` 模板安装 |
| **.NET 10 SDK** | 已有 10.0.7，兼容 Avalonia 预览版 |
