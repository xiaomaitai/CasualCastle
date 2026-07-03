# 当前任务：士兵移动改用 Godot NavigationAgent2D RVO 寻路避障

## 背景

当前士兵移动方案是自己实现的：域层 `Soldier.MoveToward()` 线性插值移向导航路径点，适配层 `SoldierLogic._Process()` 直接赋值 `_body.GlobalPosition`。NavigationAgent2D 虽然挂在场景上且 `avoidance_enabled = true`，但 RVO 从未被驱动——`Velocity` 从未赋值、`velocity_computed` 信号未连接、`radius` 为 0。士兵完全靠极弱的 `UnitSpatialService.PushForce=20` 推挤，必然重叠。

## 目标

完全去掉自定义移动逻辑，改用 Godot 4 NavigationAgent2D 原生寻路 + RVO 避障。域层只负责"去哪"（设目标）和战斗判定，"怎么走"全部交给适配层 + NavigationAgent2D。

---

## 新的移动数据流

```
域层 Soldier.UpdateBehavior()
  → 只设 NavPort.SetTarget(target)，不做 MoveToward

适配层 SoldierLogic._PhysicsProcess():
  1. 域层 UpdateTargeting() + UpdateBehavior() → 设置导航目标
  2. _agent.GetNextPathPosition() → 获取下一路径点
  3. desiredVelocity = (nextPos - currentPos).Normalized() * pixelSpeed
  4. _agent.Velocity = desiredVelocity  ← 触发 RVO 计算
  5. velocity_computed 信号回调 → 存储 _safeVelocity
  6. _body.GlobalPosition += _safeVelocity * dt
  7. _service.MoveTo() 写回域层位置
```

---

## 实施步骤

### P1. 修改 `INavigationPort` — 精简接口

**文件**: `scripts/domain/Battle/INavigationPort.cs`

- 移除 `GetNextPosition(float currentGameX, float currentGameY)` 方法
- 保留 `SetTarget(float gameX, float gameY)`
- 域层不再关心路径细节，只管设置目标

### P2. 修改 `NavigationPortAdapter` — 对接 RVO

**文件**: `scripts/adapters/godot/battle/NavigationPortAdapter.cs`

- 移除 `GetNextPosition` 实现
- `SetTarget()` 保持不变（内部设 `_agent.TargetPosition`，像素坐标）
- 新增方法 `Vector2 ComputeDesiredVelocity(float currentGameX, float currentGameY, float gameSpeed)`
  - 从当前位置（先转为像素）取 `_agent.GetNextPathPosition()`
  - 计算方向向量，归一化后乘以像素速度
  - 返回像素/秒的速度向量
- 新增事件 `event Action<Vector2> SafeVelocityComputed`
- 构造函数中连接 `_agent.VelocityCommitted` 信号（Godot 4 信号名），回调中触发 `SafeVelocityComputed`

### P3. 修改域层 `Soldier` — 去掉 MoveToward

**文件**: `scripts/domain/Battle/Soldier.cs`

- 删除 `MoveToward(float dt, float targetGameX, float targetGameY)` 方法
- `UpdateBehavior()` 中：
  - Marching 状态：只调 `NavPort.SetTarget(marchTargetGameX, marchTargetGameY)`，不再调 `GetNextPosition` + `MoveToward`
  - Fighting/Retaliating 状态（敌人超出攻击范围）：只调 `NavPort.SetTarget(TargetEnemy.GameX, TargetEnemy.GameY)`
  - Sieging 状态：不变（不移动）
- 新增 `void ApplyRvoPosition(float gameX, float gameY)` — 供适配层写回 RVO 后的位置

### P4. 修改 `SoldierService` / `ISoldierService` — 新增写回入口

**文件**: `scripts/domain/Battle/ISoldierService.cs`, `scripts/domain/Battle/SoldierService.cs`

- 新增 `void ApplyRvoPosition(float gameX, float gameY)` 方法
- 实现委托到 `Soldier.ApplyRvoPosition()`
- 保留 `MoveTo()` 不变（初始化时使用）
- 保留 `ApplyPush()` 不变（UnitSpatialService 保底推挤）

### P5. 修改 `SoldierLogic` — 核心流程重构

**文件**: `scripts/adapters/godot/battle/SoldierLogic.cs`

- 新增字段 `private Vector2 _safeVelocity` 存储 RVO 计算结果
- `InitializeFromStats()` 中：
  - 连接 `_navigationAgent.VelocityComputed += OnVelocityComputed`
