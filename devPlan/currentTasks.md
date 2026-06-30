# 当前任务

**B10: Soldier/BattleManager 架构重构 — 待实施**

---

## B10: Soldier 行为 DDD 重构

### 已完成

- domain `Soldier` 聚合：纯数据+行为，GameX/GameY，无 port 注入
- `UnitSpatialService`：推开、寻敌、建筑检测、反击传播
- adapter `Soldier.cs` 薄壳：_Process 委托 domain
- BattleManager 推开迁入 domain

### 问题

- adapter 直接持有 `_domain` 聚合根引用，绕过了 port
- `UnitRegistry.LoadFrom` 是 adapter 往 domain 推数据，应该是 domain 通过出站 port 拉
- domain 没有定义入站 port（adapter 直接调聚合方法）
- Soldier Area2D 挂着全部逻辑脚本，应该是空壳，逻辑在 Logic 子节点

### 施工步骤

**1. 出站 port（Repository）**

| 步骤 | 文件 | 内容 |
|------|------|------|
| 1a | `domain/Battle/IUnitRepository.cs` | domain 定义接口：`Get(string typeId) → UnitStats` |
| 1b | `domain/Building/IBuildingRepository.cs` | domain 定义接口：`Get(string typeId) → BuildingData` |
| 1c | `adapter/persistence/SqliteUnitRepository.cs` | 实现：SQLite 查询 → `UnitStats` |
| 1d | `adapter/persistence/SqliteBuildingRepository.cs` | 实现：SQLite 查询 → `BuildingData` |
| 1e | `CompositionRoot.cs` | DI 注册 repository |
| 1f | `UnitRegistry.cs` / `BuildingDefinitions.cs` | 删除静态字典 + `LoadFrom`，改为通过 repository 按需获取 |

**2. 入站 port（Service）**

| 步骤 | 文件 | 内容 |
|------|------|------|
| 2a | `domain/Battle/ISoldierService.cs` | 入站 port：`Create()`、`TakeDamage()`、`Update()` |
| 2b | `domain/Battle/SoldierService.cs` | 实现：操作聚合、调 `IUnitRepository`、调 `UnitSpatialService` |
| 2c | `adapter/Soldier.cs` | `_domain` 替换为 `ISoldierService` 引用 |

**3. 节点结构**

| 步骤 | 文件 | 内容 |
|------|------|------|
| 3a | `prefabs/Soldier.tscn` | Area2D 去脚本，变纯容器；Logic 子节点挂 `SoldierLogic.cs` |
| 3b | `adapter/SoldierLogic.cs` | 新建：原 `Soldier.cs` 逻辑移入，DI 在 Logic 节点上做 |
