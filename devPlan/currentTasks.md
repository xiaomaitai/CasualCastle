# 当前任务

**当前焦点：C# 原生模块化 + 依赖注入（`todo.md` §2）。**

目标：用 C# 项目拆分和 `Microsoft.Extensions.DependencyInjection` 替代静态 `Instance` 单例，让每个模块成为可独立编译、可替换的单元。

参考：`todo.md`、`codeStructure.md`、`outline/c05ProjectArchitecture.md`。

---

## 背景

§1 已把 `scripts/` 整理为 domain/ports/adapters 三层，但所有模块仍编译在同一个 Godot 项目中，通过静态 `Instance` 互相耦合。

§2 要做的：**用 C# 项目边界 + DI 容器**真正切开这些模块。

---

## 目标项目结构

```
CasualCastle/
├── src/
│   ├── CasualCastle.Domain/           # 核心域类库（纯 C#，无 Godot）
│   │   ├── Coordinates/               # GameVector2、GameCoordinateRules
│   │   ├── Building/                  # BuildingRules、AdjacentRules、OccupancyGrid
│   │   ├── Battle/                    # SoldierData、CombatRules
│   │   ├── Fusion/                    # FusionRecipe、FusionRules
│   │   ├── Shop/                      # ShopRules、CardRules
│   │   ├── Night/                     # NightRules
│   │   ├── Report/                    # ReportBuilder
│   │   ├── Replay/                    # MirrorRules
│   │   └── Core/                      # GameRules（常量）
│   │
│   ├── CasualCastle.Game/             # Godot 主项目（场景、预制体、适配器）
│   │   ├── scripts/
│   │   │   ├── adapters/
│   │   │   │   ├── Building/          # Castle、Building、BuildingSystem（Godot 节点）
│   │   │   │   ├── Battle/            # Soldier、UnitSpawn
│   │   │   │   ├── Fusion/            # FusionSystem（Node）
│   │   │   │   ├── Shop/              # ShopSystem（Node）
│   │   │   │   ├── Card/              # CardSystem（Node）
│   │   │   │   ├── Night/             # NightSystem（Node）
│   │   │   │   ├── BattleReport/      # BattleReportSystem（Node）
│   │   │   │   ├── Replay/            # ReplayAiSystem（Node）
│   │   │   │   ├── UI/                # UIManager + 子控制器
│   │   │   │   ├── Flow/              # TitleScreen、MainGameController
│   │   │   │   ├── Autoload/          # GameManager（DI 容器宿主）
│   │   │   │   └── Persistence/       # BattleReportStorage
│   │   │   └── CompositionRoot.cs     # DI 注册入口
│   │   ├── scenes/
│   │   ├── prefabs/
│   │   ├── project.godot
│   │   └── CasualCastle.Game.csproj
│   │
│   └── CasualCastle.Tests/            # 单元测试（xunit）
│       └── CasualCastle.Tests.csproj
│
└── CasualCastle.sln
```

**依赖方向：** `Game` → `Domain`，`Tests` → `Domain`。`Domain` 不引用任何项目。

---

## Phase 2A: 项目拆分与 DI 基础设施

### Step 1: 建立项目结构

- [ ] 在 `src/` 下创建 `CasualCastle.Domain/` 类库项目（`<TargetFramework>net8.0</TargetFramework>`，不含 Godot SDK）
- [ ] 将现有 `scripts/domain/` 下所有文件移至 `src/CasualCastle.Domain/`，保持目录结构
- [ ] 将现有 `scripts/ports/` 下所有文件移至 `src/CasualCastle.Domain/Ports/`
- [ ] 创建 `src/CasualCastle.Game/`，将原 `CasualCastle.csproj` + `scripts/` + `scenes/` + `prefabs/` 迁入
- [ ] 更新 `CasualCastle.Game.csproj`：添加 `ProjectReference` 到 `CasualCastle.Domain`
- [ ] 更新 `CasualCastle.sln`：三个项目（Domain、Game、Tests）
- [ ] 删除根目录的旧 `scripts/`、`scenes/`、`prefabs/`（已迁入 `src/CasualCastle.Game/`）

