# 当前任务

**当前焦点：六边形架构分层（`todo.md` §1）。**

目标：把游戏规则从 Godot 节点与静态单例中剥离，建立**游戏核心域 / 防腐层 / 外部依赖**三层边界，为后续真模块化（`todo.md` §2）与显示缩放重构（`todo.md` §3）打基础。

参考：`todo.md`、`codeStructure.md`、`outline/c05ProjectArchitecture.md`；已有试点代码 `scripts/core/GameCoordinates.cs`、`scripts/battle/UnitSpawn.cs`（仍混在引擎目录，待迁入新结构）。

---

## 1. 边界与目录规划

- [ ] 在 `devPlan/codeStructure.md` 与 `outline/c05ProjectArchitecture.md` 落地目标目录：

```
scripts/
├── domain/                 # 游戏核心域：纯 C#，无 Godot、无 Node
│   ├── coordinates/        # 游戏坐标、占地、产兵点等
│   ├── building/           # 占地、放置规则、产兵间隔等领域模型
│   └── …                   # 按业务逐步迁入
├── ports/                  # 端口（接口）：核心域对外契约、外部依赖抽象
└── adapters/               # 防腐层 + 外部依赖实现
    ├── godot/              # Godot 节点、场景、输入、渲染适配
    └── persistence/        # 文件 / 未来数据库（如 battle_reports）
```

- [ ] 约定依赖方向：`domain` 不引用 `adapters`；`adapters` 实现 `ports` 并调用 `domain`；现有 `scripts/building/` 等逐步变为适配层或拆为 domain + adapter。
- [ ] 列出**首批迁入核心域**的类型清单（见 §2），其余模块暂留原位、经防腐层调用。

---

## 2. 核心域试点：坐标与产兵

以已验收的产兵坐标为第一个垂直切片，验证「核心域可单测、适配层只翻译」。

- [ ] 将 `GameCoordinates` 中**整数游戏坐标**与**产兵点计算**迁入 `domain/coordinates/`（无 `Godot.Vector2`）。
- [ ] 核心域 API（命名实施时可调整）：
  - `GameVector2`（int X, Y）、`UnitsPerCell`
  - `GetBuildingFootprintSpawnPoint(footprint, anchor, spawnIndex)` — 占地框左下角、连续产兵错开
- [ ] `adapters/godot/GameCoordinatesAdapter` 负责 `ToLocalPixels` / `FromLocalPixels` / `FloorGridFromLocalPixels`。
- [ ] `UnitSpawn` 保留在适配层：先入树再设 `GlobalPosition`；核心域只输出游戏坐标。
- [ ] 为 `GetBuildingFootprintSpawnPoint` 编写**不启动 Godot** 的单元测试。

---

## 3. 防腐层模式与现有代码迁移

- [ ] 定义防腐层职责：类型翻译、坐标换算、生命周期（`Node` 创建/销毁）、信号 ↔ 领域事件。
- [ ] `Castle` / `Building`：绘制与预览可暂留 Godot 侧；产兵、占地判定、格占用等**规则调用**改走 `domain` + 适配器。
- [ ] `BuildingSystem` 定义表：区分**领域数据**（间隔、占地、生命）与**表现数据**（纹理、缩放）；表现留在适配层或 `ports` 的 `IBuildingVisuals` 之后实现。
- [ ] 禁止新增：业务模块直接 `GD.Load`、直接读 `GameManager.Instance` 完成领域判定（迁移期旧代码可保留，新代码走端口）。

---

## 4. 外部依赖归类

| 依赖 | 归属 | 当前示例 |
| --- | --- | --- |
| Godot 场景树、节点、绘制 | `adapters/godot` | `Castle._Draw`、`Building` 工作特效 |
| 输入与 UI | `adapters/godot` | `HandUiController`、设置面板 |
| 文件持久化 | `adapters/persistence` | `BattleReportStorage`、`display_settings.cfg` |
| 未来数据库 | `adapters/persistence` | 暂未实现 |

- [ ] `BattleReportSystem` 录制/加载：抽出 `IBattleReportRepository` 端口，文件实现放 `persistence`（可与坐标试点并行或作第二批）。
- [ ] `DisplaySettingsManager` 标为 Godot 窗口适配，不进入 `domain`。

---

## 5. 文档与 AGENTS 同步

- [ ] 更新 `codeStructure.md`：新目录、依赖规则、试点说明。
- [ ] 更新 `outline/c05ProjectArchitecture.md` 为「当前 + 迁移目标」一致表述。
- [ ] `AGENTS.md` 目录表增加 `domain/`、`ports/`、`adapters/` 说明。

---

## 6. 验收标准

- [ ] `domain/coordinates` 产兵点与占地左下角规则有单元测试，测试不引用 Godot。
- [ ] 运行时产兵位置与已验收行为一致（建筑占地框左下角、多格建筑正确）。
- [ ] 依赖检查：`domain` 内无 `using Godot`。
- [ ] 文档与目录一致，后续可基于本分层推进 `todo.md` §2、§3。

---

## 暂不进入范围（本任务）

- 全量模块接口化（`todo.md` §2）
- 显示与业务缩放拆分（`todo.md` §3）
- 开发者模式开关（`todo.md` §4）
- 删除或大规模重写现有 `scripts/building/`、`GameManager`（仅试点迁移 + 模式确立）

---

## 建议实施顺序

1. 目录骨架 + `ports` 空接口 + 文档  
2. 迁入 `GameCoordinates` 领域部分 + 适配器 + 单测  
3. `UnitSpawn` / `Building` 产兵改调新 API  
4. `BattleReportStorage` 端口化（可选第二批）  
5. 验收 + 更新架构文档
