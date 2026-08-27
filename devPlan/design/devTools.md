# 开发者工具

## 开发者模式

设置面板底部"开发者模式"勾选框，开启后显示跳过阶段按钮和按键日志。

## 建筑编辑器（科技树编辑器）

从标题界面"开发者工具"菜单进入。可视化编辑派系科技树：顶部按钮切换派系，画布区左键拖拽调整建筑卡片位置，右键拖拽创建进阶连线，双击编辑属性，右键点击连线删除。右侧素材库面板支持库↔画布双向拖拽增减节点。

**进阶连线自动分类**：保存时按高级建筑的入边数判定——入边 > 1 条落 `combine_recipes`（组合，画布白/橙），入边 = 1 条落 `upgrade_recipes`（升级，画布青色）。不存在同类建筑双融合。

**价值字段**：双击建筑可编辑**价值**（= 商店价），升级费用 = 高级价值 − 低级价值，自动计算并展示。

保存回写 `config.db` 并同步 `building_defs` 和 `shop_catalog`，同步以 `tech_tree_nodes.type_id` 为准：重建该派系建筑定义与商店条目（保留 `production_rate`），商店卡 ID 与建筑 `type_id` 一致。关闭返回开发者工具菜单，切换派系自动保存。

## Shader 预览器

左侧编辑着色器代码，右侧实时预览，F5 编译，自动注入 time/resolution/mouse 三个 uniform。

## 参数编辑器

从标题界面"开发者工具"菜单进入。集中展示全局参数并按业务模块分组（经济、战斗、显示/像素换算），支持运行时热修改：修改输入框即时生效、无需重新编译；"重置"恢复单个参数默认值，"全部重置"恢复所有参数。

- 可编辑来源：domain 层 `GameParameters`（默认值取自 `GameRules` 常量，热修改供 `Shop`/`MatchPhaseMachine` 等消费）、adapter 层 `GameCoordinatesAdapter.PixelsPerCell` 与 `SfxPolicy`
- 当前参数：每局初始金币、修理每点生命花费、战斗/合成阶段时长、战斗开始等待、每格像素、音效限流 4 项
- 持久化：值按业务分别存在 `config.db` 的业务配置表（`economy_config`/`battle_config`/`display_config`/`sound_config`，结构为 key/value），编辑器修改/重置即时写回对应表，核心启动（`CompositionRoot.Build`）从各表读入静态参数；不为编辑器专门建表

## 按键日志

开发模式下打印所有按键的 Keycode、Physical、Unicode、修饰键到控制台。
