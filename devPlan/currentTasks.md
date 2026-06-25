# 当前任务

M0–M4 已完成。**当前焦点：M5 融合系统。**

规则以 `fusionSystemDesign.md` 为准：**入夜自动融合**；底部为「禁止融合」工具，**无**融合工具按钮。

## 已完成（M4 夜战单位）

- [x] `HasNightCombat` 接入 `BuildingSystem` 定义与 `InitFromType` / 产兵传递
- [x] 狼穴（`WolfDen`）夜战建筑，产狼人单位
- [x] 夜战单位夜晚豁免休眠；建筑信息面板显示「夜晚可行动 / 夜晚休眠」

## 下一步（M5）

### 1. 数据与配方

- [ ] `FusionRecipe`：`MainTypeId`、`MaterialTypeId`、`MaterialCount`、`GoldCost`、`ResultTypeId`
- [ ] `FusionSystem` 静态配方表与 `GetRecipeForGroup` / 按主体查询 API
- [ ] `BuildingSystem` 登记 `BarracksT2`、`WolfDenT2`（属性见 `fusionSystemDesign.md`）
- [ ] `Building` 增加 `IsFusionProhibited`（默认 `false`）

### 2. 融合逻辑（入夜自动）

- [ ] `CanFuseGroup(castle, main, materials)`：夜晚、玩家侧、配方匹配、辅材均与主体四向邻接、0 阶、非核心、未禁止、未被敌军占据、金币足够
- [ ] `TryFuseGroup(...)`：扣费 → 在**主体**锚点生成结果（**满血**）→ 移除所有辅材 → `AdjacentSystem.RefreshCastle`
- [ ] `FindFusibleGroups(castle)`：扫描所有合法 (主体 + 辅材) 组
- [ ] `ResolveNightFusions(castle)`：入夜循环选取并执行，直至无可融合组；选取规则见设计文档
- [ ] `GameManager` 切换至 `Night` 时调用 `ResolveNightFusions`（在开商店等夜晚流程之前）
- [ ] 产兵进度融合后清零；不继承材料生命比例与禁止标记

### 3. 禁止融合 UI

- [ ] 底部「禁止融合」工具按钮（与暂停、修复并列；**不**做「融合」按钮）
- [ ] `FusionProhibitUiController`：toggle 工具、左键切换建筑 `IsFusionProhibited`、自定义光标
- [ ] 右键 / Esc 退出工具；与暂停 / 修复互斥（`ButtonGroup`）
- [ ] 建筑状态图标或信息面板显示「已禁止融合」
- [ ] `UIManager` 接入 `Process` 与 `HandleInput`

### 4. 验证

- [ ] 两座邻接兵营入夜自动 → 强化兵营（间隔 4s、生命 130、满血）
- [ ] 两座邻接狼穴入夜自动 → 强化狼穴（可选，与兵营同期或紧随其后）
- [ ] 标记禁止融合后，入夜不融合该建筑
- [ ] 金币不足、非邻接主体、多格建筑、白天均不触发融合

## 暂不进入范围

- 靶场 / 马厩多格融合（M5.1）
- 手动点选融合的 UI
- AI 对手（M6）
- 完整设置界面

## 验收标准（M5）

- [ ] 入夜自动将满足配方、且辅材与主体邻接的单格建筑组融合为强化版
- [ ] 结果满血、扣费与占格释放正确
- [ ] 「禁止融合」工具可阻止指定建筑参与融合
- [ ] 融合与商店 / 修复 / 手牌 / 昼夜循环输入不冲突