- `_Process()` 改为 `_PhysicsProcess()`（Godot RVO 在物理帧处理）
- `_PhysicsProcess()` 新流程：
  1. 存活/激活检查（不变）
  2. 域层 `UpdateTargeting()` + `UpdateBehavior()`（不变）
  3. 调用 `_navPort.ComputeDesiredVelocity()` 获取期望速度
  4. `_navigationAgent.Velocity = desiredVelocity`（触发 RVO）
  5. 使用上一帧的 `_safeVelocity` 移动 body（首帧降级用 desiredVelocity）
  6. 像素坐标转游戏单位，`_service.ApplyRvoPosition()` 写回域层
  7. `_body.QueueRedraw()`（不变）
- 删除 `AvoidanceEnabled` 动态切换（始终开启，由 Godot 管理）
- 新增 `OnVelocityComputed(Vector2 safeVelocity)` 信号回调：存储 `_safeVelocity`
- 删除手动 `_body.GlobalPosition = new Vector2(...)`（第 191-193 行）
- 删除 `SelectTarget()` 中的像素坐标转换（域层已有 marchX/marchY 游戏坐标）

### P6. 修改 `Soldier.tscn` — NavigationAgent2D 配置

**文件**: `prefabs/Soldier.tscn`

- NavigationAgent2D 节点设置 `radius = 62.5`（`DisplaySize / 2`）
- 确认 `avoidance_enabled = true`（已设）
- 确认 `path_desired_distance = 4.0`（已有）
- 确认 `target_desired_distance = 4.0`（已有）
- 将 `CollisionShape2D` 从 `Logic` 移到根 `Soldier`（Area2D）下（修复无效碰撞）

### P7. 削弱 `UnitSpatialService` — RVO 保底推挤

**文件**: `scripts/domain/Battle/UnitSpatialService.cs`

- `PushForce` 从 20 提升到 500（RVO 正常时基本不触发，仅极端重叠时弹开）
- 不改动计算逻辑

### P8. 清理 `BattleManager`

**文件**: `scripts/adapters/godot/battle/BattleManager.cs`

- 保持不变（仍每帧调 `PushSoldiers`，但现在只是轻量保底）

---

## 验收项

1. 士兵行军时不再互相重叠
2. 多个士兵追击同一敌人时保持间距
3. 士兵绕过障碍物（建筑物）而非穿越
4. 战斗/反击状态下士兵仍然保持合理间距
5. 删除 `Soldier.MoveToward()` 后无编译错误
6. 删除 `INavigationPort.GetNextPosition()` 后无编译错误
7. RVO 避障效果在 2 个以上士兵同向移动时可观察到
8. `CollisionShape2D` 挂载到正确的 Area2D 父节点下

---

## 涉及文件清单

| 文件 | 变更类型 |
|------|---------|
| `scripts/domain/Battle/INavigationPort.cs` | 删除 GetNextPosition |
| `scripts/domain/Battle/Soldier.cs` | 删除 MoveToward，新增 ApplyRvoPosition，UpdateBehavior 简化 |
| `scripts/domain/Battle/ISoldierService.cs` | 新增 ApplyRvoPosition |
| `scripts/domain/Battle/SoldierService.cs` | 新增 ApplyRvoPosition 实现 |
| `scripts/domain/Battle/UnitSpatialService.cs` | PushForce 20→500 |
| `scripts/adapters/godot/battle/NavigationPortAdapter.cs` | 新增 ComputeDesiredVelocity + SafeVelocityComputed 事件 |
| `scripts/adapters/godot/battle/SoldierLogic.cs` | _Process→_PhysicsProcess，RVO 流程，VelocityComputed 信号 |
| `prefabs/Soldier.tscn` | NavigationAgent2D radius，CollisionShape2D 父节点修复 |

## 注意事项

- Godot `velocity_computed` 信号在导航服务异步处理后触发，有 1 帧延迟——首帧用 `desiredVelocity` 降级
- `_PhysicsProcess` 替代 `_Process` 是因为 NavigationServer 在物理帧同步 RVO 计算结果
- NavigationAgent2D 的全局位置由父节点链自动推断（`Soldier` → `Logic` → `NavigationAgent`），保持父节点结构不变
- 域层 `INavigationPort` 变为单方法接口（仅 `SetTarget`），符合六边形架构最小接口原则
