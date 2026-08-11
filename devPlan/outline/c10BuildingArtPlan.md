# 十三、建筑美术（v0.7 方案大纲）

## 1. 现状与问题

- 全部 52 座建筑（联盟 36 / 部落 16）复用同一张占位图 assets/art/placeholders/test_image.png，靠 building_defs.sprite_modulate_* 颜色滤镜区分，视觉上无法辨识建筑类型与派系
- 建筑精灵被拉伸填满占地碰撞框，无透视高度感
- 城堡 8x8 网格始终绘制 4 张泥土地基贴图（cell_dirt_01..04），建筑脚下也铺着泥土，放置后不消失

## 2. 目标

- 每座建筑一张独立透明（RGBA）建筑图，不画底座/地面/阴影，不再用颜色滤镜区分建筑
- 建筑放置后，其占地格子的泥土地基消失，建筑直接"站"在格子上；建筑被摧毁后泥地恢复
- 建筑按画布等比缩放，底边中点锚定占地中心，采用平视略俯视角（正面略俯视），保留纸片人风格

## 3. 范围

- 战场内双方城堡的建筑显示（玩家/敌方/战报回放共用 Building.tscn 预制体，一处改动全覆盖）
- 不涉及：卡牌图标、商店列表、科技树编辑器、单位美术

## 4. 素材规范（建筑主图）

正式名称：**建筑主图**。完整规格见 `devPlan/design/assetSpecs.md` 第 2 章，此处只列关键规则：

- 目录与命名：`assets/art/buildings/{typeId}_{taskId}.png`；`building_defs.texture_uid` 登记游戏纹理的 Godot 资源 uid
- 每座建筑一张独立 RGBA 透明图；**不画底座/地面/阴影**，底边即落地线；不得用颜色滤镜区分建筑
- 尺寸分级（不同占地不同画布）：1×1：512×512；2×2 标准：768×768；2×2 高地标（城堡之心、王庭、部落核心 L4/L5）：1024×1024；主体横向铺满画布（左右 ≤2% 边距），底边贴画布下缘
- 视角：固定关键词 orthographic 3/4 top-down（正交 3/4 俯视，非等距菱形格、非透视、非平视）
- 锚定：底边中点对齐占地中心，等比缩放 scale = 占地像素宽 ÷ 画布宽
- 风格三要素全局一致（flat 2D、粗描边、扁平色）；**材质/装饰/配色按种族微调**（联盟石木旗帜 / 兽人木骨尖刺 / 哥布林废料棚 / 半人马皮帐 / 半人鹿藤蔓树屋 / 蜥蜴人沼泽泥巢）
- 管线：generate-image 生图（纯白底 RGB）→ 人工筛选 → remove-bg 抠图（RGBA 原地替换）→ godot-import 导入 → 登记 texture_uid
- 导入沿用现有纹理设置（无损、fix_alpha_border）

### 风格候选（F0）

以用户定稿提示词为基准（含固定关键词 orthographic 3/4 top-down），搭配参考图 assets/art/references/马厩.png，尝试多个模型/API 生成候选，人工筛选定稿后按种族微调批量执行。

## 5. 程序改动

### 5.1 数据层

- building_defs 新增列：sprite_offset_y（可选，Y 轴微调像素，默认 0，仅个别构图微调）；无底座后不再需要底座矩形列
- 迁移沿用现有 Ensure*Column 幂等模式；旧数据默认 0 即标准底边锚定
- 全部建筑 sprite_modulate_* 重置为 1,1,1,1（列保留，受伤/摧毁灰化仍使用）
- BuildingVisualData 增加字段，SqliteBuildingRepository 按序号读取新列

### 5.2 渲染对齐（BuildingSystem.ApplyVisual）

- 等比缩放：缩放系数 = 占地像素宽 ÷ 画布宽（纹理宽）
- 锚定：图像底边中点 = 占地中心；精灵中心 Y 上移 = 画布高 × 0.5 × 缩放系数，再叠加 sprite_offset_y
- 受击灰化、摧毁淡出、工作跳动/进度着色全部沿用现有 Sprite 逻辑；透明像素不受工作 shader 影响

### 5.3 地基去除（Castle）

- _Draw 绘制泥地时跳过 _occupancy.IsCellPassable == false 的格子（即被建筑占用的格子）
- 占用/释放变化后调用 QueueRedraw()：PlaceBuilding、ReleaseBuildingFootprint、ReplaceHeart、ClearNonCoreBuildings
- 建筑摧毁 → 释放格子 → 泥地恢复；放置预览不改（放置前仍显示泥地 + 半透明预览色块）

## 6. 开发者工具（辅助回填锚定数据）

- 新增「建筑对齐预览」开发者工具（scripts/adapters/godot/dev/ + scenes/ui/dev_tools/）：游戏内叠加显示底边中点与占地中心参考线，逐座核对、微调 sprite_offset_y
- 与现有 dev tools 模式一致，不参与单元测试

## 7. 实施批次与验收

| 批次 | 内容 | 验收 |
|------|------|------|
| F0 风格候选 | 马厩候选 Stable_ref_118 / Stable_plain_119 定稿，参考图入 assets/art/references/ | 提示词、视角与模型定稿 |
| F1 基建 | schema、渲染对齐、地基去除 | 试做 2~3 张透明建筑图跑通全流程（生图→抠图→导入→登记→入局） |
| F2 联盟 36 张 | 按线分批：王国军线 → 神殿卫士线 → 冒险者公会线 → 核心与杂项 | 每批入局检查：底座对齐、无滤镜辨识 |
| F3 部落 16 张 | 兽人/哥布林/半人马/半人鹿/蜥蜴人 + 部落核心 L1-L5 + 地洞 | 同上 |
| F4 打磨 | 逐座对齐/遮挡/y-sort 检查、摧毁恢复、性能、文档 | 全量验收 |

总验收标准：

- [ ] 52 座建筑全部为独立透明图，不再使用颜色滤镜辨识
- [ ] 建筑等比缩放、底边中点对齐占地中心，无拉伸变形
- [ ] 放置后占地泥地消失，建筑底边直接落地；摧毁后泥地恢复
- [ ] 单位寻路/碰撞/弹体/光环/受击表现不受影响
- [ ] 素材 .import/.uid 随版本提交，config_db_dump.sql 由 pre-commit 更新

## 8. 文档同步

- assetSpecs.md：重写建筑章节（目录、尺寸、无底座与视角规范、生图/抠图流程、锚定规则）
- c08RenderPerspective.md：勾选「建筑 sprite」待办
- c05Milestones.md：登记 v0.7