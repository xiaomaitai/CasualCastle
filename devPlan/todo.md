# 架构待办（TODO）

当前任务详见 `currentTasks.md`。这里是长期架构与技术债 backlog。

---
## B10: Soldier 行为 DDD 重构

### 当前问题

- adapter 直接持有 `_domain` 聚合引用，绕过了入站 port
- `UnitRegistry.LoadFrom` 是 adapter 推数据入 domain，应该 domain 定义出站 port 让 adapter 实现
- domain 没有 Repository 接口

### 目标

- 入站 port：domain 定义 `ISoldierService`，adapter 不直接碰聚合
- 出站 port：domain 定义 `IUnitRepository`/`IBuildingRepository`，adapter 实现
