# 动画工作流（帧动画 · 像素图集切片）

> **决策（2026-09-02）**：单位动画采用**帧动画（sprite sheet 切片）**方案；**Spine 2D 骨骼动画方案已彻底弃用**（不再购买 Spine Essential 许可、不启用 Gemini 立绘拆件、无 spine 管线脚本）。

本文档记录帧动画工作流的规则与接入方式，供后续兵种动画复用。素材规格见 `assetSpecs.md`，工具见 `tools/slice_spritesheet.py`。

## 流程速查（Agent 遵循）

1. 源图集入库 → `assets/art/units/{TypeId_lower}_sheet_out.png`。
2. 切帧 → `python tools\slice_spritesheet.py --input <sheet>.png --output assets\art\units\{TypeId_lower}\frames --anims <按行顺序，逗号分隔> --global-cell --pad 4`；产物 `{anim}_{NN}.png` + `manifest.json`。
3. 导入 → `python tools\godot_import.py --project <根> --godot <Godot exe>`；生成 `.import`/`.uid`。
4. 战场自动启用：`UnitVisual.BuildFrames` 按 `res://assets/art/units/{typeId_lower}/frames` 是否存在切换「动画身体 `View/Body`」/「卡面 `View/UnitCard`」——有目录=身体，无目录=卡面。
5. 状态映射：`UnitLogic`（`UpdateBodyAnimation` + `OnAttack/OnDamaged/OnDied`）→ `run`/`attack`/`hurt`/`death`；无 idle（站定冻结）。
6. 调节奏：`UnitVisual.cs` 的 `*Fps` 与 `HurtFrameLimit`/`GenericFrameLimit`。
7. 预览：`scenes/ui/dev_tools/unit_anim_preview.tscn`（数字键 1-5）。

## 1. 方案概述

- 单位使用**逐帧序列帧**（帧动画），而非骨骼动画。
- 源素材为一张**像素角色图集**（sprite sheet），按行划分不同动画状态，按列划分帧。
- 由 `tools/slice_spritesheet.py` 自动识别行列网格 → 逐帧裁切 → 白底洪水填充转透明 → 统一单元格（每行动画等尺寸、底部居中对齐）输出。

## 2. 试点单位：Swordsman 剑士

- 源图集：1536×1024 像素（原 `Downloads/test1.png`，已更名为 `assets/art/units/swordsman_sheet_out.png` 入库，`_out` 后缀用于与内部 `asset_gen_tasks` 数据库来源区分）。
- 切片结果共 **42 帧**，位于 `assets/art/units/swordsman/frames/`：

| 动画 | 帧数 | 单元格 | 说明 |
|------|------|--------|------|
| `attack` | 8 | 258×170 | 基础挥砍（起手→突刺→横扫→刀光） |
| `attack_combo` | 8 | 258×170 | 连击/重击（月牙→跃斩→下劈溅水） |
| `run` | 9 | 258×170 | 奔跑（含扬尘） |
| `hurt` | 9 | 258×170 | 受击（红色受伤粒子） |
| `death` | 8 | 258×170 | 倒地→俯卧 |

> 所有帧使用**统一全局单元格 258×170**（底部居中对齐，`--global-cell`），保证切换动画时脚底锚点稳定不跳变。

- ⚠️ 该图集**无独立 idle 行**（第 1、2 行均为攻击变体）；`idle` 待补（或临时用 `attack_00` 站姿做单帧待机）。

## 3. 切帧工具

`tools/slice_spritesheet.py`：

```powershell
python tools/slice_spritesheet.py --input <sheet.png> --output <帧目录> --anims idle,attack,run,hurt,death --bg-thresh 235 --pad 4
```

- `--anims`：按行顺序给定每行动画名；`--bg-thresh`：白底判定阈值；`--pad`：单元格内边距。
- 输出：每动一帧 `{anim}_{NN}.png` + `manifest.json`（含源框、单元格位置）。

## 4. Godot 接入

- 帧资源：`AnimatedSprite2D` + `SpriteFrames`；帧为独立 PNG，运行时按路径组帧，避免手写 `.tres` 的 uid 依赖。
- 战场接入（2026-09-02）：`Unit.tscn` 新增 `View/Body`（AnimatedSprite2D）与 `View/BodyHealth`（独立血条）；`UnitVisual` 检测单位是否有帧目录 `res://assets/art/units/{typeId_lower}/frames`，有则隐藏卡面、显示帧动画身体，无则保留卡面占位。
- 预览场景：`scenes/ui/dev_tools/unit_anim_preview.tscn` + `scripts/adapters/godot/dev/UnitAnimPreview.cs`（数字键 `1-5` 切换状态）。

## 5. 状态

- ✅ 图集切片 + 素材入库（`assets/art/units/swordsman_sheet_out.png` + `frames/`）
- ✅ 预览场景与状态切换（`unit_anim_preview`）
- ✅ 接入正式单位（`Unit.tscn` + `UnitVisual` + `UnitLogic` 状态驱动：移动→`run`、攻击→`attack`、受击→`hurt`、死亡→`death`；无 idle，站定冻结当前帧）
- ✅ 按需扩展其他兵种（有帧目录即自动启用身体，无则保留卡面）
- ⬜ 身体缩放 / 血条位置微调、按单位体型差异化；站定帧细化
