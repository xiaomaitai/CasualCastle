# AGENTS.md - CasualCastle Development Guide

## Project Overview

**CasualCastle** is a 2D RTS game built in **Godot 4.6** using **C# / .NET** (Mono), similar to Clash Royale. The game features auto-generating barracks, unit combat, and castle destruction mechanics. This is an early-stage project focused on MVP development.

## Architecture & Directory Structure

### Core Organization
- **`scripts/`** - All C# code organized by purpose:
  - `autoload/` - Global singletons and managers (auto-loaded services)
  - `systems/` - Game systems (e.g., BattleSystem, UISystem)
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
3. **Autoload Pattern**: Place global managers in `scripts/autoload/` to auto-initialize singletons.
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

- 项目开发大纲存放于 dev_plan 文件夹，主文件为 dev_plan/development_outline.md。
- 当前任务文件为 dev_plan/current_tasks.md，包含极简MVP的分阶段开发任务。
- 当 agent 需要读取开发计划、当前任务、里程碑或任务优先级时，应首先读取该文件夹内容以获取最新计划。
