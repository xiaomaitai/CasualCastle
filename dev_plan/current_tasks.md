# 当前任务（极简 MVP）

说明：极简MVP仅包含最核心的游戏循环：初始兵营 → 自动出兵 → 双方战斗 → 胜负判定。

---

## MVP 核心目标
- ✅ 玩家与敌方各有一个兵营建筑
- ✅ 兵营自动产出士兵单位
- ✅ 单位自动移动并战斗
- ✅ 城堡血量与胜负判定

---

## 分阶段任务

### 阶段 1 — 项目基础框架
- [x] 1.1 创建主游戏场景（main_game.tscn）
  - 完成：2026-06-09 by Assistant — 创建了 `scenes/main/main_game.tscn`，包含战场布局、玩家/敌方区域、UI层（血量条、游戏结束面板）
- [x] 1.2 设置窗口大小与基本配置
  - 完成：2026-06-09 by Assistant — 在 `project.godot` 中设置窗口大小为 1000x600，固定窗口大小
- [x] 1.3 创建游戏根节点结构
  - 完成：2026-06-09 by Assistant — 创建了脚本目录结构（`scripts/autoload/`、`scripts/systems/`、`scripts/nodes/`、`prefabs/`），并创建了 `GameManager.cs` 和 `UIManager.cs` 自动加载管理器

### 阶段 2 — 兵营与单位系统
- [x] 2.1 创建兵营预制体（Barracks.tscn）：包含生产计时器
  - 完成：2026-06-13 by git commit 573cbb5 — 创建 `prefabs/barracks.tscn` 和 `scripts/nodes/Barracks.cs`，包含生产计时器
- [x] 2.2 创建士兵预制体（Soldier.tscn）：包含血量、伤害、速度属性
  - 完成：2026-06-13 by Assistant — 创建 `scripts/nodes/Soldier.cs`（血量30、伤害10、速度80、攻击范围30、冷却1s）和 `prefabs/Soldier.tscn`，使用 goblin.png 贴图，Area2D 碰撞检测
- [x] 2.3 兵营自动产兵逻辑：每隔N秒生成一个士兵
  - 完成：2026-06-13 by Assistant — Barracks.cs 已有产兵逻辑，使用 Timer 自动生成 Soldier 实例并设置阵营

### 阶段 3 — 战斗系统
- [x] 3.1 单位移动逻辑：向敌方城堡前进
  - 完成：2026-06-15 by Assistant — `Soldier.cs` 按阵营向右/左移动，遇敌追击，到达敌方城堡区域后停止
- [x] 3.2 碰撞检测：单位之间、单位与城堡
  - 完成：2026-06-15 by Assistant — 士兵 `Area2D`（layer 2）互检；`CastleArea` 改为 `Area2D`（layer 4），士兵 mask=6 可检测城堡区域
- [x] 3.3 攻击与伤害结算
  - 完成：2026-06-13 by Assistant — 攻击范围内按冷却造成伤害（伤害10，冷却1s，范围30px）
- [x] 3.4 单位死亡处理
  - 完成：2026-06-13 by Assistant — 血量≤0 时禁用碰撞、半透明淡出，0.5s 后 `QueueFree`

### 阶段 4 — 城堡与胜负
- [x] 4.1 创建城堡预制体（Castle.tscn）：包含血量显示
  - 完成：2026-06-15 by Assistant — 创建 `prefabs/Castle.tscn` 和 `scripts/nodes/Castle.cs`，含 8×8 网格、碰撞体、血量条；主场景改为实例化预制体，移除 `CastleArea`
- [x] 4.2 单位攻击城堡逻辑
  - 完成：2026-06-15 by Assistant — 士兵进入敌方城堡区域后按冷却调用 `Castle.TakeDamage`
- [x] 4.3 胜负判定：任一城堡血量归0则游戏结束
  - 完成：2026-06-15 by Assistant — `Castle.TakeDamage` 联动 `GameManager`，血量归零触发 `EndGame` 并更新顶部血条
- [x] 4.4 游戏结束界面与重新开始
  - 完成：2026-06-15 by Assistant — 主场景挂载 `GameManager`/`UIManager`，游戏结束显示面板，重开时 `ReloadCurrentScene` 重置战场

### 阶段 5 — 场景配置
- [x] 5.1 布置战场：玩家兵营、敌方兵营、玩家城堡、敌方城堡
  - 完成：2026-06-15 by Assistant — 兵营由城堡按格子分配（玩家 7,4 / 敌方 0,4），作为城堡子节点放置，移除像素定位
- [x] 5.2 设置碰撞层与物理属性
  - 完成：2026-06-15 by Assistant — 沿用 Layer2 士兵 / Layer3 城堡约定，兵营不再单独配置碰撞
- [ ] 5.3 测试完整游戏循环

---

## 完成记录
- 完成格式示例：
  - [x] 任务项
    - 完成：2026-06-09 by Assistant — 说明完成内容

---

## 文件路径
- 本文件：`dev_plan/current_tasks.md`
- 开发大纲：`dev_plan/development_outline.md`

---

## 当前进度
| 阶段 | 状态 | 完成率 |
|------|------|--------|
| 1 - 基础框架 | ✅ 已完成 | 100% |
| 2 - 兵营与单位 | ✅ 已完成 | 100% |
| 3 - 战斗系统 | ✅ 已完成 | 100% |
| 4 - 城堡与胜负 | ✅ 已完成 | 100% |
| 5 - 场景配置 | 进行中 | 67% |

---

## 下一步
运行游戏验证完整循环（5.3）：产兵 → 战斗 → 攻城 → 胜负 → 重开

### 碰撞层约定
| 层 | 值 | 用途 |
|----|-----|------|
| Layer 2 | 2 | 士兵单位 |
| Layer 3 | 4 | 城堡区域 |
