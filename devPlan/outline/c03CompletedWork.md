# 三、已完成内容

## 3.1 极简 MVP（M0，2026-06-15）

最小可玩循环：**兵营产兵 → 士兵前进/战斗 → 攻城堡 → 胜负判定**。

| 目标 | 状态 |
|------|------|
| 双方各有一座兵营 | ✅ |
| 兵营自动产出士兵 | ✅ |
| 单位自动移动并战斗 | ✅ |
| 城堡血量与胜负判定 | ✅ |
| 完整游戏循环人工验证 | ✅ |

| 模块 | 交付内容 | 关键文件 |
|------|----------|----------|
| 基础框架 | 主场景 1280×720、`GameManager` / `UIManager` | `scenes/main/main_game.tscn`, `scripts/autoload/`, `scripts/ui/`, `scripts/flow/` |
| 兵营与单位 | 兵营计时产兵、士兵属性与预制体 | `prefabs/Barracks.tscn`, `prefabs/Soldier.tscn`, `scripts/building/Barracks.cs`, `scripts/battle/Soldier.cs` |
| 战斗系统 | 移动、互殴、攻建筑、死亡 | `scripts/battle/Soldier.cs` |
| 城堡与胜负 | 城堡预制体、扣血、结算 | `prefabs/Castle.tscn`, `scripts/building/Castle.cs` |
| 场景配置 | 格子放置兵营、碰撞层、战场布局 | `scripts/building/Building.cs`, `scripts/building/Castle.cs` |

**场景约定：** 玩家兵营 (7,4)、敌方 (0,4)；建筑碰撞 56×56；产兵于建筑占地框**左下角**附近（`GameCoordinates` + `UnitSpawn`）。

概念与实现细节见 `../concepts.md`。

## 3.2 体验增强（M0+，M0 之后追加）

| 模块 | 交付内容 | 关键文件 |
|------|----------|----------|
| 标题界面 | 开始游戏 / 退出，场景切换 | `scenes/ui/title_screen.tscn`, `scripts/flow/TitleScreen.cs` |
| 基础 UI | 双方血条、结算遮罩、「返回标题」 | `scripts/ui/UIManager.cs`, `main_game.tscn` UI 节点 |
| BGM | 标题与主游戏循环播放 | `scripts/audio/BgmPlayer.cs`, `assets/audio/bgm/` |
| 开发工具 | 按键日志、作弊产兵（P 键） | `scripts/dev/DevInputLogger.cs`, `GameManager.cs` |

**入口流程：** `project.godot` → `title_screen.tscn` → `main_game.tscn`。

## 3.3 游戏流程框架（M1）

白天 / 夜晚两阶段循环已完成，并完成游戏内人工验证。

| 模块 | 交付内容 | 关键文件 |
|------|----------|----------|
| 阶段状态机 | `Day` / `Night` 自动循环，支持手动跳过阶段 | `scripts/autoload/GameManager.cs`, `scripts/core/GameConfig.cs` |
| 夜晚休眠门控 | 普通兵营与士兵夜晚停止工作/行动，白天恢复 | `scripts/night/NightSystem.cs`, `scripts/building/Building.cs`, `scripts/battle/Soldier.cs` |
| 阶段 UI | 显示当前阶段、剩余时间和跳过阶段按钮 | `scripts/ui/UIManager.cs`, `scenes/main/main_game.tscn` |
| 系统占位 | `ShopSystem`、`CardSystem` 占位，供 M2 扩展 | `scripts/shop/ShopSystem.cs`, `scripts/card/CardSystem.cs` |
| 建筑工作特效 | 兵营产兵前播放自下而上变亮、跳动、恢复效果 | `scripts/building/Building.cs`, `scripts/building/Barracks.cs`, `assets/shaders/building_work.gdshader` |

## 3.4 夜晚经济与手牌（M2）

商店、手牌与建筑卡放置已完成，并通过 Godot 人工验证。

| 模块 | 交付内容 | 关键文件 |
|------|----------|----------|
| 商店系统 | 商品刷新、购买扣费、夜晚自动弹出、拖拽商品直放城堡 | `scripts/shop/ShopSystem.cs`, `scripts/ui/ShopUiController.cs` |
| 手牌系统 | 手牌增删、选中、点击放置、拖拽放置 | `scripts/card/CardSystem.cs`, `scripts/card/CardData.cs`, `scripts/ui/HandUiController.cs` |
| 放置预览 | 合法/非法地块高亮，Esc / 右键取消 | `scripts/building/Castle.cs`, `scripts/ui/HandUiController.cs` |
| UI 联动 | 商店打开不遮挡操作；结算时屏蔽输入 | `scripts/ui/UIManager.cs`, `scenes/main/main_game.tscn` |

