# 当前任务：零散 QOL 收尾（掉落物飘字 / 金币飘字 / 建筑耐久条）

> 本次决策：**不飘「击杀」字样**，只在死亡位置显示实际掉落物（金币/经验/黑魔法，有则显示）；建筑耐久条**仅受损时显示**；产兵倒计时圆环**不做**（保留 `building_work.gdshader` 填充 + 属性面板剩余秒数）。核心建筑已有 `ProgressBar HealthBar`，不重复做。

## 背景与现状（已核对代码）

**掉落物来源盘点**（单位死亡时实际结算的资源）：

| 派系 | 结算位置 | 资源 | 数值 |
| --- | --- | --- | --- |
| 死灵 | `BlackMagicService.OnUnitDied(isFriendly, race)` | 黑魔法 | 我方阵亡 +25 / 敌方阵亡 +10；`UndeadRules.IsNonLiving(race)` 为真则 0 |
| 地下城 | `KingExperienceService.OnUnitDied(isFriendly)` | 经验 | 我方阵亡 +5 / 敌方阵亡 +10（`DemonKingRules.FriendlyDeathXp` / `EnemyDeathXp`） |
| 地下城 | `KingExperienceService.OnSmallCrystalMined()` | 小魔晶经验 | +15，仅当被击杀敌方 `UnitCost >= DemonKingRules.BossDropCostThreshold`(5000) |
| 地下城 | `KingExperienceService.OnKingKill()` | 魔王击杀经验 | `int(15 × KillXpMultiplier)`，击杀者为魔王种族时 |
| 任意 | — | 金币 | **不存在**：数据层无击杀奖励字段（`Bounty`/`kill_reward` 均无匹配） |

**接入点**：`scripts/adapters/godot/battle/DeathSideEffects.cs`（`Process()` 已按 `shop.FactionId` 分流死灵/地下城，`UnitLogic.OnDied` 第 425 行调用）；飘字复用 `scripts/adapters/godot/ui/FloatingText.cs`（现为世界空间、伤害/治疗样式，受 `DisplaySettingsManager.ShowDamageNumbersEnabled` 控制）。

**金币变化**：`Shop.GoldChanged` → `HudUiController.UpdateGoldDisplay`（已有缩放跳动）+ `ShopUiController`（夜晚商店），飘字需**屏幕空间**实现，`FloatingText` 是世界空间不能直接复用。

**建筑耐久条**：`Building.HealthChanged`（Godot 信号，`Building.cs` 第 62/389 行）、`Building.IsDamaged`、`BuildingSystem.GetBuildingVisualSizePixels(typeId)` 齐备；`BuildingStateIcon` 是「`Effects` 下代码创建 Node2D、ZIndex 16」的现成范式；`prefabs/HealthBar.tscn` + `HealthBarView`（`Empty`/`Full` 两张纹理 + `Fill` + `Ping(persistent)`）可直接复用，`Ping(true)` 即常显不淡出。

## 步骤

1. **掉落数值由 domain 返回**（供 adapter 展示，避免在 adapter 重写规则）：
   - `BlackMagicService.OnUnitDied(...)` 返回本次实际增加量（非活体 → 0）
   - `KingExperienceService.AddExperience(int)` 返回实际生效量（`<= 0` 或已满级 L5 → 0），`OnUnitDied` / `OnKingKill` / `OnSmallCrystalMined` 随之返回实际值
   - 补 domain 单测：黑魔法我方 25 / 敌方 10 / 非活体 0；经验我方阵亡 5 / 敌方阵亡 10 / 满级 0 / 魔王击杀含 `KillXpMultiplier` / 小魔晶 15
