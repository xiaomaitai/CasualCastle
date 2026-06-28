# 六、战斗系统：施工方案

基于 `c02` 目标定义和 `c03` 路线图，结合 Godot 4.6 内置工具制定。

---

## 6.1 可用 Godot 资产

| 资产 | 来源 | 用途 | 状态 |
|------|------|------|------|
| **Jolt Physics** | 项目已启用 | 物理引擎，碰撞性能 2-20× 优于默认 | ✅ 已就绪 |
| **Area2D 碰撞检测** | Godot 内置 | 前方障碍检测、建筑碰撞体用于避障 | ✅ 已使用 |
| **Area2D** | Godot 内置 | 单位/建筑碰撞检测 | ✅ 已使用 |
| **C# Task / async** | .NET 8 | 异步路径计算 | ✅ 可用 |
| **ShaderMaterial** | Godot 内置 | 受击闪烁、死亡溶解特效 | ✅ 可用 |
| **Resource (.tres)** | Godot 内置 | 单位属性、克制矩阵配置 | ✅ 可用 |

**不需要的外部插件**：Jolt + C# 已覆盖核心需求。直接移动 + 避障、空间哈希、BattleManager 均为项目内实现。

---

## 6.2 架构设计

### 新增文件

```
scripts/
├── domain/Battle/
│   ├── UnitType.cs              # 枚举：Melee/Ranged/Cavalry/Beast
│   ├── UnitStats.cs             # 单位模板数据（血、攻、速、甲…）
│   ├── UnitRegistry.cs          # TypeId → UnitStats 注册表
│   ├── DamageType.cs            # 枚举：Normal/Pierce/Siege/Magic
│   ├── ArmorType.cs             # 枚举：Light/Heavy/Fortified/Beast
│   └── DamageMatrix.cs          # 克制倍率矩阵
│
├── adapters/godot/battle/
│   ├── BattleManager.cs         # 全局战斗管理器（索敌、寻路调度）
│   ├── AttackBehavior.cs        # 攻击行为策略接口
│   ├── MeleeAttack.cs           # 近战攻击实现
│   ├── RangedAttack.cs          # 远程攻击 + 弹体生成
│   ├── Projectile.cs            # 弹体节点
│   ├── SoldierSwordsman.tscn    # 剑士预制体
│   ├── SoldierArcher.tscn       # 弓箭手预制体
│   ├── SoldierCavalry.tscn      # 骑兵预制体
│   ├── SoldierWerewolf.tscn     # 狼人预制体
│   └── UnitHealthBar.cs         # 头顶血条组件
```

### 修改现有文件

| 文件 | 改动 |
|------|------|
| `Soldier.cs` | 重构：移除 `[Export]` 属性，改为从 `UnitStats` 初始化；拆出攻击行为 |
| `BuildingSystem.cs` | `ApplySoldierSpawnStats` → 从 `UnitRegistry` 查模板 |
| `BuildingDefinitions.cs` | `SoldierDamage` 等覆写字段 → `UnitTypeId` |
| `CombatRules.cs` | `CalculateDamage` 纳入克制矩阵 |

---

## 6.3 移动与避障

战场是任意开放空间，城堡网格仅用于建筑放置。士兵移动不需要网格寻路。

### 移动模型

```
_Process(delta):
    1. 计算目的地（BattleManager 给定）
    2. 向目的地方向移动
    3. 检测前方障碍（建筑碰撞体）
    4. 有障碍 → 沿障碍边缘切线方向滑行
    5. 同方单位间距 < 阈值 → 施加排斥力
```

### 目的地

由 `BattleManager` 为每个士兵计算：

```
玩家单位：目的地 = 敌方城堡战线（enemyCastle 左边缘 - 160 unit，Y 取士兵当前 Y）
敌方单位：目的地 = 玩家城堡战线（playerCastle 右边缘 + 160 unit，Y 取士兵当前 Y）
有攻击目标：目的地 = 目标位置（追击）
```

### 建筑避障

```
前方检测：
    向移动方向做短距离射线或小范围 Area2D 查询
    检测到建筑碰撞体 →
        计算建筑边缘切线方向
        沿切线偏移移动方向
        继续前进
建筑摧毁 →
    碰撞体消失
    自动通过
```

不需要 bake 导航网格、不需要 A*、不需要流场。战场建筑稀疏，直线移动 + 简单避障足够。

### 单位推挤

```
同方士兵间距 < 25 unit →
    施加垂直于连线方向的微小排斥力
    力度随距离减小而增大
```

防止同位置堆叠，但不影响战斗和目标锁定。

---

## 6.4 碰撞优化：BattleManager + 空间哈希

### BattleManager 职责

替代 `AreaEntered/AreaExited` 信号驱动模式，改为主动轮询：

```
BattleManager（Node，挂在 main_game.tscn）
  ├── List<Soldier> playerUnits
  ├── List<Soldier> enemyUnits
  ├── Dictionary<Vector2I, List<Soldier>> spatialGrid  # 空间哈希
  └── 每 0.2s：UpdateTargeting()
```

