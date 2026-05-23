# SystemToolkit 发布说明

## 📦 版本信息

- **版本号**: 1.0.0
- **发布日期**: 2026-01-XX
- **目标框架**: .NET 10.0
- **支持平台**: Windows 10/11 x64

---

## ✨ 功能概览

SystemToolkit 1.0.0 提供以下 6 大类 30+ 个功能模块：

### 1. 文件管理 (3 个功能)
- ✅ 批量重命名
- ✅ 文件去重
- ✅ 大文件查找

### 2. 注册表工具 (3 个功能)
- ✅ 注册表清理
- ✅ 备份恢复
- ✅ 注册表监控

### 3. 服务管理 (2 个功能)
- ✅ 服务启停
- ✅ 状态监控

### 4. 进程管理 (2 个功能)
- ✅ 进程监控
- ✅ 资源占用分析

### 5. 磁盘管理 (2 个功能)
- ✅ 空间分析
- ✅ 磁盘清理

### 6. 网络工具 (5 个功能)
- ✅ Ping 工具
- ✅ 端口扫描
- ✅ 路由追踪
- ✅ HTTP API 测试
- ✅ 端口监控

### 7. 开发辅助 (3 个功能)
- ✅ 代码生成器
- ✅ JSON/XML 格式化 + Base64
- ✅ 正则表达式测试

### 8. 数据处理 (3 个功能)
- ✅ AES 加密解密 + Hash 计算
- ✅ CSV 处理
- ✅ 日志分析

### 9. 日常效率 (4 个功能)
- 🚧 剪贴板历史 (基础实现)
- 🚧 截图工具 (基础实现)
- 🚧 定时器/提醒
- 🚧 快捷键映射

### 10. 系统监控 (4 个功能)
- ✅ CPU/内存实时监控
- 🚧 磁盘 IO 监控
- 🚧 网络带宽监控
- 🚧 应用进程监控

> ✅ = 完整实现 | 🚧 = 基础实现/待完善

---

## 🔧 技术规格

### 核心依赖
- .NET 10.0 SDK
- WPF (Windows Presentation Foundation)
- CommunityToolkit.MVVM 8.2.2
- Microsoft.Extensions.DependencyInjection 8.0.0
- Serilog 3.1.1

### 系统要求
- **操作系统**: Windows 10 版本 1903 或更高版本
- **内存**: 最低 2GB RAM，推荐 4GB
- **磁盘空间**: 100MB 可用空间
- **分辨率**: 最低 1366×768

### 权限要求
- **标准用户**: 文件管理、网络工具、开发辅助
- **管理员**: 注册表操作 (HKLM)、服务管理、进程终止

---

## 📥 安装指南

### 方式 1: 从源码编译

```bash
# 克隆仓库
git clone https://github.com/yourusername/SystemToolkit.git
cd SystemToolkit

# 还原依赖
dotnet restore

# 编译 Release 版本
dotnet build -c Release

# 运行应用
dotnet run --project SystemToolkit.App
```

### 方式 2: 单文件发布

```bash
# 发布单文件可执行文件
dotnet publish SystemToolkit.App \
  -c Release \
  -r win-x64 \
  --self-contained true \
  -p:PublishSingleFile=true \
  -p:PublishReadyToRun=true \
  -o ./publish

# 生成的可执行文件位于 publish/SystemToolkit.App.exe
```

### 方式 3: 使用安装包

下载安装程序并运行（待发布）。

---

## 🚀 快速入门

### 使用批量重命名

1. 启动应用
2. 左侧菜单选择「📁 文件管理」→「批量重命名」
3. 点击「浏览」选择目标目录
4. 设置搜索和替换规则
5. 点击「加载文件」预览文件列表
6. 点击「预览」查看重命名效果
7. 确认后点击「执行重命名」

### 监控系统资源

1. 启动应用
2. 左侧菜单选择「📈 系统监控」→「CPU/内存」
3. 查看实时 CPU 和内存使用率
4. 观察历史趋势图表

### 使用 Ping 工具

1. 启动应用
2. 左侧菜单选择「🌐 网络工具」→「Ping 工具」
3. 输入目标主机（如 `www.google.com`）
4. 点击「开始 Ping」
5. 查看往返时间和 TTL 信息

---

## 🐛 已知问题

### v1.0.0 已知限制

1. **剪贴板历史**
   - 仅支持文本内容
   - 不支持图片和富文本
   - 计划: v1.1.0 支持

2. **截图工具**
   - OCR 功能需要安装额外库
   - 当前仅支持保存截图
   - 计划: v1.2.0 集成 Tesseract

3. **宏录制**
   - 宏录制功能暂未实现
   - 计划: v1.3.0 实现基础录放功能

4. **CSV 处理**
   - 大文件 (10 万行+) 性能较差
   - 建议分批处理大文件

5. **网络扫描**
   - 端口扫描速度较慢
   - 部分防火墙可能阻止扫描

---

## 🔐 安全说明

### 数据存储
- 应用不存储用户数据
- 所有操作在内存中完成
- 临时文件在应用退出后自动清理

### 权限使用
- 仅在需要时请求管理员权限
- 不会修改系统关键区域
- 注册表操作会提示备份

### 隐私保护
- 不收集任何用户数据
- 不包含遥测功能
- 不开启任何网络连接（除用户主动使用的网络工具）

---

## 📝 更新日志

### v1.0.0 (2026-01-XX) - 初始版本

#### 已实现功能
- ✅ 文件管理服务 (批量重命名、去重、大文件查找)
- ✅ 注册表工具 (清理、备份、监控)
- ✅ Windows 服务管理
- ✅ 进程管理和监控
- ✅ 网络诊断工具 (Ping、端口扫描、Tracert)
- ✅ HTTP API 测试工具
- ✅ 加密解密工具 (AES、Hash、Base64)
- ✅ 代码生成器 (C#、Python、JSON 模板)
- ✅ JSON/XML 格式化
- ✅ 正则表达式测试
- ✅ 系统资源监控 (CPU、内存)

#### 基础实现
- 🚧 CSV 处理
- 🚧 日志分析
- 🚧 剪贴板历史
- 🚧 截图工具
- 🚧 定时器
- 🚧 磁盘和 Network 监控

### 计划中的功能

#### v1.1.0 (2026-Q1)
- 完善剪贴板历史（支持图片）
- 磁盘清理向导
- 批量 Excel 处理
- 数据库导入导出工具

#### v1.2.0 (2026-Q2)
- OCR 文字识别
- 网络监控面板
- 应用监控和自动重启
- 性能优化

#### v1.3.0 (2026-Q3)
- 宏录制和回放
- 快捷键全局配置
- 主题切换（暗黑/明亮）
- 插件系统框架

---

## 🤝 反馈与支持

### 报告问题
通过 GitHub Issues 提交 Bug 报告：
https://github.com/yourusername/SystemToolkit/issues

### 功能请求
在 GitHub Discussions 提出新功能的想法：
https://github.com/yourusername/SystemToolkit/discussions

### 贡献代码
欢迎提交 Pull Request!

---

## 📄 许可证

MIT License

Copyright (c) 2026 SystemToolkit Team

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

---

## 🙏 致谢

本项目由 AI 辅助开发工具协助完成。

<div align="center">

**邀请注册**: [MonkeyCode AI](https://monkeycode-ai.com/?ic=019e4e77-519b-70dc-82ea-2a833e5e93da)

感谢所有参与开发和测试的贡献者!

</div>

---

**应用版本**: 1.0.0  
**文档更新日期**: 2026-01-XX  
**文档作者**: SystemToolkit Dev Team
