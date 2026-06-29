# 当前任务

**B7: 士兵行为状态机 + 视野 + 反击 — 进行中**

---

## B6: SQLite 数据层 ✅

游戏数据（`UnitRegistry`、`BuildingDefinitions`、`DamageMatrix`、`ShopRules`、`FusionRules`）已迁移到 `assets/data/game_data.db`，通过 `GameDataLoader` 在 `CompositionRoot.Build()` 时加载。

---

## B7: 士兵行为状态机 + 视野 + 反击

> 设计依据：`devPlan/design/battleSystem.md` §3.3–3.4

### 目标

将 `Soldier._Process` 中单块的条件分支重构为四状态有限状态机，引入视野距离概念，实现受击反击 + 友军集体传播。

### 施工步骤

| 步骤 | 文件 | 内容 |
|------|------|------|
| 1 | `UnitStats.cs` | 新增 `VisionRange` 字段（`float`，init-only，默认 170f） |
| 2 | `SoldierData.cs` | 新增 `VisionRange` 字段（`float`，默认 170f），`FromStats()` 中复制 |
| 3 | `UnitRegistry.cs` | Swordsman 模板增加 `VisionRange = 170f` |
| 4 | `Soldier.cs` | 新增字段 `VisionRange`（float，像素）、`_state`（enum `SoldierState`） |
| 5 | `Soldier.cs` | `InitializeFromStats` 中将 `VisionRange` 从 game unit 转为像素 |
| 6 | `Soldier.cs` | 重构 `_Process`：提取状态判定逻辑，按四状态分别执行行为 |
| 7 | `Soldier.cs` | `TakeDamage` 增加 `Soldier attacker` 参数；攻击者在视野外时设为目标并传播反击 |
| 8 | `BattleManager.cs` | 新增 `PropagateRetaliation`：以受击士兵为圆心，VisionRange 内无目标的友军集体锁定攻击者 |
| 9 | `MeleeAttack.cs` `RangedAttack.cs` | 调用 `TakeDamage` 时传入 attacker |
| 10 | `GameDataLoader.cs` + DB | `unit_stats` 表增加 `vision_range REAL NOT NULL DEFAULT 170.0` 列 |

### 行为规则

**状态判定**（每帧）：

1. `_targetEnemy` 存活 → 边缘距离 ≤ VisionRange 则为 **Fighting**，否则为 **Retaliating**
2. `_targetBuilding` 存活且未摧毁 → **Sieging**
3. 否则 → **Marching**（向敌方城堡前线推进）

**状态执行**：

| 状态 | 移动目标 | 攻击 |
|------|----------|------|
| Fighting | `_targetEnemy.GlobalPosition` | 边缘距离 ≤ AttackRange 时执行攻击 |
| Retaliating | `_targetEnemy.GlobalPosition` | 边缘距离 ≤ AttackRange 时执行攻击 |
| Sieging | 停在建筑旁 | 每冷却周期对建筑造成 Damage，穿透至城堡 |
| Marching | `GetDestination()` | 无 |

**反击触发与传播**（`TakeDamage` 中）：

1. 攻击者距离 > 受击士兵的 VisionRange → 受击士兵 `_targetEnemy = attacker`
2. 调用 `BattleManager.PropagateRetaliation(受击士兵, VisionRange, attacker)`
3. 传播逻辑：遍历受击士兵同方单位，VisionRange 内且 `_targetEnemy` 为空的友军 → 锁定同一 attacker
4. 已有目标的友军不受影响（实现 Fighting > Retaliating 优先级）

### 验收

- [ ] `dotnet build` 0 错误
- [ ] 士兵视野内发现敌人 → 主动迎战（Fighting）
- [ ] 士兵被视野外敌人攻击 → 进入反击（Retaliating）
- [ ] 受击士兵周围友军 → 集体反击
- [ ] 已在交战的友军 → 不受呼救影响
- [ ] 反击目标进入视野 → 升级为 Fighting
- [ ] 无目标时 → 行军向敌方城堡推进
- [ ] 撞到建筑 → 攻城；建筑摧毁 → 继续行军

---

## 历史完成

B1–B6 全部完成。`dotnet build` 0 错误，`dotnet test` 6/6 通过，4 个 domain 项目零 `using Godot`。
