# 冒烟清单与基线

> 运行规范见 `devPlan/design/smokeTestGuidelines.md`：一律经 `tools/run_smoke.sh <场景>` 执行，契约「退出码 0 且输出含 `SMOKE: PASS`」。
> 以下为 v0.7 G5 收尾后的最新基线（2026，Godot 4.6.2 本机无头运行全 PASS）。

## 全冒烟清单（8 项）

| 场景 | 验证内容 |
|---|---|
| `scenes/ui/dev_tools/audio_smoke.tscn` | 音频总线 / 资源 / AudioManager 接线 |
| `scenes/ui/dev_tools/ui_smoke.tscn` | 手牌 / 商店 / 属性面板等 UI 状态 |
| `scenes/ui/dev_tools/building_action_ring_smoke.tscn` | 行动环布局、卖出/暂停/升级/仪式交互（含确认出售） |
| `scenes/ui/dev_tools/dungeon_smoke.tscn` | 地下城技能效果 / 内容缺口 / 显示名 |
| `scenes/ui/dev_tools/necromancer_smoke.tscn` | 死灵光环 / 仪式 / 门（纯逻辑） |
| `scenes/ui/dev_tools/necromancer_battle_smoke.tscn` | 死灵真实对局：夜晚产兵 / 研习 / T2 升级 / 死亡圣殿组合 / 仪式阶段门控 |
| `scenes/ui/dev_tools/alliance_battle_smoke.tscn` | 联盟真实对局：夜晚产兵 / 兵营→军府升级 / 剑士产出 |
| `scenes/ui/dev_tools/horde_battle_smoke.tscn` | 部落真实对局：夜晚产兵 / 核心 L1→L2（兽穴前置）/ 哥布林工坊升级 / 爆破手产出 |

## 变更记录

- v0.7 G5：补建 `alliance_battle_smoke` / `horde_battle_smoke` 场景入口（此前只有脚本、无场景，从未运行）；修复升级链断言（部落核心升级步骤、`HordeCoreRules` 等级、升级后产兵加速、等待时长）。8 项全 PASS。
