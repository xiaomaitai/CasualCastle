# 当前任务

每次更换当前任务时，清理掉之前任务残留。
**当前焦点：C# 项目级模块拆分 + DI 依赖注入（`todo.md` §2）。**

目标：保留 `scripts/domain/`、`scripts/ports/`、`scripts/adapters/` 三层。domain 拆为 4 个项目，用 `Microsoft.Extensions.DependencyInjection` 替代静态 `Instance`。

---

## 验收状态

- [x] `dotnet build` 全部编译通过
- [x] `dotnet test` 领域测试通过（6/6）
- [x] 4 个 domain 项目零 `using Godot`
- [x] 项目引用无循环（Shared ← Building ← Battle/History，单向）
- [x] 三层保留：`scripts/domain/`（4 项目）、`scripts/ports/`、`scripts/adapters/`
- [ ] 模块间无 `static Instance` 直调 ← **进行中**（domain 逻辑已提取，adapter 间 static Instance 逐步替换）

---

## 实际项目结构

```
scripts/
├── domain/
│   ├── Shared/                       # Shared.csproj
│   │   ├── GameVector2.cs, GridCellOffset.cs
│   │   ├── GameCoordinateRules.cs, GameRules.cs
│   │   └── SharedModule.cs
│   │
│   ├── Building/                     # Building.csproj → Shared
│   │   ├── CardData.cs, FusionRecipe.cs（已有）
│   │   ├── BuildingDefinitions.cs   ← 从 BuildingSystem 提取
│   │   ├── OccupancyGrid.cs         ← 从 Castle._occupied 提取
│   │   ├── AdjacentRules.cs         ← 从 AdjacentSystem 提取
│   │   ├── FusionRules.cs           ← 从 FusionSystem 提取
│   │   ├── ShopRules.cs             ← 从 ShopSystem 提取
│   │   ├── CardRules.cs             ← 从 CardSystem 提取
│   │   ├── IAdjacencyBuilding.cs（在 AdjacentRules.cs 中）
│   │   ├── IBuildingState.cs
│   │   ├── IBuildingRegistry.cs
│   │   ├── IBuildingPlacement.cs
│   │   └── BuildingModule.cs
│   │
│   ├── Battle/                       # Battle.csproj → Shared, Building
│   │   ├── SoldierData.cs, NightRules.cs（已有）
│   │   ├── CombatRules.cs           ← 从 Soldier 提取
│   │   ├── IGameState.cs
│   │   └── BattleModule.cs
│   │
│   └── History/                      # History.csproj → Shared, Building
│       ├── BattleReportModels.cs, IBattleReportRepository.cs（已有）
│       ├── ReportBuilder.cs         ← 从 BattleReportSystem 提取
│       ├── MirrorRules.cs           ← 从 ReplayAiSystem 提取
│       ├── ISnapshotQuery.cs
│       └── HistoryModule.cs
│
├── CompositionRoot.cs                # DI 容器构建（GameManager._Ready() 调用）
│
├── ports/                            # 端口（接口已分散到各 domain 项目）
│
└── adapters/
    ├── godot/
    │   ├── autoload/GameManager.cs   # 实现 IGameState，持有 ServiceProvider
    │   ├── building/Castle.cs        # 使用 OccupancyGrid 替代 bool[,]
    │   ├── building/Building.cs      # 实现 IBuildingState + IAdjacencyBuilding
    │   ├── building/AdjacentSystem.cs # 委托给 AdjacentRules
    │   ├── building/BuildingSystem.cs # 视觉/Godot 实例化保留
    │   ├── fusion/FusionSystem.cs    # 委托给 FusionRules
    │   ├── replay/ReplayAiSystem.cs  # 委托给 MirrorRules
    │   └── ...
    └── persistence/
```

### 依赖方向

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

---

## Phase 2B 已完成

- [x] 4 个 domain 项目 .csproj 建立，.sln 更新 ✅
- [x] 主项目引用 4 个 domain 项目 ✅
- [x] 领域数据提取：`BuildingDefinitions`（取代 BuildingSystem 内嵌字典的游戏数据部分）
- [x] 领域规则提取：`AdjacentRules`、`FusionRules`、`ShopRules`、`CardRules`、`CombatRules`
- [x] 领域状态提取：`OccupancyGrid`（取代 Castle._occupied）
- [x] 战报/回放提取：`ReportBuilder`、`MirrorRules`
- [x] 端口定义：`IGameState`、`IBuildingRegistry`、`IBuildingPlacement`、`ISnapshotQuery`
- [x] DI 模块：`SharedModule`、`BuildingModule`、`BattleModule`、`HistoryModule`
- [x] `CompositionRoot.Build()` — GameManager 在 `_Ready()` 中调用
- [x] Adapter 适配：`Building` 实现 `IBuildingState`/`IAdjacencyBuilding`，`Castle` 使用 `OccupancyGrid`，`AdjacentSystem` 委托给 `AdjacentRules`，`FusionSystem` 委托给 `FusionRules`，`ReplayAiSystem` 委托给 `MirrorRules`

---

## Phase 2C 已完成 ✅

