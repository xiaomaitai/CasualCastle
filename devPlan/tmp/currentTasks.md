# 当前任务：亡灵派系完成（死灵法师）

阶段说明：亡灵派系含亡灵巫术 / 黑暗魔法 / 祭仪三流派。设计基准见 `devPlan/design/factions/necromancerFaction.md`，按文档实现阶段分三步落地：v0.3（T1+T2 + 黑魔法资源 + 夜行机制 + 仪式之环基础框架）→ v0.4（T3 + 黑暗魔法 + 基础仪式）→ v0.5（T4+T5 + 完整祭仪）。本期先完成 v0.3 全链路。

## 一、设计定稿（先于实现，同步更新 necromancerFaction.md）

- [x] 夜行机制：`has_night_combat=1` 的单位/建筑在**夜晚阶段**生产与战斗，白天/合成休眠；亡灵对局白天阶段缩短为 1s 过渡，避免空转
- [x] 黑魔法数值：敌方阵亡 +10、己方阵亡 +25；非生命体（机械/元素/构造体 race）不产出；上限与消耗点随 v0.4 黑暗魔法/仪式定稿
- [x] 亡灵巫术 T1/T2 数值表：墓穴/腐化之地（Shop）→ 大墓地/腐化沼泽（单边升级，费 = 价值差），4 兵种数值与技能（毒击、死亡毒云）
- [x] 驱散规则：神圣伤害对亡灵单位 ×2（死灵大君稳定光环免疫留到 v0.5）
- [x] 仪式之环基础框架：核心建筑 `RitualCircleL1~L5`，v0.3 先用金币升级占位（仪式系统 v0.4 替换）；T2 升级要求核心 Lv≥2
- [x] 升级规则对齐 D2：亡灵 T2 走 `upgrade_recipes` 单边升级，不做同类双融合

## 二、数据层（config.db）

- [x] `building_defs`：墓穴/腐化之地/大墓地/腐化沼泽 + `RitualCircleL1~L5`，`has_night_combat=1`
- [x] `unit_stats`：骷髅兵/僵尸/骷髅战士/瘟疫僵尸（race=Undead，`has_night_combat=1`）
- [x] `skill_defs` + `unit_skills`：毒击（on_hit 减速/毒）、死亡毒云（special 死亡爆发）
- [x] `tech_tree_nodes` + `upgrade_recipes`：亡灵 4 建筑 + 仪式之环升级边；`shop_catalog`：墓穴/腐化之地/仪式之环
- [x] 驱散走 race 规则，伤害矩阵不改
- [x] 改库后由 pre-commit 自动更新 `config_db_dump.sql`

## 三、domain 层（纯 C#，必须带单测）

- [x] `BlackMagicService`：死亡收集黑魔法（己方/敌方差异、非生命体免疫、数值变化事件）
- [x] `UndeadRules`：`IsUndead`、神圣驱散倍率、非生命体 race 集合
- [x] `UnitStats`/`Unit` 增加 `HasNightCombat`；夜行活动门控规则
- [x] 瘟疫僵尸死亡毒云：死亡时对周围敌人范围伤害（special）
- [x] `RitualCircleRules`：核心等级判定、升级门槛、升级配方生成；`UpgradeRules` 扩展亡灵门槛（核心 Lv≥2 才可升级 T2）
- [x] dotnet build 零 warning

## 四、adapter 接线

- [x] `UnitLogic.IsActive/IsSleeping` 按单位 `HasNightCombat` 判定；`Building.CanWork` 按建筑 `has_night_combat`
- [x] `GameManager`：亡灵对局白天阶段 1s 过渡；现有 PhaseChanged/战报/存档流程回归
- [x] HUD 显示黑魔法（亡灵派系可见，订阅事件刷新）
- [x] `TitleScreen` 派系选项改为 `faction_defs` 驱动（加入死灵法师）

## 五、素材与收尾（v0.3）

- [x] 4 兵种主图 + 技能图标（generate-image skill）并 godot-import
- [ ] 游戏内实测：夜晚产兵/战斗、白天休眠、黑魔法随阵亡增长
- [x] 单测全过、dotnet build 零 warning

## 后续阶段（v0.4 / v0.5，当前不展开）

- v0.4：死亡圣殿+死亡骑士（狂热光环）、黑暗魔法线（暗黑法师+黑魔法施法）、基础仪式 2~3 种、亡灵光环框架扩展
- v0.5：鲜血祭坛/血骑士（吸血）、亡灵巫术禁殿/导师（反魔法护罩）、死者之门/死灵大君（稳定光环）、完整仪式、素材与实测