# 代码结构文档 — CasualCastle

本文档记录当前代码中已经存在的系统、类和结构关系。概念定义以 `dev_plan/concepts.md` 为准；这里侧重运行时代码事实。

---

## 项目结构概览

CasualCastle 是 Godot 4.6 C# 项目，主入口配置在 `project.godot`：

- 启动场景：`scenes/ui/title_screen.tscn`
- 主游戏场景：`scenes/main/main_game.tscn`
- C# 脚本根目录：`scripts/`
- 可复用预制体：`prefabs/`

当前 `main_game.tscn` 的核心节点结构：

- `MainGame`
- `DevInputLogger`
- `BGM`
- `GameManager`
- `UIManager`
- `NightSystem`
- `ShopSystem`
- `CardSystem`
- `Battlefield`
  - `PlayerSide/PlayerCastle`
  - `EnemySide/EnemyCastle`
- `UI`

---

## 系统划分与现有类

### 启动与场景切换系统

负责标题界面、进入主游戏、退出游戏。

| 类 | 文件 | Godot 基类 | 职责 |
| --- | --- | --- | --- |
| `TitleScreen` | `scripts/ui/TitleScreen.cs` | `Control` | 绑定标题界面按钮，开始游戏时切换到 `main_game.tscn`，退出时关闭游戏。 |

结构关系：

- `project.godot` 将 `title_screen.tscn` 配置为启动场景。
- `title_screen.tscn` 挂载 `TitleScreen`，同时复用 `BgmPlayer` 播放标题音乐。
- `TitleScreen` 通过 `GetTree().ChangeSceneToFile()` 进入主游戏。

### 全局游戏状态系统

负责游戏胜负、双方城堡生命值、昼夜阶段和开发作弊出兵。

| 类 | 文件 | Godot 基类 | 职责 |
| --- | --- | --- | --- |
| `GameManager` | `scripts/autoload/GameManager.cs` | `Node2D` | 维护 `GameState`、`GamePhase`、双方血量、阶段计时；发出血量/状态/阶段信号；处理 P 键开发出兵。 |
| `GameManager.GameState` | `scripts/autoload/GameManager.cs` | `enum` | 当前游戏状态：`Playing` / `GameOver`。 |
| `GameManager.GamePhase` | `scripts/autoload/GameManager.cs` | `enum` | 当前昼夜阶段：`Day` / `Night`。 |

结构关系：

- `GameManager.Instance` 是运行时单例入口。
- `GameManager` 读取主场景中的 `Battlefield` 和玩家 `Castle`。
- `Castle.TakeDamage()` 会调用 `GameManager.TakeDamage()` 同步顶部 UI 血量与胜负。
- `UIManager`、`Building`、`Soldier` 订阅或查询 `GameManager` 的状态与阶段。
- `NightSystem` 最终委托 `GameManager.CanUnitWork()` 判断单位是否可行动。

### 昼夜系统

负责给建筑和士兵提供昼夜行动判断入口。

| 类 | 文件 | Godot 基类 | 职责 |
| --- | --- | --- | --- |
| `NightSystem` | `scripts/systems/NightSystem.cs` | `Node` | 提供 `CanUnitWork()` 静态方法，根据 `GameManager` 当前阶段判断单位/建筑是否可工作。 |
| `GameConfig` | `scripts/utils/GameConfig.cs` | `static class` | 保存昼夜时长和初始金币等全局配置常量。 |

结构关系：

- `GameManager.BeginPhase()` 使用 `GameConfig.DayDurationSeconds` / `NightDurationSeconds` 设置阶段时长。
- `Building.CanWork` 和 `Soldier.IsActive` 都通过 `NightSystem.CanUnitWork(HasNightCombat)` 判断是否休眠。
- 当前 `NightSystem` 只作为统一入口，没有批量管理场上单位列表。

### 城堡与建筑系统

负责城堡网格、建筑放置、兵营工作循环和产兵。

