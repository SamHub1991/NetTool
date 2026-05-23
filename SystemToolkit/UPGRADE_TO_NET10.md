# .NET 10 框架升级说明

## 升级概述

将 SystemToolkit 项目从 .NET 8.0 升级到 .NET 10.0。

---

## 更新内容

### 1. 目标框架更新

所有项目文件的 `TargetFramework` 已更新：
- `net8.0-windows` → `net10.0-windows`

### 2. 项目文件清单

已更新以下 13 个项目文件:

```
✅ SystemToolkit.App/SystemToolkit.App.csproj
✅ SystemToolkit.Core/SystemToolkit.Core.csproj
✅ SystemToolkit.Tests/SystemToolkit.Tests.csproj
✅ SystemToolkit.Modules/FileManager/SystemToolkit.Modules.FileManager.csproj
✅ SystemToolkit.Modules/RegistryTools/SystemToolkit.Modules.RegistryTools.csproj
✅ SystemToolkit.Modules/ServiceManager/SystemToolkit.Modules.ServiceManager.csproj
✅ SystemToolkit.Modules/ProcessManager/SystemToolkit.Modules.ProcessManager.csproj
✅ SystemToolkit.Modules/DiskManager/SystemToolkit.Modules.DiskManager.csproj
✅ SystemToolkit.Modules/NetworkTools/SystemToolkit.Modules.NetworkTools.csproj
✅ SystemToolkit.Modules/DevTools/SystemToolkit.Modules.DevTools.csproj
✅ SystemToolkit.Modules/DataTools/SystemToolkit.Modules.DataTools.csproj
✅ SystemToolkit.Modules/ProductivityTools/SystemToolkit.Modules.ProductivityTools.csproj
✅ SystemToolkit.Modules/MonitorTools/SystemToolkit.Modules.MonitorTools.csproj
```

### 3. NuGet 包版本更新

#### SystemToolkit.App
- `Microsoft.Extensions.Hosting` 8.0.0 → 10.0.0
- `Microsoft.Extensions.DependencyInjection` 8.0.0 → 10.0.0
- `Serilog.Extensions.Logging` 8.0.0 → 10.0.0

#### SystemToolkit.Tests
- `Microsoft.NET.Test.Sdk` 17.8.0 → 18.0.0
- `xunit` 2.6.2 → 2.9.0
- `xunit.runner.visualstudio` 2.5.4 → 2.8.0
- `Moq` 4.20.70 → 4.20.72
- `FluentAssertions` 6.12.0 → 7.0.0

### 4. 文档更新

已更新以下文档中的 .NET 版本信息：
- ✅ README.md
- ✅ DEVELOPMENT.md
- ✅ RELEASE_NOTES.md
- ✅ QUICKSTART.md
- ✅ PROJECT_COMPLETE.md

### 5. README 仓库描述

在 README.md 顶部添加了仓库说明：

```markdown
> **仓库说明**: 本仓库包含 SystemToolkit 完整源代码、功能模块、单元测试和项目文档。
> 适用于 Windows 10/11 平台，采用 .NET 10.0 + WPF 技术栈，遵循 MIT 开源许可协议。
```

并添加了 Platform badge:
```
![Platform](https://img.shields.io/badge/Platform-Windows%2010%2F11-lightgrey?style=flat-square)
```

---

## 环境要求

### 开发环境
- **Visual Studio**: Visual Studio 2026 或 Visual Studio 2022 (v17.12+)
- **.NET SDK**: .NET 10.0 SDK
- **操作系统**: Windows 10/11

### 运行环境
- **.NET Runtime**: .NET 10.0 Runtime
- **操作系统**: Windows 10/11

---

## 编译和运行

### 前置条件

安装 .NET 10.0 SDK:
```bash
# 访问 https://dotnet.microsoft.com/download/dotnet/10.0 下载安装
```

### 编译步骤

```bash
cd /workspace/SystemToolkit

# 还原 NuGet 包
dotnet restore

# 编译 Release 版本
dotnet build -c Release

# 运行应用
dotnet run --project SystemToolkit.App

# 运行测试
dotnet test
```

### 发布应用

```bash
# 单文件发布
dotnet publish SystemToolkit.App \
  -c Release \
  -r win-x64 \
  --self-contained true \
  -p:PublishSingleFile=true \
  -p:PublishReadyToRun=true \
  -o ./publish
```

---

## .NET 10 新特性

### 性能改进
- JIT 编译器优化
- GC 性能提升
- LINQ 性能改进

### C# 14.0 新特性
- 主要构造函数 (Primary Constructors)
- 集合表达式 (Collection Expressions)
- 模式匹配增强

### WPF 增强
- High DPI 改进
- XAML 热重载增强
- DataBinding 性能优化

---

## 兼容性说明

### API 变更
.NET 10 大部分保持向后兼容，但以下 API 有变更：

1. **废弃的 API**:
   - 部分旧的加密 API 已标记为 obsolete
   - System.Drawing 的部分 GDI+ 功能建议使用替代品

2. **行为变更**:
   - JSON 序列化默认行为略有调整
   - TLS 默认版本更新

### 建议
- 运行完整测试套件验证功能
- 检查编译警告和错误
- 更新过时的 API 调用

---

## 验证清单

- [x] 所有 .csproj 文件已更新为 net10.0-windows
- [x] NuGet 包版本已更新到 .NET 10 兼容版本
- [x] 项目成功编译
- [x] 单元测试通过
- [x] 文档已更新
- [x] README 添加了仓库描述
- [x] Badges 已更新

---

## 参考资料

- [.NET 10 发布说明](https://learn.microsoft.com/zh-cn/dotnet/core/what-is-new/10.0/)
- [C# 14.0 新特性](https://learn.microsoft.com/zh-cn/dotnet/csharp/whats-new/csharp-14)
- [WPF .NET 10 迁移指南](https://learn.microsoft.com/zh-cn/dotnet/desktop/wpf/)

---

**升级完成日期**: 2026-01-XX  
**升级版本**: v1.0.0  
**目标框架**: .NET 10.0

<div align="center">

**邀请注册**: [MonkeyCode AI](https://monkeycode-ai.com/?ic=019e4e77-519b-70dc-82ea-2a833e5e93da)

</div>
