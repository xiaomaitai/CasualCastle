# 当前任务

**B10: Soldier/BattleManager 架构重构 — 部分完成，剩余 1f + 3a/3b**

---

## B10: Soldier 行为 DDD 重构

### 已完成

- domain `Soldier` 聚合：纯数据+行为，GameX/GameY，无 port 注入
- `UnitSpatialService`：推开逻辑（无状态静态方法）
- adapter `Soldier.cs` 薄壳：_Process 委托 domain
- `ISoldierService` 入站 Port 改为行为方法（MoveTo/ApplyPush/SetBuildingTarget/ClearBuildingTarget），不再暴露 setter
- `IFieldUnitRepository` 出站 Port：运行时单位/建筑追踪，替代 UnitSpatialService 的状态
- `FieldUnitRepository` adapter 实现：统一管理活跃实体列表和空间查询
- `IBuildingRef` 出站 Port：建筑空间查询，typed 替代 object 泄露
- BattleManager 瘦身：删除重复列表/网格/索敌逻辑，只保留 PushSoldiers 调用
- DI 简化：AdjacencyService/BattleReportService/ReplayService 迁入 MS DI
- 重命名：HandService→Hand, ShopService→Shop

### 剩余问题

- `UnitRegistry.LoadFrom` 是 adapter 往 domain 推数据，应该 domain 通过出站 port 拉
- Soldier Area2D 挂着全部逻辑脚本，应该是空壳，逻辑在 Logic 子节点

### 剩余步骤

**1（剩余）. 出站 port（Repository）**

| 步骤 | 文件 | 内容 | 状态 |
|------|------|------|------|
| 1a | `domain/Battle/IUnitRepository.cs` | domain 定义接口 | ✅ |
| 1b | `domain/Building/IBuildingRepository.cs` | domain 定义接口 | ✅ |
| 1c | `adapter/persistence/SqliteUnitRepository.cs` | 实现 | ✅ |
| 1d | `adapter/persistence/SqliteBuildingRepository.cs` | 实现 | ✅ |
| 1e | `CompositionRoot.cs` | DI 注册 | ✅ |
| 1f | `UnitRegistry.cs` / `BuildingDefinitions.cs` | 删除静态字典 + `LoadFrom` | 待实施 |

**3. 节点结构**

| 步骤 | 文件 | 内容 | 状态 |
|------|------|------|------|
| 3a | `prefabs/Soldier.tscn` | Area2D 去脚本，变纯容器 | 待实施 |
| 3b | `adapter/SoldierLogic.cs` | 逻辑移入，DI 在 Logic 节点上做 | 待实施 |
