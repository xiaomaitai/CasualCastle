# 当前任务

**当前焦点：真模块化与模块内防腐（`todo.md` §2）。**

目标：在六边形分层（§1 已完成）基础上，各业务模块定义对外接口，消除跨模块的静态 `Instance` 直调，使每个模块可独立理解与替换。

参考：`todo.md`、`codeStructure.md`、`outline/c05ProjectArchitecture.md`。

---

## 背景：§1 已完成

`scripts/` 已整理为三层结构：

```
scripts/
├── domain/     # 核心域：coordinates、card、fusion、core、night、battle
├── ports/      # 端口：IBattleReportRepository、BattleReportModels
└── adapters/   # 适配层：godot/（全部 Godot 节点和场景脚本）、persistence/
```

依赖方向：`domain` → `ports` ← `adapters`（适配器实现端口，调用领域）。

---

## §2 真模块化与模块内防腐

### 2.1 当前问题

`scripts/adapters/godot/` 中各个模块通过静态 `Instance` 互相耦合：

| 调用方 | 被调方 | 耦合方式 |
|--------|--------|----------|
| `Building.cs` | `GameManager.Instance` | 静态单例 |
| `Building.cs` | `ShopSystem.Instance` | 静态单例 |
| `Soldier.cs` | `GameManager.Instance` | 静态单例 |
| `Soldier.cs` | `NightSystem.Instance` | 静态单例 |
| `ShopSystem.cs` | `CardSystem.Instance` | 静态单例 |
| `ShopSystem.cs` | `GameManager.Instance` | 静态单例 |
| `CardSystem.cs` | `BuildingSystem.Instance` | 静态单例 |
| `FusionSystem.cs` | `AdjacentSystem.Instance` | 静态单例 |
| `ReplayAiSystem.cs` | `BattleReportSystem.Instance` | 静态单例 |
| `ReplayAiSystem.cs` | `AdjacentSystem.Instance` | 静态单例 |
| `NightSystem.cs` | `GameManager.Instance` | 静态单例 |
| `TitleScreen.cs` | `BattleReportStorage.Instance` | 静态单例 |
| UI 控制器 | `ShopSystem.Instance`、`CardSystem.Instance` 等 | 静态单例 |

**结果：** 任何模块改动可能波及所有模块，无法独立测试任一模块。

### 2.2 目标

- [ ] 每个业务模块定义自己的**对外端口**（接口），放在 `scripts/ports/`
- [ ] 模块内部通过**注入的端口**调用其他模块，不再直调静态 `Instance`
- [ ] 禁止跨模块直接访问 Godot 节点、静态 `Instance` 或对方内部类
- [ ] 依赖方向：`domain` → `ports` ← `adapters`，模块间仅经端口通信

---

## 首批模块接口化（按影响面从小到大）

### Module A: NightSystem

当前：`NightSystem.CanUnitWork(hasNightCombat)` 是静态方法，委托给 `GameManager.Instance`。

- [ ] 提取 `IGamePhase` 端口 → `scripts/ports/IGamePhase.cs`
  - `bool IsDay { get; }` / `bool CanUnitWork(bool hasNightCombat)`
- [ ] `GameManager` 实现 `IGamePhase`
- [ ] `NightSystem` 改为接收 `IGamePhase`，消除对 `GameManager.Instance` 的直接依赖
- [ ] `Building` 和 `Soldier` 改用注入的 `IGamePhase` 而非 `NightSystem.Instance`

### Module B: ShopSystem

当前：`ShopSystem` 管理金币和商品，通过 `Instance` 被 UI 和 `Building` 访问。

- [ ] 提取 `IShopService` 端口 → `scripts/ports/IShopService.cs`
  - `int Gold { get; }` / `bool CanAfford(int)` / `bool TryPurchase(int)` / `event GoldChanged`
- [ ] `ShopSystem` 实现 `IShopService`
- [ ] `ShopUiController`、`Building` 改用注入的 `IShopService`
- [ ] 提取 `IShopRepository` 端口（商品目录查询），消除对 `CardData` 数组的直接依赖

