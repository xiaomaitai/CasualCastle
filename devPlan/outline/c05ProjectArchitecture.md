# 五、项目架构

## 5.1 实际目录结构（当前）

```
CasualCastle/
├── scripts/
│   ├── domain/                                     # 核心域层（零 Godot）
│   │   ├── CasualCastle.Domain.Shared/            # 坐标、常量、枚举（零依赖）
│   │   │   └── GameVector2, GridCellOffset, GameCoordinateRules, GameRules
│   │   ├── CasualCastle.Domain.Building/          # 建筑、邻接、商店、手牌、融合（→ Shared）
│   │   │   └── CardData, FusionRecipe
│   │   ├── CasualCastle.Domain.Battle/            # 战斗、昼夜（→ Shared, Building）
│   │   │   └── SoldierData, NightRules
│   │   └── CasualCastle.Domain.History/           # 战报、回放（→ Shared, Building）
│   │       └── BattleReportModels, IBattleReportRepository
│   │
│   ├── ports/                                      # 端口层（接口分散在各 domain 项目中）
│   │   # IGamePhase       → Domain.Battle
│   │   # IBuildingRegistry → Domain.Building
│   │   # ISnapshotQuery   → Domain.History
│   │
│   └── adapters/                                   # 基础设施层（实现端口）
│       ├── godot/                                  # Godot 节点和场景脚本
│       │   ├── autoload/     GameManager, DisplaySettingsManager
│       │   ├── building/     Castle, Building, BuildingSystem, AdjacentSystem
│       │   ├── battle/       Soldier, UnitSpawn
│       │   ├── shop/         ShopSystem
│       │   ├── card/         CardSystem
│       │   ├── night/        NightSystem
│       │   ├── fusion/       FusionSystem
│       │   ├── battle_report/BattleReportSystem
│       │   ├── replay/       ReplayAiSystem
│       │   ├── ui/           UIManager + 子控制器
│       │   ├── flow/         TitleScreen, MainGameController
│       │   ├── core/         GameConfig, GameCoordinates（shim）
│       │   ├── dev/          DevInputLogger
│       │   └── audio/        BgmPlayer
│       └── persistence/     BattleReportStorage
│
├── scenes/  prefabs/  assets/  project.godot     # Godot 资源
├── tests/CasualCastle.Domain.Tests/               # 单元测试
├── devPlan/                                       # 文档
└── CasualCastle.sln                               # 6 个项目
```

## 5.2 迁移状态

Phase 1 分层已完成。Phase 2 domain 已拆为 4 个 C# 项目，三层次保留：

```
domain/     → 4 个 .NET 类库（零 Godot，互有项目引用）
ports/      → 接口分散在各 domain 项目中
adapters/   → Godot + 持久化（与主项目编译在一起，实现端口）
```

待推进：
- [ ] 领域规则提取到 domain 项目（OccupancyGrid, AdjacentRules, ShopRules, CombatRules 等）
- [ ] DI 容器替代静态 Instance
- [ ] todo.md §3（显示缩放拆分）、§4（开发者模式）

---

## 5.3 核心系统说明

| 系统 | 归属项目 | 当前状态 |
|------|----------|----------|
| GameManager | adapters/godot/autoload/ | Godot Autoload |
| DisplaySettingsManager | adapters/godot/autoload/ | Godot Autoload |
| UIManager + 子控制器 | adapters/godot/ui/ | 已拆分 |
| ShopSystem | adapters/godot/shop/ | 待提取领域规则到 Domain.Building |
| CardSystem | adapters/godot/card/ | 待提取领域规则到 Domain.Building |
| BuildingSystem | adapters/godot/building/ | 待提取到 Domain.Building |
| AdjacentSystem | adapters/godot/building/ | 待提取规则 |
| BattleSystem | adapters/godot/battle/ | 待提取规则到 Domain.Battle |
| NightSystem | adapters/godot/night/ | 规则已提取到 Domain.Battle |
| FusionSystem | adapters/godot/fusion/ | 待提取规则到 Domain.Building |
| BattleReportSystem | adapters/godot/battle_report/ | 待提取到 Domain.History |
| ReplayAiSystem | adapters/godot/replay/ | 待提取规则 |
| BattleReportStorage | adapters/persistence/ | 已实现 IBattleReportRepository |
| TitleScreen / MainGame | adapters/godot/flow/ | 场景流转 |
| GameCoordinates | adapters/godot/core/ | shim（委托到 Domain.Shared） |

## 5.4 模块依赖关系

```
                         Domain.Shared
                     (坐标、常量、枚举)
                              ^
              +---------------+---------------+
              |               |               |
              v               v               v
      Domain.Building   Domain.Battle   Domain.History
      (建筑/商店/融合)   (战斗/昼夜)    (战报/回放)
              ^               ^               ^
              |               |               |
              +-------+-------+-------+-------+
                      |       |       |
              +-------+--+ +--+---+ +-+-------+
              | autoload | |battle| |persist. |
              |  ui flow | |night | |b_report |
              |building  | |replay| |         |
              |shop card | |fusion| +---------+
              +----------+ +------+
                  adapters/godot
```

依赖方向：从上到下。domain 项目只依赖下方的项目。adapters 实现 domain 定义的端口。无循环。

---

## 5.5 端口接口归属

| 端口 | 定义位置 | 实现方（adapter） |
|------|----------|-------------------|
| `IGamePhase` | Domain.Battle | GameManager |
| `IBuildingRegistry` | Domain.Building | BuildingSystem |
| `IBuildingPlacement` | Domain.Building | Castle |
| `IShopOutput` | Domain.Building | ShopSystem |
| `ICardOutput` | Domain.Building | CardSystem |
| `IFusionOutput` | Domain.Building | FusionSystem |
| `ISoldierSpawner` | Domain.Battle | UnitSpawn |
| `IBattleReportRepository` | Domain.History | BattleReportStorage |
| `ISnapshotQuery` | Domain.History | BattleReportSystem |
