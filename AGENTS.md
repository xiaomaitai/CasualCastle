# AGENTS.md - CasualCastle Development Guide

## Project Overview

**CasualCastle** is a card-based game built in **Godot 4.6** using **C# / .NET** (Mono). The project uses Jolt Physics for 3D simulations and Forward+ rendering. This is an early-stage project with established architectural patterns ready for feature development.

## Architecture & Directory Structure

### Core Organization
- **`scripts/`** - All C# code organized by purpose:
  - `autoload/` - Global singletons and managers (auto-loaded services)
  - `systems/` - Game systems (e.g., CardSystem, UISystem, PhysicsSystem)
  - `utils/` - Shared utility classes and helper functions
- **`scenes/`** - Scene files organized by context:
  - `main/` - Main entry point scene(s)
  - `ui/` - UI components and screens
  - `levels/` - Level-specific scenes
- **`prefabs/`** - Reusable scene templates
- **`resources/`** - Godot resource files (.tres, custom resources)
- **`assets/`** - Game content:
  - `art/cards/` - Card graphics (card_border.png, character sprites)
  - `art/placeholders/` - Temporary/test assets
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
4. **Systems Architecture**: Implement game systems (CardSystem, UISystem, etc.) in `scripts/systems/` as stateful managers that handle specific domains.

### Node Attachment & Script Organization
- Attach scripts directly to scene nodes for node-specific logic
- Use inheritance from Node3D (3D) or Node2D (2D) based on scene context
- Component-based composition: Keep scripts focused on single responsibility

### AsynchronousOperations & Signal Patterns
- Use Godot signals for inter-system communication
- Emit signals from systems; subscribe in UI or other systems
- Example: CardSystem emits `card_played` signal → UISystem listens and updates display

## Coding Practices

### File Naming
- C# scripts: `PascalCase.cs` (e.g., `CardSystem.cs`, `GameManager.cs`)
- Scene files: `snake_case.tscn` (e.g., `main_menu.tscn`, `card_display.tscn`)
- Prefab scenes: `[ComponentName]_prefab.tscn`

### Code Structure
```csharp
public partial class CardSystem : Node3D
{
    [Signal]
    public delegate void CardPlayedEventHandler(Card card);
    
    public override void _Ready()
    {
        // Initialization
    }
}
```

### Import Path Convention
- Reference scenes: `res://scenes/main/node_2d.tscn`
- Reference assets: `res://assets/art/cards/card_border.png`
- Reference scripts: Attach directly as nodes in Godot editor

## Common Tasks

### Adding a New System
1. Create `scripts/systems/YourSystem.cs` inheriting from `Node3D` or appropriate base
2. Implement `_Ready()` for initialization
3. Define signals for output events
4. Register in autoload or attach to main scene
5. Example structure in `scripts/systems/` if others exist for reference

### Adding UI Elements
1. Create scene in `scenes/ui/YourUI.tscn`
2. Attach C# control script (inherit from Control/Panel/etc.)
3. Connect signals from game systems
4. Place prefabs in `prefabs/` if reusable

### Working with Assets
- Card art: place in `assets/art/cards/`
- Character sprites: reference by filename (e.g., `goblin.png`)
- Ensure PNG files are imported (Godot auto-imports on project load)

## Physics & Rendering

### Physics Engine
- **Godot 4.6** uses **Jolt Physics** (configured in project.godot)
- Use `PhysicsBody3D` nodes for dynamic objects
- Use `StaticBody3D` for fixed environment
- Query PhysicsServer for raycasts/shapecast checks

### Rendering
- **Forward+ renderer** (efficient for many lights)
- **D3D12 backend** on Windows (fallback to OpenGL if needed)
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

