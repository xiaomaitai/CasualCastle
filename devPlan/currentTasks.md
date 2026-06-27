# 当前任务

**当前焦点：C# 项目级模块拆分 + DI 依赖注入（`todo.md` §2）。**

目标：保留 `scripts/domain/`、`scripts/ports/`、`scripts/adapters/` 三层。domain 拆为 4 个项目，用 `Microsoft.Extensions.DependencyInjection` 替代静态 `Instance`。

---

## 目标项目结构

```
scripts/
├── domain/
│   ├── CasualCastle.Domain.Shared/       # 共享内核（零依赖）
│   │   └── GameVector2, GridCellOffset, GameCoordinateRules,
│   │     GameRules, GamePhase
│   │
│   ├── CasualCastle.Domain.Building/     # 建筑（→ Shared）
│   │   └── OccupancyGrid, AdjacentRules, BuildingDefinitions,
│   │     ShopRules, CardRules, FusionRules
│   │
│   ├── CasualCastle.Domain.Battle/       # 战斗（→ Shared, Building）
│   │   └── CombatRules, NightRules, SoldierData
│   │
│   └── CasualCastle.Domain.History/      # 战报回放（→ Shared, Building）
│       └── ReportBuilder, MirrorRules, BattleReportModels
│
├── ports/                                # 端口（接口分散在各 domain 项目中）
│   ├── IGamePhase.cs               # 在 Domain.Battle
│   ├── IShopOutput.cs              # 在 Domain.Building
│   ├── IBuildingRegistry.cs        # 在 Domain.Building
│   ├── IBuildingPlacement.cs       # 在 Domain.Building
│   ├── IFusionOutput.cs            # 在 Domain.Building
│   ├── ISoldierSpawner.cs          # 在 Domain.Battle
│   ├── IBattleReportRepository.cs  # 在 Domain.History
│   └── ISnapshotQuery.cs           # 在 Domain.History
│
├── adapters/godot/                       # Godot 适配层（→ 4 个 domain 项目）
│   ├── autoload/     GameManager（实现 IGamePhase 等端口）
│   ├── building/     Castle, Building, BuildingSystem, AdjacentSystem
│   ├── battle/       Soldier, UnitSpawn
│   ├── shop/         ShopSystem（实现 IShopOutput）
│   ├── card/         CardSystem（实现 ICardOutput）
│   ├── night/        NightSystem
│   ├── fusion/       FusionSystem
│   ├── battle_report/BattleReportSystem
│   ├── replay/       ReplayAiSystem
│   ├── ui/           UIManager + 子控制器
│   ├── flow/         TitleScreen, MainGameController
│   └── core/         GameConfig, GameCoordinates（shim）
│
└── adapters/persistence/                 # 持久化（→ Domain.History）
    └── BattleReportStorage（实现 IBattleReportRepository）
```

**主 Godot 项目在根目录**（`CasualCastle.csproj`），引用 4 个 domain 项目。adapters 和主项目编译在一起。

### 依赖方向

```
         Domain.Shared
              ↑
    ┌─────────┼─────────┐
    │         │         │
Domain.Building  Domain.Battle  Domain.History
    ↑              ↑              ↑
    └──────────────┴──────────────┘
                   │
          adapters (Godot + persistence)
```

---

## Phase 2A: 项目骨架

- [ ] 在 `scripts/domain/` 下创建 4 个 `.csproj`
- [ ] 每个 domain 项目引用 `Microsoft.Extensions.DependencyInjection.Abstractions`
- [ ] 主项目 `.csproj` 添加 4 个 `ProjectReference`
- [ ] 更新 `.sln`

### DI 注册模板

```csharp
// scripts/domain/CasualCastle.Domain.Building/BuildingModule.cs
public static class BuildingModule
{
    public static IServiceCollection AddDomainBuilding(this IServiceCollection services)
    {
        services.AddSingleton<OccupancyGrid>();
        services.AddSingleton<AdjacentRules>();
        services.AddSingleton<ShopRules>();
        services.AddSingleton<CardRules>();
        services.AddSingleton<FusionRules>();
        return services;
    }
}
```

---

## Phase 2B: 代码迁移

### Step 1: `Domain.Shared`
- [ ] `scripts/domain/coordinates/` → 坐标类型
- [ ] `scripts/domain/core/GameRules.cs` → 常量
- [ ] `scripts/domain/night/NightRules.cs` → 已有，移入 Shared

### Step 2: `Domain.Building`
- [ ] 从 `adapters/godot/building/Castle.cs` 提取 `OccupancyGrid`
- [ ] 从 `adapters/godot/building/AdjacentSystem.cs` 提取 `AdjacentRules`
- [ ] 从 `adapters/godot/building/BuildingSystem.cs` 提取 `BuildingDefinitions`（去除 Godot 类型）
- [ ] 从 `adapters/godot/shop/ShopSystem.cs` 提取 `ShopRules`
- [ ] 从 `adapters/godot/card/CardSystem.cs` 提取 `CardRules`
- [ ] 从 `adapters/godot/fusion/FusionSystem.cs` 提取 `FusionRules`
- [ ] `scripts/domain/card/CardData.cs` → 建筑模块的项目
- [ ] `scripts/domain/fusion/FusionRecipe.cs` → 建筑模块的项目
- [ ] 定义 `IBuildingRegistry`、`IBuildingPlacement`、`IShopOutput`、`ICardOutput`、`IFusionOutput` 端口

### Step 3: `Domain.Battle`
- [ ] `scripts/domain/battle/SoldierData.cs` → 已有
- [ ] `scripts/domain/night/NightRules.cs` → 已有
- [ ] 从 `adapters/godot/battle/Soldier.cs` 提取 `CombatRules`
- [ ] 定义 `IGamePhase`、`ISoldierSpawner` 端口

### Step 4: `Domain.History`
- [ ] `scripts/ports/BattleReportModels.cs` → 已有
- [ ] `scripts/ports/IBattleReportRepository.cs` → 已有
- [ ] 从 `adapters/godot/battle_report/BattleReportSystem.cs` 提取 `ReportBuilder`
- [ ] 从 `adapters/godot/replay/ReplayAiSystem.cs` 提取 `MirrorRules`
- [ ] 定义 `ISnapshotQuery` 端口

### Step 5: adapters
- [ ] 现有 `scripts/adapters/` 中的 Godot 节点改为实现端口
- [ ] `CompositionRoot.cs` 放在 `scripts/` 下
- [ ] `GameManager._Ready()` 调用 `CompositionRoot.Build()`

---

## 验收标准

- [ ] `dotnet build` 全部编译通过
- [ ] `dotnet test` 领域测试通过
- [ ] 4 个 domain 项目零 `using Godot`
- [ ] 项目引用无循环
- [ ] 模块间无 `static Instance` 直调
- [ ] 三层保留：`scripts/domain/`（4 项目）、`scripts/ports/`、`scripts/adapters/`
