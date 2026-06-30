# 当前任务

**B8: 节点结构重构 — Logic/View/Effects 三层分离 — 未开始**

---

## B8: 节点结构重构 — Logic/View/Effects 三层分离

> 设计依据：`devPlan/design/nodeStructure.md`

### 目标

将 Soldier 和 Building 的平铺子节点拆为 Logic / View / Effects 三层。逻辑不碰渲染，特效不改碰撞。

### Soldier

| 步骤 | 文件 | 内容 |
|------|------|------|
| 1 | `prefabs/Soldier.tscn` | 新增 Logic(Node) / View(Node2D) / Effects(Node2D)；NavigationAgent + CollisionShape 移入 Logic；Sprite 移入 View；SleepZEffect 移入 Effects |
| 2 | `Soldier.cs` | 路径更新：`"NavigationAgent"`→`"Logic/NavigationAgent"`，`"Sprite"`→`"View/Sprite"`，`"CollisionShape"`→`"Logic/CollisionShape"`，`"SleepZEffect"`→`"Effects/SleepZEffect"` |

### Building

| 步骤 | 文件 | 内容 |
|------|------|------|
| 3 | `prefabs/Building.tscn` | 新增 Logic / View / Effects；CollisionShape + NavigationObstacle 移入 Logic；Sprite 移入 View |
| 4 | `Building.cs` | 路径更新：`"Sprite"`→`"View/Sprite"`，`"CollisionShape"`→`"Logic/CollisionShape"`，`"NavigationObstacle"`→`"Logic/NavigationObstacle"`；`_stateIcon` AddChild 到 Effects 下 |
| 5 | `BuildingSystem.cs` | `ApplyVisual` 中路径更新同上 |

### 验收

- [ ] `dotnet build` 0 错误
- [ ] Godot 运行：士兵生成、移动、战斗正常
- [ ] Godot 运行：建筑放置、产兵、碰撞检测正常
- [ ] 碰撞调试可视化（_Draw 圈）位置正确
