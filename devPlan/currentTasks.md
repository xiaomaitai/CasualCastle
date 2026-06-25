# 当前任务

M0–M5 已完成。**当前焦点：M6 战报与回放式敌方。**

设计文档：`battleReportDesign.md`（录制与存储）、`aiSystemDesign.md`（镜像复刻）、`concepts.md` §战报。

**M6 核心：** 战报 = 每夜**结束**时玩家城堡快照的集合；结算可永久保存；敌方**不自主决策**，每局选一条战报，在每晚**开始**时按序号镜像复刻。

---

## 已完成（M5 融合）

- [x] 入夜自动免费融合；`BarracksT2` / `WolfDenT2`
- [x] 「禁止融合」工具与 `IsFusionProhibited`

## 已完成（M4 夜战）

- [x] `HasNightCombat`；狼穴 / 狼人；夜晚豁免休眠

---

## 下一步（M6）

### 1. 数据与 `BattleReportSystem`

- [x] 落地 `BuildingSnapshot`、`CastleSnapshot`、`BattleReport`（见 `dataStructures.md`）
- [x] `scripts/battle_report/BattleReportSystem.cs` 挂 `main_game.tscn`
- [x] 局内 `NightIndex` 计数（与录制 / 复刻共用）
- [x] `CaptureNightSnapshot(Castle)`：遍历玩家城堡建筑，跳过 `CastleHeart`，写入类型 / 锚点 / 生命 / 暂停 / 禁止融合
- [x] 局内战报缓存：`StartMatch` 清空；每夜结束 `AppendSnapshot`
- [x] 持久化：`user://battle_reports/` 读写；`ReportId`、`DisplayName`、`SavedAtUnix`
- [x] API：`GetSavedReports()`、`LoadReport(id)`、`SaveCurrentReport(name)`、`DiscardCurrentReport()`

### 2. 录制钩子

- [x] `GameManager.BeginPhase(Day)`（夜晚→白天）时调用 `CaptureNightSnapshot`，固定顺序：夜晚开始时 `NightIndex++`，夜晚结束录制同序号快照
- [x] 首局：第一夜结束前缓存为空；第一夜结束产生 `NightIndex == 1` 的首条快照
- [ ] 验证：多格建筑（靶场、马厩）快照含正确锚点与 `TypeId`

### 3. 结算保存 UI

- [x] `GameOverUiController`：胜负展示后增加「是否记录战报？」（保存 / 不保存）
- [x] 保存 → `BattleReportSystem.SaveCurrentReport`；不保存 → `DiscardCurrentReport`
- [x] 返回标题后局内缓存已清理；已保存战报重启游戏仍可 `LoadReport`

### 4. 战报选择（开局）

- [x] 标题页或开局流程：列出已保存战报（名称 + 夜数），选一条作为本局敌方参考
- [x] 无战报时：敌方保持场景初始布局（当前双兵营），不阻断开局
- [x] 选定战报 ID 传入 `ReplayAiSystem` / `GameManager.StartGameSession`

### 5. `ReplayAiSystem` 镜像复刻

- [x] `scripts/replay/ReplayAiSystem.cs` 挂 `main_game.tscn`
- [x] `ApplyNightSnapshot(enemyCastle, nightIndex)`：读取战报 `NightIndex == nightIndex` 的条目
- [x] 格位**水平镜像**：玩家 `(anchorX, anchorY)` → 敌方镜像坐标（8×8 网格，多格按 footprint 计算）
- [x] 占格检测：任一格有存活**玩家** `Soldier` → 跳过该建筑
- [x] 同步：移除非核心冲突建筑 → `CreateBuilding` → `BindToGrid` → 恢复快照生命与状态
- [x] 不复制 `CastleHeart`；快照中已是 `BarracksT2` 等则直接放置，**不**对敌方再跑 `FusionSystem`
- [x] 无对应 `NightIndex` 条目：敌方布局不变

### 6. 入夜流程串联

- [x] `GameManager.BeginPhase(Night)` 顺序固定为：
  1. 更新本局「当前夜序号」`N`（第 N 夜开始）
  2. `FusionSystem.ResolveNightFusions(playerCastle)`
  3. `ReplayAiSystem.ApplyNightSnapshot(enemyCastle, N)`
  4. `PhaseChanged` → 商店等玩家夜晚流程
- [ ] 玩家融合、商店、修复、禁止融合行为不受影响（待完整回归验证）

### 7. 建筑与战场辅助

- [x] `BuildingSystem` / `Castle`：支持在**敌方城堡**按快照批量放置（或复用放置逻辑，去掉 `IsPlayerCastle` 限制的内部 API）
- [x] `Castle` 或战场工具：`IsCellOccupiedByPlayerSoldier(gridX, gridY)`（或等价检测）
- [x] 敌方建筑移除：`ReleaseBuildingFootprint` + `QueueFree`，不破坏城堡之心

### 8. 联调与验收

- [ ] **录制链**：打一局 → 至少经历 2 个夜晚结束 → 结算保存战报 → 重启后列表可见
- [ ] **复刻链**：选该战报新开一局 → 第 1 夜开始敌方与战报第 1 条快照镜像一致
- [ ] **挡格**：玩家士兵站在某格 → 该建筑本夜不复刻
- [ ] **序号**：第 2 夜开始应用第 2 条快照；战报无后续条目时敌方保持现状
- [ ] **回归**：手牌、商店、融合、夜战、暂停、修复正常；可打满至结算

---

## 暂不进入范围（M6）

- 敌方自主购卡、放置、融合、修复
- 战报逐夜动画回放、详情浏览 UI（超列表+选取即可）
- 云端同步、战报分享、删除以外的编辑
- M5.1 多格融合扩展
- `BattleSystem` 抽离、寻路
- 完整设置界面、通用存档

---

## 验收标准（M6）

- [ ] 每夜结束自动追加玩家城堡快照；字段与场上建筑一致
- [ ] 结算弹窗可永久保存或丢弃本局战报
- [ ] 开局可选一条已保存战报；入夜按 `NightIndex` 镜像复刻到敌方
- [ ] 玩家士兵占据的格不复刻对应建筑
- [ ] 完整单局可对战至胜负，玩家侧现有功能无回归

---

## 实现顺序建议

1. 数据结构 + `BattleReportSystem` 录制与持久化  
2. `GameManager` 夜末钩子 + 结算保存 UI  
3. 开局战报选择  
4. `ReplayAiSystem` + 入夜串联 + 敌方放置 API  
5. 端到端联调
