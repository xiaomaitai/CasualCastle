# AGENTS.md - CasualCastle Development Guide

## Project Overview

**CasualCastle** is a 2D RTS game built in **Godot 4.6** using **C# / .NET** (Mono), similar to Clash Royale. The game features auto-generating barracks, unit combat, castle destruction mechanics, and an in-progress day/night loop for the MVP roadmap.

## Game Concepts

> 遇到游戏术语（如地块、城堡、建筑卡、建筑、战场等）时，优先参考 `devPlan/concepts.md` 中的定义，确保术语使用与概念文档一致。

## Architecture & Directory Structure

### Core Organization
- **`scripts/`** - All C# code organized by purpose:
  - `autoload/` - Scene-attached manager singletons such as `GameManager` and `UIManager`; they are not currently registered as Godot Project Settings autoloads
  - `systems/` - Game systems and placeholders such as `NightSystem`, `ShopSystem`, and `CardSystem`
  - `utils/` - Shared utility classes and helper functions
- **`scenes/`** - Scene files organized by context:
  - `main/` - Main entry point scene(s)
  - `ui/` - UI components and screens
  - `levels/` - Level-specific scenes
- **`prefabs/`** - Reusable scene templates (Barracks, Soldier, Castle)
- **`resources/`** - Godot resource files (.tres, custom resources)
- **`assets/`** - Game content:
  - `art/` - Game graphics (sprites, UI elements)
  - `audio/` - Sound files
  - `fonts/` - Custom fonts

### Key Files
- `project.godot` - Engine configuration, contains physics engine (Jolt), rendering backend selection
- `.editorconfig` - Code formatting rules (UTF-8 encoding enforced)

## Development Patterns

### Godot 4.6 + C# Conventions
1. **Language**: Use C# (not GDScript). The project is configured with .NET assembly name "CasualCastle".
2. **Scene Structure**: Scenes are `.tscn` files (Godot text format); attach C# scripts as nodes.
3. **Manager Singleton Pattern**: Current `scripts/autoload/` managers are scene nodes with static `Instance` references. Do not assume they are Project Settings autoloads unless `project.godot` is updated.
4. **Systems Architecture**: Implement game systems in `scripts/systems/` as stateful managers that handle specific domains.

### Node Attachment & Script Organization
- Attach scripts directly to scene nodes for node-specific logic
- Use inheritance from Node2D for 2D game elements
- Component-based composition: Keep scripts focused on single responsibility

### Signal Patterns
- Use Godot signals for inter-system communication
- Emit signals from systems; subscribe in UI or other systems

## Coding Practices

### File Naming
- C# scripts: `PascalCase.cs` (e.g., `BattleSystem.cs`, `GameManager.cs`)
- Scene files: `snake_case.tscn` (e.g., `main_game.tscn`, `barracks.tscn`)
- Prefab scenes: `[ComponentName].tscn`

### Code Structure
```csharp
public partial class BattleSystem : Node2D
{
    [Signal]
    public delegate void UnitSpawnedEventHandler(Unit unit);
    
    public override void _Ready()
    {
        // Initialization
    }
}
```

### Import Path Convention
- Reference scenes: `res://scenes/main/main_game.tscn`
- Reference assets: `res://assets/art/sprites/barracks.png`
- Reference scripts: Attach directly as nodes in Godot editor

## Common Tasks

### Adding a New System
1. Create `scripts/systems/YourSystem.cs` inheriting from `Node2D` or appropriate base
2. Implement `_Ready()` for initialization
3. Define signals for output events
4. Register in autoload or attach to main scene

### Adding UI Elements
1. Create scene in `scenes/ui/YourUI.tscn`
2. Attach C# control script (inherit from Control/Panel/etc.)
3. Connect signals from game systems
4. Place prefabs in `prefabs/` if reusable

## Physics & Rendering

### Physics Engine
- **Godot 4.6** uses **Jolt Physics** (configured in project.godot)
- Use `Area2D` and `CollisionShape2D` for 2D collision detection
- Query PhysicsServer2D for raycasts/shape checks

### Rendering
- **Forward+ renderer**
- Use CanvasLayer for UI elements
- Adjust in project.godot `[rendering]` section if testing cross-platform

## Build & Deployment

### Running the Project
- **Editor**: Open in Godot 4.6+ editor (File → Open Project)
- **Export**: Use Godot export templates (configured via project.godot's `export_presets.cfg` if present)

### Common Editor Actions
- Attach script: Right-click node → "Attach Script" → Save in `scripts/` hierarchy
- Create scene: Scene → New Scene → Choose root node type → Save in `scenes/` hierarchy
- Add autoload: Project → Project Settings → Autoload tab → Add script

## Gitignore & Version Control
- `.godot/` cache excluded (auto-generated)
- `.mono/` and `data_*/` excluded (C# cache)
- `*.csproj.user` excluded (IDE user settings)
- `mono_crash.*.json` excluded (crash dumps)

## When Extending This Project

1. **Before adding code**: Check if it belongs in `systems/`, `utils/`, or `autoload/`
2. **New game systems**: Use signal-based architecture for loose coupling
3. **UI elements**: Keep in `scenes/ui/` and use separate scripts per component
4. **Utilities**: Pure functions in `scripts/utils/` (no external state)
5. **Test with Godot editor**: Most issues caught in real-time editing

## References

- **Godot 4.6 C# Docs**: https://docs.godotengine.org/en/4.6/tutorials/scripting/c_sharp/
- **Godot Signals**: https://docs.godotengine.org/en/4.6/tutorials/step_by_step/signals.html
- **Jolt Physics**: https://godotengine.org/news/godot-4-0-brings-powerful-vulkan-rendering-support/
- **Project-specific patterns**: See existing code in `scripts/systems/` and `scenes/`

## Development Plan Folder

- 项目开发大纲按章节拆分存放于 devPlan/outline/ 文件夹。
- 当前任务文件为 devPlan/currentTasks.md，记录当前阶段任务、验收项和待办。
- 运行时代码结构文档为 devPlan/codeStructure.md，记录现有系统、类图、主要运行链路和维护建议。
- 数据结构设计见 devPlan/dataStructures.md。
- 当 agent 需要读取开发计划、当前任务、系统模块、里程碑或任务优先级时，应首先读取 devPlan 文件夹内容以获取最新计划。

## 开发规则

### Git提交规则
- **只有用户明确要求时才提交代码**
- 不要自动提交任何更改
- 等待用户明确指示后才执行 git add 和 git commit

### 当前状态
- M0 极简 MVP 与 M0+ 体验增强已完成。
- 当前焦点是 M1 白天 / 夜晚流程框架；核心实现已完成，待游戏内人工验证（见 `devPlan/currentTasks.md`）。
- `devPlan/outline/c05ProjectArchitecture.md` 记录系统模块设计和模块依赖图。
