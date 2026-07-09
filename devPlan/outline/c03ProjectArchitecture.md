# 五、项目架构

## 5.1 目录结构

```
CasualCastle/
├── scripts/
│   ├── domain/                                     # 核心域层（4 个 C# 项目，零 Godot）
│   │   ├── Shared/       Shared.csproj             # 坐标、常量
│   │   ├── Building/     Building.csproj → Shared  # 建筑、邻接、商店、手牌、组合
│   │   ├── Battle/       Battle.csproj → Shared, Building  # 战斗、昼夜
│   │   └── History/      History.csproj → Shared, Building # 战报、回放
│   │
│   ├── CompositionRoot.cs                          # MS DI 容器构建入口
│   │
│   └── adapters/                                   # 基础设施层（实现 domain 端口）
│       ├── godot/
│       │   ├── autoload/
│       │   │   ├── GameManager.cs                 # Godot Autoload，DI 根，实现 IGameState
│       │   │   ├── DisplaySettingsManager.cs      # 分辨率/窗口管理 + DevModeEnabled
│       │   │   └── AdapterRegistry.cs             # Godot 节点服务定位器
│       │   ├── building/  Castle, Building, BuildingSystem, AdjacentSystem, AdjacentLinkPulse, CastleHighlightOverlay
│       │   ├── battle/    Soldier, UnitSpawn
│       │   ├── shop/      ShopSystem
│       │   ├── card/      CardSystem
│       │   ├── night/     NightSystem
│       │   ├── combine/    CombineSystem
│       │   ├── battle_report/ BattleReportSystem
│       │   ├── replay/    ReplayAiSystem
│       │   ├── ui/        UIManager + 子控制器（纯 C# 类）
│       │   ├── flow/      TitleScreen, MainGameController
│       │   ├── core/      GameConfig, GameCoordinatesAdapter
│       │   ├── dev/       DevInputLogger（需 DevMode 才生效）
│       │   └── audio/     BgmPlayer
│       └── persistence/   BattleReportStorage（实现 IBattleReportRepository）
│
├── devPlan/design/                                  # 系统设计文档
│   ├── combineSystem.md
│   ├── battleReport.md
│   ├── replayAi.md
│   ├── battleSystem.md
│   ├── uiDesign.md
│   └── assetSpecs.md
├── scenes/  prefabs/  assets/  project.godot
├── tests/CasualCastle.Domain.Tests/
├── devPlan/
└── CasualCastle.sln                               # 6 个项目
```

## 5.2 DI 机制

### 双层设计

Godot 管理节点生命周期（场景树实例化、`_Ready`/`_ExitTree`），无法使用 MS DI 的构造函数注入。因此采用双层 DI：

```
第一层：MS DI (ServiceProvider)
  ├── 纯 C# 服务：IBattleReportRepository → BattleReportStorage
  └── IGameState（工厂委托到 AdapterRegistry）

第二层：AdapterRegistry（轻量服务定位器）
  ├── GameManager（同时注册 IGameState）
  ├── DisplaySettingsManager
  ├── NightSystem, BuildingSystem, AdjacentSystem
  ├── CardSystem, ShopSystem, CombineSystem
  ├── BattleReportSystem, ReplayAiSystem
  └── 动态实例（Building）按需解析
```

### 启动流程

```
GameManager._Ready()
  └── CompositionRoot.Build()           # 构建 MS DI 容器
  └── AdapterRegistry.Register(this)    # 注册自身

场景树中后续节点 ._Ready():
  └── AdapterRegistry.Register(this)    # 注册自身
  └── AdapterRegistry.Resolve<T>()      # 解析依赖
```

### 解析方式

| 场景 | 方法 |
|------|------|
| MS DI 注册的纯 C# 服务 | `GameManager.Get<T>()` |
| Godot 节点（AdapterRegistry） | `AdapterRegistry.Resolve<T>()` |

### 注册表

