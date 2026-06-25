# 当前任务

M0–M5 已完成。**当前焦点：M6 AI 对手。**

规则见 `aiSystemDesign.md`；融合规则见 `fusionSystemDesign.md`。

## 下一步（M6）

### 1. 基础与放置入口

- [ ] `scripts/ai/AISystem.cs` 挂 `main_game.tscn`
- [ ] `BuildingSystem` 开放非玩家城堡放置（如 `TryPlaceForCastle`），AI 与玩家共用验证逻辑
- [ ] 敌方独立金币与虚拟手牌（不经过玩家 UI）

### 2. 夜晚经济回合

- [ ] `AISystem.OnNightBegin`：按 Catalog 购卡（够钱、手牌未满）
- [ ] 放置决策：邻接同类优先、朝玩家方向扩建（见 `aiSystemDesign.md` 评分表）
- [ ] 购后放置失败则保留手牌至下一夜

### 3. 入夜融合（双侧）

- [ ] `GameManager.BeginPhase(Night)` 对 `EnemyCastle` 也调用 `ResolveNightFusions`
- [ ] 顺序：玩家融合 → 敌方融合 → AI 购卡放置 → 玩家商店 UI

### 4. 联调与验收

- [ ] 完整单局：敌方随夜晚扩建、可融合，胜负正常
- [ ] 玩家商店 / 手牌 / 禁止融合不受影响
- [ ] （可选）结算优化、已知 Bug 修复

## 暂不进入范围（M6）

- M5.1 多格融合、2 阶链式融合
- 敌方修复、禁止融合、难度档位
- BattleSystem 抽离、寻路
- 完整设置界面、存档

## 验收标准（M6）

- [ ] 敌方夜晚自主购卡并放置建筑
- [ ] 敌方入夜自动融合（与玩家同规则）
- [ ] 完整单人对战 AI 可玩至结算