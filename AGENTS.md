# AGENTS.md — CasualCastle

Godot **4.6** + **C# / .NET** 的 2.5D RTS 项目（斜视角纸片人，类皇室战争）。物理：Jolt；UI 用 `CanvasLayer`。

## 术语

游戏术语（地块、城堡、建筑卡、战场等）以 `devPlan/concepts/general.md` 为准。

## 架构与目录归属

项目采用**六边形架构（Hexagonal Architecture）**与 **DDD（领域驱动设计）** 规范开发，代码按三层放在 `scripts/`：

> 项目所有文档、代码、素材、测试与工具的具体位置见 `devPlan/aiIndex.md`（AI 项目索引）。

| 目录 | 层级 | 用途 |
| --- | --- | --- |
| `domain/` | 核心域 | 纯 C# 业务规则（Shared、Building、Battle、History 四个子项目），零 Godot；端口接口契约也在此 |
| `adapters/godot/` | Godot 适配 | 所有 Godot 节点和场景脚本（autoload、building、battle、ui、flow 等） |
| `adapters/persistence/` | 持久化 | SQLite 与文件 IO（BattleReportStorage、Sqlite*Repository） |

场景：`scenes/`；预制体：`prefabs/`；素材：`assets/`。

新代码按三层归属放入对应目录；`GameManager` 是 Autoload；系统挂在 `main_game.tscn`，通过端口接口通信。

## 废弃资产

所有废弃资产统一存放在项目根目录下的 `_deprecated/` 文件夹（不再在子目录里单设废弃子目录）。**除非用户主动要求，不要读取、操作、提交该文件夹下的任何内容**。

## 编码约定

- 只用 **C#**，不用 GDScript
- 脚本 `PascalCase.cs`，场景 `snake_case.tscn`，预制体 `[ComponentName].tscn`
- 系统间用 **Godot 信号** 通信；脚本保持单一职责
- 共享常量放 `scripts/domain/Shared/`
- `scripts/domain/` 各子项目禁止引用 Godot 类型（使用端口与适配器模式）
- UI 场景在 `scenes/ui/`，逻辑在 `scripts/adapters/godot/ui/` 或 `scripts/adapters/godot/flow/`
- 每个代码文件开头写一行注释说明文件用途，除此之外不写任何注释（包括 XML 文档注释、行注释、块注释）
- 禁止使用魔术数字，统一用命名常量
- 变量声明使用显式类型，禁止 `var` 关键字
- 避免过度防御性编程，非必要不使用`?`等null操作符
- 代码文件统一使用 **tab** 缩进，不使用空格
- 每个方法尽量控制在 **10 行以内**，最多不超过 **20 行**（各行结构完全相似的平铺注册代码和枚举类代码除外）
- **domain 层改动必须有单元测试**；改动后提交前运行单元测试确保不破坏已有功能
- **bug 修复涉及领域层代码时**，必须写一个针对该 bug 的 `RealBug` 单元测试（命名体现 bug 场景），确保 bug 不会复现
- **开发者工具不走单元测试**；开发者工具代码放 `scripts/adapters/godot/dev/`，场景放 `scenes/ui/dev_tools/`，虽可调用 domain 层但文件位置与 domain 层严格区分
- **单元测试文件不添加任何注释**（包括文件头注释、行注释、块注释）

## 文档规范

- 设计文档以描述**规则和逻辑**为主，**非必要不写代码**
- 代码实现细节应在代码注释中说明，而非文档中重复
- 文档应简洁清晰，突出核心规则和流程

## 文档来源（进度与架构）

**不要在本文件维护里程碑或当前任务。**

所有文档、代码、素材、测试与工具的定位入口统一见 **`devPlan/aiIndex.md`**（AI 项目索引）；涉及计划、优先级、阶段状态时先查该索引，再按需打开对应文档。

## 临时任务文件（`devPlan/tmp/`）

`devPlan/tmp/` 下有两个临时文档，用于追踪当前施工进度：

