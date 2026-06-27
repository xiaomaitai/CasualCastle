# AGENTS.md — CasualCastle

Godot **4.6** + **C# / .NET** 的 2D RTS 项目（类皇室战争）。物理：Jolt；UI 用 `CanvasLayer`。

## 术语

游戏术语（地块、城堡、建筑卡、战场等）以 `devPlan/concepts.md` 为准。

## 目录与归属

代码按业务模块放在 `scripts/`：


| 目录                                             | 用途                                       |
| ---------------------------------------------- | ---------------------------------------- |
| `autoload/`                                    | Godot Autoload（`GameManager`）            |
| `core/`                                        | 全局配置（`GameConfig`；`GameCoordinates` 为弃用 shim） |
| `flow/`                                        | 场景流转（`TitleScreen`、`MainGameController`） |
| `ui/`                                          | `UIManager` 及子控制器                        |
| `shop/` `card/` `night/` `building/` `battle/` | 对应玩法系统                                   |
| `audio/` `dev/`                                | 音频与开发工具                                  |
| `adapters/godot/`                              | Godot 适配层（坐标换算）                         |
| `adapters/persistence/`                        | 持久化适配（战报文件 IO）                         |


场景：`scenes/`；预制体：`prefabs/`；资源：`resources/`、`assets/`。

新功能放进**拥有该业务的模块目录**，不要堆到无关文件。`GameManager` 是 Autoload；`UIManager`、`ShopSystem` 等挂在 `main_game.tscn`，用静态 `Instance`。

## 编码约定

- 只用 **C#**，不用 GDScript
- 脚本 `PascalCase.cs`，场景 `snake_case.tscn`，预制体 `[ComponentName].tscn`
- 系统间用 **Godot 信号** 通信；脚本保持单一职责
- 共享常量放 `scripts/core/`
- 核心域项目 `CasualCastle.Domain/` 禁止引用 Godot 类型（使用端口与适配器模式）
- UI 场景在 `scenes/ui/`，逻辑在 `scripts/ui/` 或 `scripts/flow/`
- 不写 XML 文档注释；避免过度防御性编程

## 文档规范

- 设计文档以描述**规则和逻辑**为主，**非必要不写代码**
- 代码实现细节应在代码注释中说明，而非文档中重复
- 文档应简洁清晰，突出核心规则和流程

## 文档来源（进度与架构）

**不要在本文件维护里程碑或当前任务。**


| 需要了解     | 读取                                          |
| -------- | ------------------------------------------- |
| 当前任务、验收项 | `devPlan/currentTasks.md`                   |
| 架构 backlog   | `devPlan/todo.md`                           |
| 里程碑与路线图  | `devPlan/outline/`                          |
| 模块设计与依赖  | `devPlan/outline/c05ProjectArchitecture.md` |
| 融合 / AI 规则 | `devPlan/fusionSystemDesign.md`、`devPlan/battleReportDesign.md`、`devPlan/aiSystemDesign.md` |
| 运行时代码结构  | `devPlan/codeStructure.md`                  |
| 数据结构     | `devPlan/dataStructures.md`                 |


涉及计划、优先级、阶段状态时，**先读 `devPlan/`**。

## 工作规则

- 实现任务时若发现**需要重构**的结构（职责混乱、重复逻辑、模块边界不清、技术债阻碍当前改动），在回复中**明确提出来**，说明问题、影响和建议方向。
- 不要擅自扩大范围做大规模重构；是否纳入当前任务或记入 `devPlan` 由用户决定

## Git 规则

- **仅用户明确要求时**才 `git add` / `git commit`；不要自动提交
- 新建 `.cs`、`.tscn`、`.tres` 时，**一并提交**同名 `.uid` 文件
- 不提交 `.godot/`、`.mono/`、`*.csproj.user` 等缓存（已在 gitignore）

