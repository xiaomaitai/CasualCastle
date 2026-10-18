# 运行环境与命令（本机 Windows / 云）

本文件是构建、测试、导入与运行命令的唯一入口。除特别说明外，命令均在仓库根目录执行。

## 一、本机（Windows + DSH Desktop）

### 环境

| 项 | 值 |
| --- | --- |
| Godot | `C:\Program Files (x86)\Godot_v4.6.2-stable_mono_win64\`（`Godot_v4.6.2-stable_mono_win64.exe` 用于编辑器/游戏，`*_console.exe` 用于取 stdout） |
| .NET | .NET 8 SDK，项目目标框架 `net8.0` |
| 主场景 | 标题 `res://scenes/ui/title_screen.tscn`；对战 `res://scenes/main/main_game.tscn` |
| 冒烟运行器 | `tools/run_smoke.sh`（需 Git for Windows 的 bash） |

### 命令

| 操作 | 命令 |
| --- | --- |
| 还原/构建 | `dotnet build CasualCastle.sln` |
| 单元测试（纯域） | `dotnet test tests/CasualCastle.Domain.Tests/CasualCastle.Domain.Tests.csproj` |
| 存档往返测试 | `dotnet test tests/CasualCastle.Persistence.Tests/CasualCastle.Persistence.Tests.csproj` |
| 应用用例测试 | `dotnet test tests/CasualCastle.Application.Tests/CasualCastle.Application.Tests.csproj` |
| UID / 架构 / 冒烟路径校验 | `python tools/check_uids.py`、`python tools/check_architecture.py`、`python tools/check_smoke_paths.py` |
| 导入资源 | `python tools/godot_import.py --project . --godot <Godot console exe>` |
| 启动编辑器 + 游戏 | `pwsh -ExecutionPolicy Bypass -File tools/run_game.ps1` |
| 冒烟测试 | `tools/run_smoke.sh scenes/ui/dev_tools/<名>_smoke.tscn` |

### 启动游戏的约束

- 用 `Start-Process` 后台启动，**不要**加 `-Wait`
- **不要**运行 `godot --help`（部分环境会挂起）
- **禁止**用 winget / choco 安装或升级 Godot，使用本机已装目录
- 编辑器是否运行：`Get-CimInstance Win32_Process` 过滤 `Godot*.exe`，命令行含 `--editor` 与本项目路径（`tools/run_game.ps1` 已实现该判断）
- 手动启动等价命令：

```powershell
$godot = "C:\Program Files (x86)\Godot_v4.6.2-stable_mono_win64\Godot_v4.6.2-stable_mono_win64.exe"
$project = (Get-Location).Path
Start-Process $godot -ArgumentList '--editor','--path',$project -WorkingDirectory $project
Start-Process $godot -ArgumentList '--path',$project,'res://scenes/ui/title_screen.tscn' -WorkingDirectory $project
```

### 非显然要点

- 没有独立 lint 命令；`dotnet build` 零 warning 即充当静态检查。
- 改过 C# 后需重新 `dotnet build`，否则 Godot 跑的是旧程序集。
- 新增或变更素材（PNG/.tscn/.tres/.cs）后先跑 `godot-import`，否则 `.import`/`.uid` 侧车缺失会导致运行时报资源不存在。
- 美术资源走 **Git LFS**：仓库内 `*.png` 可能只是指针，需 `git lfs pull` 后才能导入。
- `assets/data/config.db` 未入库（`.gitignore`），由 `assets/data/config_db_dump.sql` 重建；缺失该 db 游戏无法正确加载数据。修改数据直接操作 `config.db`，提交时 pre-commit hook 覆盖生成 dump，**不要手动编辑 dump**。
- 已知非致命：`UIManager` 初始化时 `HandUiController.SetInputBlocked` 会打印一次 `NullReferenceException`，游戏仍正常渲染与运行（既有行为，非环境问题）。

## 二、云环境（Linux 快照）

云环境已预装工具链（随快照持久化，启动更新脚本无需重复安装）：**.NET 8 SDK** 与 **Godot 4.6.2 mono（Linux headless）**，均安装在 `/usr/local`，`dotnet` 与 `godot` 已在 PATH。

| 操作 | 命令 |
| --- | --- |
| 还原/构建 | `dotnet build CasualCastle.sln` |
| 单元测试（纯域，无需 Godot） | `dotnet test tests/CasualCastle.Domain.Tests/CasualCastle.Domain.Tests.csproj` |
| 存档往返测试（纯 C#，无需 Godot） | `dotnet test tests/CasualCastle.Persistence.Tests/CasualCastle.Persistence.Tests.csproj` |
| 应用用例测试（纯 C#，无需 Godot） | `dotnet test tests/CasualCastle.Application.Tests/CasualCastle.Application.Tests.csproj` |
| UID / 架构约束校验 | `python tools/check_uids.py`、`python tools/check_architecture.py` |
| 导入资源（运行游戏前必须先跑一次） | `godot --headless --import` |
| 运行游戏 | `DISPLAY=:1 godot --path . --rendering-driver opengl3` |

启动更新脚本已完成 `git lfs pull` 与 `config.db` 重建；`dotnet build` 会自动 restore。

### 云环境非显然要点

- 云 VM 无 GPU：Godot 的 Vulkan（Forward+）会失败并自动回退到 OpenGL 软件渲染（Mesa llvmpipe）。显式加 `--rendering-driver opengl3` 可避免 Vulkan 报错刷屏。
- 图形需 X 显示 `:1`（VNC，1920x1200）；无声卡，音频回退 dummy 驱动，ALSA 报错可忽略。
- 云 agent 的 `core.hooksPath` 指向别处，`githooks/` 的 checkout/merge 重建不生效，故改由启动更新脚本 `sqlite3 assets/data/config.db < assets/data/config_db_dump.sql` 重建。
