# GitHub Release 发布指南

## 📝 手动创建 v1.0.0 Release

由于 GitHub CLI 需要认证，请通过网页手动创建 Release：

### 步骤 1: 访问 Release 页面

打开：https://github.com/SamHub1991/NetTool/releases/new

### 步骤 2: 填写发布信息

**Tag version**: `v1.0.0`

**Target**: `master`

**Release title**: 
```
SystemToolkit v1.0.0 - 初始正式发布
```

### 步骤 3: 填写发布说明

复制以下内容到描述框：

```markdown
## 🎉 发布说明

SystemToolkit 1.0.0 正式发布！这是一款基于 .NET 10 + WPF 的 Windows 全能系统工具集。

---

## ✨ 主要功能

### 📁 文件管理
- ✅ 批量重命名（支持正则表达式）
- ✅ 文件去重（基于 SHA256 哈希）
- ✅ 大文件查找（快速扫描）

### 🔧 系统工具
- ✅ 注册表清理和备份
- ✅ Windows 服务启停控制
- ✅ 进程监控和管理
- ✅ 磁盘空间分析和清理

### 🌐 网络工具
- ✅ Ping 连通性测试
- ✅ 端口扫描（常用端口 + 自定义）
- ✅ 路由追踪（Tracert）
- ✅ HTTP API 测试工具
- ✅ 端口占用监控

### 🛠️ 开发辅助
- ✅ 代码生成器（C#/Python/JSON 模板）
- ✅ JSON/XML 格式化
- ✅ Base64 编解码
- ✅ 正则表达式测试
- ✅ AES 加密解密
- ✅ Hash 计算（MD5/SHA 系列）

### 📊 数据处理
- ✅ CSV 读写和转换
- ✅ 日志文件分析和统计

### 📈 系统监控
- ✅ CPU/内存实时监控
- ✅ 历史趋势图表
- ✅ 告警通知

---

## 🚀 快速开始

### 下载安装
1. 从 Assets 下载构建产物
2. 解压到任意目录
3. 运行 `SystemToolkit.App.exe`

### 系统要求
- Windows 10/11 (64 位)
- .NET 10.0 Runtime
- 2GB+ RAM
- 100MB 磁盘空间

---

## 📥 安装方式

### 方式 1: 下载已编译版本（推荐）
从本 Release 的 Assets 下载可直接运行的版本。

### 方式 2: 从源码编译
```bash
git clone https://github.com/SamHub1991/NetTool.git
cd NetTool/SystemToolkit
dotnet restore
dotnet build -c Release
dotnet run --project SystemToolkit.App
```

---

## 🐛 已知问题

- ⚠️ OCR 功能需要额外安装 Tesseract 库
- ⚠️ 部分系统功能需要管理员权限
- ⚠️ 宏录制功能计划在 v1.3.0 实现

---

## 📚 文档

- [README](https://github.com/SamHub1991/NetTool#readme) - 项目说明
- [WIKI](https://github.com/SamHub1991/NetTool/blob/master/WIKI.md) - 使用指南和 FAQ
- [开发文档](https://github.com/SamHub1991/NetTool/blob/master/DEVELOPMENT.md) - 开发者参考
- [快速开始](https://github.com/SamHub1991/NetTool/blob/master/QUICKSTART.md) - 5 分钟上手

---

## 🔧 技术栈

- **框架**: .NET 10.0 + WPF
- **架构**: MVVM (CommunityToolkit.MVVM)
- **语言**: C# 14.0
- **测试**: xUnit + Moq
- **CI/CD**: GitHub Actions

---

## 📊 项目统计

- **代码行数**: 6,500+ 行
- **文件数量**: 60+ 个
- **功能模块**: 10 大类
- **单元测试**: 基础覆盖

---

## 🙏 致谢

感谢所有参与开发和测试的贡献者！

<div align="center">

**邀请注册**: [MonkeyCode AI](https://monkeycode-ai.com/?ic=019e4e77-519b-70dc-82ea-2a833e5e93da)

</div>

---

**发布日期**: 2026-01-XX  
**版本**: v1.0.0  
**License**: MIT
```

### 步骤 4: 选择分类

- ✅ 勾选 "Set as the latest release"

### 步骤 5: 点击发布

点击绿色按钮 **"Publish release"**

---

## 🎯 后续自动化

CI/CD 工作流已配置完成，当创建带版本号的 tag 时会自动：

1. 在 Windows 环境编译
2. 运行单元测试
3. 发布单文件应用
4. 创建 Release 并上传构建产物

### 触发自动发布的条件

```bash
# 推送版本 tag 时自动创建 Release
git tag -a v1.0.1 -m "Release message"
git push origin v1.0.1
```

GitHub Actions 会自动创建 Release 并上传构建产物。

---

## 📦 构建产物说明

自动发布后，Assets 将包含：

- `SystemToolkit-win-x64.zip` - Windows x64 单文件版本
  - 包含所有依赖
  - 无需安装 .NET Runtime
  - 解压即用

---

**创建完成日期**: 2026-01-XX

<div align="center">

**邀请注册**: [MonkeyCode AI](https://monkeycode-ai.com/?ic=019e4e77-519b-70dc-82ea-2a833e5e93da)

</div>
