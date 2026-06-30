# 架构待办（TODO）

当前任务详见 `currentTasks.md`。这里是长期架构与技术债 backlog。

---
## B10: Soldier 行为 DDD 重构

### 拆分

- **聚合** `Soldier`（domain）：状态机、寻敌、攻击、受伤（游戏坐标）
- **领域服务** `UnitSpatialService`：空间查询、推开
- **port** `IPositionAccessor` / `IPathAccessor`：聚合通过 port 访问坐标和寻路
- **adapter** 实现 port：`GodotPositionAccessor` / `GodotPathAccessor`