| 文件 | 用途 | 何时写入 | 何时清理 |
|------|------|---------|---------|
| `currentTasks.md` | 当前正在做的任务的细化步骤和验收项 | 用户要求细化任务时 | 任务全部完成时，**直接删除文件全部内容，不需要保留总结** |
| `todo.md` | 长期架构 backlog，当前不想做、以后可能做的想法 | 发现需要记录但不立即处理的问题时 | 某项确定要做时移出；确定不做时删除该项 |

**清理规则**：
- 清理时直接删除全部内容，让文件变空，不写"已完成"、"历史记录"等总结
- 当用户说"继续当前任务"时，先查 `currentTasks.md`；如果已完成就查大纲找下一个 Phase
- 当用户要求把新任务细化到当前任务时，先彻底清理 `currentTasks.md` 和 `todo.md` 中已完成的内容

## Skills

- 用户要求生成图片、素材、图标等图像时，使用 **`generate-image` skill**（Codex 版：`$CODEX_HOME/skills/generate-image`，源自 `.claude/skills/generate-image.md`）
- 用户要求抠图/去背景时，使用 **`remove-bg` skill**（Codex 版：`$CODEX_HOME/skills/remove-bg`，源自 `.claude/skills/remove-bg.md`）
- 用户要求细化任务、准备当前任务时，使用 **`refine-tasks` skill**（Codex 版：`$CODEX_HOME/skills/refine-tasks`）
- 新增或变更 Godot 素材（PNG/.tscn/.tres/.cs 等）后，使用 **`godot-import` skill**（Codex 版：`$CODEX_HOME/skills/godot-import`，源自 `.claude/skills/godot-import.md`）无头导入，自动生成 `.import`/`.uid` 侧车文件，避免用户手动打开编辑器导入
- AI 素材管线脚本（生图 `tools/liblib_gen.py`、抠图 `tools/liblib_remove_bg.py`、Godot 导入 `tools/godot_import.py`）统一存放在仓库 `tools/` 并随项目版本控制，不在 `$CODEX_HOME` 单独维护，避免重装 Codex 后丢失

## 工作规则

- 文件编辑禁用 `apply_patch` 工具；一律使用 PowerShell 精确文本替换（读取原文 → 校验目标内容 → 替换 → 回写），保持原行尾格式
- 执行 Shell / Bash 命令时，使用 **Windows 风格**的路径分隔符（`\`）和命令（如 `dir`、`type`、`del`），而非 Unix 风格（`/`、`ls`、`cat`、`rm`）
- 当用户要求继续当前任务时，在查找 `devPlan/tmp/currentTasks.md`，如果里面所有任务已经完成，就在大纲中查找下一个 Phase 的任务，询问用户是否细化到当前任务。**不要**直接在 `devPlan/tmp/todo.md` 中查找
- 实现任务时若发现**需要重构**的结构（职责混乱、重复逻辑、模块边界不清、技术债阻碍当前改动），在回复中**明确提出来**，说明问题、影响和建议方向。
- 不要擅自扩大范围做大规模重构；是否纳入当前任务或记入 `devPlan` 由用户决定
- 一旦进行大规模重构后，要更新文档同步改动
- 当长时间思考无法解决问题或尝试一次失败后，不要靠猜测反复试错。应在关键路径上添加详细日志（`GD.Print` / `Debug.Log`），输出运行时状态和变量值，让用户运行后根据日志辅助调试

## 网络代理

- 需要翻墙时使用
- http_proxy=http://127.0.0.1:10809
- https_proxy=http://127.0.0.1:10809

## Git 规则

- **小改动和已完成的 bug 修复**可以直接 `git add` / `git commit` 提交，无需额外确认
- **大的修改、重构或涉及架构的改动**，需先向用户说明并得到确认后再提交
- 新建 `.cs`、`.tscn`、`.tres` 时，**一并提交**同名 `.uid` 文件
- 不提交 `.godot/`、`.mono/`、`*.csproj.user` 等缓存（已在 gitignore）

## Git Hooks

自定义钩子放在 `githooks/`，clone 后需执行一次：

    git config core.hooksPath githooks

`assets/data/config_db_dump.sql` 由 pre-commit hook 自动 dump 生成，**不要手动阅读或编辑此文件**。修改数据库应直接操作 `config.db`，提交时 hook 会自动更新 dump。