| 类 | 文件 | Godot 基类 | 职责 |
| --- | --- | --- | --- |
| `Castle` | `scripts/nodes/Castle.cs` | `Node2D` | 管理城堡地块网格、生命值、血条、建筑放置和兵营初始化。 |
| `Building` | `scripts/nodes/Building.cs` | `Area2D` | 建筑基类；记录所属城堡与地块坐标；处理工作循环、昼夜暂停和工作视觉效果。 |
| `Barracks` | `scripts/nodes/Barracks.cs` | `Building` | 兵营建筑；按间隔生成士兵，阵营由所属城堡决定。 |

结构关系：

- `Castle` 在 `_Ready()` 中调用 `SetupBarracks()`，实例化 `prefabs/Barracks.tscn`。
- `Barracks` 继承 `Building`，调用 `BindToGrid()` 绑定所属 `Castle` 和地块坐标。
- `Castle.PlaceBuilding()` 将建筑放入地块中心并记录占用。
- `Building` 订阅 `GameManager.PhaseChanged`，昼夜变化时暂停或恢复工作循环。
- `Barracks.PerformWork()` 实例化 `prefabs/Soldier.tscn`，将 `Soldier` 加入 `Battlefield`。

### 士兵与战斗系统

负责士兵推进、索敌、攻击敌方士兵和攻打敌方城堡。

| 类 | 文件 | Godot 基类 | 职责 |
| --- | --- | --- | --- |
| `Soldier` | `scripts/nodes/Soldier.cs` | `Area2D` | 基础战斗单位；按阵营推进，检测敌方士兵/建筑，攻击目标并处理死亡。 |

结构关系：

- `Barracks` 和 `GameManager` 都可以实例化 `Soldier`。
- `Soldier.IsPlayerUnit` 决定移动方向和敌我关系。
- `Soldier` 通过 `AreaEntered` / `AreaExited` 检测敌方 `Soldier` 和 `Building`。
- 当 `Soldier` 进入敌方 `Building` 碰撞区时，通过 `Building.GetCastle()` 获取目标 `Castle` 并调用 `Castle.TakeDamage()`。
- `Soldier` 订阅 `GameManager.PhaseChanged`，夜晚休眠时停止移动和攻击。

### UI、音频与开发辅助系统

负责主游戏 UI、背景音乐和开发输入日志。

| 类 | 文件 | Godot 基类 | 职责 |
| --- | --- | --- | --- |
| `UIManager` | `scripts/autoload/UIManager.cs` | `Node2D` | 更新顶部血条、昼夜显示、阶段倒计时、游戏结束面板，并处理返回标题和跳过阶段按钮。 |
| `BgmPlayer` | `scripts/nodes/BgmPlayer.cs` | `AudioStreamPlayer` | 加载并循环播放背景音乐。 |
| `DevInputLogger` | `scripts/utils/DevInputLogger.cs` | `Node` | 打印按键输入信息，用于开发调试。 |

结构关系：

- `UIManager` 从父节点获取 `CanvasLayer` 下的 UI 节点。
- `UIManager` 订阅 `GameManager` 的血量、状态和阶段信号。
- `UIManager` 的跳过阶段按钮调用 `GameManager.AdvancePhase()`。
- `UIManager` 的返回标题按钮切回 `title_screen.tscn`。
- `BgmPlayer` 同时用于标题场景和主游戏场景。

### 待扩展系统

当前存在脚本与场景节点，但尚未实现业务逻辑。

| 类 | 文件 | Godot 基类 | 当前状态 |
| --- | --- | --- | --- |
| `CardSystem` | `scripts/systems/CardSystem.cs` | `Node` | 空实现，预留建筑卡系统。 |
| `ShopSystem` | `scripts/systems/ShopSystem.cs` | `Node` | 空实现，预留夜晚商店系统。 |

---

## 类图

### 总览类图

总览类图只展示系统级关系，具体字段、方法和继承关系放在后面的分系统类图中。

