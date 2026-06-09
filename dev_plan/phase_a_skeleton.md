# 阶段 A — 项目骨架与公共基础

**目标**：建立项目基础架构、场景层次、自动加载系统和输入框架，为后续系统开发提供稳定的基础。

**优先级**：★★★★★（最高优先）— 所有后续工作依赖此阶段完成。

---

## A.1 项目初始化与目录/场景模板

### 目标
建立主场景、基础输入系统和场景层次结构，为后续 UI、建筑、单位的场景集成做准备。

### 实施内容

#### 1. 主场景结构 (`scenes/main/castle_game.tscn`)
创建一个根场景，包含以下子节点层次：

```
Node (root)
├── World (Node3D) — 游戏世界容器
│   ├── PlayerCastle (Node3D) — 玩家城堡位置/模型
│   ├── EnemyCastle (Node3D) — 敌方城堡位置/模型
│   ├── Battlefield (Node3D) — 战场区域
│   │   ├── Units (Node3D) — 动态单位容器
│   │   └── Buildings (Node3D) — 建筑放置点容器
│   └── Environment (Node3D) — 地面、天空等静态元素
├── UI (CanvasLayer) — 所有 UI 元素
│   ├── GameUI (Control) — 主游戏 UI 容器
│   │   ├── TopBar (HBoxContainer) — 顶部信息栏（货币、资源等）
│   │   ├── HandPanel (Panel) — 下方手牌区
│   │   ├── ShopPanel (Panel) — 商店面板（初始隐藏）
│   │   └── CastleHealthBar (ProgressBar) — 城堡血条
│   └── DebugUI (Control) — 调试信息（开发时显示）
├── Input (Node) — 输入管理器（C# 脚本）
└── GameState (Node) — 游戏状态脚本（C# 脚本）
```

#### 2. 基础输入系统 (`scripts/utils/InputManager.cs`)
- 集中管理输入事件（鼠标点击、右键、键盘快捷键等）。
- 向其他系统广播输入事件（通过信号或回调）。
- 支持鼠标射线拾取（用于选择建筑放置点、单位等）。

**功能列表**：
- 监听 `_Input()` 事件，解析鼠标位置、点击类型。
- 发出信号示例：`input_left_clicked(Vector3 world_pos)`, `input_right_clicked(mouse_pos)`, `input_key_pressed(key)`。

#### 3. 场景初始化脚本 (`scripts/GameInitializer.cs`)
- 挂载在主场景根节点上，负责初始化游戏状态。
- `_Ready()` 中调用各系统初始化（如 CardSystem、Economy、UIManager 等）。
- 作为主 GameLoop 的入口。

### 检查清单
- [ ] 创建 `scenes/main/castle_game.tscn` 并构建上述节点层次。
- [ ] 实现 `scripts/utils/InputManager.cs`，支持鼠标/键盘事件捕获和信号发射。
- [ ] 实现 `scripts/GameInitializer.cs`，连接各系统初始化逻辑。
- [ ] 在 Godot 编辑器中验证场景能够加载，输入系统响应正常。

---

## A.2 自动加载（Autoload）基础管理器空壳

### 目标
为全局单例管理器建立框架，确保各系统（CardSystem、UIManager、Economy 等）能在游戏启动时自动初始化并全局可访问。

### 实施内容

#### 1. 全局单例基类 (`scripts/autoload/SingletonBase.cs`)
创建一个可复用的单例基类，所有全局管理器继承它：

```csharp
public abstract partial class SingletonBase : Node
{
    public static T GetInstance<T>() where T : SingletonBase
    {
        var instance = GetTree().Root.GetChild(0).GetNode<T>(InstancePath);
        return instance;
    }
    
    protected virtual string InstancePath => GetClass(); // 默认使用类名
    
    public override void _Ready()
    {
        GD.Print($"Initialized singleton: {GetClass()}");
    }
}
```

#### 2. 核心管理器空壳

创建以下管理器脚本（先建空壳，后续各阶段补充实现）：

**`scripts/autoload/GameManager.cs`**
- 角色：全局游戏状态管理器（游戏阶段、暂停状态等）
- 关键属性：`IsGamePaused`, `CurrentGamePhase`（购物、战斗等）
- 信号：`game_state_changed`, `game_paused`, `game_resumed`

**`scripts/autoload/UIManager.cs`**
- 角色：UI 显隐/更新协调器
- 关键方法：`ShowPanel(string panelName)`, `HidePanel(string panelName)`, `UpdateTopBar(data)`
- 信号：`ui_panel_opened`, `ui_panel_closed`