**空间哈希网格：** 桶大小 = 2×2 格（200×200 unit）。每个桶记录其中的士兵。

**索敌流程（每 0.2s 对每个士兵）：**
```
1. 查询士兵所在桶 + 8 个相邻桶
2. 收集桶中所有敌方单位
3. 按优先级排序：
   a. AttackRange 内的敌方士兵（可立刻攻击）
   b. 追击范围内的敌方士兵
   c. AttackRange 内的敌方建筑
   d. 最近的敌方单位
4. 选最优 → 赋值 soldier.Target
```

**性能：** 200 个单位，9 个桶/单位，平均 2-3 个单位/桶 → 200 × 9 × 2.5 = 4500 次距离检查，每 0.2s 执行一次，完全可承受。

### 保留的物理碰撞

- `Area2D` 碰撞形状保留，但仅用于物理推挤
- 移除 `AreaEntered/AreaExited` 信号订阅
- 单位间推挤：简单排斥力（距离 < 阈值时施加远离方向的速度分量）

---

## 6.5 战斗 AI 实现

### 目的地计算

```
Vector2 CalculateDestination(Soldier soldier):
    if soldier.HasTarget:
        return soldier.Target.GlobalPosition  # 追击

    if soldier.IsPlayerUnit:
        return enemyCastle.GetForwardLine(soldier.GlobalPosition)
    else:
        return playerCastle.GetForwardLine(soldier.GlobalPosition)
```

**战线位置：**
- 玩家方：`playerCastle.RightEdge + 200 unit`，Y 取士兵当前 Y
- 敌方方：`enemyCastle.LeftEdge - 200 unit`，Y 取士兵当前 Y

### 目标切换

```
void UpdateTarget(Soldier soldier, List<Soldier> candidates):
    Soldier best = FindBestTarget(soldier, candidates)

    if soldier.CurrentTarget == null:
        soldier.CurrentTarget = best    # 首次索敌
    else if best != null && ShouldSwitch(soldier, best):
        soldier.CurrentTarget = best    # 切换目标

bool ShouldSwitch(Soldier current, Soldier candidate):
    if current.CurrentTarget.IsDead:
        return true                     # 当前目标已死，必切
    if candidate.Priority > current.CurrentTarget.Priority:
        return true                     # 新目标优先级更高
    float distCurrent = Distance(current, current.CurrentTarget)
    float distCandidate = Distance(current, candidate)
    if distCurrent > 300 && distCandidate < distCurrent * 0.7:
        return true                     # 当前目标太远，新目标更近
    return false
```

### 攻击执行

```
void ExecuteAttack(Soldier soldier, float delta):
    if soldier.AttackBehavior is MeleeAttack:
        if Distance(soldier, target) <= AttackRange && CooldownReady:
            target.TakeDamage(soldier.Damage)
            ResetCooldown()
    else if soldier.AttackBehavior is RangedAttack:
        if Distance(soldier, target) <= AttackRange && CooldownReady:
            SpawnProjectile(soldier, target)
            ResetCooldown()
```

---

## 6.6 远程攻击：弹体系统

```
Projectile（Area2D）
  ├── Speed: 600 unit/s
  ├── Damage: 继承发射者
  ├── Target: 发射时锁定
  └── _Process:
        MoveToward(target)
        if Distance < 10 unit:
            target.TakeDamage(damage)
            QueueFree()
```

- 弹体碰撞层：Layer 5（弹体专用，不与其他弹体碰撞）
- 弹体碰撞掩码：检测士兵 + 建筑
- 弹体池：预分配 50 个弹体，复用减少 GC

---

## 6.7 施工顺序

| 步骤 | 内容 | 对应 Phase |
|------|------|-----------|
| 1 | 新建 domain 类：`UnitType`, `DamageType`, `ArmorType`, `DamageMatrix` | B1+B3 domain |
| 2 | 新建 `UnitStats`, `UnitRegistry`，改造 `BuildingDefinitions` | B1 |
| 3 | 重构 `Soldier.cs`：移除 `[Export]`，改为 `UnitStats` 初始化 | B1 |
| 4 | 新建 `AttackBehavior` / `MeleeAttack` / `RangedAttack` | B2 |
| 5 | 新建 `Projectile`，接入 `RangedAttack` | B2 |
| 6 | 创建各兵种 `.tscn`（占位 sprite 颜色区分） | B4 |
| 7 | 新建 `BattleManager` + 空间哈希索敌 | B1-B4 集成 |
| 8 | 新建 `UnitHealthBar` + hit_flash shader | B5 |
| 9 | 建筑避障 + 单位推挤集成 | 移动 |
| 10 | 配置文件 `.tres` 化 | B6 |

---

## 6.8 验收 Checkpoint

每步完成后验证：

- [ ] `dotnet build` 0 错误
- [ ] `dotnet test` 全部通过
- [ ] 运行 main_game.tscn 无回归（现有功能正常）
- [ ] 新增行为可手动验证（远程射击、克制伤害等）