### Module C: CardSystem

当前：`CardSystem` 管理手牌，通过 `Instance` 被 `ShopSystem` 和 UI 访问。

- [ ] 提取 `ICardHand` 端口 → `scripts/ports/ICardHand.cs`
  - `IReadOnlyList<CardData> Hand { get; }` / `bool TryAddCard(CardData)` / `bool TryPlaceCard(...)` / `event HandChanged`
- [ ] `CardSystem` 实现 `ICardHand`
- [ ] `ShopSystem`、`HandUiController` 改用注入的 `ICardHand`

### Module D: BuildingSystem

当前：`BuildingSystem` 管理建筑类型注册表和创建工厂，通过 `Instance` 被几乎所有模块访问。

- [ ] 提取 `IBuildingRegistry` 端口 → `scripts/ports/IBuildingRegistry.cs`
  - `IReadOnlyList<Vector2I> GetFootprint(string)` / `string GetDisplayName(string)` / `int GetMaxHealth(string)` / `float GetSpawnInterval(string)` / `bool IsCoreBuilding(string)` / 等
- [ ] 提取 `IBuildingFactory` 端口 → `scripts/ports/IBuildingFactory.cs`
  - `Building CreateBuilding(string typeId)` / `void ApplyVisual(Building)`
- [ ] `BuildingSystem` 实现两个端口
- [ ] 消除 `BuildingSystem.Instance` 静态引用

### Module E: GameManager

当前：`GameManager` 是 Godot Autoload，承担了过多职责（状态、阶段、血量、作弊键）。

- [ ] 提取 `IGameState` 端口（已在 ports/ 目录规划中）
  - `GameState CurrentState { get; }` / `GamePhase CurrentPhase { get; }` / `int PlayerHealth { get; }` / `int EnemyHealth { get; }`
  - `event PhaseChanged` / `event HealthChanged` / `event GameOver`
- [ ] 提取 `IGameController` 端口
  - `void AdvancePhase()` / `void TakeDamage(bool isPlayer, int amount)` / `void EndGame(bool playerWon)`
- [ ] `GameManager` 实现两个端口，保留 Godot 信号发射（桥接到端口事件）

### Module F: AdjacentSystem

当前：邻接计算和视觉特效混在一起。

- [ ] 将邻接算法提取到 `scripts/domain/building/AdjacentRules.cs`（已在 §1 规划）
- [ ] `AdjacentSystem` 改为 adapter，委托到 `AdjacentRules`
- [ ] 定义 `IAdjacencyService` 端口

### Module G: FusionSystem / BattleReportSystem / ReplayAiSystem

这些模块已在 §1 中部分提取（模型已在 domain/ports/，存储已接口化）。

- [ ] `FusionSystem` 配方匹配逻辑 → `domain/fusion/FusionRules.cs`
- [ ] `FusionSystem` adapter 改为委托到 `FusionRules`
- [ ] `BattleReportSystem` 快照构建 → `domain/battle_report/ReportBuilder.cs`
- [ ] `ReplayAiSystem` 镜像坐标 → `domain/replay/MirrorRules.cs`

---

## 暂不进入范围（本任务）

- 显示与业务缩放拆分（`todo.md` §3）
- 开发者模式开关（`todo.md` §4）
- `Castle._Draw`、`Building` 视觉特效等纯渲染代码从 shim 迁移（低优先级）
- DI 容器引入（当前用 Godot 场景树手动注入即可）

---

## 建议实施顺序

1. **NightSystem + IGamePhase** — 最小改动，验证端口模式
2. **ShopSystem + IShopService** — 纯数据端口，无 Godot 依赖
3. **CardSystem + ICardHand** — 同上
4. **BuildingSystem 拆分** — 最大改动，分两次：先 IBuildingRegistry，再 IBuildingFactory
5. **GameManager 拆分** — 核心改动，影响全局
6. **Fusion / BattleReport / Replay** — 算法提取到 domain
7. **AdjacentSystem** — 收尾

每次改动后验证：`dotnet build` + `dotnet test`。
