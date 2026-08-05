# 十三、音乐与音效（v0.6 大纲计划）

## 一、定位

v0.6 是音频完成体阶段：为 CasualCastle 建立完整的音乐与音效体系，覆盖音频总线架构、BGM 曲目、SFX 事件、音量设置与素材管线。目标不是"加几个声音"，而是形成可持续扩展的音频基础——后续派系、玩法与场景只需接入事件名即可发声。

## 二、现状

| 项 | 现状 |
|----|------|
| BGM | 仅 `assets/audio/bgm/feel_good_island_loop.ogg` 一首，硬编码在 `BgmPlayer.cs` |
| 挂载方式 | 标题与主战场场景各挂一个 `AudioStreamPlayer`（BGM 节点），切场景直接重启 |
| SFX | 无 |
| 音频总线 | 无自定义总线，全部走默认 Master |
| 音量设置 | 无；设置面板仅窗口/分辨率/开发者模式/拖拽速度 |
| 持久化 | 显示设置存 `user://display_settings.cfg`（ConfigFile），可复用 |

## 三、场景与音频需求确定（E0）

E0 是 v0.6 的第一步：先确定"有哪些场景、要几个 BGM、几个音效、分别用在什么地方"，产出唯一权威的音频需求清单。素材采购（E5）与系统实现（E2/E3）都以清单为准；后续新增场景必须先更新清单再补音频。

### 3.1 场景盘点（当前）

| 场景 | 文件 | 形态 | 说明 |
|------|------|------|------|
| 标题 | `scenes/ui/title_screen.tscn` | 独立场景 | 标题演示战场 + 主菜单 |
| 主战场 | `scenes/main/main_game.tscn` | 独立场景 | 白天/夜晚/商店/暂停/放置/结算为叠加状态 |
| 设置面板 | `scenes/ui/settings_panel.tscn` | 叠加面板 | 标题与主战场内均可打开 |
| 开发者工具 | `scenes/dev/`、`scenes/ui/dev_tools/` | 独立场景 | 不进正式包，不要求音频 |

### 3.2 BGM 需求

| 场景/状态 | 曲目 | 数量 | 用途 |
|---------|------|------|------|
| 标题 | `bgm_title` | 1 | 标题氛围 |
| 主战场·白天 | `bgm_day` | 1 | 战斗进行 |
| 主战场·夜晚 | `bgm_night` | 1 | 商店与休整 |
| 结算 | `bgm_result` | 1 | 胜负结果 |

规则：暂停/商店/放置等叠加状态不换曲，跟随当前主状态（白天曲或夜晚曲）。**BGM 共 4 首。**

### 3.3 SFX 需求

| 归属 | 事件 | 用途 |
|------|------|------|
| 全局 UI | `button_hover`、`button_click` | 所有按钮悬停/点击 |
| 标题 | `ui_start` | 开始游戏 |
| 设置面板 | `slider_change` | 音量/拖拽滑条调整 |
| 主战场·UI | `shop_open`、`shop_close` | 商店开合 |
| 主战场·UI | `buy`、`coin_change` | 购买与金币变化 |
| 主战场·UI | `place`、`place_cancel`、`repair` | 放置/取消放置/修复 |
| 主战场·UI | `combine`、`upgrade` | 组合/升级完成 |
| 主战场·战斗 | `melee_attack` | 近战攻击 |
| 主战场·战斗 | `projectile_launch`、`projectile_hit` | 弹体发射/命中 |
| 主战场·战斗 | `hit`、`unit_death` | 受击/单位死亡 |
| 主战场·战斗 | `building_hit`、`building_destroy` | 建筑受击/摧毁 |
| 主战场·全局 | `phase_change` | 昼夜阶段切换提示 |
| 主战场·结算 | `victory`、`defeat` | 胜负播报 |

### 3.4 数量汇总

- BGM：**4 首**（标题/白天/夜晚/结算）
- SFX：**23 个事件**（每事件 1 个基础音效；高频事件后续可加变体，不增加事件数）
- 素材采购与实现均以本清单为准

## 四、目标拆解