**`scripts/autoload/Economy.cs`**
- 角色：货币与资源管理
- 关键属性：`PlayerGold`, `PlayerResources`
- 关键方法：`AddGold(int amount)`, `SpendGold(int amount, bool canAffect)`
- 信号：`gold_changed(int new_amount)`, `resource_changed`

**`scripts/autoload/EventBus.cs`**
- 角色：全局事件分发中心（可选，但推荐用于解耦）
- 功能：发出和监听全局事件（如 `card_played`, `building_placed`, `unit_died` 等）

#### 3. Autoload 注册
在 Godot 编辑器中（Project → Project Settings → Autoload）：
1. 将上述管理器脚本路径添加为自动加载节点。
2. 节点名称应与脚本名一致（如 `GameManager`, `UIManager` 等）。

**或** 编辑 `project.godot`：
```ini
[autoload]
GameManager="res://scripts/autoload/GameManager.cs"
UIManager="res://scripts/autoload/UIManager.cs"
Economy="res://scripts/autoload/Economy.cs"
EventBus="res://scripts/autoload/EventBus.cs"
```

### 检查清单
- [ ] 创建 `scripts/autoload/SingletonBase.cs` 基类。
- [ ] 创建 `scripts/autoload/GameManager.cs`（空壳 + 基础信号）。
- [ ] 创建 `scripts/autoload/UIManager.cs`（空壳 + 基础信号）。
- [ ] 创建 `scripts/autoload/Economy.cs`（空壳 + 基础数据结构）。
- [ ] 创建 `scripts/autoload/EventBus.cs`（全局信号汇总）。
- [ ] 在 `project.godot` 或编辑器中注册为 Autoload。
- [ ] 验证在主场景启动时，所有管理器都能正常初始化（观察控制台输出）。

---

## A.3 项目公共结构验证

### 目标
确保项目目录、命名约定和基础架构可正常工作。

### 实施内容

#### 1. 验证目录结构
检查以下目录是否存在，若不存在则创建：
```
scripts/
├── autoload/       → 全局单例
├── systems/        → 各系统实现（CardSystem 等）
├── utils/          → 工具类
└── components/     → 可复用组件（未来补充）

scenes/
├── main/           → 主场景
├── ui/             → UI 场景组件
└── levels/         → 关卡场景

prefabs/
├── buildings/      → 建筑预制体
└── units/          → 单位预制体

assets/
├── art/
│   ├── cards/      → 卡牌美术
│   └── characters/ → 角色/单位美术
├── audio/          → 音效
└── fonts/          → 字体
```

#### 2. 编码约定快速检查
- [ ] 所有 C# 文件使用 `PascalCase` 命名（如 `CardSystem.cs`, `InputManager.cs`）。
- [ ] 所有 Godot 场景文件使用 `snake_case` 命名（如 `castle_game.tscn`, `card_display.tscn`）。
- [ ] 所有代码文件采用 UTF-8 编码（通过 `.editorconfig` 强制）。
- [ ] 类声明使用 `public partial class` 模式（Godot C# 标准）。

#### 3. 首次启动测试
- [ ] 在 Godot 编辑器中打开项目。
- [ ] 加载 `scenes/main/castle_game.tscn`。
- [ ] 按 F5 或点击播放按钮运行场景。
- [ ] 预期结果：
  - 场景加载无错误。
  - 所有 Autoload 管理器初始化完成（控制台打印）。
  - 鼠标/键盘输入能被捕获（可在 InputManager 中打印日志验证）。
  - 主场景节点树正常显示。

### 检查清单
- [ ] 验证/创建所有必要的目录。
- [ ] 检查编码约定一致性。
- [ ] 完成首次启动测试。

---

## 后续依赖与交接

完成阶段 A 后，后续阶段可以：
- **阶段 B (Economy/Shop)** 直接使用 `Economy` 单例和 `UIManager` 协调商店 UI。
- **阶段 C (CardSystem)** 使用 `EventBus` 广播卡牌事件，使用 `InputManager` 处理手牌交互。
- **阶段 D-G** 所有系统都通过 Autoload 管理器和事件总线进行松耦合通信。

---

## 实现顺序建议
1. **先建场景结构** → `castle_game.tscn`（最直观，便于验证）。
2. **再建 Autoload 管理器** → 空壳 + 基础信号（为阶段 B 做准备）。
3. **最后验证** → 首次启动测试。

---

## 时间估计
- 场景搭建：1-2h（包括编辑器调整）。
- 管理器编码：2-3h（5 个空壳 + 基础逻辑）。
- 测试验证：0.5-1h。
- **总计：3.5-6h（1 人）**。

