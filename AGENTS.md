# AGENTS.md - CasualCastle Development Guide

## Project Overview

**CasualCastle** is a 2D RTS game built in **Godot 4.6** using **C# / .NET** (Mono), similar to Clash Royale. The game features auto-generating barracks, unit combat, castle destruction mechanics, and an in-progress day/night loop for the MVP roadmap.

## Game Concepts

> 遇到游戏术语（如地块、城堡、建筑卡、建筑、战场等）时，优先参考 `devPlan/concepts.md` 中的定义，确保术语使用与概念文档一致。

## Architecture & Directory Structure

### Core Organization
- **`scripts/`** - All C# code organized by business module:
  - `autoload/` - Godot Project Settings autoloads (`GameManager`)
  - `core/` - Global config and shared game constants (`GameConfig`)
  - `flow/` - Scene flow (`TitleScreen`, `MainGameController`)
  - `ui/` - Main game UI entry and UI sub-controllers (`UIManager`, `HudUiController`, etc.)
  - `shop/` - Shop logic (`ShopSystem`)
  - `card/` - Hand and card placement (`CardSystem`, `CardData`)
  - `night/` - Day/night action gating (`NightSystem`)
  - `building/` - Castle grid and buildings (`Castle`, `Building`, `Barracks`)
  - `battle/` - Combat units (`Soldier`)
  - `audio/` - Audio helpers (`BgmPlayer`)
  - `dev/` - Development utilities (`DevInputLogger`)
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
3. **Manager Singleton Pattern**: `GameManager` is a Godot Autoload singleton. Scene-local controllers such as `UIManager` and systems such as `ShopSystem` use static `Instance` references while attached to `main_game.tscn`.
4. **Module Architecture**: Place new code in the business module folder that owns the feature. Scene-attached systems such as `ShopSystem` use static `Instance` references while attached to `main_game.tscn`.

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
1. Create the script under the matching module folder, e.g. `scripts/shop/YourSystem.cs`
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

1. **Before adding code**: Check which business module owns the feature (`shop/`, `card/`, `building/`, etc.)
2. **New game systems**: Use signal-based architecture for loose coupling
3. **UI elements**: Keep scene files in `scenes/ui/` and scripts in `scripts/ui/` or `scripts/flow/`
4. **Global config**: Put shared constants in `scripts/core/`
5. **Test with Godot editor**: Most issues caught in real-time editing

## References

- **Godot 4.6 C# Docs**: https://docs.godotengine.org/en/4.6/tutorials/scripting/c_sharp/
- **Godot Signals**: https://docs.godotengine.org/en/4.6/tutorials/step_by_step/signals.html
- **Jolt Physics**: https://godotengine.org/news/godot-4-0-brings-powerful-vulkan-rendering-support/
- **Project-specific patterns**: See existing code in `scripts/` module folders and `scenes/`

## Development Plan Folder

- 项目开发大纲按章节拆分存放于 devPlan/outline/ 文件夹。
- 当前任务文件为 devPlan/currentTasks.md，记录执行任务、验收项和待办；阶段状态以大纲为准。
- 运行时代码结构文档为 devPlan/codeStructure.md，记录现有系统、类图、主要运行链路和维护建议。
- 数据结构设计见 devPlan/dataStructures.md。
- 当 agent 需要读取开发计划、当前任务、系统模块、里程碑或任务优先级时，应首先读取 devPlan 文件夹内容以获取最新计划；当前进度与里程碑状态以 devPlan/outline/ 为准。

## 开发规则

### Git提交规则
- **只有用户明确要求时才提交代码**
- 不要自动提交任何更改
- 等待用户明确指示后才执行 git add 和 git commit

### 项目状态维护规则
- `AGENTS.md` 不维护当前进度、当前焦点或里程碑状态。
- 项目状态以 `devPlan/outline/` 下的大纲章节为准。
- `devPlan/outline/c05ProjectArchitecture.md` 记录系统模块设计和模块依赖图。