```mermaid
classDiagram
    class TitleScreen
    class GameManager
    class UIManager
    class NightSystem
    class Castle
    class Building
    class Barracks
    class Soldier
    class ShopSystem
    class CardSystem
    class BgmPlayer
    class DevInputLogger

    TitleScreen --> GameManager : 进入主游戏场景
    GameManager --> UIManager : 状态/血量/阶段信号
    GameManager --> Castle : 读取城堡与同步血量
    GameManager --> Soldier : 开发作弊生成
    GameManager --> NightSystem : 提供阶段判断
    UIManager --> GameManager : 查询状态与跳过阶段
    NightSystem --> GameManager : 委托行动判断

    Castle --> Building : 放置建筑
    Castle --> Barracks : 初始化兵营
    Building --> NightSystem : 判断昼夜工作
    Barracks --> Soldier : 周期产兵
    Soldier --> NightSystem : 判断昼夜行动
    Soldier --> Building : 检测建筑碰撞
    Soldier --> Castle : 攻击城堡

    ShopSystem ..> GameManager : 预留
    CardSystem ..> Building : 预留
    BgmPlayer ..> TitleScreen : 标题音乐
    BgmPlayer ..> GameManager : 主游戏音乐
    DevInputLogger ..> GameManager : 开发调试
```

### 启动与场景切换系统类图

```mermaid
classDiagram
    class Control
    class TitleScreen {
        -OnStartPressed()
        -OnExitPressed()
    }
    class BgmPlayer

    Control <|-- TitleScreen
    TitleScreen --> BgmPlayer : 标题音乐
    TitleScreen --> GameManager : 切换到主游戏场景
```

### 全局游戏状态系统类图

```mermaid
classDiagram
    class Node2D
    class GameManager {
        +Instance GameManager
        +CurrentState GameState
        +CurrentPhase GamePhase
        +PlayerHealth int
        +EnemyHealth int
        +PhaseTimeRemaining float
        +AdvancePhase()
        +CanUnitWork(bool) bool
        +TakeDamage(bool, int)
        +EndGame(bool)
    }
    class GameState {
        <<enumeration>>
        Playing
        GameOver
    }
    class GamePhase {
        <<enumeration>>
        Day
        Night
    }
    class GameConfig {
        <<static>>
        +DayDurationSeconds float
        +NightDurationSeconds float
        +InitialGold int
    }

    Node2D <|-- GameManager
    GameManager --> GameState : 使用
    GameManager --> GamePhase : 使用
    GameManager --> GameConfig : 读取阶段时长
    GameManager --> Castle : 获取玩家城堡
    GameManager --> Soldier : 开发作弊生成
    GameManager --> UIManager : 游戏结束时通知
```

### 昼夜系统类图

```mermaid
classDiagram
    class Node
    class NightSystem {
        +Instance NightSystem
        +CanUnitWork(bool) bool
    }
    class GameManager {
        +CurrentPhase GamePhase
        +CanUnitWork(bool) bool
    }
    class Building {
        +HasNightCombat bool
        +CanWork bool
    }
    class Soldier {
        +HasNightCombat bool
        +IsActive bool
    }

    Node <|-- NightSystem
    NightSystem --> GameManager : 委托行动判断
    Building --> NightSystem : 判断是否工作
    Soldier --> NightSystem : 判断是否行动
```

### 城堡与建筑系统类图

```mermaid
classDiagram
    class Node2D
    class Area2D
    class Castle {
        +IsPlayerCastle bool
        +MaxHealth int
        +GridColumns int
        +GridRows int
        +CellSize int
        +Health int
        +PlaceBuilding(Building, int, int) bool
        +GetBuildingSpawnPosition(int, int, Vector2I, int) Vector2
        +TakeDamage(int)
        +ResetHealth()
    }
    class Building {
        +CollisionSize int
        +HasNightCombat bool
        +CanWork bool
        +BindToGrid(Castle, int, int)
        +GetCastle() Castle
        #StartWorkCycle()
        #PerformWork()
        #BeginWork(float)
    }
    class Barracks {
        +SpawnInterval float
        +IsPlayerBarracks bool
        +BindToGrid(Castle, int, int)
        +SpawnUnits(int)
    }

    Node2D <|-- Castle
    Area2D <|-- Building
    Building <|-- Barracks
    Castle --> Barracks : 初始化兵营
    Castle --> Building : 放置建筑
    Castle --> GameManager : 同步城堡伤害
    Building --> Castle : 绑定所属城堡
    Building --> GameManager : 订阅阶段变化
    Barracks --> Castle : 计算出生点
    Barracks --> Soldier : 生成士兵
```

