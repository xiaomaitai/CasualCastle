# 素材规格

本文档以**游戏单位（unit）**为坐标系定义所有尺寸。渲染输出（像素）由 `GameCoordinatesAdapter` 自动换算。

---

## 设计基准

| 参数 | 值 | 来源 |
|------|-----|------|
| 设计分辨率 | 1920 × 1080 px | `GameRules.DesignWidth/Height` |
| 格子 | 100 × 100 unit | `GameCoordinateRules.UnitsPerCell` |
| 渲染 | 100 px/格 | `GameCoordinatesAdapter.PixelsPerCell` |
| 缩放模式 | CanvasItems + Expand | `DisplaySettingsManager` |

**换算公式：** `像素 = 游戏单位`（PixelsPerCell = UnitsPerCell = 100，1:1 对应）

所有战场内实体（建筑、单位、弹体）的尺寸、距离、速度一律用 game unit 定义。像素仅用于 UI 和输出分辨率。

---

## 素材目录

```
assets/
├── art/
│   ├── cards/          # 卡牌（边框、图标）
│   ├── buildings/      # 建筑精灵
│   ├── units/          # 单位精灵
│   ├── ui/             # UI 元素（仅图标类）
│   └── placeholders/   # 开发占位图
├── audio/
│   └── bgm/            # BGM (.ogg)
└── shaders/            # 着色器 (.gdshader)
```


## 素材阶段命名（后缀规范）

游戏素材按处理阶段落盘，**后缀语义固定，后续不再变更**。各阶段文件同时保留、不原地覆盖；游戏只引用最终阶段文件。

| 阶段 | 后缀 | 格式 | 说明 |
|------|------|------|------|
| 源图 | `{TypeId}_{taskId}.png` | RGB 纯白底 | AI 生图产物（taskId 为 `asset_gen_tasks.id`），保留原分辨率，仅存档/回溯，不直接用于游戏 |
| 抠图 | `{TypeId}_{taskId}_cut.png` | RGBA 透明 | remove-bg 产物（nodeId=17），保留原分辨率 |
| 游戏纹理 | `{TypeId}_{taskId}_game.png` | RGBA 透明 | 裁掉透明边距 + 按长边等比缩放的最终纹理，游戏实际引用 |

- 游戏纹理生成：`python tools/build_game_texture.py assets/art/buildings/{TypeId}_{taskId}_cut.png --long-edge <目标长边>`
- 目标长边见"尺寸决策表"：2×2 建筑 512、1×1 建筑 256
- 无抠图需求的素材（单位主图、技能图标等）直接使用源图，无 `_cut`/`_game` 阶段
- 阶段文件变更后需重新导入（godot-import）并同步 `building_defs.texture_uid` / `asset_gen_tasks.local_path`

---

## 1. 单位/单位

### 节点结构

```
Unit (Area2D)
├── Logic               # UnitLogic：领域单位驱动与表现
├── CollisionShape      # CircleShape2D 碰撞体，半径 = UnitStats 碰撞半径
└── View
    ├── UnitCard        # 卡面占位：无帧图集的单位
    ├── Body            # 帧动画身体：有 frames/atlas.png 的单位（见 animationWorkflow.md）
    └── BodyHealth      # 帧动画身体的独立血条
```

- **显示体**：卡面（正方形，边长 = 显示体边长）或帧动画身体（高度 = 显示体边长），纯视觉
- **碰撞体**：`CircleShape2D`，受击判定 + 推挤 + 建筑碰撞；半径由 `UnitStats` 按体型给出
- **碰撞圈**：`CollisionOverlay` 绘制，仅作弊模式（`CheatState.ShowCollisionEnabled`）可见

### 碰撞圈颜色

| 阵营 | 颜色 | 绘制 |
|------|------|------|
| 我方 | 绿色 `(0.2, 1, 0.3, 0.9)` | `CollisionOverlay.FriendlyColor` |
| 敌方 | 红色 `(1, 0.25, 0.2, 0.9)` | `CollisionOverlay.EnemyColor` |

### 四种体型

