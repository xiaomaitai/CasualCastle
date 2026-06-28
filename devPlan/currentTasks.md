# 当前任务

**当前焦点：开发者模式已完成，待定下一个任务（见 `todo.md`）。**

---

## 验收状态

- [x] `dotnet build` 全部编译通过
- [x] `dotnet test` 领域测试通过（6/6）
- [x] 4 个 domain 项目零 `using Godot`
- [x] 项目引用无循环（Shared ← Building ← Battle/History，单向）
- [x] 三层保留：`scripts/domain/`（4 项目）、`scripts/ports/`、`scripts/adapters/`
- [x] 核心 adapter 间 `static Instance` 已消除（单例系统全部使用 AdapterRegistry）
- [x] UI 层 `static Instance` 全部替换为 AdapterRegistry（72 处 → 0）

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
│   │   ├── CardData.cs, FusionRecipe.cs
│   │   ├── BuildingDefinitions.cs, OccupancyGrid.cs
│   │   ├── AdjacentRules.cs, FusionRules.cs
│   │   ├── ShopRules.cs, CardRules.cs
│   │   ├── IBuildingState.cs, IBuildingRegistry.cs, IBuildingPlacement.cs
│   │   └── BuildingModule.cs
│   │
│   ├── Battle/                       # Battle.csproj → Shared, Building
│   │   ├── SoldierData.cs, NightRules.cs, CombatRules.cs
│   │   ├── IGameState.cs
│   │   └── BattleModule.cs
│   │
│   └── History/                      # History.csproj → Shared, Building
│       ├── BattleReportModels.cs, IBattleReportRepository.cs
│       ├── ReportBuilder.cs, MirrorRules.cs
│       ├── ISnapshotQuery.cs
│       └── HistoryModule.cs
│
├── CompositionRoot.cs                # MS DI 容器构建
│
└── adapters/
    ├── godot/
    │   ├── autoload/
    │   │   ├── GameManager.cs        # 实现 IGameState，持有 ServiceProvider
    │   │   ├── DisplaySettingsManager.cs  # 分辨率/窗口 + DevModeEnabled
    │   │   └── AdapterRegistry.cs    # Godot 节点服务定位器
    │   ├── building/  battle/  shop/  card/  night/  fusion/
    │   ├── battle_report/  replay/  ui/  flow/  dev/  audio/
    │   └── core/   # GameConfig, GameCoordinatesAdapter
    └── persistence/  # BattleReportStorage
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

## Phase 2: 模块化 + DI ✅

Domain 拆为 4 个 C# 项目，MS DI + AdapterRegistry 双层注入：

- **2A**: 4 个 domain .csproj 建立，领域逻辑提取到 domain 层
- **2B**: 端口定义（IGameState 等），DI 模块，CompositionRoot
- **2C**: AdapterRegistry 建立，单例系统全迁移，消除核心 adapter 间 static Instance
- **2 追加**: `GameManager.Get<T>()` 简化 MS DI 解析

---

## Phase 3: 显示与缩放二次重构 ✅

- 删除 deprecated `GameCoordinates.cs` shim
- `Castle._Draw()` 直接使用 `GameCoordinateRules` (domain) + `GameCoordinatesAdapter` (adapter)
- 显示配置（`OutputResolutions`）归属 `DisplaySettingsManager`
- `GameConfig` 精简为纯领域常量委托
- domain 项目零像素值

---

## Phase 4: 开发者模式 ✅

- `DisplaySettingsManager.DevModeEnabled` — 全局开关（默认 false）
- 设置面板增加 CheckBox
- `GameManager` P 键作弊产兵 → DevModeEnabled 门控
- `DevInputLogger` 按键日志 → DevModeEnabled 门控

---

## Phase 5: UI static Instance 全面清理 ✅

- `scripts/` 下所有 `.Instance` 直调（72 处）替换为 `AdapterRegistry.Resolve<T>()` 或 `GameManager.Get<T>()`
- `DisplaySettingsManager` 注册到 AdapterRegistry
- `grep -r "\.Instance" scripts/` 返回空

---

**下一步：`todo.md` 中无新条目，等待新需求。**