**交互要点：**

- 商店可随时手动打开/关闭；进入夜晚自动弹出
- 购买按钮将卡牌加入手牌；拖拽商品名可在金币足够时直接放置
- 手牌支持点击选中后点击城堡放置，或拖拽到城堡放置
- 支持兵营、靶场、马厩三种建筑及多格占地

## 3.5 建筑与邻接（M3）

多建筑放置、统一放置系统与邻接加成已完成最小切片。

| 模块 | 交付内容 | 关键文件 |
|------|----------|----------|
| BuildingSystem | 统一 `TryPlace`、占地与 main 格配置 | `scripts/building/BuildingSystem.cs` |
| 多建筑 | 兵营、靶场（2 格）、马厩（L 形 4 格） | `ArcheryRange.cs`, `Stable.cs`, `prefabs/` |
| AdjacentSystem | 邻接检测、兵营加速、放置触发刷新 | `scripts/building/AdjacentSystem.cs` |
| 邻接特效 | main 格 shader 光圈 | `assets/shaders/adjacent_link_pulse.gdshader`, `AdjacentLinkPulse.cs` |

## 3.6 已具备、可复用的基础 API

以下代码为完整版开发提供了基础，但尚未接入玩家交互：

| API / 能力 | 位置 | 说明 |
|------------|------|------|
| 城堡格子与放置 | `Castle.PlaceBuilding`, `IsCellPassable`, `SetPlacementPreview` | 手牌与商店直放已接入 |
| 手牌放置 | `BuildingSystem.TryPlace` | 手牌与商店直放统一入口 |
| 占地与 main 格 | `BuildingSystem.GetFootprint`, `GetMainCellOffset` | 按建筑类型配置 |
| 邻接加成 | `AdjacentSystem.RefreshCastle` | 兵营邻接产兵加速 |
| 格子通行检测 | `Castle.IsCellPassable` | 士兵未使用寻路，API 已预留 |
| 建筑基类 | `Building.cs` | 碰撞层、阵营、昼夜工作循环；无独立 HP |
| 卡牌边框素材 | `assets/art/cards/card_border.png` | 已导入，UI 尚未引用 |

## 3.6 夜战单位（M4）

| 模块 | 交付内容 | 关键文件 |
|------|----------|----------|
| 夜战词条 | `HasNightCombat` 数据化，入夜休眠门控 | `BuildingSystem`, `Building`, `Soldier`, `NightSystem` |
| 狼穴 | 夜战建筑，产紫色狼人 | `WolfDen` 定义，`ShopSystem` 目录 |
| UI | 信息面板「夜晚可行动 / 夜晚休眠」 | `BuildingInfoUiController` |

## 3.7 融合系统（M5）

| 模块 | 交付内容 | 关键文件 |
|------|----------|----------|
| 入夜融合 | 配方匹配、主体邻接、免费、满血结果 | `FusionSystem`, `FusionRecipe` |
| 强化建筑 | `BarracksT2`、`WolfDenT2` | `BuildingSystem` |
| 禁止融合 | 工具按钮 + `IsFusionProhibited` | `FusionProhibitUiController`, `BuildingStateIcon` |

设计文档：`../fusionSystemDesign.md`。

## 3.8 战报与回放式敌方（M6）

| 模块 | 交付内容 | 关键文件 |
|------|----------|----------|
| 战报录制 | 每夜结束玩家城堡快照、局内缓存 | `BattleReportSystem`, `BattleReportModels` |
| 持久化 | `user://battle_reports/` 保存 / 加载 / 列表 | `BattleReportStorage` |
| 结算 UI | 胜负后询问是否保存战报 | `GameOverUiController` |
| 开局选报 | 标题页选已保存战报作为敌方参考 | `TitleScreen` |
| 镜像复刻 | 第 N 夜开始按快照镜像敌方布局；士兵占格跳过 | `ReplayAiSystem` |
| 入夜串联 | 融合 → 敌方复刻 → 夜晚流程 | `GameManager.BeginPhase` |

设计文档：`../battleReportDesign.md`、`../aiSystemDesign.md`。

## 3.9 游戏坐标与产兵（架构试点前奏）

| 模块 | 交付内容 | 关键文件 |
|------|----------|----------|
| 游戏坐标 | 整数坐标（每格 100 单位）、像素防腐换算 | `scripts/core/GameCoordinates.cs` |
| 产兵放置 | 建筑占地框左下角产兵；先入树再设 `GlobalPosition` | `scripts/battle/UnitSpawn.cs`, `Building.cs` |

待迁入六边形核心域，见 `currentTasks.md`。

---
