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
| 基础框架 | 主场景 1280×720、`GameManager` / `UIManager` | `scenes/main/main_game.tscn`, `scripts/autoload/` |
| 兵营与单位 | 兵营计时产兵、士兵属性与预制体 | `prefabs/Barracks.tscn`, `prefabs/Soldier.tscn`, `scripts/nodes/Barracks.cs`, `scripts/nodes/Soldier.cs` |
| 战斗系统 | 移动、互殴、攻建筑、死亡 | `scripts/nodes/Soldier.cs` |
| 城堡与胜负 | 城堡预制体、扣血、结算 | `prefabs/Castle.tscn`, `scripts/nodes/Castle.cs` |
| 场景配置 | 格子放置兵营、碰撞层、战场布局 | `scripts/nodes/Building.cs`, `scripts/nodes/Castle.cs` |

**场景约定：** 玩家兵营 (7,4)、敌方 (0,4)；建筑碰撞 56×56；产兵于相邻格左下角。

概念与实现细节见 `../concepts.md`。

## 3.2 体验增强（M0+，M0 之后追加）

| 模块 | 交付内容 | 关键文件 |
|------|----------|----------|
| 标题界面 | 开始游戏 / 退出，场景切换 | `scenes/ui/title_screen.tscn`, `scripts/ui/TitleScreen.cs` |
| 基础 UI | 双方血条、结算遮罩、「返回标题」 | `scripts/autoload/UIManager.cs`, `main_game.tscn` UI 节点 |
| BGM | 标题与主游戏循环播放 | `scripts/nodes/BgmPlayer.cs`, `assets/audio/bgm/` |
| 开发工具 | 按键日志、作弊产兵（P 键） | `scripts/utils/DevInputLogger.cs`, `GameManager.cs` |

**入口流程：** `project.godot` → `title_screen.tscn` → `main_game.tscn`。

## 3.3 已具备、可复用的基础 API

以下代码为完整版开发提供了基础，但尚未接入玩家交互：

| API / 能力 | 位置 | 说明 |
|------------|------|------|
| 城堡格子与放置 | `Castle.PlaceBuilding`, `GetBuildingSpawnPosition` | 目前仅 `SetupBarracks()` 自动放置 |
| 格子通行检测 | `Castle.IsCellPassable` | 士兵未使用寻路，API 已预留 |
| 建筑基类 | `Building.cs` | 碰撞层、阵营；无独立 HP |
| 卡牌边框素材 | `assets/art/cards/card_border.png` | 已导入，代码未引用 |

---
