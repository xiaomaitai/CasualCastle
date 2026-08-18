# 行为清单与基线（Phase 一）

记录时间：2026-08-17。本文件是迁移期间的行为基线：每个待迁移链路的可重复验证方式与已知缺口。
基线变化或任务完成时同步更新，迁移全部完成后按清理规则清空。

## 一、基线结果

| 检查 | 命令 | 结果 |
| --- | --- | --- |
| 解决方案构建 | dotnet build CasualCastle.sln | 成功，0 警告 0 错误 |
| 领域单元测试 | dotnet test tests/CasualCastle.Domain.Tests/CasualCastle.Domain.Tests.csproj | 592 通过 / 0 失败 |
| 存档往返测试 | dotnet test tests/CasualCastle.Persistence.Tests/CasualCastle.Persistence.Tests.csproj | 5 通过 / 0 失败 |
| UID 校验 | python tools/check_uids.py | PASS 0 问题 |
| 架构约束校验 | python tools/check_architecture.py | PASS 0 问题（已接入 pre-commit） |
| 无头导入 | godot --headless --import | 成功 |
| 无头冒烟 | 见下方冒烟清单 | 全部 PASS |
| 主场景装配 | godot --headless --quit-after 240 scenes/main/main_game.tscn | 240 帧无脚本错误 |

冒烟清单（自动退出，打印 SMOKE: PASS/FAIL）：

| 场景 | 覆盖 |
| --- | --- |
| scenes/ui/dev_tools/probe/phase_cycle_probe.tscn | 阶段循环探针 |
| scenes/ui/dev_tools/audio_smoke.tscn | 音频总线/资源/管理器 |
| scenes/ui/dev_tools/dungeon_smoke.tscn | 地下城数据/商店过滤/核心规则/舒适度/经验/技能池/魔王战斗 |
| scenes/ui/dev_tools/necromancer_smoke.tscn | 死灵数据/黑魔法/狂热光环/施法/仪式 |
| scenes/ui/dev_tools/necromancer_battle_smoke.tscn | 法术栏 UI/夜晚产兵/仪式研习/阶段门控 |
| scenes/ui/dev_tools/building_action_ring_smoke.tscn | 建筑行动环/暂停/出售/升级/确认弹窗 |
| scenes/ui/dev_tools/ui_smoke.tscn | 14 个面板场景装配、Esc 模态优先级（设置→暂停）、暂停状态投影、商店开关 |

## 二、行为清单

### 建筑（待迁移：Building.cs，目标见 Phase 四）

| 行为 | 规则/实现 | 现有验证 | 缺口 |
| --- | --- | --- | --- |
| 生产/工作/暂停 | Building.cs 工作循环 | building_action_ring_smoke | 生产进度无领域测试（Phase 四补 BuildingRuntime 聚合测试） |
| 受损/修复 | Building.cs TakeDamage / RepairRules | RepairRulesTests、BuildingActionRulesTests | 摧毁再生链无 smoke |
| 摧毁/再生 | Building.cs | — | 无自动验证（Phase 四补聚合测试与 smoke） |
| 仪式研习/完成 | RitualService / RitualCircleRules | RitualServiceTests、RitualCircleRulesTests、necromancer_battle_smoke | — |
| 陷阱/宝箱 | TrapService / TreasureBox | TrapServiceTests、TrapRulesTests、TrapCapTests、TreasureBoxTests、dungeon_smoke | — |
| 活体生长/繁殖 | LivingBuildingRules | LivingBuildingRulesTests | 繁殖链无 smoke |

### 单位（待迁移：UnitLogic.cs，目标见 Phase 五）

| 行为 | 规则/实现 | 现有验证 | 缺口 |
| --- | --- | --- | --- |
| 生成装配 | UnitSpawn（adapter） | — | 生成装配无单测/smoke（Phase 五统一工厂时补） |
| 攻击/索敌 | Unit / CombatRules / TacticalQueryService | CombatRulesTests、TacticalQueryServiceTests、UnitBehaviorTests | — |
| 死亡/尸体 | CorpseService / UnitLogic.OnDied | CorpseServiceTests、UndeadCombatTests、RealBugShadowElementTests | 死亡副作用（掉落/酒馆解绑/黑魔法）无测试（Phase 五事件化时补） |
| 隐身/光环/技能 | StealthService / AuraService / SkillService | StealthServiceTests、AuraServiceTests、SkillServiceTests、SkillSetTests、FurySkillTests、FlameSlashTests、HealGraceTests | — |