| 系统 | 注册方式 | 解析的依赖 |
|------|---------|-----------|
| `GameManager` | AdapterRegistry | — |
| `DisplaySettingsManager` | AdapterRegistry | — |
| `NightSystem` | AdapterRegistry | `IGameState` |
| `BuildingSystem` | AdapterRegistry | `AdjacentSystem` |
| `AdjacentSystem` | AdapterRegistry | — |
| `CardSystem` | AdapterRegistry | `BuildingSystem` |
| `ShopSystem` | AdapterRegistry | `CardSystem`, `GameManager` |
| `CombineSystem` | AdapterRegistry | `IGameState`, `AdjacentSystem` |
| `BattleReportSystem` | AdapterRegistry | `IBattleReportRepository`（MS DI） |
| `ReplayAiSystem` | AdapterRegistry | `BattleReportSystem`, `AdjacentSystem` |
| `BattleReportStorage` | MS DI（CompositionRoot） | — |
| `Building`（动态实例） | 不注册，仅消费 | `GameManager`, `ShopSystem`, `AdjacentSystem`, `NightSystem` |

## 5.3 领域逻辑提取状态

| 领域类 | 位置 | 来源 | 职责 |
|--------|------|------|------|
| `GameVector2`, `GridCellOffset` | Domain.Shared | 新建 | 游戏坐标/偏移值类型 |
| `GameCoordinateRules` | Domain.Shared | 新建 | 格子中心、角落、出生点计算 |
| `GameRules` | Domain.Shared | 新建 | 全局常量（时长、金币、血量） |
| `BuildingDefinitions` | Domain.Building | BuildingSystem | 建筑类型数据（占地、属性、产出） |
| `OccupancyGrid` | Domain.Building | Castle._occupied | 网格占用追踪 |
| `AdjacentRules` | Domain.Building | AdjacentSystem | 邻接检测、兵营加成计算 |
| `CombineRules` | Domain.Building | CombineSystem | 配方匹配、合成组查找、参与条件 |
| `ShopRules` | Domain.Building | ShopSystem | 卡牌目录、随机生成商品 |
| `CardRules` | Domain.Building | CardSystem | 手牌上限、索引校验 |
| `SoldierData` | Domain.Battle | 新建 | 战斗属性数据类 |
| `NightRules` | Domain.Battle | NightSystem | 昼夜行动判断 |
| `CombatRules` | Domain.Battle | Soldier | 伤害计算、冷却计时 |
| `ReportBuilder` | Domain.History | BattleReportSystem | 快照捕获、战报构建、克隆 |
| `MirrorRules` | Domain.History | ReplayAiSystem | 镜像锚点计算 |
| `BattleReportModels` | Domain.History | 新建 | 战报/快照数据模型 |
| `IBattleReportRepository` | Domain.History | 新建 | 持久化端口 |

Adapter 类（Godot 节点）委托给上述领域类，自身只保留 Godot 特定逻辑（信号、节点操作、渲染）。

## 5.4 模块依赖关系

```
                         Domain.Shared
                     (坐标、常量)
                              ^
              +---------------+---------------+
              |               |               |
      Domain.Building   Domain.Battle   Domain.History
      (建筑/商店/组合)   (战斗/昼夜)    (战报/回放)
              ^               ^               ^
              |               |               |
              +-------+-------+-------+-------+
                      |       |       |
              +-------+--+ +--+---+ +-+-------+
              | autoload | |battle| |persist. |
              |  ui flow | |night | |b_report |
              |building  | |replay| |         |
              |shop card | |combine| +---------+
              +----------+ +------+
                  adapters/godot
```

依赖方向：从上到下。domain 项目只依赖下方的 domain 项目。adapters 实现 domain 定义的端口。无循环。

## 5.5 端口接口

