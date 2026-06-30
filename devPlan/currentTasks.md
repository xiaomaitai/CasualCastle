# 当前任务

**B9: 移除物理碰撞，RTS 推开 — 进行中**

---

## B9: 移除物理碰撞，RTS 推开

> 设计依据：`devPlan/design/rtsPushApart.md`

### 目标

物理碰撞响应全关。碰撞体保留作为传感器（只发 AreaEntered/Exited 信号）。推开全部用代码温柔实现。

### 施工步骤

| 步骤 | 文件 | 内容 |
|------|------|------|
| 1 | `prefabs/Soldier.tscn` | collision_mask 去掉 unit 层，只留 building 层（建筑检测） |
| 2 | `prefabs/Building.tscn` | 移除 NavigationObstacle2D |
| 3 | `Soldier.cs` | `ApplyPendingStats` 删除碰撞形状设置；`OnAreaEntered`/`OnAreaExited` 保留 |
| 4 | `Building.cs` | 删除 NavigationObstacle2D 代码 |
| 5 | `BuildingSystem.cs` | `ApplyVisual` 删除 NavigationObstacle2D 代码 |
| 6 | `BattleManager.cs` | 温和推开：同方士兵推开 + 士兵-建筑推开 |

### 验收

- [ ] `dotnet build` 0 错误
- [ ] 士兵之间推开正常（距离判定）
- [ ] 士兵-建筑推开正常
- [ ] 士兵走到建筑旁触发攻城（AreaEntered 保留）
- [ ] 建筑摧毁后 AreaExited 触发，士兵继续行军
- [ ] NavigationObstacle 已移除，导航仍正常工作
