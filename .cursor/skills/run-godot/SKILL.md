---
name: run-godot
description: >-
  启动 CasualCastle 的 Godot 编辑器与游戏。从本机 Godot 安装目录解析可执行文件；
  编辑器未运行时同时启动编辑器和游戏，编辑器已运行时仅启动游戏。
  在用户要求运行、启动、测试 Godot 项目或打开游戏时使用。
---

# 启动 Godot 编辑器与游戏

## 约束

- **禁止**通过 winget、choco 等方式安装 Godot
- **不要**读取桌面快捷方式，使用本机已安装的 Godot 文件夹
- 用 `Start-Process` 后台启动，**不要**加 `-Wait`
- **不要**运行 `godot --help`，在部分环境下会挂起

## 本机路径

| 项 | 路径 |
|----|------|
| Godot 安装目录 | `C:\Program Files (x86)\Godot_v4.6.2-stable_mono_win64` |
| 可执行文件 | 安装目录下 `Godot*.exe`（当前为 `Godot_v4.6.2-stable_mono_win64.exe`） |
| 项目目录 | 仓库根目录 |
| 主场景 | `res://scenes/ui/title_screen.tscn`（标题）；对战 `res://scenes/main/main_game.tscn` |

## 启动流程

```
编辑器已打开（含 --editor 且命令行含本项目路径）？
├─ 否 → 启动编辑器 → 等待 3 秒 → 启动游戏
└─ 是 → 仅启动游戏
```

## 执行方式

优先运行项目脚本（从仓库根目录）：

```powershell
powershell -ExecutionPolicy Bypass -File ".cursor/skills/run-godot/scripts/run.ps1"
```

脚本从 Godot 安装目录查找 `Godot*.exe`，不依赖桌面快捷方式。

## 手动命令

```powershell
$godotDir = "C:\Program Files (x86)\Godot_v4.6.2-stable_mono_win64"
$godot = (Get-ChildItem $godotDir -Filter "Godot*.exe" | Select-Object -First 1).FullName
$project = "c:\Users\zhouy\Documents\GodotProjects\CasualCastle"

# 仅编辑器
Start-Process $godot -ArgumentList '--editor','--path',$project -WorkingDirectory $project

# 仅游戏
Start-Process $godot -ArgumentList '--path',$project,'res://scenes/main/main_game.tscn' -WorkingDirectory $project
```

## 编辑器检测

查询 `Win32_Process`，进程名匹配 `Godot*.exe`，且命令行同时包含 `--editor` 和本项目路径。

## C# 编译

改过 C# 后可选先编译：

```powershell
dotnet build
```

## Godot `.uid` 文件（必须提交）

在编辑器中新建 C# 脚本、场景（`.tscn`）、资源（`.tres`）时，Godot 会自动生成同名 `.uid` 文件（如 `DevInputLogger.cs.uid`）。

- 这些文件**不是**可丢弃的缓存，**必须**与对应资源一起纳入 git
- Agent 新增脚本/场景后，提交时**不要遗漏** `.uid`；否则他人拉代码或场景引用可能不稳定
- 本仓库惯例：`scripts/**/*.cs.uid` 等与源文件成对提交
