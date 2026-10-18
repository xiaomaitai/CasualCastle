# 音频需求清单（v0.6 · E0 定稿）

## 一、说明

- 本清单由 v0.6 E0「场景与音频需求确定」产出，是素材采购（E5）与系统实现（E2/E3）的**唯一依据**
- 后续新增场景、派系或玩法行为，必须先更新本清单再采购素材与实现

## 二、场景清单

| 场景 | 文件 | 形态 | 说明 |
|------|------|------|------|
| 标题 | `scenes/ui/title_screen.tscn` | 独立场景 | 主菜单（派系/战报选择、开始、设置、退出） |
| 主战场 | `scenes/main/main_game.tscn` | 独立场景 | 核心玩法，叠加状态见下 |
| 设置面板 | `scenes/ui/settings_panel.tscn` | 叠加面板 | 标题与主战场内均可打开 |
| 开发者工具 | `scenes/dev/`、`scenes/ui/dev_tools/` | 独立场景 | 不进正式包，不要求音频 |

主战场叠加状态：白天、夜晚、商店面板、暂停菜单、建筑放置预览、结算（胜利/失败）、设置面板。

## 三、BGM 需求（15 首，曲目池轮播）

### 3.1 播放形式

- BGM 按**曲目池轮播**，不单曲循环：播放完一首 → 静默间隔（3~6 秒）→ 播下一首
- 池内默认随机顺序（不立即重复刚播过的曲目），顺序播放可配置
- 主战场曲目按**派系 × 昼夜**分组：联盟/部落各有白天池与夜晚池，不同派系主战场 BGM 不同
- 场景/状态切换用约 1 秒交叉淡化；池内切换用静默间隔衔接

### 3.2 曲目池

| 编号 | 曲目池 | 使用场景/状态 | 数量 |
|------|--------|-------------|------|
| B1–B3 | 标题池 | 标题 | 3 |
| B4–B6 | 联盟·白天池 | 主战场·联盟·白天（含白天叠加状态） | 3 |
| B7–B8 | 联盟·夜晚池 | 主战场·联盟·夜晚 | 2 |
| B9–B11 | 部落·白天池 | 主战场·部落·白天 | 3 |
| B12–B13 | 部落·夜晚池 | 主战场·部落·夜晚 | 2 |
| B14–B15 | 结算池 | 结算 | 2 |

命名：`bgm_title_1..3`、`bgm_alliance_day_1..3`、`bgm_alliance_night_1..2`、`bgm_horde_day_1..3`、`bgm_horde_night_1..2`、`bgm_result_1..2`。

### 3.3 切换规则

- 标题 → 对局：标题池 → 所选派系白天池（派系由存档/标题选择决定）
- 白天 ↔ 夜晚：同派系昼夜池切换（交叉淡化）
- 结算：切入结算池；返回标题切回标题池
- 暂停/商店/放置/设置不换池，跟随当前主状态
- 新增派系（亡灵/地下城等）时，为其新增白天/夜晚曲池并登记清单

### 3.4 BGM 编号、风格与入库

BGM 统一编号 1–15（与 3.2 的 B1–B15 对应），编号、曲名、风格、曲目池与 Godot 资源 uid 登记在 `assets/data/config.db` 的 `bgm_defs` 表（uid 取自各 `.ogg` 的 `.import` 文件，与建筑 `texture_uid` 同机制）；运行时由 `GameDataLoader.BgmCatalog` 加载，`AudioManager` 按 uid 加载曲目并轮播，不硬编码资源路径。

| 编号 | 文件 | 曲名 | 风格 |
|------|------|------|------|
| 1 | bgm_title_1.ogg | Menu Music (awesomeness) | 舒缓 |
| 2 | bgm_title_2.ogg | Medieval: The Old Tower Inn | 舒缓 |
| 3 | bgm_title_3.ogg | Shrine | 舒缓 |
| 4 | bgm_alliance_day_1.ogg | Town Theme RPG | 舒缓 |
| 5 | bgm_alliance_day_2.ogg | Happy Adventure (Loop) | 激昂 |
| 6 | bgm_alliance_day_3.ogg | Medieval: Minstrel Dance | 激昂 |
| 7 | bgm_alliance_night_1.ogg | Mysterious Ambience (song21) | 舒缓 |
| 8 | bgm_alliance_night_2.ogg | Night Prowler | 激昂 |
| 9 | bgm_horde_day_1.ogg | Battle Theme A | 激昂 |
| 10 | bgm_horde_day_2.ogg | Fast Fight / Battle Music | 激昂 |
| 11 | bgm_horde_day_3.ogg | Epic Boss Battle (Juhani Junkala) | 激昂 |
| 12 | bgm_horde_night_1.ogg | Dark Forest Theme | 舒缓 |
| 13 | bgm_horde_night_2.ogg | Battle RPG Theme (CleytonRX) | 激昂 |
| 14 | bgm_result_1.ogg | Determined Pursuit (Epic Orchestra Loop) | 激昂 |
| 15 | bgm_result_2.ogg | Lament for a Warrior's Soul | 舒缓 |

天规则（仅作用于主战场四池：联盟/部落 × 白天/夜晚）：