每个单位归入一种体型。显示体是正方形，碰撞体是内切圆。下表为**当前实现值**（显示体边长见 `UnitLogic` 的 `*DisplaySize` 常量，碰撞半径见 `UnitStats` 的 `*CollisionRadius` 常量）。

| 体型 | 显示体边长 | 碰撞半径 | 适用单位 |
|------|-----------|---------|---------|
| **小型** Small | 60 unit | 18 unit | 小怪物、小精灵、蝙蝠等 |
| **中型** Medium | 90 unit | 27 unit | 剑士、弓箭手、狼人、步兵等类人生物 |
| **大型** Large | 120 unit | 36 unit | 骑兵、野兽、食人魔等 |
| **特大型** Huge | 180 unit | 54 unit | 猛犸、黑龙、巨人等巨兽 |

碰撞半径 = 显示体边长 × 0.3，保证单位彼此靠近时视觉紧凑。

### 体型汇总

| 体型 | 显示体 | 碰撞半径 | 代表兵种 |
|------|--------|---------|---------|
| Small | 60×60 | 18 | 小精灵、蝙蝠 |
| Medium | 90×90 | 27 | 剑士、弓箭手、狼人 |
| Large | 120×120 | 36 | 骑兵 |
| Huge | 180×180 | 54 | 猛犸、巨人、黑龙 |

### 有效攻击距离

近战：碰撞圈边缘到碰撞圈边缘 = `AttackRange`。判定由领域完成：`Unit.EdgeDistanceTo(target) ≤ AttackRange` 时结算伤害（`Unit.cs`）。

| 攻击方 | 防守方 | AttackRange | 生效距离（中心距） |
|--------|--------|-------------|------------------|
| Medium | Medium | 60 | 114 unit (27+60+27) |
| Large | Medium | 60 | 123 unit (36+60+27) |
| Huge | Medium | 80 | 161 unit (54+80+27) |

远程与近战走同一判定；`Projectile` 只做飞行表现，不参与命中结算。上表 AttackRange 为示例值，实际取 `config.db` 的 `unit_stats.attack_range`。

### 间距规则

| 规则 | 阈值 | 作用 |
|------|------|------|
| 同方推挤 | 间距 < 双方半径之和 × 1.6 | 施加排斥力（`SteeringService.SeparationRadiusMultiplier`） |
| 建筑避障 | 碰撞圈边缘触碰建筑碰撞体 | 沿切线滑行 |
| 伤害结算 | 边缘距离 ≤ `AttackRange` | 领域直接结算，弹体仅表现 |

---

## 2. 建筑主图

建筑主图 = 战场内建筑显示用的完整建筑精灵图。最终形态必须是 RGBA 透明图。**图内不画底座、地面或阴影**，图像底边即建筑与地面的接触线；放置后脚下泥土地基消失，建筑直接"站"在格子上。

### 目录与命名

- 目录：`assets/art/buildings/`
- 命名：按"素材阶段命名"规范，源图 `{typeId}_{taskId}.png`、抠图 `{typeId}_{taskId}_cut.png`、游戏纹理 `{typeId}_{taskId}_game.png`
- 引用：`building_defs.texture_uid` 登记**游戏纹理**（`_game.png`）对应的 Godot 资源 uid；重新生成后更新 uid

### 尺寸分级

不同占地面积的建筑**需要不同画布规格**：占地越大，画布分辨率要求越高；高大地标（城堡/王庭/高阶核心）需要更高分辨率保证细节。

| 占地 | 适用建筑 | 源图画布 | 游戏纹理长边 | 横向铺满比例 |
|------|---------|----------|--------------|---------------|
| 1×1 | 地洞等单格建筑 | 512×512 | 256 | 建筑主体占画布宽 ≈ 96% |
| 2×2 标准 | 绝大多数建筑（51 座） | 768×768 | 512 | 建筑主体占画布宽 ≈ 96% |
| 2×2 高地标 | 城堡之心、王庭、部落核心 L4/L5 等 | 1024×1024 | 512 | 建筑主体占画布宽 ≈ 96% |

