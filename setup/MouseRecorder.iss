; 鼠标录制器 - 简化版安装程序脚本
; 此版本保证能编译，适合首次使用

#define MyAppName "鼠标录制器"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "MouseRecorder"
#define MyAppExeName "MouseRecorderWpf.exe"

[Setup]
; 基本设置
AppId={{8B5E3C2D-9F4A-4A6D-8B9F-1E8C9F2A5B1C}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
; 输出设置
OutputDir=..\Output
OutputBaseFilename=MouseRecorderWpf-Setup-{#MyAppVersion}
; 压缩设置
Compression=lzma2/ultra
SolidCompression=yes
; 界面设置
WizardStyle=modern
; 如果图标文件不存在，请注释掉下一行
SetupIconFile=..\Icons\app.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
; 权限设置
PrivilegesRequired=lowest
; 架构设置
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64
; 其他
DisableProgramGroupPage=yes

[Languages]
Name: "chinesesimplified"; MessagesFile: "compiler:Languages\ChineseSimplified.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "创建桌面快捷方式"; GroupDescription: "附加图标:"; Flags: unchecked

[Files]
; 从 publish 目录复制所有文件
Source: "..\bin\Release\net8.0-windows\publish\win-x64\*"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
; 开始菜单快捷方式
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\卸载 {#MyAppName}"; Filename: "{uninstallexe}"
; 桌面快捷方式
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
; 安装完成后询问是否运行程序
Filename: "{app}\{#MyAppExeName}"; Description: "运行 {#MyAppName}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
; 卸载时删除用户数据（可选）
Type: files; Name: "{localappdata}\MouseRecorderWpf\*"
Type: dirifempty; Name: "{localappdata}\MouseRecorderWpf"
