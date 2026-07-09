# P5 邻接加成扩展

## 目标

扩展邻接加速从"仅兵营同类型"到"全部王国军 T1+T2 同线互认"，并调整数值。

## 背景

P3 王国军 10 座建筑已落地。当前 `AdjacentRules.CalculateWorkSpeedMultiplier` 使用精确 TypeId 匹配和 +20% 加成，需要改为：
- T1 与 T2 同线互认（如 Barracks ↔ Armory）
- -15% 产兵间隔（即 work speed = 1/0.85 ≈ 1.176），可叠加


## 任务拆解

### 5.1 同线判定 ✅

`FusionRules` 新增 `IsSameLine(typeA, typeB)`：两个建筑类型相同，或通过融合配方链关联（一个的 main_type 对应另一个的 result_type）。

修改 `AdjacentRules.CalculateWorkSpeedMultiplier` 和 `AdjacencyService.GetAdjacentSameTypeTargets`，将精确 TypeId 匹配替换为 `FusionRules.IsSameLine`。

**验收项：**
- Barracks ↔ Armory 判定为同线
- ShieldCamp ↔ Bulwark 判定为同线
- ArcheryRange ↔ CrossbowTower 判定为同线
- Stable ↔ Ranch 判定为同线
- ScoutCamp ↔ RangerPost 判定为同线
- Barracks ↔ Stable 判定为非同线
- 同类型（Barracks ↔ Barracks）判定为同线

### 5.2 加成数值修正 ✅

`CalculateWorkSpeedMultiplier` 返回值从 `1 + 0.2*n` 改为 `1 / (0.85^n)`（乘法叠加 -15% 产兵间隔）。

**验收项：**
- 0 邻接：multiplier = 1.0
- 1 邻接：multiplier = 1/0.85 ≈ 1.176（产兵间隔 -15%）
- 2 邻接：multiplier = 1/0.7225 ≈ 1.384（产兵间隔 -27.75%）

### 5.3 集成测试

在 Godot 运行验证邻接加速的视觉效果和实际产兵加速。

**验收项：**
- 两座兵营相邻放置 → 产兵加速线连接 → 产兵间隔从 8s 缩短至约 6.8s
- 兵营 + 军府相邻 → 同样触发邻接加速（同线互认）
- 三座同线建筑链式邻接 → 叠加加速
- 不同线建筑相邻（如兵营+靶场）→ 不触发加速
- UI 信息面板正确显示邻接关系