| 编号 | 目标 | 主要内容 |
|------|------|---------|
| E0 | 场景与音频需求确定 | 场景盘点、BGM/SFX 清单与数量定稿 |
| E1 | 音频架构 | 总线布局、AudioManager、事件接口 |
| E2 | BGM 系统 | 曲目表、交叉淡化、昼夜/场景联动 |
| E3 | SFX 系统 | 事件清单、播放规则 |
| E4 | 音量设置 | 设置面板、持久化 |
| E5 | 素材与导入 | 素材来源、目录规范、Godot 导入 |
| E6 | 打磨与验收 | 平衡、防爆音、性能、验收 |

## 五、技术方案

### 5.1 音频总线

- 三条总线：`Master`（总控）→ `Music`、`SFX`
- 音量调节只作用于 Music/SFX 总线，Master 保留默认 0dB
- 总线布局在 Godot 项目音频设置（default bus layout）中配置

### 5.2 AudioManager

- `scripts/adapters/godot/audio/AudioManager.cs`，autoload，经 AdapterRegistry 注册
- 职责：
  - BGM：加载曲目表、播放/暂停/切换、交叉淡化（fade out / fade in 约 1 秒）
  - SFX：按事件名一次性播放，支持音量与音高扰动
  - 音量：读写 Music/SFX 总线音量，加载/保存用户设置
- 曲目表：`场景状态 → 曲目路径`，资源路径集中管理，禁止场景内硬编码
- 事件接口：SFX 以事件名调用（如 `PlaySfx("buy")`），调用方不关心具体资源

### 5.3 BGM 映射

| 状态 | 曲目 | 触发时机 |
|------|------|---------|
| 标题 | `bgm_title` | 进入标题场景 |
| 白天 | `bgm_day` | 对局开始/白天阶段开始 |
| 夜晚 | `bgm_night` | 夜晚阶段开始（从白天曲淡出） |
| 结算 | `bgm_result` | 胜负产生 |

### 5.4 SFX 事件清单

以 E0 清单（3.3）为准，按归属分组接入；规则：

- 同类音效短时重复合并（同一事件 50ms 内不重复触发）
- SFX 统一走 SFX 总线；战场内来源明确的音效后续可选 `AudioStreamPlayer2D`（位置感），本期不强求

### 5.5 音量设置与持久化

- 设置面板新增「音乐音量」「音效音量」HSlider（参考现有拖拽速度滑条模式）
- 即时生效：滑条变动 → AudioManager 写总线音量
- 持久化：沿用 `user://display_settings.cfg`，新增 `music_volume`、`sfx_volume`（0~1）
- 默认：音乐 100%、音效 90%

## 六、素材规划

| 类别 | 数量 | 格式 | 来源 | 目录 |
|------|------|------|------|------|
| BGM 标题 | 1 | .ogg 循环 | AI 生成或免费素材 | `assets/audio/bgm/` |
| BGM 白天 | 1 | .ogg 循环 | 同上 | `assets/audio/bgm/` |
| BGM 夜晚 | 1 | .ogg 循环 | 同上 | `assets/audio/bgm/` |
| BGM 结算 | 1 | .ogg | 同上 | `assets/audio/bgm/` |
| SFX 音效包 | 23 个事件 | .ogg/.wav | 优先 CC0 素材库（Kenney 等），缺口 AI 生成 | `assets/audio/sfx/` |

- 命名：`bgm_<场景>.ogg`、`sfx_<事件>.ogg`
- 采购范围严格按 E0 清单，不随意增加
- 新增素材后执行 `godot-import` skill 无头导入，提交 `.import`/`.uid` 侧车文件
- 素材规格章节同步扩展 `devPlan/design/assetSpecs.md`

## 七、阶段顺序

0. E0 场景与音频需求确定（清单定稿）
1. E1 音频架构（总线 + AudioManager 骨架）
2. E5 素材（按 E0 清单落地曲目与音效包，可与 E1 并行）
3. E2 BGM 系统（曲目表 + 交叉淡化）
4. E3 SFX 接入（事件清单逐类接入）
5. E4 音量设置（滑条 + 持久化）
6. E6 打磨与验收

## 八、验收标准

- [ ] 音频需求清单覆盖全部正式场景（含叠加状态），BGM/SFX 数量与用途明确并定稿
- [ ] 标题/白天/夜晚/结算四个状态各有 BGM，切换交叉淡化无爆音
- [ ] UI 与战斗核心事件均有音效（与 E0 清单一致）
- [ ] 音乐/音效独立调节，重启后保持
- [ ] 静音可玩、高频战斗无叠炸、无性能回退
- [ ] AudioManager 无硬编码资源路径（曲目表集中管理）