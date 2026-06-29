# 战报系统设计 — M6

战报是 M6 的**核心产出**：记录玩家每一夜结束时的城堡快照，永久保存后供敌方「回放式 AI」复刻。概念定义见 `devPlan/concepts.md`；敌方复刻规则见 `devPlan/design/replayAi.md`。

---

## 战报是什么

**战报（Battle Report）** = 一局游戏中，按夜晚序号排列的**玩家城堡快照**列表。

- 每条快照在**夜晚结束**（切回白天之前）拍摄
- 快照内容：玩家侧所有已放置建筑（类型、锚点格、生命值、手动暂停、禁止融合等实现所需字段）
- 不包含敌方城堡、手牌、金币

---

## 录制时机

```text
夜晚进行中（商店 / 修复 / 融合等）
  → 玩家操作或等待
  → 阶段切换：Night → Day（夜晚结束）
      → BattleReportSystem.CaptureNightSnapshot(playerCastle, nightIndex)
  → 白天战斗…
  → 下一夜开始…
```

**`NightIndex`：** 本局第几次**结束**的夜晚，从 1 起计。首夜结束记为 1，第二夜结束记为 2。

局内维护**本局战报缓存**（尚未持久化），结算时可选择写入永久库。

---

## 结算与永久保存

对局结束（`GameState.GameOver`）时，结算弹窗增加：

- 文案询问：**是否将本局战报保存为永久记录？**
- **保存**：写入本地持久化（`user://battle_reports/` 或项目约定路径），获得唯一 `ReportId`、可选默认名称（日期时间）
- **不保存**：丢弃本局缓存，不影响已有战报

已保存战报**永久保留**，可列表查看、删除（M6 最低：能选一条作为 AI 参考即可）。

---

## 快照数据结构（草案）

见 `devPlan/dataStructures.md` §战报。

单条快照至少包含：

| 字段 | 说明 |
|------|------|
| `NightIndex` | 夜晚序号 |
| `Buildings` | 建筑列表：`TypeId`、`AnchorGridX/Y`、多格 `Footprint` 偏移、`Health`、`IsManuallyPaused`、`IsFusionProhibited` |

城堡之心若需记录可单独字段；M6 默认不复制核心建筑到敌方（见 AI 文档）。

---

## 系统职责

| 模块 | 职责 |
|------|------|
| `BattleReportSystem` | 局内缓存、夜晚结束快照、结算保存/丢弃、持久化读写、战报列表 |
| `GameManager` | 夜晚结束钩子触发快照；维护 `NightIndex` |
| `GameOverUiController` | 结算弹窗「是否保存战报」 |
| `ReplayAiSystem` | 开局选战报、入夜复刻（见 `devPlan/design/replayAi.md`） |

---

## 验收标准（战报部分）

- [ ] 每夜结束自动追加玩家城堡快照到局内缓存
- [ ] 结算时可选择保存；保存后重启游戏仍可读取
- [ ] 不保存时本局缓存清除
- [ ] 快照含建筑类型与格位，与场上状态一致

---

## 不做事项（M6）

- 战报回放 UI 动画、逐夜回放演示
- 云端同步、分享
- 录制敌方或士兵位置