2. **掉落飘字**（世界空间）：`DeathSideEffects.Process()` 收集掉落项（黑魔法紫 / 经验青），在死亡单位位置逐个飘出「+N」，复用 `FloatingText` 并扩展为可按「资源样式」出字（前缀 `+`、指定颜色、非暴击）；无掉落项时不飘字；受 `ShowDamageNumbersEnabled` 控制（与伤害/治疗数字同一开关）
3. **金币变化飘字**（HUD 屏幕空间）：新增 `scripts/adapters/godot/ui/UiFloatingText.cs`（CanvasLayer 下，参数：文本、颜色、锚点偏移），`HudUiController` 缓存上一次 `Gold`，在 `GoldChanged` 回调里按差值飘「+N」绿 /「−N」红，锚定 `GoldLabel`；覆盖购卡、出售、升级、修建筑、仪式奖励、开局设值；**不受** `ShowDamageNumbersEnabled` 控制（HUD 反馈，非战场数字）
4. **建筑耐久条**（仅受损显示）：新增 `scripts/adapters/godot/building/BuildingHealthBar.cs`（代码创建 Node2D 挂 `Effects`，ZIndex 16，内含 `prefabs/HealthBar.tscn` 实例或 `HealthBarView`），订阅 `Building.HealthChanged` → 更新 `Fill`，`IsDamaged` 时显示、满血隐藏；宽度按 `GetBuildingVisualSizePixels(typeId)` 缩放，位置在建筑视觉顶边之上（`-visualSize.Y / 2 - gap`）；`IsCoreBuilding` 直接跳过（核心有 Castle 血条）
5. **文档同步**：`c05Milestones.md` G4 中「金币变化/击杀飘字待做」「建筑耐久条待做」两处改为已实现并注明形态（掉落物飘字，无「击杀」字样）；若 `uiDesign.md` 有 HUD 飘字/建筑血条小节则同步

## 验收项

- [ ] 死灵派系：我方单位阵亡飘「+25」黑魔法紫字、敌方单位阵亡飘「+10」，位置为死亡单位处；亡灵等非活体阵亡不飘（**待实机确认**：faction 冒烟不含单位死亡，见下）
- [ ] 地下城派系：敌方阵亡飘「+10」经验、我方阵亡飘「+5」经验；被击杀敌方 `UnitCost ≥ 5000` 时额外飘小魔晶「+15」；魔王击杀时额外飘「+15 × 倍率」（**待实机确认**）
- [x] 魔王满级（L5）时经验飘字不再出现（实际生效量为 0）——`AddExperience_AtMaxLevel_ReturnsZero` 单测覆盖
- [x] 掉落数值链路由 domain 单测覆盖：`OnUnitDied` 我方 25 / 敌方 10 / 非活体 0、经验 5/10、魔王击杀含倍率、小魔晶 15（Domain 657 全绿，本次 +6 例）
- [x] 金币变化飘字：`building_action_ring` 运行期探针实测触发 `+1 / -3 / +1`（`prev/now` 差值正确），无异常后已移除探针
- [x] 建筑耐久条：运行期探针实测建筑 `visual=(186,186)` → `scale=0.3174`、`pos=(0,-113.66)`（血条整体位于建筑视觉顶边之上），无异常后已移除探针
- [x] `dotnet build CasualCastle.sln` 零 warning；三套单测全绿（Domain 657 / Persistence 35 / Application 25）
- [x] 全量 8 项冒烟 PASS 且无 `Exception` / `SCRIPT ERROR`（audio / ui / building_action_ring / dungeon / necromancer / necromancer_battle / alliance_battle / horde_battle）
- [x] 新增 `.cs` 已用 `godot-import` 无头导入生成 `.uid` 侧车，`tools/check_uids.py` PASS
- [x] 文档同步：`c05Milestones.md` 两处「待做」标注已更新；`uiDesign.md` §展示 QOL 增补三条；`aiIndex.md` 补 `BuildingHealthBar.cs` / `UiFloatingText.cs` / `DeathSideEffects.cs`
- [ ] 实机目视确认：死亡掉落数字（死灵/地下城）、金币飘字、建筑耐久条；以及关闭「显示伤害数字」后掉落飘字消失

## 施工记录

- domain：`BlackMagicService.OnUnitDied/Add`、`KingExperienceService.OnUnitDied/OnKingKill/OnCrystalUsed/OnSmallCrystalMined/AddExperience` 改为返回实际生效量（满级或非正数返回 0），供 adapter 展示而不重写规则。
- adapter 新增：`scripts/adapters/godot/ui/UiFloatingText.cs`（HUD 屏幕空间飘字）、`scripts/adapters/godot/building/BuildingHealthBar.cs`（受损显示的建筑耐久条，复用 `prefabs/HealthBar.tscn`）。
- adapter 改动：`DeathSideEffects.cs`（掉落收集 + 出字，`ShowDrops` 受 `ShowDamageNumbersEnabled` 控制）、`FloatingText.cs`（新增 `SpawnGain` 资源样式，颜色改为显式传入）、`HudUiController.cs`（金币差值飘字）、`BuildingView.cs`（创建耐久条并订阅 `HealthChanged`，核心建筑跳过）。
- 未做：产兵倒计时圆环（用户决策不做）；金币掉落不存在（数据层无击杀奖励字段）。
