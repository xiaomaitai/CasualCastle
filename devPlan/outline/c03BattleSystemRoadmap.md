# 三、战斗系统：路线图

基于 `c02BattleSystemOverview.md` 的目标定义，以下为分阶段开发路线。

---

## 3.1 阶段总览

```
B1 单位类型体系 ──→ B4 单位差异化
    ↓                    ↓
B2 远程攻击        B3 伤害护甲体系
    ↓                    ↓
    └──→ B5 战斗表现 ←──┘
              ↓
          B6 数值平衡
              ↓
          B7 后续扩展（延后）
```

---

## 3.2 各阶段详情

### B1: 单位类型体系

**目标**：从单一 `Soldier` 类拆分为多单位类型。

| 任务 | 预估 |
|------|------|
| `UnitType` 枚举（Melee/Ranged/Cavalry/Beast） | 0.5d |
| `UnitStats` 结构体（攻击/护甲/速度/射程） | 0.5d |
| `UnitRegistry`：按 TypeId 注册单位模板 | 0.5d |
| `BuildingData` 产出改为 `UnitType` | 0.5d |
| 验证：4 种建筑产出的士兵属性不同但行为相同 | — |

**依赖**：无

**产出**：`UnitStats` 类、`UnitType` 枚举、`UnitRegistry`、改造后的 `BuildingSystem.ApplySoldierSpawnStats`

---

### B2: 远程攻击

**目标**：靶场产出远程士兵，射程内射击而非冲脸。

| 任务 | 预估 |
|------|------|
| `AttackBehavior` 策略接口（MeleeAttack / RangedAttack） | 1d |
| `RangedAttack`：检测射程 → 停步 → 发射弹体 → 冷却 | 1d |
| `Projectile` 节点（直线飞行、命中检测） | 1d |
| 靶场/弓箭手端到端验证 | 0.5d |

**依赖**：B1

**产出**：`AttackBehavior`、`MeleeAttack`、`RangedAttack`、`Projectile`

---

### B3: 伤害与护甲体系

**目标**：攻击/护甲类型形成克制关系。

| 任务 | 预估 |
|------|------|
| `DamageType` / `ArmorType` 枚举 | 0.5d |
| 克制矩阵（4×4 倍率表） | 0.5d |
| `CombatRules.CalculateDamage()` 纳入克制 | 0.5d |
| 各兵种配置攻击/护甲类型并验证 | 0.5d |

**依赖**：B1

**产出**：`DamageType`、`ArmorType`、克制矩阵配置、`CombatRules` 扩展

---

### B4: 单位差异化

**目标**：6 种士兵各有独立 `PackedScene` 和视觉区分。

| 任务 | 预估 |
|------|------|
| 各兵种独立 `.tscn`（Swordsman/Archer/Cavalry/Werewolf 等） | 1d |
| 单位 sprite/颜色区分（占位图即可） | 0.5d |
| 强化兵营/强化狼穴产出强化版本 | 0.5d |

**依赖**：B1

**产出**：6+ 个 `PackedScene`，`UnitRegistry` 完整注册表

---

### B5: 战斗表现

**目标**：受击闪烁、血条、弹体飞行、死亡动画。

| 任务 | 预估 |
|------|------|
| 单位头顶血条 | 1d |
| 受击闪烁（shader 或 modulate 闪白） | 0.5d |
| 弹体飞行可视化（弓箭轨迹） | 1d |
| 死亡动画（2-3 帧或粒子消散） | 1d |

**依赖**：B2、B4

**产出**：`HealthBar` 组件、hit_flash shader、弹体 sprite、死亡粒子

---

### B6: 数值平衡与配置化

**目标**：战斗数值从 `.tres` 加载，克制矩阵可配置。

| 任务 | 预估 |
|------|------|
| `UnitStats` 转 Godot `Resource`（`.tres`） | 1d |
| 克制矩阵配置文件 | 0.5d |
| 平衡公式文档 + 1v1 模拟测试 | 1d |

**依赖**：B3、B4

**产出**：`.tres` 资源文件、平衡文档、模拟测试

---

### B7: 后续扩展（非 MVP）

- AOE / 溅射伤害
- 状态效果（减速、中毒、眩晕）
- 单位经验/升级
- 地形影响

---

## 3.3 里程碑

| 里程碑 | Phase | 可验证产出 |
|--------|-------|-----------|
| M-B1 | B1 + B2 | 弓箭手在射程内停步射击，剑士冲脸近战 |
| M-B2 | B3 + B4 | 6 种兵种独立场景，克制伤害倍率生效 |
| M-B3 | B5 | 血条、受击闪烁、弹体、死亡动画在线 |
| M-B4 | B6 | 所有数值从 `.tres` 加载，可热调 |
