# 动画工作流（帧动画 · 像素图集切片）

> **决策（2026-09-02）**：单位动画采用**帧动画（sprite sheet 切片）**方案；**Spine 2D 骨骼动画方案已彻底弃用**。帧以**单张图集 + `AtlasTexture` 区域**形式供游戏使用。

本文档记录帧动画工作流的规则与接入方式，供后续兵种动画复用。素材规格见 `assetSpecs.md`，工具见 `tools/slice_spritesheet.py` 与 `tools/pack_sprite_atlas.py`。

## 流程速查（Agent 遵循）

1. 源图集入库 → `assets/art/units/{TypeId_lower}_sheet_out.png`。
2. 切帧（中间产物）→ `python tools\slice_spritesheet.py --input <sheet.png> --output assets\art\units\{TypeId_lower}\frames --anims <按行顺序，逗号分隔> --global-cell --pad 4`。
3. 打包图集（最终资产）→ `python tools\pack_sprite_atlas.py --frames assets\art\units\{TypeId_lower}\frames --atlas assets\art\units\{TypeId_lower}\frames\atlas.png --anims <同上> --cols 7`；产物 `atlas.png` + `atlas.json`（各帧区域矩形）。之后可删除中间的 `{anim}_{NN}.png`。
4. 导入 → `python tools\godot_import.py --project <根> --godot <Godot exe>`；生成 `.import`/`.uid`。
5. 战场自动启用：`UnitVisual.ConfigureFrameBody` 检测 `res://assets/art/units/{typeId_lower}/frames/atlas.png` 是否存在——有则用「帧动画身体 `View/Body`」，否则用「卡面 `View/UnitCard`」；`BuildFrames` 读 `atlas.json` 区域→每帧一个 `AtlasTexture`。
6. 状态映射：`UnitLogic`（`UpdateBodyAnimation` + `OnAttack/OnDamaged/OnDied`）→ `run`/`attack`/`hurt`/`death`；无 idle（站定冻结）。
7. 调节奏：`UnitVisual.cs` 的 `*Fps` 与 `HurtFrameLimit`/`GenericFrameLimit`。
8. 预览：`scenes/ui/dev_tools/unit_anim_preview.tscn`（数字键 1-5）。

## 1. 方案概述

- 单位使用**逐帧序列帧**（帧动画），而非骨骼动画。
- 源素材为**像素角色图集**（sprite sheet），按行划分动画状态、按列划分帧。
- `slice_spritesheet.py` 自动识别网格 → 逐帧裁切 → 白底洪水填充转透明 → **统一全局单元格**（底部居中）→ `pack_sprite_atlas.py` 打包成**单张透明图集 + 区域元数据**。

## 2. 试点单位：Swordsman 剑士

- 源图集：1536×1024 像素（原 `Downloads/test1.png`，已名为 `assets/art/units/swordsman_sheet_out.png`，`_out` 后缀与内部 `asset_gen_tasks` 来源区分）。
- 打包为**单张图集** `assets/art/units/swordsman/frames/atlas.png`（1806×1020）＋区域元数据 `atlas.json`，共 **42 帧**：

| 动画 | 帧数 | 单元格 | 说明 |
|------|------|--------|------|
| `attack` | 8 | 258×170 | 基础挥砍（起手→突刺→横扫→刀光） |
| `attack_combo` | 8 | 258×170 | 连击/重击（月牙→跃斩→下劈溅水） |
| `run` | 9 | 258×170 | 奔跑（含扬尘） |
| `hurt` | 9 | 258×170 | 受击（红色受伤粒子） |
| `death` | 8 | 258×170 | 倒地→俯卧 |

> **所有帧放进同一尺寸画布（258×170），角色脚底对齐画布底边中点**；各帧内容在画布内底部居中。因此游戏切换动作（run/attack/hurt/death）时脚底锚点不跳，仅宽度随刀光/动作变化。

- ⚠️ 无独立 idle 行（第 1、2 行均为攻击变体）；按用户指示士兵无 idle，站定冻结当前帧。

## 3. 切帧与打包工具

`tools/slice_spritesheet.py`：识别网格、裁切、白底转透明、统一单元格。`tools/pack_sprite_atlas.py`：把 `frames/` 里的帧打包成单张图集并写 `atlas.json` 区域。

```powershell
python tools/slice_spritesheet.py --input <sheet.png> --output <帧目录> --anims idle,attack,run,hurt,death --bg-thresh 235 --pad 4 --global-cell
python tools/pack_sprite_atlas.py --frames <帧目录> --atlas <帧目录>/atlas.png --anims idle,attack,run,hurt,death --cols 7
```

- `atlas.json`：`{ "atlas": "atlas.png", "cell": [w,h], "frames": { "anim_NN": [x,y,w,h], ... } }`。

## 4. Godot 接入

- 帧资源：`AnimatedSprite2D` + `SpriteFrames`；所有帧共享**一张图集纹理**，每帧为 `AtlasTexture` 区域（`UnitVisual.BuildFrames` 按 `atlas.json` 生成），避免逐帧独立纹理的切换开销，利于批处理。
- 战场接入（2026-09-02）：`Unit.tscn` 有 `View/Body`（AnimatedSprite2D）+ `View/BodyHealth`（独立血条）+ `View/UnitCard`；`UnitVisual.ConfigureFrameBody` 检测 `atlas.png` 是否存在，有则隐藏卡面、显示身体并驱动，无则保留卡面占位。
- 预览场景：`scenes/ui/dev_tools/unit_anim_preview.tscn` + `scripts/adapters/godot/dev/UnitAnimPreview.cs`（数字键 `1-5`）。

## 5. 状态

- ✅ 源图集入库 + 切片 + 打包单张图集（`atlas.png` + `atlas.json`）
- ✅ 预览场景与状态切换（`unit_anim_preview`）
- ✅ 接入正式单位（`Unit.tscn` + `UnitVisual` + `UnitLogic`：移动→`run`、攻击→`attack`、受击→`hurt`、死亡→`death`；无 idle，站定冻结当前帧）
- ✅ 按需扩展其他兵种（有 `atlas.png` 即启用身体，无则卡面）
- ⬜ 身体缩放 / 血条位置微调、按单位体型差异化；站定帧细化
