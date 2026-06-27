# 当前任务

**当前焦点：C# 项目级模块拆分 + DI 依赖注入（`todo.md` §2）。**

目标：保留现有 `scripts/domain/`、`scripts/ports/`、`scripts/adapters/` 三层，在 `domain/` 内部按模块拆分为多个 C# 项目，用 `Microsoft.Extensions.DependencyInjection` 替代静态 `Instance`。

---

## 目标项目结构

```
CasualCastle/
├── scripts/
│   ├── domain/                                     # ← 核心域层（拆为多个项目）
│   │   ├── CasualCastle.Domain.Shared/            # 共享内核（零依赖）
│   │   │   └── GameVector2, GridCellOffset, GameCoordinateRules,
│   │   │     GameRules, GamePhase
│   │   │
│   │   ├── CasualCastle.Domain.Data/              # 数据资源（→ Shared）
│   │   │   └── CardData, FusionRecipe, BuildingDefinitions,
│   │   │     SoldierData, BattleReportModels
│   │   │
│   │   ├── CasualCastle.Domain.Night/             # 昼夜（→ Shared）
│   │   │   └── NightRules + IGamePhase 端口
│   │   │
│   │   ├── CasualCastle.Domain.Shop/              # 商店手牌（→ Shared, Data）
│   │   │   └── ShopRules, CardRules + 输出端口
│   │   │
│   │   ├── CasualCastle.Domain.Building/          # 建筑（→ Shared, Data, Night）
│   │   │   └── OccupancyGrid, AdjacentRules + 端口
│   │   │
│   │   ├── CasualCastle.Domain.Fusion/            # 融合（→ Shared, Data, Building）
│   │   │   └── FusionRules + 输出端口
│   │   │
│   │   ├── CasualCastle.Domain.Battle/            # 战斗（→ Shared, Data, Night, Building）
│   │   │   └── CombatRules + 输出端口
│   │   │
│   │   ├── CasualCastle.Domain.Report/            # 战报（→ Shared, Data）
│   │   │   └── ReportBuilder + 输出端口
│   │   │
│   │   └── CasualCastle.Domain.Replay/            # 回放（→ Shared, Data, Building, Report）
│   │       └── MirrorRules + 输出端口
│   │
│   ├── ports/                                     # ← 端口层（接口分散在各 domain 项目中）
│   │   # 端口接口和被依赖方放在同一个 domain 项目中
│   │   # 此处仅保留不归属特定模块的跨域接口
│   │
│   ├── adapters/godot/                            # ← Godot 适配层（→ 所有 domain 项目）
│   │   ├── autoload/     GameManager（实现 IGamePhase 等端口）
│   │   ├── building/     Castle, Building, BuildingSystem, AdjacentSystem
│   │   ├── battle/       Soldier, UnitSpawn
│   │   ├── shop/         ShopSystem（实现 IShopOutput）
│   │   ├── card/         CardSystem（实现 ICardOutput）
│   │   ├── night/        NightSystem
│   │   ├── fusion/       FusionSystem
│   │   ├── battle_report/BattleReportSystem
│   │   ├── replay/       ReplayAiSystem
│   │   ├── ui/           UIManager + 子控制器
│   │   ├── flow/         TitleScreen, MainGameController
│   │   └── core/         GameConfig, GameCoordinates（shim）
│   │
│   ├── adapters/persistence/                      # ← 持久化适配（→ Domain.Data）
│   │   └── BattleReportStorage（实现 IBattleReportRepository）
│   │
│   └── CompositionRoot.cs                         # DI 容器配置（→ 所有项目）
│
├── scenes/  prefabs/  project.godot               # 原 Godot 主项目在根目录
├── CasualCastle.sln
└── tests/CasualCastle.Tests/
```

**依赖方向（项目引用链）：**

```
Domain.Shared ← Domain.Data
     ↑              ↑
     ├── Domain.Night ────┤
     ├── Domain.Shop      ├── Domain.Building ← Domain.Fusion
     ├── Domain.Report    │        ↑          ← Domain.Replay
     └── Domain.Battle ───┘   Domain.Battle
```

**所有 domain 项目零 Godot 引用。** 端口接口和被依赖方在同一个项目中定义。

---

## Phase 2A: 项目骨架搭建

- [ ] 在 `scripts/domain/` 下创建 9 个 `.csproj` 项目
- [ ] 每个 domain 项目只引用 `Microsoft.Extensions.DependencyInjection.Abstractions` + 依赖的其他 domain 项目
- [ ] `CasualCastle.Game`（主项目）的 `.csproj` 中添加 9 个 domain 项目的 `ProjectReference`
- [ ] 更新 `CasualCastle.sln`，添加所有项目

### DI 注册模板（每个 domain 项目）

```csharp
// CasualCastle.Domain.Shop/ShopModule.cs
public static class ShopModule
{
    public static IServiceCollection AddDomainShop(this IServiceCollection services)
    {
        services.AddSingleton<ShopRules>();
        services.AddSingleton<CardRules>();
        return services;
    }
}
```

---

## Phase 2B: 迁移现有代码

### Step 1: `Domain.Shared`
- [ ] 从 `scripts/domain/coordinates/` 迁入坐标类型
- [ ] 从 `scripts/domain/core/GameRules.cs` 迁入常量
- [ ] 创建 `GamePhase` 枚举

### Step 2: `Domain.Data`
- [ ] 迁入 `CardData`、`FusionRecipe`、`SoldierData`
- [ ] 从 `BuildingSystem.cs` 提取 `BuildingDefinitions`（去除 Godot 类型）
- [ ] 迁入 `BattleReportModels`、定义 `IBattleReportRepository`

### Step 3-9: 其余 domain 项目
按依赖顺序（Night → Shop → Building → Fusion → Battle → Report → Replay）逐个迁入和提取。

### Step 10: adapters
- [ ] 现有 `scripts/adapters/` 保持不变
- [ ] Godot 节点改为实现端口接口
- [ ] `GameManager` 持有 `ServiceProvider`，暴露 `Services` 静态属性

### Step 11: CompositionRoot
```csharp
// scripts/CompositionRoot.cs
public static class CompositionRoot
{
    public static ServiceProvider Build(GameManager gameManager, ...)
    {
        return new ServiceCollection()
            .AddDomainShop()
            .AddDomainBuilding()
            .AddDomainFusion()
            // ...
            .AddSingleton<IGamePhase>(gameManager)
            .AddSingleton<IBuildingRegistry>(BuildingSystem.Instance)
            // ...
            .BuildServiceProvider();
    }
}
```

---

## 验收标准

- [ ] `dotnet build` 全部项目编译通过
- [ ] `dotnet test` 领域测试通过
- [ ] domain 项目零 `using Godot`
- [ ] 项目引用链无循环
- [ ] 模块间无 `static Instance` 直调
- [ ] 三层结构保留：`scripts/domain/`（多项目）、`scripts/ports/`（接口）、`scripts/adapters/`（实现）
