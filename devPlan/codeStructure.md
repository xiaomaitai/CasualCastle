# 代码结构文档 — CasualCastle

本文档记录当前运行时代码结构。概念定义见 `devPlan/concepts.md`。

---

## 项目结构

```
CasualCastle/
├── scripts/
│   ├── domain/                                     # 核心域层（4 个 C# 项目，零 Godot）
│   │   ├── Shared/                                 # Shared.csproj — 坐标、常量、阶段状态机
│   │   │   ├── GameVector2.cs, GridCellOffset.cs
│   │   │   ├── GameCoordinateRules.cs, GameRules.cs
│   │   │   ├── MatchPhaseMachine.cs, FactionIds.cs, BuildingTypeIds.cs
│   │   │   └── SaveModels.cs, IGameSessionService.cs, ISaveRepository.cs
│   │   ├── Building/                               # Building.csproj → Shared
│   │   │   ├── CardData.cs, CombineRecipe.cs
│   │   │   ├── BuildingDefinitions.cs, OccupancyGrid.cs, Player.cs
│   │   │   ├── AdjacentRules.cs, CombineRules.cs, ShopRules.cs, CardRules.cs
│   │   │   ├── Hand.cs, Shop.cs, AdjacencyService.cs, CombineService.cs
│   │   │   ├── IBuildingState.cs, IBuildingPlacement.cs
│   │   │   ├── IBuildingRepository.cs, ICombineBuildingFactory.cs
│   │   │   └── BuildingModule.cs
│   │   ├── Battle/                                 # Battle.csproj → Shared
│   │   │   ├── Unit.cs, Unit.Combat.cs, Unit.Skills.cs, Unit.Status.cs
│   │   │   ├── UnitState.cs, UnitStats.cs, UnitSize.cs
│   │   │   ├── CombatRules.cs, DamageMatrix.cs, DamageType.cs, AttackType.cs, ArmorType.cs
│   │   │   ├── UnitSpatialService.cs, IUnitSpatialService.cs
│   │   │   ├── IFieldUnitRepository.cs, IUnitRepository.cs
│   │   │   ├── IGameState.cs, INavigationPort.cs, IUnitEventPort.cs
│   │   │   ├── IBuildingRef.cs, IBuildingTarget.cs
│   │   │   ├── SkillEffectTypes.cs
│   │   │   └── BattleModule.cs
│   │   └── History/                                # History.csproj → Shared, Building
│   │       ├── BattleReportModels.cs, IBattleReportRepository.cs
│   │       ├── ReportBuilder.cs, MirrorRules.cs
│   │       ├── BattleReportService.cs, ReplayService.cs
│   │       ├── IReplayTarget.cs
│   │       └── HistoryModule.cs
│   │
│   ├── CompositionRoot.cs                          # MS DI 容器构建
│   │
│   └── adapters/
│       ├── godot/
│       │   ├── autoload/
│       │   │   ├── GameManager.cs                  # Autoload，DI 根，实现 IGameState
│       │   │   ├── InitManager.cs                  # 场景初始化，服务组装
│       │   │   ├── DisplaySettingsManager.cs       # 分辨率/窗口 + DevModeEnabled
│       │   │   └── AdapterRegistry.cs              # Godot 节点服务定位器
│       │   ├── building/                           # Castle, Building, BuildingSystem, CastlePlacementAdapter
│       │   ├── battle/                             # Unit, UnitLogic, UnitEventRelay
│   │       │                                       # UnitLifecycle, UnitVisual
│   │       │                                       # NavigationPortAdapter, BattleManager, UnitSpawn
│       │   ├── battle_report/ BattleReportSystem.cs
│       │   ├── combine/        CombineBuildingFactory.cs
│       │   ├── replay/        ReplayTarget.cs
│       │   ├── ui/            UIManager + 子控制器
│       │   ├── flow/          TitleScreen, MainGameController, NightOrchestrator
│       │   ├── core/          GameConfig, GameCoordinatesAdapter
│       │   ├── dev/           DevInputLogger
│       │   └── audio/         AudioManager, Sfx, UiSfx
│       └── persistence/
│           ├── GameDataLoader.cs, BattleReportStorage.cs
│           ├── SqliteUnitRepository.cs, SqliteBuildingRepository.cs
│           └── FieldUnitRepository.cs
│
├── scenes/    prefabs/    assets/    project.godot
├── devPlan/design/          # 系统设计文档
├── tests/                   # CasualCastle.Domain.Tests
└── CasualCastle.sln         # 6 个项目
```

---

## 依赖方向

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

domain 项目零 `using Godot`，单向无循环。

---

## DI 机制

双层设计：

```
第一层：MS DI (ServiceProvider)
  ├── 纯 C# 领域服务：ShopRules, AdjacencyService, UnitSpatialService (IUnitSpatialService)
  ├── BattleReportService, ReplayService (IReplayUseCase)
  ├── 出站 Port 实现：IFieldUnitRepository→FieldUnitRepository
  ├── 持久化：IBattleReportRepository, IUnitRepository, IBuildingRepository
  └── IGameState（工厂委托到 AdapterRegistry）

第二层：AdapterRegistry（Godot 节点服务定位器）
  ├── GameManager, DisplaySettingsManager
  ├── BattleManager, BuildingSystem, BattleReportSystem
  ├── Hand, Shop, IBuildingFactory
  └── 动态实例（Building, Unit）按需解析
```