### Step 2: 引入 DI 容器

- [ ] `CasualCastle.Game.csproj` 添加 NuGet 包 `Microsoft.Extensions.DependencyInjection`
- [ ] 创建 `src/CasualCastle.Game/CompositionRoot.cs`：
  ```csharp
  public static class CompositionRoot
  {
      public static ServiceProvider Build(GameManager gameManager)
      {
          var services = new ServiceCollection();
          
          // Domain services
          services.AddSingleton<ShopRules>();
          services.AddSingleton<CardRules>();
          services.AddSingleton<FusionRules>();
          services.AddSingleton<NightRules>();
          services.AddSingleton<BuildingRules>();
          services.AddSingleton<AdjacentRules>();
          services.AddSingleton<CombatRules>();
          
          // Adapter services (implement domain ports)
          services.AddSingleton<IShopService>(sp => ShopSystem.Instance);
          services.AddSingleton<ICardHand>(sp => CardSystem.Instance);
          services.AddSingleton<IBuildingRegistry>(sp => BuildingSystem.Instance);
          services.AddSingleton<IGameState>(sp => gameManager);
          services.AddSingleton<IPhaseController>(sp => gameManager);
          services.AddSingleton<IBattleReportRepository>(sp => BattleReportStorage.Instance);
          
          // UI controllers
          services.AddSingleton<UIManager>();
          // ... etc
          
          return services.BuildServiceProvider();
      }
  }
  ```
- [ ] `GameManager._Ready()` 中调用 `CompositionRoot.Build(this)`，将 `ServiceProvider` 存入静态属性供 Godot 节点获取

### Step 3: Godot 节点获取依赖的约定

由于 Godot 节点由引擎创建（场景/预制体实例化），不能用构造函数注入。约定：在 `_Ready()` 中通过 `GameManager.Services.GetRequiredService<T>()` 获取依赖，缓存到私有字段。

```csharp
public partial class Building : Area2D
{
    private IGameState _gameState;
    private IShopService _shopService;
    
    public override void _Ready()
    {
        _gameState = GameManager.Services.GetRequiredService<IGameState>();
        _shopService = GameManager.Services.GetRequiredService<IShopService>();
    }
}
```

---

## Phase 2B: 提取领域规则到 Domain 项目

### Module 1: 坐标与占地（已有，仅迁移位置）

- [ ] 将 `domain/coordinates/` 完整移至 `CasualCastle.Domain/Coordinates/`

### Module 2: 建筑规则 → `CasualCastle.Domain/Building/`

- [ ] 从 `BuildingSystem.cs` 提取 `BuildingDefinition` 为纯 C# 类型（去掉 Godot 类型字段：`Vector2I` → `GridCellOffset`，`Color` → `string hex`）
- [ ] 定义 `IBuildingRegistry` 端口：
  - `IReadOnlyList<GridCellOffset> GetFootprint(string typeId)`
  - `int GetMaxHealth(string typeId)`
  - `float GetSpawnInterval(string typeId)`
  - `bool IsCoreBuilding(string typeId)`
  - `bool HasNightCombat(string typeId)`
  - `int GetFusionTier(string typeId)`
- [ ] 从 `Castle.cs` 提取占地网格逻辑 → `OccupancyGrid` 领域类
- [ ] 定义 `IOccupancyGrid` 端口

### Module 3: 邻接规则 → `CasualCastle.Domain/Building/`

- [ ] 从 `AdjacentSystem.cs` 提取纯算法到 `AdjacentRules`：
  - 建筑邻接判定
  - 同类型计数
  - 工作速度加成计算
- [ ] 定义 `IAdjacencyService` 端口

### Module 4: 战斗规则 → `CasualCastle.Domain/Battle/`

