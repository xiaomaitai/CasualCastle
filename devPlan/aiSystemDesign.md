# AI 对手系统设计 — M6

敌方城堡在昼夜循环中自主购卡、放置建筑，并在入夜时参与自动融合，形成可单人对战的完整 MVP。概念入口见 `concepts.md`；架构归属见 `outline/c05ProjectArchitecture.md`。

---

## 目标与范围

**M6 交付：** 玩家 vs AI 的完整对局——敌方拥有独立金币与手牌逻辑，夜晚能扩建城堡，白天/夜晚产兵与战斗与玩家对称（受昼夜与夜战规则约束）。

**首版不做：**

- 敌方建筑修复、禁止融合工具、商店 UI
- 复杂战术（集火、绕后、兵种克制）
- 寻路、BattleSystem 抽离
- 存档、难度档位、AI 性格

**可延后（M6.1）：** 敌方修复受损建筑、更智能的放置评分、多难度。

---

## 现状与缺口

| 能力 | 玩家 | 敌方（当前） |
|------|------|--------------|
| 初始兵营 | 1 座（可配置第二座） | 2 座（场景配置） |
| 金币 | `ShopSystem.Gold` | 无 |
| 手牌 | `CardSystem` | 无 |
| 购卡 / 放置 | 商店 + 手牌 UI | 无 |
| 入夜融合 | `FusionSystem`（仅 `IsPlayerCastle`） | 无 |
| 产兵 / 战斗 | 已有 | 已有（初始兵营） |

**需改造：**

- `BuildingSystem.TryPlace` 现限制 `castle.IsPlayerCastle`，需开放 AI 放置入口
- `ShopSystem` / `CardSystem` 现绑定玩家 UI 流程，需抽出敌方经济或平行的 `EnemyAiState`
- `FusionSystem.ResolveNightFusions` 现仅玩家城堡，需对敌方城堡调用
- `GameManager.BeginPhase(Night)` 现只融合玩家侧

---

## 时机与触发

```text
PhaseChanged → Night
  → FusionSystem.ResolveNightFusions(playerCastle)
  → FusionSystem.ResolveNightFusions(enemyCastle)    // M6 新增
  → AISystem.OnNightBegin()                          // 购卡 + 放置
  → （玩家侧）ShopSystem 开商店 UI

PhaseChanged → Day
  → （可选）AISystem.OnDayBegin()  // M6 首版可不在白天购卡，仅夜晚行动
```

**与玩家对齐：** M6 首版敌方仅在**夜晚**执行经济回合（购卡并立即放置），与玩家「夜晚打开商店」节奏一致。白天仅依赖已有建筑产兵。

---

## 敌方经济

### 金币

- 敌方独立金币池，初始值 `GameConfig.InitialGold`（与玩家相同，可配置项 `EnemyInitialGold` 预留）
- 不显示在 HUD；调试可用 `GD.Print` 或开发面板

### 购卡

- 复用 `ShopSystem.Catalog` 商品表与费用
- 从目录随机或按权重抽取可负担卡牌，加入敌方虚拟手牌（列表即可，无需 UI）
- 敌方手牌上限与玩家相同（`CardSystem.MaxHandSize`）

### 放置

- 购得卡牌后**同回合内**尝试放置到敌方城堡合法空地
- 放置成功则从敌方手牌移除；失败则保留至下一夜再试（或丢弃最弱卡——实现时择一，文档默认**保留**）

---

## 放置决策（首版）

扫描敌方城堡所有可放置 `(anchorX, anchorY)` × 卡牌 `BuildingType` 组合，按简单评分选最优：

| 优先级 | 规则 |
|--------|------|
| 1 | 能放置且花费后金币仍 ≥ 预留值（如 0） |
| 2 | 单格建筑优先贴邻已有同类型建筑（便于入夜融合） |
| 3 | 兵营优先放在已有兵营四向邻格 |
| 4 | 否则选距己方城堡之心最近的可通行边格（朝玩家方向扩建） |
| 5 | 同分取 `(AnchorGridY, AnchorGridX)` 字典序最小 |

多格建筑（靶场、马厩）：M6 首版**可购买但低优先级**；若无合法占地则跳过该卡。

---

## 入夜融合（敌方）

- 规则与玩家完全相同（`fusionSystemDesign.md`）：免费、主体邻接、满血结果
- 敌方**无**禁止融合标记
- 在 `GameManager` 入夜流程中，玩家融合完成后对 `EnemyCastle` 调用 `ResolveNightFusions`

---

## 系统职责

| 模块 | 职责 |
|------|------|
| `AISystem` | 敌方金币、手牌、夜晚购卡与放置决策；入夜/入昼钩子 |
| `FusionSystem` | 扩展为可对任意 `Castle` 调用（已具备，仅需 GameManager 双侧调用） |
| `BuildingSystem` | 提供 `TryPlaceForCastle(castle, ...)` 或放宽 `TryPlace` 供 AI 使用 |
| `GameManager` | 入夜顺序：双侧融合 → `AISystem.OnNightBegin` |
| `ShopSystem` | 提供目录与单价查询；玩家扣费逻辑不变 |

信号建议：`AiActionPerformed(Castle, string actionType)`（调试/日志用，非必须）。

---

## 验收标准（M6）

- [ ] 开局后敌方除初始兵营外，随夜晚推进会新增建筑
- [ ] 敌方金币不足时不购卡；购卡后放置到合法地块
- [ ] 敌方邻接双兵营/双狼穴入夜可融合为强化版
- [ ] 玩家与敌方士兵、建筑均受昼夜 / 夜战规则约束
- [ ] 可打满一局并正常结算胜负
- [ ] 玩家侧商店、手牌、融合禁止等现有功能不受影响

---

## 实现任务拆分

见 `currentTasks.md` M6 章节。

---

## 后续扩展（M6.1+）

- 白天微操（补放置、紧急购卡）
- 敌方修复、保留金币策略
- 难度：初始金、购卡频率、放置评分权重
- AI 融合禁止（模拟玩家策略）