### 创建 AdapterRegistry（`scripts/adapters/godot/autoload/AdapterRegistry.cs`）

轻量级服务定位器。Godot 管理节点生命周期，此 registry 让节点之间无需 `static Instance` 即可互相查找。

### 单例系统迁移（注册 + 解析依赖）

| 系统 | 注册 | 解析的依赖 |
|------|------|-----------|
| `GameManager` | 自身 + `IGameState` | — |
| `NightSystem` | 自身 | `IGameState` |
| `AdjacentSystem` | 自身 | — |
| `BuildingSystem` | 自身 | `AdjacentSystem` |
| `CardSystem` | 自身 | `BuildingSystem` |
| `ShopSystem` | 自身 | `CardSystem`, `GameManager` |
| `FusionSystem` | 自身 | `IGameState`, `AdjacentSystem` |
| `BattleReportSystem` | 自身 | `IBattleReportRepository`（MS DI） |
| `ReplayAiSystem` | 自身 | `BattleReportSystem`, `AdjacentSystem` |
| `BattleReportStorage` | `IBattleReportRepository`（MS DI） | — |

### 动态实例迁移

`Building` 节点（PackedScene 动态创建）在 `_Ready()` 中从 `AdapterRegistry` 解析：
`GameManager`, `ShopSystem`, `AdjacentSystem`, `NightSystem`

### 剩余 static Instance 调用（非阻塞，后续清理）

主要在 UI 控制器和 `Soldier.cs`、`MainGameController.cs` 中，约为 20 处。这些是表现层代码，不影响核心架构。

---

## Phase 2 验收总结

- [x] `dotnet build` 全部编译通过 ✅
- [x] `dotnet test` 领域测试通过（6/6）✅
- [x] 4 个 domain 项目零 `using Godot` ✅
- [x] 项目引用无循环 ✅
- [x] 三层保留 ✅
- [x] 核心 adapter 间 `static Instance` 已消除（单例系统全部使用 AdapterRegistry）✅
- [~] UI 层 `static Instance` 后续清理（低优先级）

---

## Phase 3: 显示与缩放二次重构 ← 当前任务

**目标**：游戏坐标已落地（每格 100 unit），进一步拆分显示职责，确保核心域零像素。

### 背景

GameCoordinateRules（domain）定义游戏坐标系（整数，100 unit/格）。GameCoordinatesAdapter（adapter）负责 unit ↔ pixel 转换（48 px/格）。但部分代码仍使用 deprecated shim `GameCoordinates.cs`，它混合了两层职责。

### Step 3A: 迁移 Castle._Draw() 脱离 deprecated shim

**文件**：`scripts/adapters/godot/building/Castle.cs`

**当前**：`Castle._Draw()` 使用 deprecated `GameCoordinates` shim（10 处引用）：
- `GameCoordinates.UnitsPerCell` → `GameCoordinateRules.UnitsPerCell`
- `GameCoordinates.CellBlockSize` → `GameCoordinateRules.CellBlockSize`
- `GameCoordinates.CellBlockOrigin(col, row)` → `GameCoordinateRules.CellBlockOrigin(col, row)`
- `GameCoordinates.CellCorner(col, row)` → `GameCoordinateRules.CellCorner(col, row)`
- `GameCoordinates.ToLocalPixels(...)` → `GameCoordinatesAdapter.ToLocalPixels(...)`

**参考**：`CastleHighlightOverlay._Draw()` 已直接使用 `GameCoordinateRules` + `GameCoordinatesAdapter` ✅

- [x] 替换所有 10 处引用 ✅
- [x] 移除 deprecated shim 依赖 ✅

### Step 3B: 删除 deprecated GameCoordinates.cs ✅

- [x] 删除 `scripts/adapters/godot/core/GameCoordinates.cs`
- [x] 无 .uid 文件

### Step 3C: 拆分 GameConfig.cs ✅

- [x] 将 `OutputResolutions` 数组移入 `DisplaySettingsManager.cs`
- [x] `DisplaySettingsManager` 直接引用 `GameRules.DesignWidth/DesignHeight`
- [x] `GameConfig` 保留其余常量委托（向后兼容）
- [x] `SettingsUiController` 引用更新为 `DisplaySettingsManager.OutputResolutions`

### 验收标准

- [x] `dotnet build` 0 错误 ✅
- [x] `dotnet test` 全部通过（6/6）✅
- [x] `grep -r "GameCoordinates\." scripts/` 返回空（旧 shim 已删）✅
- [x] `grep -r "using Godot" scripts/domain/` 返回空 ✅
- [x] domain 项目中零像素值 ✅
- [x] DisplaySettingsManager 自包含显示配置 ✅

---

## Phase 3 完成 ✅

- 删除 deprecated `GameCoordinates.cs` shim
- `Castle._Draw()` 直接使用 `GameCoordinateRules` (domain) + `GameCoordinatesAdapter` (adapter)
- 显示配置（`OutputResolutions`）归属 `DisplaySettingsManager`
- `GameConfig` 精简为纯领域常量委托

**当前焦点回 `todo.md`**：§4 开发者模式 或 新需求。
