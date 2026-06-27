# 当前任务

**当前焦点：C# 项目级模块拆分 + DI 依赖注入（`todo.md` §2）。**

目标：每个模块是一个 C# 项目，项目引用链反映架构文档（`c05ProjectArchitecture.md` §5.4）中的依赖关系，用 `Microsoft.Extensions.DependencyInjection` 替代静态 `Instance`。

---

## 目标项目结构

```
CasualCastle/
├── scripts/
│   ├── CasualCastle.Shared/            # 共享内核（零依赖）
│   │   └── Shared/
│   │       ├── GameVector2.cs
│   │       ├── GridCellOffset.cs
│   │       ├── GameCoordinateRules.cs
│   │       ├── GameRules.cs            # 游戏常量
│   │       └── GamePhase.cs            # Day / Night 枚举
│   │
│   ├── CasualCastle.Data/              # 数据资源（→ Shared）
│   │   └── Data/
│   │       ├── CardData.cs
│   │       ├── FusionRecipe.cs
│   │       ├── BuildingDefinitions.cs  # 建筑类型定义表
│   │       ├── SoldierData.cs
│   │       ├── BattleReportModels.cs
│   │       └── IBattleReportRepository.cs  # 端口
│   │
│   ├── CasualCastle.Night/             # 昼夜模块（→ Shared）
│   │   └── Night/
│   │       ├── NightRules.cs
│   │       └── IGamePhase.cs           # 端口：查当前阶段
│   │
│   ├── CasualCastle.Shop/              # 商店手牌模块（→ Shared, Data）
│   │   └── Shop/
│   │       ├── ShopRules.cs            # 购买、金币、刷新
│   │       ├── CardRules.cs            # 手牌容量、选中/打出
│   │       ├── IShopOutput.cs          # 端口：购买结果通知
│   │       └── ICardOutput.cs          # 端口：卡牌打出通知
│   │
│   ├── CasualCastle.Building/          # 建筑模块（→ Shared, Data, Night）
│   │   └── Building/
│   │       ├── OccupancyGrid.cs        # 占地网格（纯领域）
│   │       ├── AdjacentRules.cs        # 邻接判定与加成
│   │       ├── IBuildingRegistry.cs    # 端口：查建筑定义
│   │       ├── IBuildingPlacement.cs   # 端口：放置/释放占地
│   │       └── IAdjacencyOutput.cs     # 端口：邻接刷新通知
│   │
│   ├── CasualCastle.Fusion/            # 融合模块（→ Shared, Data, Building）
│   │   └── Fusion/
│   │       ├── FusionRules.cs          # 配方匹配、可行性验证
│   │       └── IFusionOutput.cs        # 端口：融合完成通知
│   │
│   ├── CasualCastle.Battle/            # 战斗模块（→ Shared, Data, Night, Building）
│   │   └── Battle/
│   │       ├── CombatRules.cs          # 攻击判定、伤害、死亡
│   │       └── ISoldierSpawner.cs      # 端口：生成士兵
│   │
│   ├── CasualCastle.Report/            # 战报模块（→ Shared, Data）
│   │   └── Report/
│   │       └── ReportBuilder.cs        # 快照构建、克隆
│   │
│   ├── CasualCastle.Replay/            # 回放模块（→ Shared, Data, Building, Report）
│   │   └── Replay/
│   │       ├── MirrorRules.cs          # 坐标镜像
│   │       ├── IReplayOutput.cs        # 端口：回放结果通知
│   │       └── ISnapshotQuery.cs       # 端口：查快照
│   │
│   ├── CasualCastle.Godot/             # Godot 适配器（→ 所有 domain 项目 + Godot）
│   │   └── Adapters/
│   │       ├── Autoload/               # GameManager（实现 IGamePhase 等）
│   │       ├── Building/               # Castle, Building, BuildingSystem, AdjacentSystem
│   │       ├── Battle/                 # Soldier, UnitSpawn
│   │       ├── Shop/                   # ShopSystem
│   │       ├── Card/                   # CardSystem
│   │       ├── Night/                  # NightSystem
│   │       ├── Fusion/                 # FusionSystem
│   │       ├── BattleReport/           # BattleReportSystem
│   │       ├── Replay/                 # ReplayAiSystem
│   │       ├── UI/                     # UIManager + 子控制器
│   │       ├── Flow/                   # TitleScreen, MainGameController
│   │       └── Persistence/            # BattleReportStorage
│   │
│   ├── CasualCastle.Game/              # 主 Godot 项目（→ Godot, 所有 domain）
│   │   ├── CompositionRoot.cs          # DI 容器配置
│   │   ├── scenes/
│   │   ├── prefabs/
│   │   └── project.godot
│   │
│   └── CasualCastle.Tests/             # 单元测试（→ 所有 domain 项目）
│
└── CasualCastle.sln
```

