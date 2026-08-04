# 战报系统设计 — M6

战报是 M6 的**核心产出**：记录玩家每个昼夜循环三个阶段结束时的城堡快照，永久保存后供敌方「回放式 AI」复刻。概念定义见 `devPlan/concepts.md`；敌方复刻规则见 `devPlan/design/replayAi.md`。

---

## 战报是什么

**战报（Battle Report）** = 一局游戏中，按循环序号排列的**玩家城堡快照**列表；每个循环含白天、夜晚、合成三张阶段快照。

- 每条快照在对应**阶段结束**时拍摄：白天结束（切夜晚前）、夜晚结束（切合成前）、合成结束（切白天前）
- 快照内容：玩家侧所有已放置建筑（类型、锚点格、生命值、手动暂停、禁止组合等实现所需字段）
- 不包含敌方城堡、手牌、金币

---

## 录制时机

```text
白天战斗 / 夜晚商店与放置 / 合成阶段组合
  → 阶段切换：Day → Night
      → CapturePhaseSnapshot(playerCastle, nightIndex, Day)   // 白天阶段结束
  → 阶段切换：Night → Combine
      → CapturePhaseSnapshot(playerCastle, nightIndex, Night) // 夜晚阶段结束
  → CombineSystem.ResolveNightCombines(playerCastle)          // 执行组合
  → 阶段切换：Combine → Day
      → CapturePhaseSnapshot(playerCastle, nightIndex, Combine) // 合成阶段结束
  → 下一天开始…
```

**`NightIndex`：** 本局第几个昼夜循环，从 1 起计；同一循环的三个阶段快照共享同一 `NightIndex`，用 `Phase`（Day / Night / Combine）区分。

局内维护**本局战报缓存**；**每天开始**时自动原地更新到永久库，结算时选择保留或丢弃。

---

## 保存与永久化

- **每天开始**（`PhaseChanged → Day`）：将本局缓存**原地更新**到一条**进度战报**并永久保存（同一局共用同一 `ReportId`，不重复建档）
- **对局结束**（`GameState.GameOver`）：结算弹窗询问是否保留本局战报

  - **保留**：最终确定名称与时间，复用进度战报条目
  - **丢弃**：移除本局已保存的进度战报并清空缓存

已保存战报**永久保留**，可列表查看、删除（M6 最低：能选一条作为 AI 参考即可）。

---

## 快照数据结构（草案）

见 `devPlan/dataStructures.md` §战报。

单条快照至少包含：

| 字段 | 说明 |
|------|------|
| `NightIndex` | 昼夜循环序号 |
| `Phase` | 阶段（`Day` / `Night` / `Combine`） |
| `Buildings` | 建筑列表：`TypeId`、`AnchorGridX/Y`、多格 `Footprint` 偏移、`Health`、`IsManuallyPaused`、`IsCombineProhibited` |

城堡之心若需记录可单独字段；M6 默认不复制核心建筑到敌方（见 AI 文档）。

---

## 系统职责

| 模块 | 职责 |
|------|------|
| `BattleReportSystem` | 局内缓存、阶段结束快照、每日保存/结算保存、持久化读写、战报列表 |
| `GameManager` | 阶段切换钩子触发三阶段快照；维护 `NightIndex` |
| `GameOverUiController` | 结算弹窗「是否保存战报」 |
| `ReplayAiSystem` | 开局选战报、白天开始时按白天阶段快照复刻（见 `devPlan/design/replayAi.md`） |

---

## 验收标准（战报部分）

- [ ] 白天、夜晚、合成三个阶段结束自动追加玩家城堡快照到局内缓存
- [ ] 结算时可选择保存；保存后重启游戏仍可读取
- [ ] 不保存时本局缓存清除
- [ ] 快照含建筑类型与格位，与场上状态一致

---

## 不做事项（M6）

- 战报回放 UI 动画、逐夜回放演示
- 云端同步、分享
- 录制敌方或单位位置