### 阶段切换（GameManager / MatchPhaseMachine）

| 行为 | 规则/实现 | 现有验证 | 缺口 |
| --- | --- | --- | --- |
| 阶段推进/计时/夜数 | MatchPhaseMachine | MatchPhaseMachineTests（8 个） | — |
| 阶段→战报阶段映射 | PhaseMapping | PhaseMappingTests（3 个） | — |
| 战报快照触发 | PhaseTransitionUseCase.Handle | phase_cycle_probe、PhaseTransitionUseCaseTests | 快照内容等价性测试待补 |
| 自动存档触发 | PhaseTransitionUseCase（night>=1 进入 Combat） | PhaseTransitionUseCaseTests（触发时机已覆盖）、SaveDatabaseTests（存储层） | — |

### 商店与手牌

| 行为 | 规则/实现 | 现有验证 | 缺口 |
| --- | --- | --- | --- |
| 刷新/购买/派系 | Shop / ShopRules | ShopTests、ShopRulesTests、RealBugShopFactionTests | — |
| 手牌/出售/物品卡 | Hand / CardRules / CardData | HandSellTests、CardRulesTests、CardDataTests | — |
| 书籍解锁/仪式可用 | ShopBookAvailability / DarkMagicBookRules | ShopBookAvailabilityTests、DarkMagicBookRulesTests | — |

### 存档（Phase 零已修）

| 行为 | 规则/实现 | 现有验证 | 缺口 |
| --- | --- | --- | --- |
| 全字段往返 | SaveDatabase（纯 C#） | SaveDatabaseTests（5 个） | — |
| 旧档兼容 | 列迁移 + InitManager 旧档回填 | LegacySaveWithoutNewColumnsLoadsWithDefaults | — |
| 运行时卡牌属性 | SaveDatabase attributes 列 | RuntimeModifiedCardAttributesSurviveRoundTrip | — |
| 恢复映射 | Application.GameSessionService.RestoreSession | GameSessionServiceTests（22 个用例测试含恢复/回填） | Godot 侧端到端读档 smoke 待补 |

### UI 模态输入（待迁移：UIManager.cs，目标见 Phase 三）

| 行为 | 规则/实现 | 现有验证 | 缺口 |
| --- | --- | --- | --- |
| 建筑行动环/暂停/确认弹窗 | BuildingActionRingController | building_action_ring_smoke | — |
| 购买/拖放/选卡 | ShopUiController / HandUiController | — | 无自动验证（Phase 三组件化时补 UI smoke） |
| 暂停/设置/结算模态 | PauseMenu/Settings/GameOverUiController | — | 无自动验证（Phase 三模态栈时补） |
| Esc 优先级/输入屏蔽 | UIManager._Input 长链 | — | 无自动验证（Phase 三 UiInteractionState 时补） |

## 三、状态唯一写入者（迁移期间约束）

迁移任何链路时，新旧实现不得同时修改同一状态；本表是当前基线的唯一写入者记录。

| 状态 | 唯一写入者 | 现状 | 计划去向 |
| --- | --- | --- | --- |
| 金币 | Shop（TrySpendGold/AddGold） | 已收口 | 保持 |
| 阶段/夜数/计时 | MatchPhaseMachine | 已收口 | 保持 |
| 手牌内容 | Hand | 已收口 | 保持 |
| 建筑生命/暂停/组合禁止 | Building（Godot 节点） | 由 Adapter 直接写 | Phase 四迁入 BuildingRuntime |
| 单位生命/目标/冷却/状态 | domain Unit 意图方法 | 已收口 | 保持 |
| 阵营资源（黑魔法/法力/魔晶） | BlackMagicService/ManaService 单例 | 单位持有同一单例 | Phase 五改为对局级资源池 |
| 存档文件 | GameSessionService → SaveStorage → SaveDatabase | 已收口（Phase 二：用例唯一入口） | 保持 |
| 战报快照 | BattleReportSystem → BattleReportService | 已收口（Phase 二）：PhaseTransitionUseCase 单一触发 | 保持 |
| 阶段副作用（商店刷新/结算/复刻/自动存档） | PhaseTransitionUseCase | 已收口（Phase 二）：GameManager 只调用用例+发信号 | 保持 |
| UI 输入屏蔽 | SetInputBlocked 分散调用 | 多处调用 | Phase 三统一 UiInteractionState |
