# 战报系统设计 — M6

战报是 M6 的**核心产出**：记录玩家每个循环三个阶段结束时的城堡快照，永久保存后供敌方「回放式 AI」复刻。概念定义见 `devPlan/concepts/general.md`；敌方复刻规则见 `devPlan/design/replayAi.md`。

---

## 战报是什么

**战报（Battle Report）** = 一局游戏中，按循环序号排列的**玩家城堡快照**列表；每个循环含作战、商店、合成三张逻辑阶段快照（快照 `Phase` 沿用 `Day / Night / Combine` 命名），战报直接记录对局派系 `FactionId`。

- 每条快照在对应**阶段结束**时拍摄：作战结束（切商店前）、商店结束（切合成前）、合成结束（切作战前）
- 快照内容：玩家侧所有已放置建筑（类型、锚点格、生命值、手动暂停、禁止组合等实现所需字段），以及快照时玩家当前金币与手牌（类型、费用）
- 不包含敌方城堡；金币与手牌只记录玩家侧，不记录敌方

---

## 录制时机

```text
作战战斗 / 商店购物与放置 / 合成阶段组合
  → 阶段切换：Combat → Shop
      → CapturePhaseSnapshot(playerCastle, nightIndex, Day)   // 作战阶段结束
  → 阶段切换：Shop → Combine
      → CapturePhaseSnapshot(playerCastle, nightIndex, Night) // 商店阶段结束
  → CombineSystem.ResolveNightCombines(playerCastle)          // 执行组合
  → 阶段切换：Combine → Combat
      → CapturePhaseSnapshot(playerCastle, nightIndex, Combine) // 合成阶段结束
  → 下一天开始…
```

**`NightIndex`：** 本局第几个循环，从 1 起计；同一循环的三个阶段快照共享同一 `NightIndex`，用 `Phase`（Day / Night / Combine）区分。

局内维护**本局战报缓存**；每次存档（每天开始自动存档 / 暂停菜单手动存档）时把进度快照写入存档 `SaveData.BattleReportJson`，**不进入永久战报库**；结算时选择保留或丢弃。

---

## 保存与永久化

- **每次存档**：当前战报进度快照序列化为 JSON 存入存档 `SaveData.BattleReportJson`，随「继续游戏」恢复；存档中的进度快照**不进入永久战报库**
- **对局结束**（`GameState.GameOver`）：结算弹窗询问是否保留本局战报

  - **保留**：最终确定名称与时间，生成一条新战报写入永久战报库
  - **丢弃**：清空本局缓存，不产生任何战报（存档在结算时已删除）

已保存战报**永久保留**，可列表查看、删除（M6 最低：能选一条作为 AI 参考即可）。

---

## 快照数据结构（草案）

见 `devPlan/dataStructures.md` §战报。

单条快照至少包含：

| 字段 | 说明 |
|------|------|
| `NightIndex` | 循环序号 |
| `Phase` | 阶段（`Day` / `Night` / `Combine`） |
| `Buildings` | 建筑列表：`TypeId`、`AnchorGridX/Y`、多格 `Footprint` 偏移、`Health`、`IsManuallyPaused`、`IsCombineProhibited` |
| `Gold` | 快照时玩家当前金币（存款） |
| `HandCards` | 玩家手牌列表：`BuildingType`、`Cost` |

战报级字段 `FactionId`（对局派系，`alliance` / `horde` / `necromancer`）随战报一并持久化，供敌方亡灵判定与派系展示。

城堡之心若需记录可单独字段；M6 默认不复制核心建筑到敌方（见 AI 文档）。

---

## 系统职责

| 模块 | 职责 |
|------|------|
| `BattleReportSystem` | 局内缓存、阶段结束快照、每日保存/结算保存、持久化读写、战报列表 |
| `GameManager` | 阶段切换钩子触发三阶段快照；维护 `NightIndex` |
| `GameOverUiController` | 结算弹窗「是否保存战报」 |
| `ReplayAiSystem` | 开局选战报、作战开始时按 `Phase == Day` 快照复刻（见 `devPlan/design/replayAi.md`） |

---

## 验收标准（战报部分）

- [ ] 作战、商店、合成三个阶段结束自动追加玩家城堡快照到局内缓存
- [ ] 每次存档时进度快照随存档保存；存档中的进度快照不进入永久战报库
- [ ] 结算时可选择保存；保存后重启游戏仍可读取
- [ ] 不保存时本局缓存清除
- [ ] 快照含建筑类型与格位，与场上状态一致

---

## 不做事项（M6）

- 战报回放 UI 动画、逐夜回放演示
- 云端同步、分享
- 录制敌方或单位位置
