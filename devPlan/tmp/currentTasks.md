# P7 存档与打磨

## 目标

实现游戏进度存档/读取和摄像机缩放平移。数值平衡跳过。

## 任务拆解

### 7.1 存档系统 ✅

保存到 `user://saves/`，SQLite 格式（一个存档一个 `.db`）。保存时机：手动（暂停菜单）+ 自动（每夜结束时）。

**保存内容：**
- 建筑布局（类型、格位坐标、血量）
- 金币
- 昼夜轮次
- 当前手牌
- 关联战报 ID

**代码改动：**
- `domain/Shared/SaveModels.cs` — SaveData、BuildingSaveEntry、CardSaveEntry
- `domain/Shared/ISaveRepository.cs` — 端口接口
- `adapters/persistence/SaveStorage.cs` — SQLite 读写 `user://saves/save_0.db`
- `GameManager` — SaveGame/LoadSaveData/HasSave 方法 + PendingLoadSlot
- `InitManager.LoadSaveIntoGame` — 从存档恢复建筑/金币/手牌/夜数
- `PauseMenuUiController` — 「保存游戏」按钮
- `TitleScreen` — 「继续游戏」按钮（无存档时灰显）

**验收项：**
- 手动保存 → 关闭游戏 → 标题界面继续 → 恢复建筑布局、金币、轮次 ⏳
- 每夜结束自动保存
- 无存档时「继续游戏」按钮灰显
- 游戏结束时删除存档

### 7.2 摄像机系统 ✅

**功能：**
- 滚轮缩放（限制 0.5× ~ 2.0×）
- 中键拖拽平移
- 边界限制（不超出战场区域）
- Home 键重置视角

**代码改动：**
- `adapters/godot/flow/CameraController.cs` — 挂载到 Battlefield/Camera2D
- `scenes/main/main_game.tscn` — Camera2D 节点添加 script 引用

**验收项：**
- 滚轮缩放，不超出 0.5~2.0 范围 ⏳
- 中键拖拽平移，不超出战场边界 ⏳
- Home 键重置视角到默认位置 ⏳
