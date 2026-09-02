# 动画工作流（帧动画 · 像素图集切片）

> **决策（2026-09-02）**：单位动画采用**帧动画（sprite sheet 切片）**方案；**Spine 2D 骨骼动画方案已彻底弃用**（不再购买 Spine Essential 许可、不启用 Gemini 立绘拆件、无 spine 管线脚本）。

本文档记录帧动画工作流的规则与接入方式，供后续兵种动画复用。素材规格见 `assetSpecs.md`，工具见 `tools/slice_spritesheet.py`。

## 1. 方案概述

- 单位使用**逐帧序列帧**（帧动画），而非骨骼动画。
- 源素材为一张**像素角色图集**（sprite sheet），按行划分不同动画状态，按列划分帧。
- 由 `tools/slice_spritesheet.py` 自动识别行列网格 → 逐帧裁切 → 白底洪水填充转透明 → 统一单元格（每行动画等尺寸、底部居中对齐）输出。

## 2. 试点单位：Swordsman 剑士

- 源图集：1536×1024 像素（原 `Downloads/test1.png`，已更名为 `assets/art/units/swordsman_sheet_out.png` 入库，`_out` 后缀用于与内部 `asset_gen_tasks` 数据库来源区分）。
- 切片结果共 **42 帧**，位于 `assets/art/units/swordsman/frames/`：

| 动画 | 帧数 | 单元格 | 说明 |
|------|------|--------|------|
| `attack` | 8 | 258×149 | 基础挥砍（起手→突刺→横扫→刀光） |
| `attack_combo` | 8 | 196×170 | 连击/重击（月牙→跃斩→下劈溅水） |
| `run` | 9 | 150×117 | 奔跑（含扬尘） |
| `hurt` | 9 | 143×129 | 受击（红色受伤粒子） |
| `death` | 8 | 168×114 | 倒地→俯卧 |

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
- 状态切换：按 `UnitState` 映射到动画名（`attack`/`attack_combo`/`run`/`hurt`/`death`）；`run` 循环，`attack`/`attack_combo`/`hurt` 播放一次，`death` 停在末帧。
- 预览场景：`scenes/ui/dev_tools/unit_anim_preview.tscn` + `scripts/adapters/godot/dev/UnitAnimPreview.cs`（数字键 `1-5` 切换状态）。

## 5. 状态

- ✅ 图集切片 + 素材入库（`assets/art/units/swordsman_sheet_out.png` + `frames/`）
- ✅ 预览场景与状态切换（`unit_anim_preview`）
- ⬜ 接入正式单位（`Unit.tscn` + `UnitVisual` + 单位状态机驱动）
- ⬜ 补充 `idle` 待机动画