**依赖方向（项目引用链）：**

```
Shared ← Data ← Shop
   ↑        ↑       ↑
   ├─ Night ├─ Building ←─ Fusion
   ├─ Report           ←─ Replay
   ├─ Replay
   └─ Battle

Godot → 所有 domain 项目 + Godot NuGet
Game  → Godot（composition root）
Tests → 所有 domain 项目
```

**所有 domain 项目零 Godot 引用。** 端口（接口）和被依赖方在同一个项目中定义。

---

## Phase 2A: 项目骨架搭建

- [ ] 在 `scripts/` 下创建 11 个 `.csproj` 项目（上表中除 `Game` 已有的项目）
- [ ] 每个 domain 项目只引用 `Microsoft.Extensions.DependencyInjection.Abstractions`（用于 `IServiceCollection` 扩展方法）
- [ ] `CasualCastle.Godot` 引用所有 domain 项目 + `Godot.NET.Sdk`
- [ ] `CasualCastle.Game` 引用 `CasualCastle.Godot`（composition root）
- [ ] 更新 `CasualCastle.sln`，添加所有项目

### 每个项目的 `.csproj` 模板（domain 项目）

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <RootNamespace>CasualCastle.Xxx</RootNamespace>
    <Nullable>disable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="8.0.*" />
  </ItemGroup>
  <!-- ProjectReferences here -->
</Project>
```

### 每个项目的 DI 注册模板

```csharp
// CasualCastle.Shop/Shop/ShopModule.cs
public static class ShopModule
{
    public static IServiceCollection AddShop(this IServiceCollection services)
    {
        services.AddSingleton<ShopRules>();
        services.AddSingleton<CardRules>();
        return services;
    }
}
```

---

## Phase 2B: 迁移现有代码到对应项目

按照从叶子到根的依赖顺序迁移，确保每个项目迁移后即可编译：

### Step 1: `CasualCastle.Shared`
- [ ] 从 `scripts/domain/coordinates/` 迁入 `GameVector2`、`GridCellOffset`、`GameCoordinateRules`
- [ ] 从 `scripts/domain/core/` 迁入 `GameRules` 常量
- [ ] 创建 `GamePhase` 枚举（Day / Night）

### Step 2: `CasualCastle.Data`
- [ ] 从 `scripts/domain/card/` 迁入 `CardData`
- [ ] 从 `scripts/domain/fusion/` 迁入 `FusionRecipe`
- [ ] 从 `scripts/domain/battle/` 迁入 `SoldierData`
- [ ] 从 `scripts/adapters/godot/building/BuildingSystem.cs` 提取 `BuildingDefinition`（去除 Godot 类型）
- [ ] 从 `scripts/ports/` 迁入 `BattleReportModels`、`IBattleReportRepository`

### Step 3: `CasualCastle.Night`
- [ ] 从 `scripts/domain/night/` 迁入 `NightRules`
- [ ] 定义 `IGamePhase` 端口

### Step 4: `CasualCastle.Shop`
- [ ] 从 `scripts/adapters/godot/shop/ShopSystem.cs` 提取 `ShopRules`
- [ ] 从 `scripts/adapters/godot/card/CardSystem.cs` 提取 `CardRules`
- [ ] 定义 `IShopOutput`、`ICardOutput` 端口

### Step 5: `CasualCastle.Building`
- [ ] 从 `scripts/adapters/godot/building/Castle.cs` 提取 `OccupancyGrid`
- [ ] 从 `scripts/adapters/godot/building/AdjacentSystem.cs` 提取 `AdjacentRules`
- [ ] 定义 `IBuildingRegistry`、`IBuildingPlacement`、`IAdjacencyOutput` 端口

### Step 6: `CasualCastle.Fusion`
- [ ] 从 `scripts/adapters/godot/fusion/FusionSystem.cs` 提取 `FusionRules`
- [ ] 定义 `IFusionOutput` 端口

### Step 7: `CasualCastle.Battle`
- [ ] 新建 `CombatRules`（攻击判定、伤害计算）
- [ ] 定义 `ISoldierSpawner` 端口

### Step 8: `CasualCastle.Report`
- [ ] 从 `scripts/adapters/godot/battle_report/BattleReportSystem.cs` 提取 `ReportBuilder`
- [ ] 定义 `IReportOutput` 端口

### Step 9: `CasualCastle.Replay`
- [ ] 从 `scripts/adapters/godot/replay/ReplayAiSystem.cs` 提取 `MirrorRules`
- [ ] 定义 `IReplayOutput`、`ISnapshotQuery` 端口

### Step 10: `CasualCastle.Godot`
- [ ] 迁入所有 Godot 节点脚本（完整 `scripts/adapters/` 目录）
- [ ] 每个 Godot 节点实现对应的 domain 端口
- [ ] `GameManager` 持有 `ServiceProvider`，提供 `Services` 静态属性

### Step 11: `CasualCastle.Game`
- [ ] 实现 `CompositionRoot.Build()`，调用各模块的 `AddXxx()` 扩展方法
- [ ] `GameManager._Ready()` 调用 `CompositionRoot.Build()`

---

## Phase 2C: Godot 节点获取依赖的模式

Godot 节点由引擎创建，不能用构造函数注入。约定：

```csharp
public partial class Building : Area2D
{
    private IBuildingRegistry _registry;
    private IBuildingPlacement _placement;

