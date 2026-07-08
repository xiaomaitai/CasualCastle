# P2 建筑与通道系统

## 目标

建筑碰撞体贴合格子边缘（100 游戏单位/格），格间形成 ~15 单位通行通道；多格建筑内部无通道；导航网格随建筑放置/摧毁动态更新。

---

## 任务拆解

### 2.1 建筑碰撞体尺寸调整

将建筑碰撞体从当前 80×80（默认）改为填满所在格子（单格 100×100 游戏单位，多格按占地累加）。碰撞体变更影响：士兵物理阻挡、NavigationObstacle2D 尺寸、城堡网格渲染。

**验收项：**
- `BuildingData.CollisionWidth/Height` 默认值改为 `CellBlockSize`（94）或 100
- `config.db` 中现有建筑记录的 `collision_width/height` 更新为填满格子的值
- `BuildingSystem.ApplyVisual` 碰撞尺寸逻辑适配多格建筑（总尺寸 = 占地格数 × 格大小）
- 多格建筑碰撞体为**单个矩形**，覆盖全部占地格子，内部无间隙

### 2.2 导航网格动态障碍

建筑放置时在导航网格中挖洞，使士兵无法穿越建筑所在格子；建筑摧毁时恢复可通行。格间空隙（~15 单位）保持可通行。

**实现方案：** 为 Building 预制体添加 `NavigationObstacle2D` 子节点，放置时根据建筑占地动态设置障碍尺寸与位置。

**验收项：**
- `Building.tscn` 预制体新增 `NavigationObstacle2D` 节点
- 建筑放置时，`NavigationObstacle2D` 自动在导航网格中挖出建筑占地范围的洞
- 建筑摧毁时，障碍自动移除，对应区域恢复可通行
- 两座相邻单格建筑之间存在可通行的格间通道（~15 单位）
- 多格建筑（2×2 等）内部无通道，士兵只能从外围绕行

### 2.3 碰撞层配置

启用建筑与士兵之间的物理碰撞检测，使士兵无法穿过建筑碰撞体。

**验收项：**
- `Building.tscn` 碰撞层设为 Layer 3（值 4）
- `Soldier.tscn` 碰撞掩码含 Layer 3（值 4），检测建筑
- 士兵无法穿越建筑碰撞体（物理阻挡 + 导航绕行双重保障）

### 2.4 OccupancyGrid 与导航同步

确保 `OccupancyGrid` 状态与导航障碍保持一致。放置/摧毁建筑时同步更新占用状态和导航网格。

**验收项：**
- `Castle.PlaceBuilding` 中，`OccupancyGrid.OccupyCells` 与 `NavigationObstacle2D` 激活同步
- `Castle.ReleaseBuildingFootprint` 中，`OccupancyGrid.ReleaseCells` 与障碍移除同步
- `Castle.IsCellPassable` 正确反映当前占用状态（供导航查询参考）

### 2.5 集成测试

验证完整的建筑-通道-导航链路。

**验收项：**
- 放置两座相邻单格建筑 → 士兵可通过格间空隙行走
- 放置 2×2 多格建筑 → 士兵无法穿越建筑内部，自动绕行
- 摧毁建筑 → 对应区域恢复可通行
- 多建筑场景 → 士兵正确沿通道网络寻路至敌方城堡
- 放置预览时正确显示碰撞占地区域
