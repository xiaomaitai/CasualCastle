# 六、战斗系统：施工方案

基于 `c02` 目标定义和 `c03` 路线图。

---

## 6.1 已施工

| 步骤 | 内容 | 状态 |
|------|------|------|
| 1 | domain 类：UnitSize/AttackType/DamageType/ArmorType/DamageMatrix | ✅ |
| 2 | UnitStats/UnitRegistry，BuildingDefinitions 改造 | ✅ |
| 3 | Soldier 重构：InitializeFromStats，移除 [Export] | ✅ |
| 4 | AttackBehavior/MeleeAttack/RangedAttack | ✅ |
| 5 | Projectile 弹体 | ✅ |
| 6 | 单位颜色/尺寸区分（B4） | ✅ |
| 7 | BattleManager + 空间哈希索敌 | ✅ |
| 8 | 血条/受击闪烁/死亡动画（B5） | ✅ |
| 9 | 建筑避障 + 目的地（移动） | ✅ |
| 10 | SQLite 配置化（B6） | — |

---

## 6.2 B6: SQLite 数据层

详见 `currentTasks.md`。