- 源图分辨率 = 生成分辨率，作为存档 master 不缩放；**游戏纹理**由 `tools/build_game_texture.py` 裁边 + 按长边等比缩放（预留高清显示余量）
- 建筑主体**横向铺满画布**（左右留 ≤2% 边距），**底边贴画布下缘**，不得悬空留白
- 运行时等比缩放：`BuildingSystem.ApplyVisual` 按碰撞尺寸与纹理长边统一缩放，保持宽高比不拉伸；锚点 = 图像底边中点，对齐占地中心

### 视角（固定关键词）

提示词固定使用 `orthographic 3/4 top-down`（正交 3/4 俯视）：建筑对称构图、完整可见，适配正方形格子；**不是等距菱形格（isometric diamond）**、不是透视视角、不是平视。建筑主体从底边向上延伸，完整建筑不能出画布。

### 风格规范

- 提示词必须包含：`flat 2D, anime style, thick bold outlines, no shading, no gradient, simple flat colors`
- 背景：**纯白背景**（便于 remove-bg 抠图），主体完整
- 禁止：底座/地面/平台/阴影/投影（建筑底边直接落地）

### 提示词模板（联盟基准，F0 定稿）

```
A minimalist {building} for an RTS game, perfectly centered inside a square footprint, designed to fit naturally into a square grid tile.

3/4 top-down view (NOT isometric diamond grid), symmetrical composition, entire building fully visible.

Cute stylized cartoon illustration, flat 2D vector style, bold thick black outlines, clean cel shading, simplified geometric shapes, minimal details, highly readable game asset.

{distinct features}.

Balanced proportions, compact building, square silhouette, optimized as a strategy game building icon.

Pure white background.
```

负向提示词：`isometric diamond tile, rotated building, perspective view, realistic, painterly, 3D render, sketch, watercolor, heavy textures, photorealistic, cluttered details, shadows outside object, background scenery, people, trees, mountains, text, logo, watermark`

风格约束：固定关键词 `orthographic 3/4 top-down`；不画底座/地面/阴影；纯白背景；参考图可作构图与造型基准（如 `assets/art/references/马厩.png`）。

### 种族风格微调

风格三要素（flat 2D 粗描边）、视角与无底座规则全局不变，**材质、装饰、配色按种族微调**：

| 种族/线 | 风格词 |
|---------|--------|
| 联盟 | medieval stone and wood, banners, blue and gold accents |
| 部落·兽人 | rough timber and bone, spikes, dark red and black |
| 部落·哥布林 | ramshackle hut, gears and scrap metal, green |
| 部落·半人马 | leather tent, feathers and hide, ochre |
| 部落·半人鹿 | forest shrine, vines and leaves, nature green |
| 部落·蜥蜴人 | swamp mud mound, bones and teeth, teal and brown |
| 部落核心 | camp fortress upgrade levels, campfire to stone walls |

（亡灵、地下城等后续派系接入时再补充）

### 碰撞体

建筑碰撞体为矩形，按 footprint 计算，尺寸 = 占地尺寸 − 走廊宽 112 unit（1×1 占地钳位到最小 24 unit），相邻建筑之间留出略大于最大单位直径（巨型单位 108 unit）的通行走廊，避免单位卡在两建筑之间。贴图视觉仍按占地尺寸 − 格间隙 14 unit 显示，与碰撞体解耦。碰撞体不受建筑主图影响，逻辑保持不变。

### 管线

生图（RGB 纯白底，`_{taskId}.png`）→ 用户筛选 → remove-bg 抠图（nodeId=17，输出 `_{taskId}_cut.png`）→ 后处理生成游戏纹理（`tools/build_game_texture.py`，`_{taskId}_game.png`）→ godot-import 导入 → 登记 texture_uid（`_game.png` 的 Godot 资源 uid）
- 参考图：submit 传 --image-url <url>、batch 传 image_infos（字符串或 URL 列表）附带参考图构图/造型基准；参考图需公网 URL

---

## 3. 卡牌

卡牌为 UI 元素，使用**屏幕像素**（不受游戏坐标影响）。

| 素材 | 尺寸 (px) | 说明 |
|------|----------|------|
| 卡牌边框 | ~154 × 119 | 手牌区单卡约 8%×11%（1920×1080 基准） |
| 卡牌图标 | ~80 × 60 | 卡牌内上部 |
| 商品卡（商店）| 同手牌 | 复用卡牌边框 |

