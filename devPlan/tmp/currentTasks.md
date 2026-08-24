# v0.7 完善版 — G1 派系机制修复（当前任务）

> 版本规划见 `devPlan/outline/c05Milestones.md`（v0.7 完善版 G1~G5）。本文件仅细化当前阶段 **G1**。

目标：让四派系「设计已定但实际失效」的机制真正生效——修复隐藏 bug（闪避失效、死建筑、无效技能、错误配方），死建筑清零，全部带单元测试。

## 1. 联盟：鱼龙混杂闪避失效

- 现状：`skill_defs.motley_crew` 的 `config_json.dodge_chance = 0.0`，而 `GetDodgeChance` 从该值乘算 → 冒险者闪避恒为 0
- 修复：config.db 中 `dodge_chance` 0.0 → 1.0
- 测试：`RealBug` 单测验证 4 种族（不同种族数量）闪避 = 设计值（4 种族 30%），并对照 SkillSetTests 现有基准

## 2. 联盟：T4/T5 组合链断链（死建筑）

- 现状：射手线/斥候线缺配方，皇家射场、皇家游骑、天罚塔、暗影圣所不可达；天罚塔被误配为单入边升级；皇家壁垒来源误为军府
- 修复（config.db，经科技树编辑器或 sqlite 直改）：
  - upgrade_recipes 补 3 条：射楼→皇家射场、斥候营→游骑哨、游骑哨→皇家游骑
  - tech_tree_nodes 补 RangerPost（斥候线节点）行
  - 天罚塔：删除「皇家射场→天罚塔」升级边，改为 combine_recipes「皇家射场+皇家马场→天罚塔」（跨线组合）
  - 皇家壁垒：修正为「壁垒→皇家壁垒」升级（来源 Armory→RoyalBulwark 删除），并确认 shop_catalog 可直购
- 测试：可达性单测/脚本——遍历联盟 tech_tree，断言每个 T2+ 建筑至少一条可达配方；combine_recipes 与 upgrade_recipes 入边数与设计一致（入边>1=组合、=1=升级）

## 3. 联盟：冒险者公会 T2 卡死

- 现状：公会 T2 满编需 6 种族，当前仅 4 种族 → 升级即永久卡队无法出发
- 决策：满编种族数 6→4（与当前可组队种族数对齐；新增种族线属后续扩展，不入本版本）
- 测试：TavernPartyService 单测更新为 4 种族满编出发

## 4. 地下城：选了无效技能补齐

- 色孽恩赐（主动技）：实现「舒适度消耗减半」效果，接 DemonKingController
- 万变迷宫「陷阱上限翻倍」：实现 TrapCapDoubled 实际置 true 的路径（万变迷宫在场时陷阱上限 4→8）
- 极乐领域「侍奉建筑效果翻倍」：实现（当前仅舒适上限 +50）
- 宴会厅「每夜金币」、享乐舞台「周期攻击 Buff」：效果落地
- 测试：每项一个单测（技能激活后行为变化），技能池加载断言非空配置

## 5. 死灵法师：黑魔法数值 DB 可配

- 现状：黑魔法 ±25/10 硬编码在 domain 常量（BlackMagicService），不可调
- 修复：迁移为 config.db 可配（读取一次，端口注入；domain 保持零 Godot）
- 测试：DB 配置读取 + 默认值回退单测

## 验收标准

- [ ] 全库无死建筑：每个 T2+ 建筑至少一条可达配方（脚本/单测验证）
- [ ] 四派系「设计内机制」实测生效（单测覆盖，冒烟不阻塞本阶段）
- [ ] `dotnet test` 全绿、`dotnet build` 零 warning
- [ ] 改动提交时 pre-commit 自动更新 config_db_dump.sql
