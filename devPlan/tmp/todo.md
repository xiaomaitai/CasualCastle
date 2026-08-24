# 架构待办（TODO）

## 建筑美术与素材管线（原 v0.7，2026-08-25 移入 backlog）

原 v0.7「建筑美术完成体」整体延后，由 v0.7 完善版替代。后续如需推进见 `devPlan/outline/c10BuildingArtPlan.md`（联盟 36 张 + 部落 16 张透明建筑图、底座锚定、泥地去除），另含四派系通用素材债：

- 建筑贴图：全库 100 座建筑仅 Stable 有实图，其余 `texture_path=test_image.png` 占位
- 地下城：15 个单位（14 恶魔 + 魔王）无 portrait_uid；33 座建筑全占位；技能图标占位
- 技能描述：部分 skill_defs 描述为 `????` / 乱码文本

## 建筑素材图集化（素材量增大后）

同屏数十座建筑时，单张 `_game.png` 纹理的绑定/批处理收益有限，暂不改造；等建筑素材量或同屏规模进一步增大后，改用 Godot 图集导入（`ResourceImporterTextureAtlas`）。

- 前置：保持 `_cut → _game` 阶段不变，图集只打包裁边 + 512/256 的 `_game.png`
- 按阵营或阶级拆 2~3 张图集（单张 4096 上限装不下全部 51 座）
- DB 引用 Godot 生成的图集区域资源，重打包自动跟随，不写死坐标
- 开启 `trim_alpha_border_from_region`；注意图集区域与 mipmap 冲突（当前已关 mipmap）
- 先试点（如联盟建筑）验证导入流程与批处理收益，再铺开
## 动画工作流构建（DSH 工具 + Skill 编排）

以 DeepSeek Harness 工具 + Skill 结合的方式构建端到端动画工作流：输入开发者文字描述，输出游戏内可用的动画（默认首尾无缝循环，个别可不循环）。

- 输入：开发者描述（对象、动作意图、时长、是否循环）
- 中间步骤（含但不限于）：AI 生成单位主图/建筑主图等静态效果图 → AI 生成视频 → 必要时抽帧或精修（去背景、裁边、补帧、循环对齐）→ 接入 Godot（SpriteFrames / AnimatedSprite2D，经 godot-import 导入）
- 输出：游戏内可用动画，多数首尾循环，个别可不循环
- 中间所需软件/API（需求暂空，待用户补充）：

| 环节 | 现状 | 需求/备选（待补充） |
|------|------|--------------------|
| 生图 | Liblib.art（generate-image skill、tools/liblib_gen.py） |  |
| 抠图 | Liblib remove-bg（tools/liblib_remove_bg.py，nodeId=17） |  |
| 视频生成 | 无 |  |
| 抽帧/精修 | 无 |  |
| Godot 导入 | godot-import skill、tools/godot_import.py |  |

## 参数编辑器（开发者模式 · 主菜单）

主菜单"开发者工具"下新增参数编辑器：把选中的全局参数集中到一处统一修改，支持运行时热修改。

- 覆盖参数示例：每局初始金钱、每回合获得金钱、每游戏单位对应像素（PixelsPerCell）等全局信息
- 按业务模块分类展示（经济、战斗、显示/像素换算等），纳入参数列表由用户选定
- 运行时热修改即时生效；是否持久化由用户选择
- 代码位置：场景 `scenes/ui/dev_tools/`，逻辑 `scripts/adapters/godot/dev/`（开发者工具不走单测）
- 前置：部分参数当前散落在 domain 常量（GameRules、GameCoordinateRules）与 adapter（GameCoordinatesAdapter）中，需先整理出可编辑来源

## 生图/视频工作流双源（可替换）

生图工作流与视频工作流各准备两个源：早期开发用 affordable 源，最终版用高质量源；源独立抽出、可替换。

- 双源：低成本源（早期开发，如当前 Liblib.art Qwen-Image 性价比模型）+ 高质量源（最终版，模型/分辨率/后处理更高规格）
- 可替换：现有及未来工作流只依赖"源"抽象，不写死具体服务，通过配置/环境变量切换
- 覆盖：生图源与视频生成源分别抽离，供"动画工作流构建"等任务复用

## Godot MCP 接口接入

接入 Godot MCP（Model Context Protocol），让 DSH/AI agent 能直接与 Godot 编辑器及运行实例通信，替代人工在编辑器内操作。

- 能力范围：读取/修改场景树与节点属性、启动/运行游戏、截图取景、触发素材导入与保存等
- 配合：与现有 godot-import skill、动画工作流构建、参数编辑器等任务打通，agent 可直接操作 Godot 而无需用户手动点按
- 待定：所选 MCP server 实现（社区方案）及其与 Godot 4.6 / .NET 的兼容性、授权与安全边界
