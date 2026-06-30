# 当前任务

**B10: Soldier/BattleManager 架构重构 — 等待方案**

---

## B10: Soldier 行为 DDD 重构

### 拆分原则

- **聚合**：单个 Soldier 自己做得了的行为放聚合上
- **领域服务**：跨聚合操作（空间查询、推开）放领域服务
- **port**：聚合和领域服务通过 port 访问坐标/寻路

### 施工步骤

| 步骤 | 位置 | 内容 |
|------|------|------|
| 1 | `ports/` | `IPositionAccessor`（读写游戏坐标）、`IPathAccessor`（寻路下一位置） |
| 2 | `domain/Battle/` | `Soldier`：状态机、寻敌、攻击、受伤，全部用游戏坐标 |
| 3 | `domain/Battle/` | `UnitSpatialService`：网格空间查询（找最近敌人、找重叠建筑、推开） |
| 4 | `adapter/` | `GodotPositionAccessor` 实现 `IPositionAccessor`（GlobalPosition） |
| 5 | `adapter/` | `GodotPathAccessor` 实现 `IPathAccessor`（NavigationAgent2D） |
| 6 | `adapter/` | `Soldier.cs` 瘦身：薄壳，持有 domain `Soldier`，`_Process` 委托 |
| 7 | `adapter/` | `BattleManager.cs` 瘦身：委托给 `UnitSpatialService` |
