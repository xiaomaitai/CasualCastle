# P4 融合系统扩展

## 目标

移除单格建筑限制，让多格建筑（2×2）能正常参与融合，并添加占地兼容性校验。

## 背景

P3 王国军 T1+T2 已完成，5 条融合配方已入库（Barracks+Barracks→Armory 等），但 `IBuildingRepository.IsFusibleMaterial` 仍检查 `Footprint.Length == 1`，导致所有 2×2 T1 建筑无法通过 `CanParticipate` 校验，融合完全被阻塞。

## 任务拆解

### 4.1 移除单格限制 ✅

`IBuildingRepository.IsFusibleMaterial` 删除 `bd.Footprint.Length == 1` 条件。

**验收项：**
- 2×2 T1 建筑（Barracks/ShieldCamp/ArcheryRange/Stable/ScoutCamp）的 `IsFusibleMaterial` 返回 true
- CastleHeart 仍返回 false（`IsCore == true`）
- T2 建筑仍返回 false（`FusionTier != 0`）

### 4.2 占地兼容性校验 ✅

`FusionRules.CanFuseGroup` 增加主体与辅材占地大小一致性检查。

**验收项：**
- 同占地建筑可融合（2×2 + 2×2 ✓）
- `CanFuseGroup` 在校验材料类型前先比对待融合建筑的 footprint 长度

### 4.3 集成测试 ⏳

验证入夜后 2×2 建筑正常触发融合。需在 Godot 运行验证。

**验收项：**
- 在兵营旁放置兵营 → 入夜后两座兵营融合为军府
- 军府占领原主体兵营的格位，满血、产兵进度清零
- 五条融合链均可正常执行
- 仅邻接的同类建筑触发融合，不相邻的不会误融
