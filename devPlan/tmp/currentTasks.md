# v0.7 完善版 — G2 高阶单位技能化（当前任务）

> 版本规划见 `devPlan/outline/c05Milestones.md`。本文件细化当前阶段 **G2**。
> 前置：G1 已完成；skill_defs 全表数据已从 09deb3c 恢复干净（68f5ac4）。

目标：终局兵种有差异机制——联盟 T4/T5 高阶单位技能配置 + AuraService 支持 Human、死亡天使差异化被动，带单测。

## 设计依据（allianceFaction.md / necromancerFaction.md）

- 圣盾守护者（圣壁产）：范围减伤光环，周围友军受伤害 -20%
- 国王（王庭产）：全屏光环（伤害+攻速+回复），每 30s 召唤一队皇家禁卫
- 皇家神射手（皇家射场产）：超远程高射速，对 Beast +30%
- 龙骑统帅（天罚塔产）：骑兵+射手合体，马上骑射（高速+远程，属性已满足）
- 皇家骑士（皇家马场产）：最高单体冲击力，冲锋首目标 2x
- 皇家斥候（皇家游骑产）：永久潜行（不被远程锁定），全图最高视野
- 暗影之主（暗影圣所产）：潜行 + 攻击不显形 + 被击杀目标不产生战报
- 死亡天使（死灵仪式产）：穿刺高伤，需新增差异化被动

## 实施清单

### A. 域层机制（新增 + 单测）

- [ ] A1. AuraService 支持 Human：`IsAuraEligibleRace`/`IsReceiverEligible` 增加 Human（现仅 horde/undead/demon）
- [ ] A2. damage_reduction 减伤光环类型：AuraBonusTypes/AuraEffects/AuraService.Apply/GameContext/SkillService/Unit.TakeDamage（受击乘算）
- [ ] A3. Charge 冲锋首击 2x：Unit.TryAttack 首目标乘 DamageMult（新增 _chargeUsed 标记）
- [ ] A4. Summon 召唤：IUnitEventPort 增加 OnSummon，Unit 按 cooldown 定时触发，SpecialConfig 增加 UnitType 字段

### B. 数据配置（config.db）

- [ ] B1. skill_defs 新增：king_aura / king_guard_summon / holy_guardian_aura / royal_knight_charge / shadow_lord_stealth / shadow_lord_no_report / royal_scout_stealth / dragon_rider_kiting
- [ ] B2. unit_skills 关联：King/HolyShieldGuardian/RoyalKnight/ShadowLord/RoyalScout/DragonRiderCommander/RoyalSharpshooter
- [ ] B3. 皇家神射手对 Beast +30%（克制系统或技能，先查 damage_matrix）

### C. 死亡天使差异化被动

- [ ] C1. 确定被动（候选：斩杀低血 / 吸血 / 亡语），配置 + 单测

### D. NoBattleReport 战报过滤

- [ ] D1. History 战报录制处：被暗影之主击杀的单位不写入战报

## 验收标准

- [ ] 每个联盟高阶单位至少一个可生效技能（config + 运行时生效）
- [ ] AuraService Human 单测覆盖
- [ ] 新增机制均有单测；dotnet test 全绿、dotnet build 零 warning
- [ ] pre-commit 自动更新 config_db_dump.sql
