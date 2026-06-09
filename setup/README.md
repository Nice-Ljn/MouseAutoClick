# 鼠标录制器 - 安装包制作指南

本目录包含用于创建安装程序的 Inno Setup 脚本。

## 📋 目录结构

```
setup/
├── MouseRecorder.iss    # 主安装脚本（推荐使用）
├── README.md            # 本说明文件
└── Output/              # 编译后的安装包输出目录（自动创建）
```

## 🚀 快速开始

### 1. 准备工作

#### 安装 Inno Setup Compiler

从官网下载并安装：https://jrsoftware.org/isdl.php

推荐使用 **Inno Setup 6.x 或更高版本**。

#### 发布应用程序

在 `MouseRecorderWpf` 项目目录下，运行以下命令之一：

**选项 A：单文件发布（推荐）**
```powershell
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true
```

**选项 B：自包含部署（不需要用户安装 .NET 运行时）**
```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

**选项 C：框架依赖部署（需要用户安装 .NET 8 运行时）**
```powershell
dotnet publish -c Release -r win-x64 --self-contained false
```

发布完成后，文件应该位于：
```
bin/Release/net8.0-windows/publish/win-x64/
```

### 2. 编译安装程序

#### 方法一：使用 GUI

1. 打开 **Inno Setup Compiler**
2. 点击 **File** → **Open**
3. 选择 `MouseRecorder.iss`
4. 点击 **Build** → **Compile**（或按 `F9`）
5. 编译完成后，安装包将位于 `Output` 目录

#### 方法二：使用命令行

```powershell
# 进入 setup 目录
cd setup

# 使用命令行编译器（假设 Inno Setup 安装在默认路径）
& "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" MouseRecorder.iss
```

## ⚙️ 自定义配置

### 修改应用程序信息

打开 `MouseRecorder.iss`，修改顶部的常量定义：

```iss
#define MyAppName "鼠标录制器"      ; 应用程序名称
#define MyAppVersion "1.0.0"         ; 版本号
#define MyAppPublisher "MouseRecorder" ; 发布者名称
#define MyAppExeName "MouseRecorderWpf.exe" ; 主程序文件名
```

### 修改安装图标

将您的图标文件放置在 `Icons` 目录，确保文件名是 `app.ico`。

或者修改脚本中的 `SetupIconFile` 路径：

```iss
SetupIconFile=..\Icons\app.ico
```

### 修改默认安装目录

```iss
DefaultDirName={autopf}\{#MyAppName}
```

其他可用选项：
- `{pf}` - Program Files 目录
- `{pf32}` - 32位 Program Files 目录
- `{pf64}` - 64位 Program Files 目录
- `{autopf}` - 自动选择 Program Files 目录
- `{userappdata}` - 用户应用数据目录
- `{commonappdata}` - 公共应用数据目录

### 修改输出文件名

```iss
OutputBaseFilename=MouseRecorderWpf-Setup-{#MyAppVersion}
```

## 📦 安装包功能

### 当前脚本包含的功能

✅ **现代化安装界面**（WizardStyle=modern）

✅ **多语言支持**（中文简体 + 英文）

✅ **压缩优化**（LZMA2 最高压缩级别）

✅ **可选的桌面快捷方式**

✅ **可选的任务栏快捷方式**

✅ **开始菜单快捷方式**

✅ **自动创建脚本数据目录**

✅ **卸载时清理用户数据**

✅ **无需管理员权限**（可在普通用户账户安装）

✅ **x64 架构检测**

### 如何添加功能

#### 添加 .NET 运行时检测

在 `[Code]` 部分添加检测逻辑，如果用户没有安装 .NET 8 运行时，提示他们下载安装。

#### 添加自定义许可协议

1. 创建 `License.txt` 文件
2. 在 `[Setup]` 部分添加：
   ```iss
   LicenseFile=License.txt
   ```

#### 添加自述文件

1. 创建 `Readme.txt` 或 `Readme.html`
2. 在 `[Setup]` 部分添加：
   ```iss
   InfoAfterFile=Readme.txt
   ```

#### 添加注册表项

在 `[Registry]` 部分添加：

```iss
Root: HKCU; Subkey: "Software\MouseRecorder"; ValueType: string; ValueName: "InstallPath"; ValueData: "{app}"; Flags: uninsdeletekey
```

## 📊 文件大小预估

| 发布模式 | 源文件大小 | 安装包大小（预估） |
|---------|-----------|------------------|
| 单文件+框架依赖 | ~150 MB | ~50-60 MB |
| 自包含部署 | ~200 MB | ~80-100 MB |

## 🔧 高级主题

### 签名安装包

如果您有代码签名证书，可以添加数字签名：

```iss
; 在 [Setup] 部分添加
SignTool=signtool
SignedUninstaller=yes

; 编译时使用命令
ISCC /S"signtool=signtool.exe sign /f cert.pfx /p password /t http://timestamp.comodoca.com/authenticode $f" MouseRecorder.iss
```

### 添加自定义安装步骤

在 `[Code]` 部分添加 Pascal 脚本，可以实现：
- 检测系统环境
- 检查依赖项
- 自定义安装逻辑
- 安装后配置

## 🎯 最佳实践

1. **始终在干净的环境测试**：在没有安装过 .NET 的虚拟机中测试安装
2. **版本号管理**：每次发布都更新 `MyAppVersion`
3. **备份配置**：保存好 `.iss` 文件，不要只依赖编译后的安装包
4. **测试卸载**：确保卸载程序能正确清理所有文件
5. **压缩测试**：比较不同压缩级别得到的文件大小

## 📚 参考资源

- **Inno Setup 官方文档**：https://jrsoftware.org/ishelp/
- **Inno Setup 示例**：安装目录下的 `Examples` 文件夹
- **.NET 部署指南**：https://learn.microsoft.com/dotnet/core/deploying/
- **代码签名指南**：https://learn.microsoft.com/windows/win32/seccrypto/cryptographic-tools

## ⚠️ 常见问题

### Q: 编译时提示找不到 ChineseSimplified.isl？

A: 确保安装了 "Inno Setup Preprocessor" 和多语言支持文件。可以在安装时选择完整安装，或者从 Inno Setup 安装目录复制语言文件到 `Languages` 目录。

### Q: 安装后无法运行程序？

A: 检查是否已安装 **.NET 8 Desktop Runtime**。可以从这里下载：
https://dotnet.microsoft.com/download/dotnet/8.0

### Q: 如何制作自包含的安装包（不需要 .NET 运行时）？

A: 使用以下命令发布：
```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

这会将 .NET 运行时打包到应用程序中。

### Q: 安装包太大，如何减小体积？

1. 启用更高级的压缩（当前已使用 LZMA2/ultra）
2. 使用框架依赖部署（更小，但需要用户安装 .NET）
3. 考虑使用 UPX 等工具压缩可执行文件（可能会被杀毒软件误报）

### Q: 如何创建无人值守安装？

运行时使用命令行参数：

```powershell
MouseRecorderWpf-Setup-1.0.0.exe /VERYSILENT /SUPPRESSMSGBOXES /NORESTART
```

---

**制作愉快！如果有任何问题，请参考 Inno Setup 官方文档。**