### 士兵与战斗系统类图

```mermaid
classDiagram
    class Area2D
    class Soldier {
        +Health int
        +Damage int
        +Speed float
        +AttackRange float
        +AttackCooldown float
        +HasNightCombat bool
        +IsPlayerUnit bool
        +IsAlive bool
        +TakeDamage(int)
    }
    class Building {
        +GetCastle() Castle
    }
    class Castle {
        +TakeDamage(int)
    }
    class GameManager {
        +CurrentState GameState
    }

    Area2D <|-- Soldier
    Soldier --> Soldier : 攻击敌方士兵
    Soldier --> Building : 碰撞检测建筑
    Soldier --> Castle : 攻击所属城堡
    Soldier --> GameManager : 查询游戏状态/订阅阶段
    Soldier --> NightSystem : 判断昼夜行动
```

### UI、音频与开发辅助系统类图

```mermaid
classDiagram
    class Node
    class Node2D
    class AudioStreamPlayer
    class GameManager {
        +AdvancePhase()
    }
    class UIManager {
        +Instance UIManager
        +UpdatePlayerHealth(int)
        +UpdateEnemyHealth(int)
        +OnGameStateChanged(GameState)
    }
    class BgmPlayer {
        +_Ready()
    }
    class DevInputLogger {
        +_Input(InputEvent)
    }

    Node2D <|-- UIManager
    AudioStreamPlayer <|-- BgmPlayer
    Node <|-- DevInputLogger
    UIManager --> GameManager : 订阅信号/查询阶段
    UIManager --> TitleScreen : 返回标题场景
    DevInputLogger ..> GameManager : 开发调试
    BgmPlayer ..> TitleScreen : 标题音乐
    BgmPlayer ..> GameManager : 主游戏音乐
```

### 待扩展系统类图

```mermaid
classDiagram
    class Node
    class ShopSystem
    class CardSystem
    class GameManager
    class Building

    Node <|-- ShopSystem
    Node <|-- CardSystem
    ShopSystem ..> GameManager : 预留夜晚商店
    CardSystem ..> Building : 预留建筑卡
```

---

## 当前主要运行链路

1. `project.godot` 启动 `title_screen.tscn`。
2. `TitleScreen` 点击开始后进入 `main_game.tscn`。
3. `main_game.tscn` 创建 `GameManager`、`UIManager`、`NightSystem`、`ShopSystem`、`CardSystem`、`Battlefield` 和双方 `Castle`。
4. 每个 `Castle` 初始化网格和血条，并创建一座 `Barracks`。
5. `Barracks` 绑定所属 `Castle` 后进入 `Building` 工作循环。
6. `GameManager` 维护昼夜倒计时，阶段变化时发出 `PhaseChanged`。
7. `Building` 和 `Soldier` 根据 `NightSystem.CanUnitWork()` 在夜晚休眠或继续行动。
8. `Barracks` 周期性生成 `Soldier`；`Soldier` 推进、索敌、攻击敌方士兵或敌方城堡。
9. `Castle.TakeDamage()` 同步到 `GameManager.TakeDamage()`；血量归零后进入 `GameOver`。
10. `UIManager` 根据 `GameManager` 信号刷新血条、阶段显示和游戏结束界面。

---

## 维护建议

- 新增运行时类时，同步更新“系统划分与现有类”和类图。
- `CardSystem`、`ShopSystem` 开始实现后，应补充它们与 `GameManager`、`UIManager`、建筑卡数据的关系。
- 如果后续将 `GameManager`、`UIManager` 改为真正的 Godot Autoload，需要在本文档中区分“场景节点单例”和“项目 Autoload 单例”。