- `CompositionRoot.Build()` 构建 MS DI 容器
- `GameManager.Get<T>()` 解析 MS DI 服务
- `AdapterRegistry.Resolve<T>()` 解析 Godot 节点
- 领域服务优先走 MS DI，Godot 节点走 AdapterRegistry

---

## 系统清单

### Autoload（Godot 常驻节点）

| 类 | 职责 |
|----|------|
| `GameManager` | 游戏状态、昼夜阶段、双方血量、DI 根 |
| `InitManager` | 场景初始化，服务组装和相位回调注册 |
| `DisplaySettingsManager` | 分辨率/窗口模式、DevModeEnabled 开关 |

### 主场景节点（main_game.tscn）

| 类 | 职责 |
|----|------|
| `MainGameController` | 场景入口，注册 Battlefield/Castle 到 GameManager |
| `UIManager` | UI 总控，组合 HUD/商店/手牌/暂停/设置/结算控制器 |
| `BattleManager` | 每帧调用 UnitSpatialService.PushUnits |
| `BuildingSystem` | 统一放置、占地配置表、产兵属性应用 |
| `BattleReportSystem` | 夜末快照、局内缓存、持久化 |

### 动态实例

| 类 | 基类 | 职责 |
|----|------|------|
| `Castle` | Node2D | 网格占用、建筑放置、血条、放置预览 |
| `Building` | Area2D | 建筑基类，工作循环，实现 IBuildingState/IBuildingTarget/IBuildingRef |
| `Unit` | Area2D | 战斗单位容器（空壳），逻辑在 UnitLogic 子节点 |
| `UnitLifecycle` | — | 单位死亡动画、队列移除（纯 Godot 视觉） |
| `UnitVisual` | — | 单位外观（颜色、睡眠视觉、受击闪白） |

### 领域服务

| 类 | 子域 | 注册方式 |
|----|------|---------|
| `UnitSpatialService` | Battle | MS DI Singleton（通过 IUnitSpatialService） |
| `UnitService` | Battle | 手动 new（每个单位实例，通过 UnitLogic） |
| `AdjacencyService` | Building | MS DI Singleton |
| `ShopRules` | Building | MS DI Singleton |
| `CombineService` | Building | 手动 new（每次合成阶段组合，通过 NightOrchestrator） |
| `Hand` | Building | AdapterRegistry（依赖 Godot 桥接） |
| `Shop` | Building | AdapterRegistry（依赖 Hand） |
| `Player` | Building | 手动 new（通过 InitManager） |
| `BattleReportService` | History | MS DI Singleton |
| `ReplayService` | History | MS DI Singleton（通过 IReplayUseCase） |
| `NightOrchestrator` | flow | 手动 new（通过 InitManager） |

### 纯 C# 控制器（无 Godot 继承）

| 类 | 职责 |
|----|------|
| `HudUiController` | 顶部血条、金币、昼夜显示、跳过阶段 |
| `ShopUiController` | 商店面板、购买、拖拽直放 |
| `HandUiController` | 手牌展示、选中、拖拽放置 |
| `BuildingInfoUiController` | 建筑悬停信息 |
| `BuildingManageUiController` | 暂停/修复/禁止组合工具 |
| `CombineProhibitUiController` | 禁止组合标记 |
| `PauseMenuUiController` | 暂停菜单 |
| `GameOverUiController` | 结算弹窗 |
| `SettingsUiController` | 设置面板（显示模式/分辨率/开发者模式） |

---

## 主要运行链路

1. `project.godot` 启动 `title_screen.tscn`
2. 点击开始 → `main_game.tscn`
3. `MainGameController._Ready()` 注册 Battlefield/Castle 到 GameManager
4. `GameManager.StartGameSession()` → 开始昼夜循环（Day 60s / Night 30s）
5. 白天：兵营产兵、单位推进战斗（UnitLogic → IFieldUnitRepository → UnitSpatialService）
6. 夜晚：组合 → 敌方复刻 → 开商店 → 全军休眠
7. 任一方城堡血量归零 → GameOver → 结算弹窗（保存战报/返回标题）

---

## 端口接口

| 端口 | 定义位置 | 实现方 | 类型 |
|------|----------|--------|------|
| `IGameState` | Domain.Battle | GameManager | 入站 |
| `IUnitSpatialService` | Domain.Battle | UnitSpatialService | 领域服务 |
| `IFieldUnitRepository` | Domain.Battle | FieldUnitRepository | 出站 |
| `IUnitRepository` | Domain.Battle | SqliteUnitRepository | 出站 |
| `INavigationPort` | Domain.Battle | NavigationPortAdapter | 出站 |
| `IUnitEventPort` | Domain.Battle | UnitEventRelay | 出站 |
| `IBuildingRef` | Domain.Battle | Building | 出站 |
| `IBuildingTarget` | Domain.Battle | Building | 出站 |
| `ICombineUseCase` | Domain.Building | CombineService | 入站 |
| `IBuildingPlacement` | Domain.Building | CastlePlacementAdapter | 入站 |
| `IBuildingRepository` | Domain.Building | SqliteBuildingRepository | 出站 |
| `IBuildingState` | Domain.Building | Building | 领域内接口 |
| `ICombineBuildingFactory` | Domain.Building | CombineBuildingFactory | 出站 |
| `IBattleReportRepository` | Domain.History | BattleReportStorage | 出站 |
| `IReplayUseCase` | Domain.History | ReplayService | 入站 |
| `IReplayTarget` | Domain.History | ReplayTarget | 出站 |
| `IBuildingFactory` | adapters/godot/building | BuildingFactory | 出站 |