| 端口 | 定义位置 | 实现方 | 用途 |
|------|----------|--------|------|
| `IGameState` | Domain.Battle | GameManager | 查询昼夜/游戏状态 |
| `IBuildingRegistry` | Domain.Building | （BuildingDefinitions 静态类直接服务） | 建筑类型查询 |
| `IBuildingPlacement` | Domain.Building | Castle/OccupancyGrid | 放置校验 |
| `ISnapshotQuery` | Domain.History | BattleReportSystem | 查询已保存快照 |
| `IBattleReportRepository` | Domain.History | BattleReportStorage | 战报持久化 |

`IBuildingState` 和 `IAdjacencyBuilding` 是领域内接口（非端口），由 adapter 的 `Building` 节点实现，供 `AdjacentRules`、`CombineRules` 消费。

## 5.6 核心系统说明

| 系统 | 文件 | 类型 | 状态 |
|------|------|------|------|
| GameManager | autoload/GameManager.cs | Godot Autoload，DI 根 | 实现 IGameState ✅ |
| AdapterRegistry | autoload/AdapterRegistry.cs | 静态服务定位器 | ✅ |
| CompositionRoot | scripts/CompositionRoot.cs | MS DI 容器构建 | ✅ |
| DisplaySettingsManager | autoload/DisplaySettingsManager.cs | Godot Autoload | 含 DevModeEnabled ✅ |
| BuildingSystem | building/BuildingSystem.cs | Godot Node | 游戏数据委托给 BuildingDefinitions ✅ |
| AdjacentSystem | building/AdjacentSystem.cs | Godot Node | 邻接计算委托给 AdjacentRules ✅ |
| Castle | building/Castle.cs | Godot Node | 网格占用委托给 OccupancyGrid ✅ |
| Building | building/Building.cs | Godot Node | 实现 IBuildingState + IAdjacencyBuilding ✅ |
| CombineSystem | combine/CombineSystem.cs | Godot Node | 组合逻辑委托给 CombineRules ✅ |
| NightSystem | night/NightSystem.cs | Godot Node | 昼夜判断委托给 NightRules ✅ |
| ReplayAiSystem | replay/ReplayAiSystem.cs | Godot Node | 镜像计算委托给 MirrorRules ✅ |
| BattleReportSystem | battle_report/BattleReportSystem.cs | Godot Node | 战报构建委托给 ReportBuilder ✅ |
| BattleReportStorage | persistence/BattleReportStorage.cs | 纯 C# | 实现 IBattleReportRepository ✅ |
| DevInputLogger | dev/DevInputLogger.cs | Godot Node | DevMode 门控 ✅ |

## 5.7 开发者模式

- 开关：`DisplaySettingsManager.DevModeEnabled`（默认 false）
- 入口：设置面板 → CheckBox
- 门控：
  - `GameManager`：P 键作弊产兵
  - `DevInputLogger`：按键日志打印

## 5.8 数据存储

项目统一使用 **SQLite** 数据库存储配置数据和运行时存档，非必要不使用 `.tres` 资源文件或 `.tscn` 预制体存储数据。

### 配置数据库

`assets/data/config.db` — 游戏静态配置：

| 表 | 用途 | 消费方 |
|----|------|--------|
| `building_defs` | 建筑类型定义（占地、血量、产兵属性、组合阶数） | `SqliteBuildingRepository` → `IBuildingRepository` |
| `unit_stats` | 单位属性（血量、伤害、速度、攻防类型） | `SqliteUnitRepository` → `IUnitRepository` |
| `combine_recipes` | 组合配方 | （待迁移至 SQLite） |
| `damage_matrix` | 伤害/护甲倍率矩阵 | `DamageMatrix.LoadFrom()` |
| `shop_pool` | 商店卡池（建筑类型、出现权重、费用） | `ShopRules` |

### 存档数据库

`user://saves/` — 玩家游戏存档（P7），一个存档一个 `.db` 文件。

### 资源文件策略

- `.tres` / `.tscn`：仅用于 Godot 引擎必需的场景结构、节点层级、资源引用
- 游戏配置数据（数值、配方、属性）：全部走 SQLite，不存入 `.tres`
- 新增数据表时先在 `config.db` 中建表，再通过 MS DI 注册对应的 Repository