    public override void _Ready()
    {
        var sp = GameManager.Services;
        _registry    = sp.GetRequiredService<IBuildingRegistry>();
        _placement   = sp.GetRequiredService<IBuildingPlacement>();
    }
}
```

逐步消除 `public static Xxx Instance`：
1. 在 `CompositionRoot` 中注册服务
2. 调用方改为从 DI 容器获取
3. 确认无外部引用后删除 `Instance`

---

## 端口接口归属

| 端口 | 定义在 | 实现于 |
|------|--------|--------|
| `IGamePhase` | `CasualCastle.Night` | `GameManager`（Godot） |
| `IShopOutput` / `ICardOutput` | `CasualCastle.Shop` | `ShopSystem` / `CardSystem`（Godot） |
| `IBuildingRegistry` / `IBuildingPlacement` / `IAdjacencyOutput` | `CasualCastle.Building` | `BuildingSystem` / `Castle`（Godot） |
| `IFusionOutput` | `CasualCastle.Fusion` | `FusionSystem`（Godot） |
| `ISoldierSpawner` | `CasualCastle.Battle` | `UnitSpawn`（Godot） |
| `IReportOutput` | `CasualCastle.Report` | `BattleReportSystem`（Godot） |
| `IReplayOutput` / `ISnapshotQuery` | `CasualCastle.Replay` | `ReplayAiSystem` / `BattleReportSystem`（Godot） |
| `IBattleReportRepository` | `CasualCastle.Data` | `BattleReportStorage`（Godot） |

---

## 验收标准

- [ ] `dotnet build CasualCastle.sln` 全部 12 个项目编译通过
- [ ] `dotnet test` 领域测试通过（无需 Godot）
- [ ] 无 domain 项目引用 Godot SDK
- [ ] 项目引用链与架构文档依赖方向一致（无循环引用）
- [ ] 模块间无 `static Instance` 直调（全部经 DI 端口）
- [ ] 游戏运行时行为不变

---

## 暂不进入范围

- 显示与业务缩放拆分（`todo.md` §3）
- 开发者模式（`todo.md` §4）
- 多个项目进一步拆分为 Abstractions + Implementation（当前每个模块一个项目足够）
