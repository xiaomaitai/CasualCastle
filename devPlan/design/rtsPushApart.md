# RTS 推开系统

## 原则

- 士兵之间：不用物理碰撞，每帧距离判定推开
- 士兵-建筑：保留 Area2D 碰撞信号（AreaEntered → 攻城），加上推开防止重叠
- NavigationObstacle2D：删除，建筑用推开代替导航避障

## 士兵-士兵

BattleManager 每帧对所有同方士兵对：`dist < radius_a + radius_b + 4` → 沿连线推开差额。

## 士兵-建筑

1. 推开：士兵中心到建筑碰撞矩形的距离 < 士兵半径 → 推出
2. 攻城检测：AreaEntered/AreaExited 保留，士兵进入建筑 Area2D → Sieging

## 移除项

- Soldier 的 CollisionShape2D
- Building 的 NavigationObstacle2D
