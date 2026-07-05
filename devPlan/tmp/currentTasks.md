# 当前任务：P1 草地战场

## 背景

当前战场为灰色单色背景（`ColorRect` 纯色填充），无可视化地形。P1 目标是将战场替换为绿色草地场景，包含草地底纹、装饰草和山云背景。

## 目标

用绿色草地纹理、随机装饰草和远景山云图片替换现有灰色战场，战场功能区域（城堡位置、交战区）不变。

---

## 实施步骤

### P1.0 素材生成（使用 Liblib.art）

**工具**: `.claude/skills/generate-image.md`

使用已配置的 Liblib API 生成 5 张素材图：

| 素材 | 尺寸 | 保存路径 | 说明 |
|------|------|---------|------|
| 草地底纹 | 512×512 | `assets/textures/terrain/grass_tile.png` | 绿色草地无缝平铺纹理 |
| 装饰草 A | 256×256 | `assets/textures/terrain/grass_clump_1.png` | 草丛 variant，透明背景 |
| 装饰草 B | 256×256 | `assets/textures/terrain/grass_clump_2.png` | 草丛 variant，透明背景 |
| 装饰草 C | 256×256 | `assets/textures/terrain/grass_clump_3.png` | 草丛 variant，透明背景 |
| 远山背景 | 1920×540 | `assets/textures/background/mountains.png` | 远山剪影，底部透明过渡 |
| 云层背景 | 1920×540 | `assets/textures/background/clouds.png` | 云朵，透明背景 |

**生图流程**：逐张调用 `generate-image` skill → 下载到指定路径 → 验证尺寸和透明通道 → 导入 Godot（`CompressedTexture2D`，无损压缩）。

### P1.1 山云背景

**文件**: 修改 `scenes/main_game.tscn` — 在 Battlefield 节点下新增 `ParallaxBackground` 节点

- 在 Battlefield 节点下、现有 `Background` ColorRect 之上添加 `ParallaxBackground`
- 包含两层 `ParallaxLayer`：
  - **远山层**：`Sprite2D` 使用 `mountains.png`，`motion_scale` = (0.1, 0.1)，置于战场后方
  - **云层**：`Sprite2D` 使用 `clouds.png`，`motion_scale` = (0.05, 0.05)
- 替换现有绿色 `ColorRect` Background 为 `mountains.png` 底部延伸的纯色底
- `ParallaxBackground` 的 `layer`（z_index）低于草地和游戏对象

### P1.2 草地底纹

**文件**: 修改 `scenes/main_game.tscn` — 在 Battlefield 节点下新增草地底纹层

- 使用 `TextureRect` 平铺 `grass_tile.png`（`TextureRect` 设置 `stretch_mode = tile`）
- 覆盖范围与战场区域一致（设计分辨率 1920×1080，需根据实际战场游戏单位范围换算像素后进行适当外扩）
- 草地底纹层 z_index 在背景之上、装饰草之下
- 替换现有绿色 `ColorRect` Background（P1.1 完成后即可移除）

### P1.3 装饰草

**文件**: 新建 `scripts/adapters/godot/battlefield/GrassDecoration.cs`

- 继承 `Node2D`，作为 Battlefield 的子节点
- `_Ready()` 中初始化 `MultiMeshInstance2D`：
  - 读取 `GameConfig.DesignWidth` / `GameConfig.DesignHeight` 确定散布范围（像素）
  - 随机生成每簇草的位置（排除两侧城堡网格区域）、旋转（0–360°）、缩放（0.8–1.2）、variant index（0/1/2）
  - 密度：约 500–800 簇覆盖整个战场（而非按 100×100 单位计算，避免过度密集）
  - `instance_count` 控制在 2000 以内
- 装饰草无碰撞体，纯视觉，z_index 在底纹之上、游戏对象之下
- 无需实现接口，直接挂载到 Battlefield 节点即可
- 精灵资源：`grass_clump_1/2/3.png`

### P1.4 集成与验证

**文件**: `scenes/main_game.tscn`

- z_index 层级：背景(ParallaxBackground) < 草地底纹 < 装饰草 < 城堡 < 建筑 < 士兵 < UI(CanvasLayer)
- 移除或替换现有 `Background` ColorRect（被 P1.1/P1.2 替代）
- Battlefield 的 `y_sort_enabled` 保持启用
- 验证导航网格和碰撞检测不受影响

---

## 验收项

1. 战场显示为绿色草地纹理，不再是纯色灰色/绿色背景
2. 草地上可见随机散布的装饰草丛，无明显规律感
3. 战场后方可见山和云的远景，摄像机移动时有视差效果
4. 城堡、建筑、士兵在草地上的渲染正确（z_index 排序、y_sort 无误）
5. 装饰草不影响士兵移动、寻路、战斗
6. 装饰草不影响建筑放置和碰撞检测
7. 运行帧率与当前相当（MultiMeshInstance2D 开销可忽略）

---

## 涉及文件清单

| 文件 | 变更类型 |
|------|---------|
| `scenes/main_game.tscn` | 修改 Battlefield 子节点结构（背景、底纹、装饰草） |
| `scripts/adapters/godot/battlefield/GrassDecoration.cs` | 新建 |
| `assets/textures/terrain/grass_tile.png` | 新建（Liblib 生成） |
| `assets/textures/terrain/grass_clump_1.png` | 新建（Liblib 生成） |
| `assets/textures/terrain/grass_clump_2.png` | 新建（Liblib 生成） |
| `assets/textures/terrain/grass_clump_3.png` | 新建（Liblib 生成） |
| `assets/textures/background/mountains.png` | 新建（Liblib 生成） |
| `assets/textures/background/clouds.png` | 新建（Liblib 生成） |

## 注意事项

- **素材先行**：P1.0 先生成所有 6 张素材图，后续步骤依赖素材就绪。素材生成后立即导入 Godot
- 装饰草散布范围排除城堡网格区域（城堡内部地面由城堡自身渲染）
- 所有位置计算使用游戏单位 → `GameCoordinatesAdapter` 转换为像素
- `ParallaxBackground` 跟随 `Battlefield/Camera2D` 自动滚动
- 山/云图片尺寸 1920×540 覆盖战场宽度，高度为半屏
