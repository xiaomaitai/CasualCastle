# 战斗系统开发计划

基于 `design/battleSystem.md` 设计文档和当前代码状态制定。

---

## Phase B1: 单位类型体系

**目标**：从单一 `Soldier` 类拆分为多单位类型，每类有独立行为和属性。

- [ ] `SoldierData` 扩展：新增 `UnitType` 枚举（Melee/Ranged/Cavalry/Beast）
- [ ] `UnitStats` 结构体：攻击类型、护甲类型、移动速度、攻击前摇
- [ ] `UnitRegistry`：按 TypeId 注册单位模板（替代 `Soldier` 的 `[Export]` 硬编码）
- [ ] `BuildingData` 改为产出 `UnitType` 而非覆写 Soldier 属性
- [ ] `UnitSpawn` 改造：根据 `UnitType` 实例化对应 `PackedScene`

## Phase B2: 远程攻击

**目标**：靶场产远程士兵，在射程内原地射击，不再冲向目标。

- [ ] `Soldier` 拆分为 `MeleeSoldier` / `RangedSoldier` 子类或 `AttackBehavior` 策略
- [ ] 远程攻击逻辑：检测射程内敌人 → 停步 → 发射弹体/立即伤害 → 冷却
- [ ] 弹体（可选）：`Projectile` 节点，直线飞行，命中检测
- [ ] 近战/远程的 `AttackRange` 差异化生效（近战 30px，远程 50px+）
- [ ] 靶场 `SpawnInterval` / `SoldierAttackRange` 已在数据中，行为对齐

## Phase B3: 伤害与护甲体系

**目标**：引入攻击类型和护甲类型，形成克制关系。

- [ ] `DamageType` 枚举：Normal / Pierce / Siege / Magic
- [ ] `ArmorType` 枚举：Light / Heavy / Fortified / Beast
- [ ] 克制矩阵：攻击类型 × 护甲类型 → 伤害倍率（如 Pierce→Heavy 1.5×，Pierce→Light 0.75×）
- [ ] `UnitStats` 中定义单位的攻击类型和护甲类型
- [ ] `CombatRules.CalculateDamage(attacker, defender)` 纳入克制计算

## Phase B4: 单位差异化

**目标**：每种建筑产出的士兵有视觉和行为的实质区别。

| 建筑 | 单位类型 | 攻击类型 | 护甲 | 特点 |
|------|---------|---------|------|------|
| 兵营 | 剑士 | Normal | Light | 均衡，普适 |
| 靶场 | 弓箭手 | Pierce | Light | 远程，克重甲 |
| 马厩 | 骑兵 | Normal | Heavy | 快速，高血量 |
| 狼穴 | 狼人 | Normal | Beast | 夜战，高伤害 |
| 强化兵营 | 重剑士 | Normal | Heavy | 高血量 |
| 强化狼穴 | 狼人首领 | Magic | Beast | 夜战，更高伤害 |

- [ ] 每个单位类型独立 `PackedScene`（`Swordsman.tscn`, `Archer.tscn`, `Cavalry.tscn`, `Werewolf.tscn` 等）
- [ ] 单位 sprite / 颜色区分
- [ ] `SoldierData` 调整为模板数据，运行时从模板实例化

## Phase B5: 战斗表现

**目标**：战斗反馈可感知，不再只是数字加减。

- [ ] 单位头顶血条（`ProgressBar` 或 shader 血条）
- [ ] 受击闪烁（白帧或 shader hit_flash）
- [ ] 死亡动画（当前仅 fade-out，改为 2-3 帧动画或粒子消散）
- [ ] 攻击动画（近战挥砍帧、远程拉弓帧）
- [ ] 弹体飞行（弓箭飞行轨迹，可选）

## Phase B6: 数值平衡与配置化

**目标**：战斗数值可配置、可调，脱离硬编码。

- [ ] `UnitStats` 从 `.tres` 资源文件加载（Godot `Resource`）
- [ ] 克制矩阵可配置（JSON 或 `.tres`）
- [ ] 基础平衡公式文档化：DPS = Damage / AttackCooldown，TTK = TargetHP / DPS
- [ ] 模拟测试：1v1 各兵种对战胜率预期

## Phase B7: 后续扩展（非 MVP，延后）

- [ ] AOE / 溅射伤害（投石车等）
- [ ] 状态效果（减速、中毒、眩晕）
- [ ] 单位升级/经验系统
- [ ] 地形影响（高低、通道宽度）
- [ ] 玩家手动操控单位技能

---

## 优先级建议

```
B1（单位类型体系）──→ B4（单位差异化）
  ↓                      ↓
B2（远程攻击）      B3（伤害护甲体系）
  ↓                      ↓
  └────→ B5（战斗表现）←─┘
            ↓
        B6（数值平衡）
```

B1+B2 优先：它们是战斗系统从「极简互殴」到「有策略深度」的关键跳变。B3 紧随其后确保数值有克制逻辑。B4 与 B1/B2 可部分并行。
