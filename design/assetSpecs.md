# 素材规格

本文档定义项目中所有图片素材的类型、尺寸要求以及 Godot 缩放后的规格。

---

## 设计基准

| 参数 | 值 | 来源 |
|------|-----|------|
| 设计分辨率 | 1920 × 1080 | `GameRules.DesignWidth/Height` |
| 格子像素 | 48 × 48 px | `GameCoordinatesAdapter.PixelsPerCell` |
| 游戏单位/格 | 100 unit | `GameCoordinateRules.UnitsPerCell` |
| 缩放模式 | CanvasItems + Expand | `DisplaySettingsManager` |

所有素材以 **1920×1080 逻辑像素** 为基准制作。Godot 通过 `ContentScaleSize` 自动缩放到实际窗口。素材不需要为不同分辨率准备多套。

---

## 素材目录

```
assets/
├── art/
│   ├── cards/          # 卡牌相关（边框、图标、商品图）
│   ├── buildings/      # 建筑精灵（待补充）
│   ├── units/          # 单位精灵（待补充）
│   ├── ui/             # UI 元素（按钮、面板、图标，待补充）
│   └── placeholders/   # 开发用占位图
├── audio/
│   └── bgm/            # 背景音乐 (.ogg)
└── shaders/            # 着色器 (.gdshader)
```

---

## 素材类型与规格

### 1. 卡牌

| 素材 | 逻辑尺寸 | 说明 |
|------|---------|------|
| 卡牌边框 | ~154 × 119 px | 手牌区单卡约 8% × 11%（1920×1080 基准），实际卡牌含边框 |
| 卡牌图标 | ~80 × 60 px | 卡牌内上部图标区域 |
| 商品卡（商店）| 同手牌 | 商店商品槽复用卡牌边框 |

**Godot 要求：** 导入为 `CompressedTexture2D`，默认设置（无 mipmap，fix_alpha_border=true）。

### 2. 建筑

| 素材 | 逻辑尺寸 | 说明 |
|------|---------|------|
| 单格建筑（兵营、狼穴）| 48 × 48 px | 1×1 格，略小于格子（碰撞 56×56） |
| 双格建筑（靶场）| 96 × 48 px 或 48 × 96 px | 2×1 或 1×2 格 |
| L 形建筑（马厩）| 按 footprint 计算 | 如 3 格 L 形 = 最大 96 × 96 px |
| 城堡之心 | 48 × 48 px | 核心建筑，不可融合 |

**Godot 要求：**
- 建筑 sprite 居中放置在 `Area2D` 节点下
- 碰撞体略小于格子（如 56×56 for 64px 格），避免邻接建筑碰撞重叠
- 建筑的 `.tscn` 预制体在 `prefabs/`，素材文件在 `assets/art/buildings/`

### 3. 士兵/单位

| 素材 | 逻辑尺寸 | 说明 |
|------|---------|------|
| 士兵（剑士）| ~32 × 32 px | 占约 2/3 格高 |
| 狼人 | ~36 × 36 px | 略大于普通士兵 |
| 远程士兵（靶场）| ~32 × 32 px | 与剑士同级 |
| 骑兵（马厩）| ~40 × 32 px | 稍宽 |

**Godot 要求：**
- 单位 sprite 居中放置在 `Area2D` 节点下
- 碰撞体与 sprite 尺寸匹配

### 4. UI 元素

| 素材 | 逻辑尺寸 | 说明 |
|------|---------|------|
| 按钮 | 按文案自适应，最小 80 × 32 px | 使用 Godot `Button` 节点 + 主题样式 |
| 面板背景 | 按布局百分比 | 使用 `ColorRect` + 半透明色，不强制图片 |
| 血条 | 按布局百分比 | 使用 `ProgressBar` 节点 |
| 图标（金币、菜单等）| 24 × 24 ~ 32 × 32 px | 小图标 |

**Godot 要求：**
- UI 元素优先使用 Godot 内置节点（Button、Panel、ColorRect），图片素材仅用于图标和装饰
- UI 放在 `CanvasLayer` 下，不受战场相机影响

### 5. 特效/着色器

| 素材 | 说明 |
|------|------|
| 邻接光圈 | `adjacent_link_pulse.gdshader`，程序化生成 |
| 建筑工作指示 | `building_work.gdshader`，程序化生成 |

着色器不需要位图素材，通过 shader 参数控制。

### 6. 音频

| 格式 | 说明 |
|------|------|
| `.ogg` | Godot 推荐格式，循环播放 |

当前 BGM：`feel_good_island_loop.ogg`

---

## Godot 导入设置

所有 `.png` 素材使用默认导入配置：

```ini
importer="texture"
type="CompressedTexture2D"

[params]
compress/mode=0          # 无损
mipmaps/generate=false   # 2D 游戏不需要 mipmap
process/fix_alpha_border=true
```

如需像素风格（点采样），将材质的 `texture_filter` 设为 `TEXTURE_FILTER_NEAREST`。

---

## 格子换算速查

| 格数 | 像素 | 游戏单位 |
|------|------|---------|
| 1 格 | 48 px | 100 unit |
| 2 格 | 96 px | 200 unit |
| 4 格 (2×2) | 96 × 96 px | 200 × 200 unit |
| 8 格 (城堡边长) | 384 px | 800 unit |

像素 ↔ 单位换算：`pixels = units × 48 / 100`