导入为 `CompressedTexture2D`，默认设置。

### 单位主图

战斗内单位卡牌上显示的兵种肖像图，由 `UnitCardView.LoadPortrait` 按 `unit_stats.portrait_uid` 精确加载，交 `CardArtView` 按短边填满遮罩显示。

| 属性 | 值 |
|------|-----|
| 用途 | 战斗中单位卡牌的兵种肖像 |
| 存放位置 | `assets/art/cards/{TypeId}_{taskId}.png`（TypeId 与 `unit_stats` 表一致，taskId 为 `asset_gen_tasks.id`） |
| 加载方式 | 按 `unit_stats.portrait_uid`（Godot 资源 uid）精确加载，`CardArtView` 按短边填满遮罩显示 |
| 生图尺寸 | 512×512 |
| 最终尺寸 | 256×256（缩放后） |
| 模型 | Qwen-Image（`bf085132c7134622895b783b520b39ff` / `75e0be0c93b34dd8baeec9c968013e0c`） |
| 采样 | sampler=1, steps=30, cfgScale=4.0, clipSkip=2 |

**提示词模板：**

```
A medieval [unit type] unit, [weapon/gear], facing right, full body character, flat 2D, anime style, thick bold outlines, no shading, no gradient, simple flat colors, game character icon
```

示例：`A medieval spearman unit, holding a long spear, facing right, full body character, flat 2D, anime style, thick bold outlines, no shading, no gradient, simple flat colors, game character icon`

### 技能图标

| 属性 | 值 |
|------|-----|
| 用途 | 技能栏/属性面板技能图标（`SkillIconView` 56×56 显示） |
| 存放位置 | `assets/art/skills/{skillId}_{taskId}.png`（skillId 与 `skill_defs.id` 一致） |
| 加载方式 | 按 `skill_defs.icon_uid`（Godot 资源 uid）精确加载 |
| 生图尺寸 | 512×512 |
| 最终尺寸 | 256×256（缩放后） |

**风格要求**：

- 全铺满无框方形图：画面内容铺满整张画布（edge to edge），不留白边、无技能框/边框装饰；**提示词中不得出现 `square icon`**（实测会诱导模型画出带留白的方形框）
- 构图：主体居中，占画面约 60~80%；背景按技能情境即兴发挥（毒雾、魔法能量、暗影等），只作氛围烘托、不喧宾夺主
- **禁止白底/空背景**；背景是图案设计的一部分，成品直接用 RGB PNG，不需要透明通道
- 风格沿用定稿示例方向：`2D cartoon fantasy, exaggerated shapes, bold silhouette, thick black outline, vivid colors, high contrast, stylized fantasy RPG game icon, polished cartoon game art, readable at very small size`

**提示词模板（定稿）：**

```
2D cartoon fantasy game skill icon, [技能主题与主体描述], [按情境即兴发挥的氛围/背景描述], powerful and aggressive visual impact, exaggerated shapes, bold silhouette, thick black outline, simple but striking composition, vivid colors, high contrast, stylized fantasy RPG game icon, polished cartoon game art, centered composition, frameless full-bleed icon, artwork extends edge to edge, no border, no frame, readable at very small size
```

**负向提示词：**

- 固定使用：`frame, border, skill icon frame, gold frame, ornate frame, badge, medallion, white background, empty background, text, lettering, logo, watermark, scenery, background objects`
- 可选（仅无人物技能）：`character, people`——挥砍、变形术、气定神闲等需要人物出现的技能**不得**加入，正向词也不得写 no character，需在主体描述中写清人物动作

**定稿示例（plague_poison / 剧毒打击）：**

```
2D cartoon fantasy game skill icon, Poisonous Strike, a vicious dark blade slashing diagonally across the center, covered in glowing toxic green venom, bright green poison splashes exploding from the impact point, dripping venom, small poisonous droplets flying outward, dark purple toxic smoke surrounding the blade, powerful and aggressive visual impact, exaggerated shapes, bold silhouette, thick black outline, simple but striking composition, vivid colors, high contrast, stylized fantasy RPG game icon, polished cartoon game art, centered composition, frameless full-bleed icon, artwork extends edge to edge, no border, no frame, readable at very small size
```

