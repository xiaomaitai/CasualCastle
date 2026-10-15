# 当前任务：动画工作流收尾（帧动画身体缩放 / 血条 / 体型档位）

> 本次决策：**站定保持冻结当前帧**（图集无 idle 行，不补行）；**体型差异化只调 adapter 常量**，不改 `config.db` 表结构。

## 背景与现状

- 帧动画已接入：`prefabs/Unit.tscn` 的 `View/Body`（AnimatedSprite2D）+ `View/BodyHealth`（独立血条），`scripts/adapters/godot/battle/UnitVisual.cs` 的 `ConfigureFrameBody` 检测 `res://assets/art/units/{typeId_lower}/frames/atlas.png` 是否存在——有则隐藏卡面、显示帧动画身体。
- 试点单位剑士 `Swordsman`（`unit_stats.size = 1`，Medium）已启用；`atlas.json` 单元格 258×170，42 帧（attack / attack_combo / run / hurt / death），帧底边居中于画布。
- 现状缺口（`devPlan/design/animationWorkflow.md` §5 最后一项）：
  - 缩放基准取自 `run_00` 单帧的**内容高度**（`ComputeRegionContentHeight`），基准帧是隐式的，且 `Mathf.Max(1f, ...)` 兜底会掩盖异常值；
  - 血条位置（`BodyHealthAboveHeadRatio = 0.75`）与宽度（`BodyHealthWidthRatio = 1.15`）都按 `displaySize` 硬算，未参考身体实测尺寸，小体型单位血条可能宽于身体；
  - 四档体型常量（`UnitLogic.cs`：Small 60 / Medium 90 / Large 120 / Huge 180 游戏单位，`UnitsPerCell = PixelsPerCell = 100` 即 1:1 像素）与帧动画身体、卡面占位之间缺少一致性校验。
- 影响范围仅 adapter 表现层（`UnitVisual.cs`、`UnitLogic.cs`、必要时 `UnitCardView.cs`），**不涉及 domain**，故不新增单测；`scripts/adapters/godot/dev/UnitAnimPreview.cs` + `scenes/ui/dev_tools/unit_anim_preview.tscn` 为验证入口。

## 步骤

1. **缩放基准显式化**（`UnitVisual.cs`）：把取 `run_00` 内容高度改为「基准站立帧」命名常量（`StandReferenceFrame = "run_00"`），`scale = displaySize / 基准内容高度`；保留内容高度异常（0 或超单元格）时的命名兜底常量，去掉裸 `Mathf.Max(1f, ...)` 的隐式回退。
2. **脚底锚点校验**（`UnitVisual.cs` + `prefabs/Unit.tscn`）：确认 `Centered = true` + `Offset = new Vector2(0, -cell.Y * 0.5f)` 组合后身体底边与 `Unit` 节点原点重合；逐帧切换 run / attack / hurt / death 时脚底与站立位置不跳变。
3. **血条按身体实测尺寸定位**（`UnitVisual.cs`）：`ConfigureBodyHealth` 改为「身体实测高度 + 头顶固定间距常量」定位（新增 `BodyHealthAboveHeadGapPixels`），宽度按身体实测宽度与 `displaySize` 取较小者计，确保血条不宽于身体、不遮挡刀光；替换 `BodyHealthAboveHeadRatio`。
4. **四档体型常量复核**（`UnitLogic.cs`、`UnitCardView.cs`）：核对 Small/Medium/Large/Huge 与卡面 `CardSpan` 的比例，保证同一单位在「帧动画身体」与「卡面占位」两种形态下视觉尺寸不突变；仅调整常量，不新增 DB 字段。
5. **站定行为确认**（不改代码）：确认 `SetMoving(false)` 停在当前帧的现有行为符合预期，并把「无 idle 行、站定冻结」记录进 `animationWorkflow.md`（已在 §2 注明，补 §5 状态）。
6. **验证**：`unit_anim_preview`（数字键 1-5）逐状态目视检查；跑 `alliance_battle` 冒烟（`tools/run_smoke.sh`）确认帧动画单位在正式战斗中无异常；`dotnet build` 零 warning。
7. **文档同步**：更新 `devPlan/design/animationWorkflow.md` §5（勾掉末项、记录基准帧与常量位置）；如 `assetSpecs.md` 含单位显示尺寸章节则同步数值。

## 验收项

- [x] 剑士（Medium）帧动画身体站立高度 = 90px（±2px），脚底与 `Unit` 节点原点重合
      —— 运行期实测：`stand=(76,57) 106×109`、`scale=0.8257`、`offsetY=-81`、`bodyH=90`
- [x] run / attack / hurt / death 四态切换时脚底锚点不跳变
      —— 图集实测 42 帧「内容底边到单元格底边」恒为 4px，锚点补偿统一吸收该空白
- [x] 血条位于身体实测高度之上 6px 间隙处，宽度不超过身体实测宽度（实测 `barW=87.5` = 身体宽度）
      —— 血条底边不再压住头部（原 `0.75 × displaySize` 位于头内）；攻击帧刀光高过头顶，可越过血条，属预期
- [x] 四档体型常量与卡面占位比例一致（构造保证：卡面渲染宽度 = `displaySize`，帧动画身体高度 = `displaySize`）
- [x] 站定冻结当前帧行为与文档一致（无 idle 行）
- [ ] `unit_anim_preview` 五状态 + 实机目视确认（headless 无渲染，需用户实机查看）
- [x] `alliance_battle` 冒烟 PASS；全量 8 项冒烟（audio / ui / building_action_ring / dungeon / necromancer / necromancer_battle / alliance_battle / horde_battle）全 PASS
- [x] `dotnet build CasualCastle.sln` 零 warning；三套单测全绿（Domain 651 / Persistence 35 / Application 25）
- [x] `animationWorkflow.md` §5 末项已勾掉、§6 新增尺寸与锚点规则（常量与基准帧）

## 施工记录

- 代码改动仅 `scripts/adapters/godot/battle/UnitVisual.cs`：新增 `StandReferenceFrame` / `MinBodyContentHeightPixels` / `BodyHealthAboveHeadGapPixels` / `MaxBodyHealthWidthRatio` / `BodyHealthHeightStretch`，拆分出 `ApplyBodyTransform`、`LoadStandBounds`，`ComputeRegionContentHeight`（逐像素）替换为 `ComputeRegionContentBounds`（`Image.GetUsedRect()`）。
- `UnitLogic.cs` 的 Small 60 / Medium 90 / Large 120 / Huge 180 经复核保持不变。
- 文档同步（用户决策「以代码为准」）：`assetSpecs.md` §1 体型表改为实现值（60/90/120/180、碰撞半径 0.3×），并同步节点结构、碰撞圈可见性（仅作弊模式）、有效攻击距离与间距规则；`battleSystem.md` 软分离阈值改为「半径和 × 1.6」。
