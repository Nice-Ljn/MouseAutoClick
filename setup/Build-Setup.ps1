# 鼠标录制器 - 一键编译安装包脚本
# 运行方式: powershell -ExecutionPolicy Bypass -File .\Build-Setup.ps1

param(
    [string]$Mode = "SingleFile",  # SingleFile | SelfContained | FrameworkDependent
    [string]$Version = "1.0.0",
    [switch]$Clean
)

$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectDir = Split-Path -Parent $ScriptDir
$OutputDir = Join-Path $ScriptDir "Output"

Write-Host "=======================================" -ForegroundColor Cyan
Write-Host "  鼠标录制器 - 安装包编译器" -ForegroundColor Cyan
Write-Host "=======================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "配置信息:" -ForegroundColor Yellow
Write-Host "  模式: $Mode"
Write-Host "  版本: $Version"
Write-Host "  项目目录: $ProjectDir"
Write-Host ""

# 步骤 1: 清理旧文件（可选）
if ($Clean) {
    Write-Host "[1/3] 清理旧文件..." -ForegroundColor Green
    if (Test-Path (Join-Path $ProjectDir "bin")) {
        Remove-Item -Path (Join-Path $ProjectDir "bin") -Recurse -Force
        Write-Host "  ✓ 已清理 bin 目录"
    }
    if (Test-Path $OutputDir) {
        Remove-Item -Path $OutputDir -Recurse -Force
        New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
        Write-Host "  ✓ 已清理输出目录"
    }
} else {
    Write-Host "[1/3] 跳过清理..." -ForegroundColor Yellow
}

# 步骤 2: 发布应用程序
Write-Host ""
Write-Host "[2/3] 发布应用程序..." -ForegroundColor Green

$publishArgs = @(
    "publish",
    $ProjectDir,
    "-c", "Release",
    "-r", "win-x64",
    "--nologo"
)

switch ($Mode) {
    "SingleFile" {
        $publishArgs += @("--self-contained", "false", "-p:PublishSingleFile=true")
    }
    "SelfContained" {
        $publishArgs += @("--self-contained", "true", "-p:PublishSingleFile=true")
    }
    "FrameworkDependent" {
        $publishArgs += @("--self-contained", "false")
    }
}

try {
    $publishOutput = & dotnet @publishArgs 2>&1
    Write-Host $publishOutput
    Write-Host "  ✓ 发布成功！"
} catch {
    Write-Host "  ✗ 发布失败！" -ForegroundColor Red
    Write-Host "  错误信息: $_" -ForegroundColor Red
    exit 1
}

# 验证发布文件
$publishDir = Join-Path $ProjectDir "bin\Release\net8.0-windows\publish\win-x64"
if (-not (Test-Path (Join-Path $publishDir "MouseRecorderWpf.exe"))) {
    Write-Host "  ✗ 找不到发布文件！" -ForegroundColor Red
    exit 1
}

$fileCount = (Get-ChildItem $publishDir | Measure-Object).Count
$totalSize = (Get-ChildItem $publishDir -Recurse | Measure-Object Length -Sum).Sum / 1MB
Write-Host "  文件数: $fileCount 个"
Write-Host "  总大小: $([math]::Round($totalSize, 2)) MB"

# 步骤 3: 编译安装包
Write-Host ""
Write-Host "[3/3] 编译安装包..." -ForegroundColor Green

# 查找 Inno Setup 编译器
$innoPaths = @(
    "C:\Program Files (x86)\Inno Setup 6\ISCC.exe",
    "C:\Program Files\Inno Setup 6\ISCC.exe",
    "C:\Program Files (x86)\Inno Setup 5\ISCC.exe"
)

$innoCompiler = $null
foreach ($path in $innoPaths) {
    if (Test-Path $path) {
        $innoCompiler = $path
        break
    }
}

if ($null -eq $innoCompiler) {
    Write-Host "  ✗ 找不到 Inno Setup 编译器！" -ForegroundColor Red
    Write-Host "  请从 https://jrsoftware.org/isdl.php 下载安装" -ForegroundColor Yellow
    exit 1
}

Write-Host "  编译器路径: $innoCompiler"

# 更新版本号到脚本
$issFile = Join-Path $ScriptDir "MouseRecorder.iss"
$issContent = Get-Content $issFile -Raw
$issContent = $issContent -replace '#define MyAppVersion ".*?"', "#define MyAppVersion `"$Version`""
Set-Content -Path $issFile -Value $issContent -Encoding UTF8
Write-Host "  ✓ 版本号已更新为 $Version"

# 运行编译
try {
    $innoArgs = @(
        $issFile
    )
    
    $process = Start-Process -FilePath $innoCompiler -ArgumentList $innoArgs -Wait -PassThru -NoNewWindow -RedirectStandardOutput (Join-Path $ScriptDir "inno_output.log") -RedirectStandardError (Join-Path $ScriptDir "inno_error.log")
    
    if ($process.ExitCode -eq 0) {
        Write-Host "  ✓ 安装包编译成功！"
    } else {
        Write-Host "  ✗ 安装包编译失败！" -ForegroundColor Red
        Write-Host "  退出代码: $($process.ExitCode)" -ForegroundColor Red
        Write-Host "  查看 inno_output.log 和 inno_error.log 了解详情" -ForegroundColor Yellow
        exit 1
    }
} catch {
    Write-Host "  ✗ 编译过程出错！" -ForegroundColor Red
    Write-Host "  错误信息: $_" -ForegroundColor Red
    exit 1
}

# 输出结果
Write-Host ""
Write-Host "=======================================" -ForegroundColor Cyan
Write-Host "  完成！" -ForegroundColor Green
Write-Host "=======================================" -ForegroundColor Cyan
Write-Host ""

$setupFile = Get-ChildItem $OutputDir -Filter "*.exe" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
if ($null -ne $setupFile) {
    $sizeMB = [math]::Round($setupFile.Length / 1MB, 2)
    Write-Host "安装包文件: $($setupFile.Name)" -ForegroundColor Green
    Write-Host "文件大小: $sizeMB MB" -ForegroundColor Green
    Write-Host "文件位置: $($setupFile.FullName)" -ForegroundColor Green
    Write-Host ""
    Write-Host "提示: 您可以在资源管理器中打开此目录:" -ForegroundColor Yellow
    Write-Host "  explorer.exe `"$OutputDir`""
}

Write-Host ""
exit 0
