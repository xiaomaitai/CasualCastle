# v0.7 完善版 — G2 高阶单位技能化（当前任务）

> 版本规划见 `devPlan/outline/c05Milestones.md`。本文件细化当前阶段 **G2**。
> 前置：G1 已完成；skill_defs 全表数据已从 09deb3c 恢复干净（68f5ac4）。

目标：终局兵种有差异机制——联盟 T4/T5 高阶单位技能配置 + AuraService 支持 Human、死亡天使差异化被动，带单测。

## 设计依据（allianceFaction.md / necromancerFaction.md）

- 圣盾守护者（圣壁产）：范围减伤光环，周围友军受伤害 -20%
- 国王（王庭产）：全屏光环（伤害+攻速+回复），每 30s 召唤一队皇家禁卫
- 皇家神射手（皇家射场产）：超远程高射速，对 Beast +30%（无载体，改减速，见 todo.md）
- 龙骑统帅（天罚塔产）：骑兵+射手合体，马上骑射（kiting 放风筝）
- 皇家骑士（皇家马场产）：最高单体冲击力，冲锋首目标 2x
- 皇家斥候（皇家游骑产）：永久潜行（不被远程锁定），全图最高视野
- 暗影之主（暗影圣所产）：潜行 + 攻击不显形（keep_on_attack）+ 背刺孤立目标（no_battle_report 无载体，见 todo.md）
- 死亡天使（死灵仪式产）：穿刺高伤，终焉狂暴（HP≤30% 攻击+30%）

## 实施清单

### A. 域层机制（已完成 ✅）

- [x] A1. AuraService 支持 Human（IsAuraEligibleRace/IsReceiverEligible）
- [x] A2. damage_reduction 减伤光环（受击乘算，Unit.TakeDamage）
- [x] A3. Charge 冲锋首击 2x（_chargeUsed 标记）
- [x] A4. Summon 召唤（IUnitEventPort.OnSummon + SpecialConfig.UnitType + Unit 定时）
- [x] A5. KeepOnAttack 攻击不显形（SpecialConfig + TryAttack 破隐判断）

### B. 数据配置（已完成 ✅）

- [x] B1. skill_defs 新增 8 个：king_aura / king_summon / holy_guardian_aura / royal_knight_charge / shadow_lord_stealth / royal_scout_stealth / dragon_rider_kiting / death_angel_rage
- [x] B2. unit_skills 关联 10 条（含 ShadowLord→backstab 复用、RoyalSharpshooter→slowing_strike 复用）
- [x] B3. 皇家神射手对 Beast +30%：无落地载体（无 Beast 护甲/种族/野怪表），改复用 slowing_strike，记入 todo.md

### C. 死亡天使差异化被动（已完成 ✅）

- [x] C1. 终焉狂暴：`{"trigger":"low_health","trigger_param":0.3,"modifiers":{"attack_damage_mult":1.30}}`（机制已有，数据配置 + 加载测试）

### D. NoBattleReport 战报过滤（调整为记录）

- [x] D1. 结论：战报为敌方城堡快照式（无击杀记录），机制无消费载体；枚举+解析保留，暗影之主改配 backstab（target_isolated 2x），记入 todo.md

## 验收标准

- [x] 每个联盟高阶单位至少一个可生效技能（config + 运行时生效）
- [x] AuraService Human 单测覆盖
- [x] 新增机制均有单测（AllianceEndgameSkillTests 7 个 + RealBugHighTierUnitSkillsTests 18 个）；dotnet test 全绿、dotnet build 零 warning
- [x] pre-commit 自动更新 config_db_dump.sql
