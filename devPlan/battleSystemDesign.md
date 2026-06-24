# 战斗系统设计文档

---

## 1. 概述

本文档描述 CasualCastle 游戏的战斗系统核心规则与实现细节，涵盖建筑与士兵的交互逻辑、碰撞检测、修复机制等。

---

## 2. 建筑系统

### 2.1 建筑状态

| 状态 | 条件 | 行为 |
|------|------|------|
| 正常运行 | `Health > 0 && !IsManuallyPaused` | 正常产兵/工作 |
| 受损 | `0 < Health < MaxHealth` | 继续工作，显示受损视觉 |
| 摧毁 | `Health <= 0` | 停止工作，释放占用格子，可被穿过 |
| 暂停 | `IsManuallyPaused = true` | 停止工作，可恢复 |

### 2.2 建筑被摧毁后的行为

**核心规则：建筑被摧毁后，其占用的地块恢复空闲状态，士兵可穿过。**

#### 2.2.1 格子占用释放

当建筑生命值降至 0 时，自动调用 `Castle.ReleaseBuildingFootprint()` 释放所有占用的格子：

```csharp
// Castle.cs
public void ReleaseBuildingFootprint(Building building)
{
    IReadOnlyList<Vector2I> footprint = BuildingSystem.GetFootprint(building.TypeId);
    foreach (Vector2I offset in footprint)
    {
        int gridX = building.AnchorGridX + offset.X;
        int gridY = building.AnchorGridY + offset.Y;
        if (IsInBounds(gridX, gridY))
            _occupied[gridX, gridY] = false;
    }
}
```

#### 2.2.2 碰撞体保留

**建筑碰撞体始终保留**，用于：
- 检测敌方士兵是否站在建筑上（修复限制）
- 提供视觉反馈的碰撞基础

### 2.3 建筑修复限制

**规则：如果敌方士兵站在被摧毁的建筑上，夜晚商店无法修复该建筑。**

检测逻辑通过 `Building.HasEnemyOnTop` 属性实现：

```csharp
// Building.cs
public bool HasEnemyOnTop
{
    get
    {
        bool isPlayerCastle = CastleRef.IsPlayerCastle;
        foreach (Area2D area in GetOverlappingAreas())
        {
            if (area is Soldier soldier && soldier.IsAlive && soldier.IsPlayerUnit != isPlayerCastle)
                return true;
        }
        return false;
    }
}
```

修复时检查：

```csharp
// ShopSystem.cs
public bool TryRepairBuilding(Building building)
{
    // ... 其他检查 ...
    if (building.HasEnemyOnTop)
        return false;
    // ... 执行修复 ...
}
```

---

## 3. 士兵系统

### 3.1 士兵行为状态

| 状态 | 条件 | 行为 |
|------|------|------|
| 正常活动 | `IsAlive && IsActive` | 移动、追击、攻击 |
| 夜晚休眠 | `IsAlive && !HasNightCombat && IsNight` | 停止移动与攻击 |
| 夜战活动 | `IsAlive && HasNightCombat && IsNight` | 继续正常活动 |
| 死亡 | `Health <= 0` | 禁用碰撞、淡出、0.5s 后移除 |

### 3.2 士兵与建筑的交互

**规则：士兵忽略已摧毁的建筑，可直接穿过。**

士兵进入建筑碰撞区时，检查建筑是否已摧毁：

```csharp
// Soldier.cs - OnAreaEntered
Building building = area as Building;
if (building != null && !building.IsDestroyed)
{
    Castle castle = building.GetCastle();
    if (castle != null && castle.IsAlive && castle.IsPlayerCastle != IsPlayerUnit)
    {
        _targetCastle = castle;
        _targetBuilding = building;
    }
}
```

### 3.3 目标锁定优先级

1. **敌方士兵**（优先）
2. **敌方建筑**（其次，仅当建筑未被摧毁）
3. **敌方城堡**（默认推进目标）

### 3.4 目标更新逻辑

士兵每帧检查当前攻击目标是否仍有效：

```csharp
// Soldier.cs - _Process
if (_targetBuilding != null && _targetBuilding.IsDestroyed)
{
    _targetCastle = null;
    _targetBuilding = null;
}
```

---

## 4. 碰撞层设计

| 层 | 值 | 用途 |
|----|----|------|
| Layer 2 | 2 | 士兵单位 |
| Layer 3 | 4 | 建筑 |

**士兵碰撞配置：**
- `CollisionLayer = 2`（值 2）
- `CollisionMask = 6`（检测士兵 + 建筑）

**建筑碰撞配置：**
- `CollisionLayer = 4`（值 4）
- `CollisionMask = 0`（不主动检测其他对象）

---

## 5. 昼夜阶段对战斗的影响

### 5.1 白天阶段
- 出兵建筑按产出间隔正常产兵
- 士兵正常移动、追击、攻击
- 所有单位无休眠

### 5.2 夜晚阶段
- 无夜战词条的建筑停止产兵
- 无夜战词条的士兵停止移动与攻击，原地休眠
- 有夜战词条的单位继续正常行动
- 可在商店修复受损建筑（需满足修复条件）

---

## 6. 关键代码引用

| 功能 | 文件 | 关键方法/属性 |
|------|------|-------------|
| 建筑摧毁处理 | `Building.cs` | `OnDestroyed()`, `IsDestroyed` |
| 格子占用管理 | `Castle.cs` | `ReleaseBuildingFootprint()`, `IsCellPassable()` |
| 敌方检测 | `Building.cs` | `HasEnemyOnTop` |
| 士兵目标更新 | `Soldier.cs` | `OnAreaEntered()`, `_Process()` |
| 修复逻辑 | `ShopSystem.cs` | `TryRepairBuilding()` |

---

## 7. 流程图

```
建筑被摧毁
    ↓
释放格子占用 (Castle.ReleaseBuildingFootprint)
    ↓
碰撞体保留（用于修复检测）
    ↓
士兵检测到建筑 → 检查 IsDestroyed
    ↓
如果已摧毁 → 忽略，继续移动
    ↓
如果未摧毁 → 锁定攻击目标
```

---

## 8. 修复流程

```
夜晚商店打开
    ↓
玩家选择修复建筑
    ↓
检查：建筑受损 && 非核心建筑 && 属于玩家
    ↓
检查：建筑上无敌方士兵 (HasEnemyOnTop)
    ↓
扣除金币 (repairCost = (MaxHealth - Health) × RepairGoldPerHealth)
    ↓
执行修复 (Building.Repair())
```