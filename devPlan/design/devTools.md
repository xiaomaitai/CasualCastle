# 开发者工具

## 开发者模式

设置面板底部"开发者模式"勾选框，开启后显示跳过阶段按钮和按键日志。

## 建筑编辑器（科技树编辑器）

独立场景 `scenes/dev/tech_tree_editor.tscn`，F6 运行。可视化编辑种族科技树：顶部 TabBar 切换种族，画布区左键拖拽调整建筑卡片位置，右键拖拽创建组合连线，双击编辑属性，右键点击连线删除。右侧素材库面板支持库↔画布双向拖拽增减节点。保存回写 `config.db` 并同步 `building_defs` 和 `shop_catalog`。关闭退出进程，切换标签自动保存。

## Shader 预览器

左侧编辑着色器代码，右侧实时预览，F5 编译，自动注入 time/resolution/mouse 三个 uniform。

## 按键日志

开发模式下打印所有按键的 Keycode、Physical、Unicode、修饰键到控制台。
