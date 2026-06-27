# CLAUDE.md — CasualCastle

本文件为 Claude Code 入口配置。项目通用规范见 `AGENTS.md`（编码约定、目录归属、文档来源等），此处补充 Claude 工作时的**行为规则**和**常用命令**。

## 行为规则

- 实现前先读 `AGENTS.md` 确认归属目录和命名规范
- 涉及计划、优先级、里程碑时，**先读 `devPlan/`**（入口：`devPlan/currentTasks.md`、`devPlan/todo.md`）
- 发现需要重构的结构问题（职责混乱、重复逻辑、模块边界不清），明确提出来，由用户决定是否纳入当前任务
- 不擅自扩展任务范围；不自动 `git add` / `git commit`（除非用户明确要求）
- 新建 `.cs`、`.tscn`、`.tres` 时一并提交同名 `.uid` 文件
- 只用 C#，不用 GDScript

## 项目速览

- **引擎**: Godot 4.6 + C# / .NET + Jolt 物理
- **类型**: 2D RTS（类皇室战争）
- **架构方向**: Phase 2 — C# 项目拆分 + DI 容器替代静态 Instance
- **项目结构**: `CasualCastle.Domain`（纯 C# 类库）、`CasualCastle.Game`（Godot 主项目）
- **代码层级**: `src/CasualCastle.Domain/`（领域 + 端口）、`src/CasualCastle.Game/scripts/adapters/`（适配器）
- **入口场景**: `main_game.tscn`（由 `scripts/adapters/Flow/MainGameController.cs` 驱动）
- **Autoload**: `GameManager`、`DisplaySettingsManager`（`scripts/adapters/Autoload/`）
- **DI**: `Microsoft.Extensions.DependencyInjection`，容器在 `CompositionRoot.cs` 中配置

## 常用命令

```bash
# 运行 Godot 编辑器（无参数打开项目选择器）
godot

# 从命令行运行当前场景
godot --path .

# 构建 C# 项目
dotnet build
```

## 关键文件索引

| 用途 | 文件 |
|------|------|
| 通用规范 | `AGENTS.md` |
| 当前任务 | `devPlan/currentTasks.md` |
| 架构 backlog | `devPlan/todo.md` |
| 游戏概念 | `devPlan/concepts.md` |
| 代码结构 | `devPlan/codeStructure.md` |
| 数据结构 | `devPlan/dataStructures.md` |
| 模块架构 | `devPlan/outline/c05ProjectArchitecture.md` |
| 融合规则 | `devPlan/fusionSystemDesign.md` |
| AI 规则 | `devPlan/aiSystemDesign.md` |
| 战报规则 | `devPlan/battleReportDesign.md` |
