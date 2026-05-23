# SystemToolkit - 系统工具箱

> 🧰 一款基于 .NET 10 + WPF 的 Windows 全能系统工具集，集成文件管理、网络诊断、系统监控、进程服务管理、开发辅助、数据加密解密等 30+ 实用功能，为开发者和系统管理员提供一站式效率解决方案。

<div align="center">

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat-square&logo=dotnet)
![C#](https://img.shields.io/badge/C%23-14.0-239120?style=flat-square&logo=c-sharp)
![WPF](https://img.shields.io/badge/WPF-Windows-007ACC?style=flat-square&logo=windows)
![License](https://img.shields.io/badge/License-MIT-green?style=flat-square)
![Platform](https://img.shields.io/badge/Platform-Windows%2010%2F11-lightgrey?style=flat-square)

</div>

> **仓库说明**: 本仓库包含 SystemToolkit 完整源代码、功能模块、单元测试和项目文档。适用于 Windows 10/11 平台，采用 .NET 10.0 + WPF 技术栈，遵循 MIT 开源许可协议。

---

## 📋 功能特性

### 1. 📁 文件管理
- **批量重命名**: 支持正则表达式、前缀后缀、搜索替换
- **文件去重**: 基于哈希值的重复文件检测和清理
- **大文件查找**: 快速扫描定位大文件，释放磁盘空间

### 2. 🔧 注册表操作
- **注册表清理**: 检测并清理无效注册表项
- **备份恢复**: 完整注册表备份和还原功能
- **注册表监控**: 实时监控注册表变化

### 3. ⚙️ 服务管理
- **服务启停**: 启动、停止、重启 Windows 服务
- **状态监控**: 查看服务状态和启动类型配置

### 4. 🚀 进程管理
- **进程监控**: 查看所有运行进程详细信息
- **资源占用分析**: CPU、内存、磁盘、网络实时监控

### 5. 💾 磁盘管理
- **磁盘空间分析**: 可视化展示磁盘使用情况
- **磁盘清理**: 智能识别可清理文件，释放空间

### 6. 🌐 网络工具
- **Ping 工具**: 网络连通性测试
- **端口扫描**: 快速扫描开放端口
- **路由追踪**: Tracert 路径追踪
- **HTTP 测试**: API 请求测试工具
- **端口监控**: 检测端口占用情况

### 7. 🛠️ 开发辅助
- **代码生成**: 模板代码快速生成
- **格式化工具**: JSON/XML 格式化、Base64 编解码
- **正则测试**: 正则表达式在线测试

### 8. 📊 数据处理
- **加密解密**: AES/RSA 加密、Hash 计算
- **CSV 处理**: CSV 转换、合并、筛选
- **日志分析**: 日志解析、统计、告警

### 9. ⏱️ 日常效率
- **剪贴板增强**: 历史记录、格式转换
- **截图工具**: 区域截图、OCR 识别
- **定时器/提醒**: 倒计时、番茄钟、任务提醒
- **快捷键映射**: 全局快捷键、宏录制

### 10. 📈 监控告警
- **CPU/内存监控**: 实时资源监控、历史趋势
- **磁盘监控**: 空间预警、IO 性能
- **网络监控**: 带宽监控、连接数统计
- **应用监控**: 进程存活检测、崩溃重启

---

## 🚀 快速开始

### 环境要求
- Windows 10/11
- .NET 10.0 Runtime
- Visual Studio 2022/2026 (开发)

### 编译运行

```bash
# 克隆项目
git clone https://github.com/yourusername/SystemToolkit.git

# 进入项目目录
cd SystemToolkit

# 还原 NuGet 包
dotnet restore

# 编译项目
dotnet build -c Release

# 运行应用
dotnet run --project SystemToolkit.App
```

### 发布应用

```bash
# 发布为独立应用
dotnet publish SystemToolkit.App -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

---

## 📦 项目结构

```
SystemToolkit/
├── SystemToolkit.App/              # 主应用程序 (WPF)
│   ├── ViewModels/                 # 视图模型
│   ├── Views/                      # 视图
│   └── Services/                   # 应用服务
├── SystemToolkit.Core/             # 核心业务逻辑
│   ├── Models/                     # 数据模型
│   └── Services/                   # 业务服务
├── SystemToolkit.Modules/          # 功能模块
│   ├── FileManager/                # 文件管理模块
│   ├── RegistryTools/              # 注册表工具
│   ├── ServiceManager/             # 服务管理
│   ├── ProcessManager/             # 进程管理
│   ├── DiskManager/                # 磁盘管理
│   ├── NetworkTools/               # 网络工具
│   ├── DevTools/                   # 开发辅助
│   ├── DataTools/                  # 数据处理
│   ├── ProductivityTools/          # 效率工具
│   └── MonitorTools/               # 监控工具
└── SystemToolkit.Tests/            # 单元测试
```

---

## 🛠️ 技术栈

- **框架**: .NET 10.0, WPF
- **架构**: MVVM (CommunityToolkit.MVVM)
- **日志**: Serilog
- **依赖注入**: Microsoft.Extensions.DependencyInjection
- **系统 API**: System.Management, System.ServiceProcess

---

## 📝 使用说明

### 文件批量重命名

1. 选择目标文件夹
2. 设置重命名规则（搜索/替换、前缀/后缀）
3. 点击"预览"查看效果
4. 确认无误后点击"执行重命名"

### 网络 Ping 测试

1. 输入目标主机地址（域名或 IP）
2. 点击"开始 Ping"
3. 查看往返时间、TTL 等信息

### AES 加密解密

1. 选择"AES 加解密"标签页
2. 设置密钥和 IV（初始化向量）
3. 输入要加密/解密的内容
4. 点击"加密"或"解密"按钮

---

## 🔐 权限说明

部分功能需要管理员权限：
- 注册表操作 (HKLM 部分)
- Windows 服务管理
- 进程终止
- 系统文件访问

应用会在需要时自动请求提升权限。

---

## 📄 许可证

MIT License - 详见 [LICENSE](LICENSE) 文件

---

## 🤝 贡献

欢迎提交 Issue 和 Pull Request！

## 📧 联系

- **项目主页**: [GitHub](https://github.com/yourusername/SystemToolkit)
- **问题反馈**: [Issues](https://github.com/yourusername/SystemToolkit/issues)

---

## 🌟 致谢

感谢 [MonkeyCode](https://monkeycode-ai.com/?ic=019e4e77-519b-70dc-82ea-2a833e5e93da) 提供 AI 辅助开发支持。

---

<div align="center" style="margin-top: 20px;">

**邀请注册**: [MonkeyCode AI](https://monkeycode-ai.com/?ic=019e4e77-519b-70dc-82ea-2a833e5e93da)

Made with ❤️ by SystemToolkit Team

</div>
