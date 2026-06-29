# 代码结构文档 — CasualCastle

本文档记录当前运行时代码结构。概念定义见 `devPlan/concepts.md`。

---

## 项目结构

```
CasualCastle/
├── scripts/
│   ├── domain/                                     # 核心域层（4 个 C# 项目，零 Godot）
│   │   ├── Shared/                                 # Shared.csproj — 坐标、常量
│   │   │   ├── GameVector2.cs, GridCellOffset.cs
│   │   │   ├── GameCoordinateRules.cs, GameRules.cs
│   │   │   └── SharedModule.cs
│   │   ├── Building/                               # Building.csproj → Shared
│   │   │   ├── CardData.cs, FusionRecipe.cs
│   │   │   ├── BuildingDefinitions.cs, OccupancyGrid.cs
│   │   │   ├── AdjacentRules.cs, FusionRules.cs
│   │   │   ├── ShopRules.cs, CardRules.cs
│   │   │   ├── IBuildingState.cs, IBuildingRegistry.cs, IBuildingPlacement.cs
│   │   │   └── BuildingModule.cs
│   │   ├── Battle/                                 # Battle.csproj → Shared, Building
│   │   │   ├── SoldierData.cs, NightRules.cs, CombatRules.cs
│   │   │   ├── IGameState.cs
│   │   │   └── BattleModule.cs
│   │   └── History/                                # History.csproj → Shared, Building
│   │       ├── BattleReportModels.cs, IBattleReportRepository.cs
│   │       ├── ReportBuilder.cs, MirrorRules.cs
│   │       ├── ISnapshotQuery.cs
│   │       └── HistoryModule.cs
│   │
│   ├── CompositionRoot.cs                          # MS DI 容器构建
│   │
│   └── adapters/
│       ├── godot/
│       │   ├── autoload/
│       │   │   ├── GameManager.cs                  # Autoload，DI 根，实现 IGameState
│       │   │   ├── DisplaySettingsManager.cs       # 分辨率/窗口 + DevModeEnabled
│       │   │   └── AdapterRegistry.cs              # Godot 节点服务定位器
│       │   ├── building/                           # Castle, Building, BuildingSystem, AdjacentSystem
│       │   ├── battle/                             # Soldier, UnitSpawn
│       │   ├── shop/          ShopSystem.cs
│       │   ├── card/          CardSystem.cs
│       │   ├── night/         NightSystem.cs
│       │   ├── fusion/        FusionSystem.cs
│       │   ├── battle_report/ BattleReportSystem.cs
│       │   ├── replay/        ReplayAiSystem.cs
│       │   ├── ui/            UIManager + 子控制器
│       │   ├── flow/          TitleScreen, MainGameController
│       │   ├── core/          GameConfig, GameCoordinatesAdapter
│       │   ├── dev/           DevInputLogger
│       │   └── audio/         BgmPlayer
│       └── persistence/       BattleReportStorage
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
  ├── 纯 C# 服务：IBattleReportRepository → BattleReportStorage
  └── IGameState（工厂委托到 AdapterRegistry）

第二层：AdapterRegistry（轻量服务定位器）
  ├── GameManager, DisplaySettingsManager
  ├── NightSystem, BuildingSystem, AdjacentSystem
  ├── CardSystem, ShopSystem, FusionSystem
  ├── BattleReportSystem, ReplayAiSystem
  └── 动态实例（Building）按需解析
```

- `CompositionRoot.Build()` 构建 MS DI 容器
- `GameManager.Get<T>()` 快捷解析 MS DI 服务
- `AdapterRegistry.Resolve<T>()` 解析 Godot 节点

---

## 系统清单

### Autoload（Godot 常驻节点）

| 类 | 职责 |
|----|------|
| `GameManager` | 游戏状态、昼夜阶段、双方血量、作弊产兵（需 DevMode） |
| `DisplaySettingsManager` | 分辨率/窗口模式、DevModeEnabled 开关 |

### 主场景节点（main_game.tscn）

| 类 | 职责 |
|----|------|
| `MainGameController` | 场景入口，注册 Battlefield/Castle 到 GameManager |
| `UIManager` | UI 总控，组合 HUD/商店/手牌/暂停/设置/结算控制器 |
| `NightSystem` | 昼夜行动判断入口 |
| `ShopSystem` | 金币、商品槽、购买、拖拽直放 |
| `CardSystem` | 手牌管理、选中、打出手牌 |
| `BuildingSystem` | 统一放置、占地配置表 |
| `AdjacentSystem` | 邻接检测、兵营加速、放置光圈 |
| `FusionSystem` | 入夜自动融合、禁止融合工具 |
| `BattleReportSystem` | 夜末快照、局内缓存、持久化 |
| `ReplayAiSystem` | 战报选读、入夜镜像复刻 |

### 动态实例

| 类 | 基类 | 职责 |
|----|------|------|
| `Castle` | Node2D | 网格占用、建筑放置、血条、放置预览 |
| `Building` | Area2D | 建筑基类，工作循环、邻接加成 |
| `Soldier` | Area2D | 战斗单位，推进/索敌/攻击/死亡 |

### 纯 C# 控制器（无 Godot 继承）

| 类 | 职责 |
|----|------|
| `HudUiController` | 顶部血条、金币、昼夜显示、跳过阶段 |
| `ShopUiController` | 商店面板、购买、拖拽直放 |
| `HandUiController` | 手牌展示、选中、拖拽放置 |
| `BuildingInfoUiController` | 建筑悬停信息 |
| `BuildingManageUiController` | 暂停/修复/禁止融合工具 |
| `FusionProhibitUiController` | 禁止融合标记 |
| `PauseMenuUiController` | 暂停菜单 |
| `GameOverUiController` | 结算弹窗 |
| `SettingsUiController` | 设置面板（显示模式/分辨率/开发者模式） |

---

## 主要运行链路

1. `project.godot` 启动 `title_screen.tscn`
2. 点击开始 → `main_game.tscn`
3. `MainGameController._Ready()` 注册 Battlefield/Castle 到 GameManager
4. `GameManager.StartGameSession()` → 开始昼夜循环（Day 60s / Night 30s）
5. 白天：兵营产兵、士兵推进战斗
6. 夜晚：融合 → 敌方复刻 → 开商店 → 休眠无夜战单位
7. 任一方城堡血量归零 → GameOver → 结算弹窗（保存战报/返回标题）

---

## 端口接口

| 端口 | 定义位置 | 实现方 |
|------|----------|--------|
| `IGameState` | Domain.Battle | GameManager |
| `IBuildingRegistry` | Domain.Building | BuildingDefinitions（静态） |
| `IBuildingPlacement` | Domain.Building | Castle/OccupancyGrid |
| `ISnapshotQuery` | Domain.History | BattleReportSystem |
| `IBattleReportRepository` | Domain.History | BattleReportStorage (MS DI) |