- [ ] `SoldierData` 已在 domain/battle/
- [ ] 提取 `CombatRules`：攻击判定、伤害计算、死亡判定

### Module 5: 融合规则 → `CasualCastle.Domain/Fusion/`

- [ ] `FusionRecipe` 已在 domain/fusion/
- [ ] 从 `FusionSystem.cs` 提取 `FusionRules`：
  - 配方匹配
  - 材料筛选
  - 融合可行性验证

### Module 6: 商店与手牌 → `CasualCastle.Domain/Shop/`

- [ ] `CardData` 已在 domain/card/ → 移至 `Domain/Shop/`
- [ ] 从 `ShopSystem.cs` 提取 `ShopRules`：购买、金币管理、商品刷新
- [ ] 从 `CardSystem.cs` 提取 `CardRules`：手牌容量、选中/取消、打出
- [ ] 定义 `IShopService`、`ICardHand` 端口

### Module 7: 昼夜 → `CasualCastle.Domain/Night/`

- [ ] `NightRules` 已在 domain/night/

### Module 8: 战报与回放 → `CasualCastle.Domain/Report/` + `CasualCastle.Domain/Replay/`

- [ ] `BattleReportModels` → `Domain/Report/Models.cs`
- [ ] `IBattleReportRepository` → `Domain/Report/IBattleReportRepository.cs`
- [ ] 从 `ReplayAiSystem.cs` 提取 `MirrorRules`：坐标镜像计算

---

## Phase 2C: 端口接口定义（放在 Domain 项目）

`CasualCastle.Domain/Ports/` 下集中定义所有跨模块契约：

| 接口 | 所属模块 | 方法摘要 |
|------|----------|----------|
| `IBuildingRegistry` | Building | 查询建筑类型定义 |
| `IBuildingFactory` | Building | 创建建筑实例、应用外观 |
| `IOccupancyGrid` | Building | 占地查询、放置/释放 |
| `IAdjacencyService` | Building | 邻接检测、加成计算 |
| `IShopService` | Shop | 购买、金币、商品槽 |
| `ICardHand` | Shop | 手牌管理、打出 |
| `IGameState` | Core | 阶段、血量、胜负状态查询 |
| `IPhaseController` | Core | 阶段推进、暂停 |
| `IBattleReportRepository` | Report | 战报存取 |
| `ISoldierSpawner` | Battle | 创建并放置士兵 |

---

## Phase 2D: Godot 适配器实现端口

在 `CasualCastle.Game` 中，每个现有 Godot 节点类改为实现对应端口：

| 类 | 实现的端口 |
|----|-----------|
| `BuildingSystem` | `IBuildingRegistry`、`IBuildingFactory` |
| `Castle` | `IOccupancyGrid` |
| `AdjacentSystem` | `IAdjacencyService` |
| `ShopSystem` | `IShopService` |
| `CardSystem` | `ICardHand` |
| `GameManager` | `IGameState`、`IPhaseController` |
| `BattleReportStorage` | `IBattleReportRepository` |
| `UnitSpawn` | `ISoldierSpawner` |

---

## Phase 2E: 消除静态 Instance

每个模块逐步消除 `public static Xxx Instance`：

1. 在 `CompositionRoot` 中注册实现类
2. 调用方通过 `GameManager.Services.GetRequiredService<T>()` 获取
3. 确认无外部引用后删除静态 `Instance`

---

## 验收标准

- [ ] `dotnet build` 所有项目通过
- [ ] `dotnet test` 领域测试通过
- [ ] 依赖方向正确：`Domain` 无 `using Godot`，`Game` 引用 `Domain`
- [ ] 无模块通过 `static Instance` 跨模块直接调用（通过端口 + DI）
- [ ] 游戏运行时行为不变

---

## 暂不进入范围

- 显示与业务缩放拆分（`todo.md` §3）
- 开发者模式（`todo.md` §4）
- UI 层的 DI 化（UI 控制器之间仍可用简单引用）
