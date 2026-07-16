# 云环境说明

云环境已预装工具链（随快照持久化，启动更新脚本无需重复安装）：**.NET 8 SDK** 与 **Godot 4.6.2 mono（Linux headless）**，均安装在 `/usr/local`，`dotnet` 与 `godot` 已在 PATH。

## 服务/命令（均在仓库根目录运行）

| 操作 | 命令 |
| --- | --- |
| 还原/构建 | `dotnet build CasualCastle.sln` |
| 单元测试（纯域，无需 Godot） | `dotnet test tests/CasualCastle.Domain.Tests/CasualCastle.Domain.Tests.csproj` |
| 导入资源（运行游戏前必须先跑一次） | `godot --headless --import` |
| 运行游戏 | `DISPLAY=:1 godot --path . --rendering-driver opengl3` |

启动更新脚本已完成 `git lfs pull` 与 `config.db` 重建；`dotnet build` 会自动 restore。

## 运行游戏的非显然要点

- 没有独立 lint 命令；`dotnet build`（0 warning）即充当静态检查。
- 云 VM 无 GPU：Godot 的 Vulkan（Forward+）会失败并自动回退到 OpenGL 软件渲染（Mesa llvmpipe）。显式加 `--rendering-driver opengl3` 可避免 Vulkan 报错刷屏。
- 图形需 X 显示 `:1`（VNC，1920x1200）；无声卡，音频回退 dummy 驱动，ALSA 报错可忽略。
- `assets/data/config.db` 未入库（`.gitignore`），由 `assets/data/config_db_dump.sql` 用 `sqlite3` 重建；缺失该 db 游戏无法正确加载数据。云 agent 的 `core.hooksPath` 指向别处，`githooks/` 的 checkout/merge 重建不生效，故改由启动更新脚本 `sqlite3 assets/data/config.db < assets/data/config_db_dump.sql` 重建。
- 该 dump 是 config.db 的唯一权威数据源（含 `asset_gen_tasks` 生图任务表 + 游戏运行时表）。修改数据请直接操作 `config.db`，提交时 pre-commit hook 会用 `sqlite3 config.db .dump` 覆盖生成 dump；不要手动编辑 dump。
- 美术资源走 **Git LFS**；仓库内 `*.png` 等可能只是 LFS 指针，需 `git lfs pull` 拉取真实内容后 Godot 才能导入。
- 已知非致命：`UIManager` 初始化时 `HandUiController.SetInputBlocked` 会打印一次 `NullReferenceException`，游戏仍正常渲染与运行（属既有代码行为，非环境问题）。