- 第一天（`CurrentNightIndex = 0`）：只播舒缓曲目；池内无舒缓曲目时回退整池
- 第二天起（`CurrentNightIndex ≥ 1`）：只播激昂曲目；池内无激昂曲目时回退整池
- 标题池与结算池不受天数影响，全部曲目照常轮播
- 规则实现见 `scripts/domain/Shared/BgmSelectionRules.cs`（含单元测试）

开发者模式（`DisplaySettingsManager.DevModeEnabled`）下，标题与主战场左上角小字显示当前曲目，格式 `BGM {编号} · {曲名}`。

## 四、SFX 需求（25 个事件）

| 编号 | 事件名 | 归属 | 触发点 | 用途 |
|------|--------|------|--------|------|
| S1 | `button_click` | 全局 UI | 所有按钮点击 | 交互反馈 |
| S2 | `ui_start` | 标题 | 开始按钮 | 开局确认 |
| S3 | `slider_change` | 设置面板 | 音量/拖拽滑条调整 | 调节反馈 |
| S4 | `unit_select` | 主战场 UI | 点击/选中单位（即使无后续操作） | 选中反馈 |
| S5 | `building_select` | 主战场 UI | 点击/选中建筑（即使无后续操作） | 选中反馈 |
| S6 | `shop_open` | 主战场 UI | 商店打开 | 面板反馈 |
| S7 | `shop_close` | 主战场 UI | 商店关闭 | 面板反馈 |
| S8 | `buy` | 主战场 UI | 商店购买成功 | 经济反馈 |
| S9 | `coin_change` | 主战场 UI | 金币增减 | 经济反馈 |
| S10 | `place` | 主战场 UI | 建筑放置成功 | 建造反馈 |
| S11 | `place_cancel` | 主战场 UI | 取消放置 | 取消反馈 |
| S12 | `repair` | 主战场 UI | 建筑修复 | 修复反馈 |
| S13 | `combine` | 主战场 UI | 手动组合完成 | 组合反馈 |
| S14 | `upgrade` | 主战场 UI | 建筑升级完成 | 升级反馈 |
| S15 | `melee_attack` | 主战场·战斗 | 近战单位攻击 | 战斗反馈 |
| S16 | `projectile_launch` | 主战场·战斗 | 远程单位发射弹体 | 战斗反馈 |
| S17 | `projectile_hit` | 主战场·战斗 | 弹体命中 | 战斗反馈 |
| S18 | `hit` | 主战场·战斗 | 单位受击掉血 | 战斗反馈 |
| S19 | `unit_death` | 主战场·战斗 | 单位死亡 | 战斗反馈 |
| S20 | `building_hit` | 主战场·战斗 | 建筑受击 | 战斗反馈 |
| S21 | `building_destroy` | 主战场·战斗 | 建筑被摧毁 | 战斗反馈 |
| S22 | `phase_change` | 主战场·全局 | 白天/夜晚切换 | 阶段提示 |
| S23 | `victory` | 主战场·结算 | 摧毁敌方城堡 | 胜利播报 |
| S24 | `defeat` | 主战场·结算 | 我方城堡被摧毁 | 失败播报 |
| S25 | `hit_slash` | 主战场·战斗 | 斩击伤害命中（剑/斧等 Slash 伤害） | 受击反馈 |
| S26 | `hit_pierce` | 主战场·战斗 | 穿刺伤害命中（矛/箭/弩等 Pierce 伤害） | 受击反馈 |
| S27 | `hit_magic` | 主战场·战斗 | 魔法伤害命中（Magic 伤害） | 受击反馈 |
| S28 | `melee_slash` | 主战场·战斗 | 近战斩击起手（Slash 近战单位挥砍） | 攻击反馈 |
| S29 | `melee_pierce` | 主战场·战斗 | 近战穿刺起手（Pierce 近战单位突刺） | 攻击反馈 |
| S30 | `launch_arrow` | 主战场·战斗 | 弓箭类发射（Pierce 远程单位拉弓） | 攻击反馈 |
| S31 | `launch_magic` | 主战场·战斗 | 魔法施放（Magic 远程单位施法） | 攻击反馈 |

## 五、数量汇总

- BGM：**15 首**（标题 3 / 联盟白天 3 / 联盟夜晚 2 / 部落白天 3 / 部落夜晚 2 / 结算 2）
- SFX：**31 个事件**（S1–S31），每事件 1 个基础音效；高频事件（S1/S15/S18/S19 等）可加变体，不增加事件数
- 素材文件预计：BGM 15 个 + SFX 31 个起，共 46 个起

## 六、素材来源策略

- **优先从网上查找免费素材**：CC0 / Public Domain 优先（如 Kenney、OpenGameArt、Freesound、itch.io 免费素材包），确保可商用、无署名与再分发限制
- 实在没有合适素材（风格不匹配、质量不足、找不到对应事件）时，才用 **AI 定制**作为兜底，不默认使用
- BGM 15 首与 SFX 25 个事件均先按此策略寻找；挑选结果与每个素材的**来源、许可**在 E5 落地时登记
- 若只能找到带署名要求（CC-BY 类）的素材，需先确认游戏可接受署名再采用，否则换选

## 七、变更规则

- 新增场景/状态：先在本清单登记（场景、BGM 归属、SFX 事件），再采购素材与实现
- 新增派系：先在本清单登记其白天/夜晚曲池（BGM 数量随之增加）
- 新增玩法行为：对应新增 SFX 事件，先登记再实现
- 删除玩法行为：同步从清单移除对应事件
