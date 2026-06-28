# 三、战斗系统：路线图

基于 `c02BattleSystemOverview.md` 的目标定义。

---

## 3.1 已完成

### B1: 单位类型体系 ✅

- `UnitSize`（Small/Medium/Large/Huge）、`AttackType`（Melee/Ranged）
- `DamageType`/`ArmorType` 枚举 + `DamageMatrix` 4×4 克制倍率
- `UnitStats` + `UnitRegistry`（6 种单位模板）
- `SoldierData` 重构：`FromStats()` / `DisplaySize()` / `CollisionRadius()`
- `BuildingDefinitions` 用 `UnitTypeId` 替代逐个属性覆写
- `Soldier.InitializeFromStats()` + `_Draw()` 碰撞圈

### B2: 远程攻击 ✅

- `AttackBehavior` 抽象基类 + `MeleeAttack` / `RangedAttack` 策略
- `Projectile` 弹体（程序化创建，碰撞命中后计算克制伤害）
- `Soldier` 集成 `AttackBehavior`

### B3: 伤害与护甲体系 ✅

- `CombatRules.CalculateDamage()` 纳入克制矩阵
- `DamageMatrix.GetMultiplier(damage, armor)` 查表

### B4: 单位差异化 ✅

- `UnitStats.UnitColor`（ARGB uint），6 种兵种各分配颜色
- `Soldier.ApplyPendingStats` 按显示尺寸缩放 sprite + 应用颜色
- 碰撞半径按体型自动计算

### B5: 战斗表现 ✅

- 头顶血条（`_Draw` 绘制背景+前景条）
- 受击闪烁（0.1s 白色 modulate）
- 死亡动画（Tween 缩放归零 + 淡出 0.25s）

### 碰撞优化 ✅

- `BattleManager`：玩家/敌方单位列表，空间哈希网格（200unit/格）
- 周期索敌（0.2s），查询 9 邻格取最近敌方
- 同方推挤：碰撞半径之和+4unit 内施加排斥力
- `Soldier` 注册/注销/`SetTarget`，移除 Area2D 士兵侦测

### 移动避障 ✅

- 目的地计算：玩家→敌方城堡左前方，敌方→玩家城堡右前方
- `MoveToward` 射线检测建筑碰撞，垂直滑行绕过
- 无目标时向敌方战线推进

---

## 3.2 待完成

### B6: 数据层 SQLite 化

**目标**：游戏数值从 SQLite 数据库加载，替代硬编码和 `.tres`。

见 `currentTasks.md`。

### B7: 后续扩展（延后）

- AOE / 溅射伤害
- 状态效果（减速、中毒、眩晕）
- 单位经验/升级
- 地形影响

---

## 3.3 里程碑

| 里程碑 | Phase | 状态 |
|--------|-------|------|
| M-B1 | B1 + B2 | ✅ |
| M-B2 | B3 + B4 | ✅ |
| M-B3 | B5 + 碰撞 + 移动 | ✅ |
| M-B4 | B6 SQLite | — |
