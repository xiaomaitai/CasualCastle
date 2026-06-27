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
- **架构方向**: Phase 2 — 保留三层，domain 拆为 9 个项目 + DI 注入
- **项目结构**: `scripts/domain/CasualCastle.Domain.{Shared,Data,Night,Shop,Building,Fusion,Battle,Report,Replay}/`
- **依赖规则**: domain 项目零 Godot，adapters 与主项目编译在一起实现端口
- **入口场景**: `scenes/main/main_game.tscn`
- **DI**: `Microsoft.Extensions.DependencyInjection`，`CompositionRoot.Build()` 注册，`GameManager.Services` 获取

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