负向提示词：`frame, border, skill icon frame, gold frame, ornate frame, badge, medallion, white background, empty background, character, people, text, lettering, logo, watermark, scenery, background objects`

---

## 4. UI 元素

UI 元素使用**屏幕像素**，放在 `CanvasLayer` 下。

| 素材 | 尺寸 (px) | 说明 |
|------|----------|------|
| 按钮 | 按文案自适应，最小 80×32 | Godot `Button` 节点 |
| 面板背景 | 按布局百分比 | `ColorRect` + 半透明色 |
| 血条 | 按布局百分比 | `ProgressBar` 节点 |
| 小图标 | 24×24 ~ 32×32 | 金币、菜单等 |

优先使用 Godot 内置节点，图片仅用于图标和装饰。

---

## 5. 特效/着色器

| 素材 | 说明 |
|------|------|
| 邻接光圈 | `adjacent_link_pulse.gdshader` |
| 建筑工作指示 | `building_work.gdshader` |

程序化生成，不需要位图素材。

---

## 6. 音频

音频采用统一 AudioManager 管线（v0.6），需求清单见 `devPlan/design/audioRequirements.md`，大纲见 `devPlan/outline/c09AudioPlan.md`。

| 类别 | 格式 | 说明 |
|------|------|------|
| BGM | `.ogg`（Vorbis，44.1kHz 立体声） | 曲目池轮播，播完静默 3~6 秒切下一首 |
| SFX | `.ogg`（Vorbis） | 25 个事件，每事件 1 个基础音效 |

### BGM 曲目池（15 首）

| 池 | 文件 | 数量 |
|----|------|------|
| 标题 | `bgm_title_1..3.ogg` | 3 |
| 联盟·白天 | `bgm_alliance_day_1..3.ogg` | 3 |
| 联盟·夜晚 | `bgm_alliance_night_1..2.ogg` | 2 |
| 部落·白天 | `bgm_horde_day_1..3.ogg` | 3 |
| 部落·夜晚 | `bgm_horde_night_1..2.ogg` | 2 |
| 结算 | `bgm_result_1..2.ogg` | 2 |

曲目路径集中登记在 `AudioManager` 曲目池表，场景内不硬编码。

### SFX 事件（25 个）

命名 `sfx_<事件>.ogg`，对应 `audioRequirements.md` 的 S1–S25：

`button_click`、`ui_start`、`slider_change`、`unit_select`、`building_select`、`shop_open`、`shop_close`、`buy`、`coin_change`、`place`、`place_cancel`、`repair`、`combine`、`upgrade`、`melee_attack`、`melee_slash`、`melee_pierce`、`projectile_launch`、`launch_arrow`、`launch_magic`、`projectile_hit`、`hit`、`hit_slash`、`hit_pierce`、`hit_magic`、`unit_death`、`building_hit`、`building_destroy`、`phase_change`、`victory`、`defeat`

### 素材来源与许可

BGM 15 首来自 OpenGameArt（CC0）；SFX 25 个来自 Kenney（CC0）。每个素材的来源与许可登记在 `assets/audio/bgm/CREDITS.txt` 与 `assets/audio/sfx/CREDITS.txt`。新增素材必须先更新需求清单与 CREDITS，再运行 `godot-import` skill 导入。

---

## Godot 导入设置

```ini
importer="texture"
type="CompressedTexture2D"

[params]
compress/mode=0          # 无损
mipmaps/generate=false
process/fix_alpha_border=true
```

---

## 换算速查

| 格数 | game unit | 像素 |
|------|-----------|------|
| 1 格 | 100 × 100 | 100 × 100 |
| 2 格 | 200 | 200 |
| 4 格 (2×2) | 200 × 200 | 200 × 200 |
| 8 格 (城堡边长) | 800 | 800 |

**换算：** `pixels = gameUnits`（1:1 对应，`PixelsPerCell = UnitsPerCell = 100`）

---

## AI 素材管线

