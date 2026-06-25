# 八、下一步开发内容（立即行动）

**当前焦点：M5 融合系统。**

M4 夜战单位已完成：`HasNightCombat` 数据化、狼穴产狼人、夜晚豁免休眠。

建议按 `../fusionSystemDesign.md` 与 `../currentTasks.md` 推进：

1. **配方与强化建筑数据**
   - `FusionRecipe`（含 `MainTypeId`、辅材数量）+ `FusionSystem` 配方表
   - `BuildingSystem` 登记 `BarracksT2`、`WolfDenT2`
   - `Building.IsFusionProhibited`

2. **入夜自动融合**
   - `FindFusibleGroups` / `TryFuseGroup`：辅材均与主体邻接、满血结果、扣费
   - `ResolveNightFusions` + `GameManager` 入夜钩子
   - 融合后刷新 `AdjacentSystem`

3. **禁止融合 UI**
   - 底部「禁止融合」工具按钮（无融合工具按钮）
   - 与暂停 / 修复互斥

4. **验收**
   - 邻接双兵营入夜 → 强化兵营（4s 产兵、130 生命、满血）
   - 禁止融合标记生效；金币不足不融合

**当前可玩基线：** 标题页 → 昼夜循环 → 商店 / 手牌放置 → 邻接加成 → 夜晚修复与暂停 → 狼穴夜战 → 对战 → 结算。

---
