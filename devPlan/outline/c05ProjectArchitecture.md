# 五、项目架构

## 5.1 实际目录结构（当前）

```
CasualCastle/
├── scripts/
│   ├── autoload/              # GameManager（Godot Autoload）
│   ├── core/                  # GameConfig 等全局配置
│   ├── flow/                  # TitleScreen、MainGameController
│   ├── ui/                    # UIManager 与子 UI 控制器
│   ├── shop/                  # ShopSystem
│   ├── card/                  # CardSystem、CardData
│   ├── night/                 # NightSystem
│   ├── building/              # Castle、Building、Barracks
│   ├── battle/                # Soldier
│   ├── audio/                 # BgmPlayer
│   └── dev/                   # DevInputLogger
├── scenes/
│   ├── main/main_game.tscn
│   └── ui/title_screen.tscn
├── prefabs/
│   ├── Castle.tscn
│   ├── Barracks.tscn
│   └── Soldier.tscn
├── assets/                    # 占位图、士兵图、BGM、卡牌边框
├── devPlan/                  # 开发文档
│   ├── outline/               # 开发大纲，按章节拆分
│   ├── concepts.md
│   ├── currentTasks.md
│   ├── codeStructure.md
│   └── dataStructures.md
└── resources/                 # 空，待放 .tres 数据资源
```

## 5.2 规划目录结构（完整版目标）

```
scripts/
├── autoload/GameManager.cs
├── core/GameConfig.cs
├── flow/TitleScreen.cs, MainGameController.cs
├── ui/UIManager.cs + UI 子控制器
├── shop/ShopSystem.cs
├── card/CardSystem.cs, CardData.cs
├── night/NightSystem.cs
├── building/Castle.cs, Building.cs, Barracks.cs, BuildingSystem.cs（待建）
├── battle/Soldier.cs, BattleSystem.cs（待建）
├── audio/BgmPlayer.cs
└── dev/DevInputLogger.cs
resources/
├── cards/                     # CardData .tres
├── buildings/                 # BuildingData .tres
└── units/                     # UnitData .tres
```

## 5.3 核心系统说明

| 系统 | 职责 | 当前状态 |
|------|------|----------|
| GameManager | 游戏状态、阶段切换、胜负 | Godot Autoload，支持 Playing / GameOver |
| UIManager | HUD、结算、商店、手牌与场景跳转 | 已拆分为 UI 入口和多个子控制器 |
| ShopSystem | 商店刷新、购买、金币、拖拽直放 | 已实现 |
| CardSystem | 手牌管理、点击/拖拽打出 | 已实现，经 `BuildingSystem` 放置 |
| BuildingSystem | 放置验证、占地配置、统一落子 | 已实现 |
| NightSystem | 夜晚休眠门控、夜战词条判定 | 已实现 |
| AdjacentSystem | 邻接检测与加成、放置光圈 | 已实现（兵营规则） |
| BattleSystem | 部队生成、战斗 AI | 逻辑在 Soldier.cs |
| FusionSystem | 融合条件与升级 | 未建 |

## 5.4 系统模块设计

系统模块按“流程控制 → 玩家操作 → 场上执行 → 数据配置”的方向组织。`GameManager` 只负责全局状态、阶段和胜负；具体玩法逻辑尽量放到对应系统中，避免继续堆到单个节点脚本里。

| 模块 | 职责 | 主要依赖 |
|------|------|----------|
| SceneFlow | 标题页、进入主游戏、返回标题 | `TitleScreen`, `MainGameController`, `GameManager` |
| GameManager | 游戏状态、昼夜阶段、胜负、全局信号 | `GameConfig`, `MainGameController`, `Castle`, `UIManager` |
| UIManager | HUD、结算、阶段显示、商店/手牌入口 | `HudUiController`, `ShopUiController`, `HandUiController`, `GameOverUiController` |
| NightSystem | 昼夜行动门控、夜战词条判定 | `GameManager`, `BuildingSystem`, `BattleSystem` |
| ShopSystem | 商店、刷新、购买、金币消费、夜晚自动弹出 | `GameManager`, `CardSystem`, `CardData` |
| CardSystem | 手牌、卡牌打出、卡牌到建筑的转换 | `CardData`, `BuildingSystem`, `UIManager` |
| BuildingSystem | 建筑放置、占格验证、建筑工作调度 | `Castle`, `BuildingData`, `NightSystem` |
| AdjacentSystem | 建筑邻接检测、加成刷新 | `BuildingSystem`, `Castle` |
| FusionSystem | 建筑融合条件、升级结果生成 | `BuildingSystem`, `BuildingData`, `UIManager` |
| BattleSystem | 士兵生成、行动、索敌、攻击与死亡 | `UnitData`, `NightSystem`, `Castle` |
| AISystem | 敌方购卡、放置、战术决策 | `GameManager`, `ShopSystem`, `CardSystem`, `BuildingSystem` |
| DataResources | 卡牌、建筑、单位、全局配置数据 | `CardData`, `BuildingData`, `UnitData`, `GameConfig` |

模块依赖关系如下：

```mermaid
flowchart LR
    SceneFlow[SceneFlow<br/>标题/场景切换]
    GameManager[GameManager<br/>状态/阶段/胜负]
    UIManager[UIManager<br/>HUD/商店/手牌 UI]
    NightSystem[NightSystem<br/>昼夜门控]
    ShopSystem[ShopSystem<br/>商店]
    CardSystem[CardSystem<br/>手牌/打出]
    BuildingSystem[BuildingSystem<br/>放置/建筑调度]
    AdjacentSystem[AdjacentSystem<br/>邻接加成]
    FusionSystem[FusionSystem<br/>融合升级]
    BattleSystem[BattleSystem<br/>单位/战斗]
    AISystem[AISystem<br/>敌方决策]
    DataResources[DataResources<br/>Card/Building/Unit/GameConfig]
    RuntimeNodes[RuntimeNodes<br/>Castle/Building/Soldier]

    SceneFlow --> GameManager
    SceneFlow --> UIManager

    GameManager --> UIManager
    GameManager --> NightSystem
    GameManager --> RuntimeNodes
    GameManager --> DataResources

    UIManager --> GameManager
    UIManager --> ShopSystem
    UIManager --> CardSystem
    UIManager --> FusionSystem

    NightSystem --> GameManager
    BuildingSystem --> NightSystem
    BattleSystem --> NightSystem

    ShopSystem --> GameManager
    ShopSystem --> CardSystem
    ShopSystem --> DataResources

    CardSystem --> UIManager
    CardSystem --> BuildingSystem
    CardSystem --> DataResources

    BuildingSystem --> RuntimeNodes
    BuildingSystem --> DataResources
    AdjacentSystem --> BuildingSystem
    AdjacentSystem --> RuntimeNodes
    FusionSystem --> BuildingSystem
    FusionSystem --> DataResources

    BattleSystem --> RuntimeNodes
    BattleSystem --> DataResources
    BattleSystem --> GameManager

    AISystem --> GameManager
    AISystem --> ShopSystem
    AISystem --> CardSystem
    AISystem --> BuildingSystem

    RuntimeNodes --> GameManager
```

---