游戏素材（单位、建筑、图标等）通过 AI 生成，可选抠图。两个阶段均由 Codex 版 skill 驱动（`$CODEX_HOME/skills/generate-image` 与 `$CODEX_HOME/skills/remove-bg`）。

### 阶段一：生图（generate-image）

使用 Liblib.art WebUI 文生图 API，根据提示词生成基础 PNG。

**触发方式：** 直接对 Claude 描述想要的素材（如"生成一个持剑骷髅单位"），Claude 自动调用 `generate-image` skill。

**关键规则：**
- 生图前必须对照本文档确定目标尺寸（game unit → 像素换算见上文速查表）
- 分辨率不得低于 512×512，小于目标尺寸时先生成大图再缩放
- 早期开发阶段优先使用 Qwen-Image 性价比模型
- 生成结果保存到 `assets/art/` 对应子目录（buildings/、units/、cards/ 等）
- **生图产出的 RGB PNG 可直接用于游戏，不需要主动抠图**

**参考：** `$CODEX_HOME/skills/generate-image`

### 阶段二：抠图（remove-bg）

用户在游戏中筛选素材后，对需要透明背景的图手动触发抠图。抠图产出 RGBA PNG **直接替换原文件**。

**触发方式：** 用户手动说"抠图"、"去背景"、"做成透明图"，Claude 调用 `remove-bg` skill。**生图完成后不要主动询问或执行抠图。**

**关键规则：**
- 输入图片必须是公网 URL（若原图来自 Liblib.art 生图，URL 记录在 `asset_gen_tasks` 表中）
- 输出多张图中，**取 nodeId=17**（硬边 RGBA 抠图），适合游戏 sprite
- 验证输出为 RGBA 且透明像素占比 > 5%
- **抠图结果保存为 `{TypeId}_{taskId}_cut.png`**，不覆盖源图
- **技能图标不参与抠图**：按技能图标规格直接生成 RGB PNG 使用，不执行 remove-bg

**参考：** `$CODEX_HOME/skills/remove-bg`

### 完整流程

```
需求描述 → [阶段一] 生图 → RGB PNG → 直接可用
							  ↓
					用户人工筛选后
							  ↓
				   [阶段二] 抠图 → RGBA PNG（_cut.png）
```

**典型对话示例：**

> 用户：帮我生成一个中型狼人单位，粗描边卡通风格
>
> Claude：
> 1. 查 assetSpecs.md → Medium 体型，生成 512×512 缩放到 256×256
> 2. 调用生图 API → 下载 → 保存 `assets/art/units/werewolf.png`
> 3. 告知完成（不询问抠图）

> （用户玩了一段时间后……）
>
> 用户：把马厩抠个图
>
> Claude：
> 1. 从 `asset_gen_tasks` 表查马厩源图的生图 URL
> 2. 调用抠图 API → 下载 nodeId=17 RGBA → 保存为 `Stable_{taskId}_cut.png`
> 3. 验证透明像素占比，告知完成

### 尺寸决策表

| 素材类型 | 目标像素（生图） | 缩放后 | 说明 |
|---------|---------------|--------|------|
| Small 单位 | 512×512 | 256×256 | 先生成大图保证质量 |
| Medium 单位 | 512×512 | 256×256 | 同上 |
| Large 单位 | 512×512 | 384×384 | 384 > 256 无需缩放 |
| Huge 单位 | 512×512 | 512×512 | 不缩放 |
| 建筑主图（1×1 占地） | 512×512 | 游戏纹理长边 256 | 底座宽 ≈ 200px |
| 建筑主图（2×2 占地标准） | 768×768 | 游戏纹理长边 512 | 底座宽 ≈ 350~420px |
| 建筑主图（2×2 高地标） | 1024×1024 | 游戏纹理长边 512 | 底座宽 ≈ 460~560px |
| 卡牌图标 | 512×512 | 256×256 | 先生成大图 |
| UI 小图标 | 512×512 | 128×128 或 64×64 | 同上 |

> 所有素材统一生成 ≥512×512，再缩放到实际用途尺寸。最终 sprite 尺寸大于游戏 unit 换算是为了支持高清显示和缩放动画。
