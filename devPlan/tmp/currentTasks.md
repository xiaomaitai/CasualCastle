# v0.7 完善版 — G1 派系机制修复（当前任务）

> 版本规划见 `devPlan/outline/c05Milestones.md`（v0.7 完善版 G1~G5）。本文件仅细化当前阶段 **G1**。

目标：让四派系「设计已定但实际失效」的机制真正生效——修复隐藏 bug（闪避失效、死建筑、无效技能、错误配方），死建筑清零，全部带单元测试。

## 完成项

- [x] 1. 联盟鱼龙混杂闪避失效：config.db `dodge_chance` 0.0→1.0 + RealBug 测试（提交 d5f3441）
- [x] 2. 联盟 T4/T5 断链：补齐五条线升级链、补 RangerPost 节点、天罚塔/暗影圣所改组合、圣壁修正 + 可达性 RealBug 测试（提交 f8b50cf）
- [x] 3. 冒险者公会 T2 卡死：满编种族数 6→4 + RealBug 测试（提交 9f033fd）
- [x] 4a. 万变迷宫陷阱上限翻倍（TrapCapDoubled 置 true 路径）
- [x] 4b. 迷魂之音光环范围 +50%（AuraRangeMultiplier 置 1.5 路径）
- [x] 4c. 极乐领域「侍奉建筑效果翻倍」（SlaaneshServiceRules doubled 参数 + RealBug 测试）
- （4a/4b/4c 一并提交 51c1abf）

## 4. 地下城：剩余功能级子项（需机制/数值决策）

- 色孽恩赐（主动技）：实现「舒适度消耗减半」效果 —— 需主动技能激活机制（UI 触发 + 定时器）
- 宴会厅「每夜金币」：每夜结束时获得少量金币 —— 需确认金额与挂载点（PhaseTransitionUseCase）
- 享乐舞台「周期攻击 Buff」：周期性为周围友军施加短暂攻击 Buff —— 需确认周期/时长/数值
- 测试：每项一个单测（技能激活后行为变化）

## 5. 死灵法师：黑魔法数值 DB 可配

- 现状：黑魔法 ±25/10 硬编码在 domain 常量（BlackMagicService），不可调
- 修复：迁移为 config.db 可配（读取一次，端口注入；domain 保持零 Godot）
- 测试：DB 配置读取 + 默认值回退单测

## 验收标准

- [x] 全库无死建筑：每个 T2+ 建筑至少一条可达配方（RealBug 可达性单测）
- [ ] 四派系「设计内机制」实测生效（单测覆盖，冒烟不阻塞本阶段）
- [ ] `dotnet test` 全绿、`dotnet build` 零 warning
- [x] 改动提交时 pre-commit 自动更新 config_db_dump.sql
