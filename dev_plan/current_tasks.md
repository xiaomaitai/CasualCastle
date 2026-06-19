# 当前任务

极简 MVP（M0）已于 2026-06-15 完成；体验增强（M0+：标题、BGM、结算）已完成。

**当前焦点：M1 白天 / 夜晚流程框架。** 阶段规则见 `concepts.md` §昼夜阶段，路线图见 `development_outline.md` §七。

## 进行中

暂无。

## 待办（M1）

### 1. 阶段状态机（`GameManager`）

- [ ] 新增 `GamePhase` 枚举：`Day`、`Night`（保留 `GameOver`）
- [ ] 初始阶段设为 `Day`
- [ ] 实现 `AdvancePhase()`：`Day` ↔ `Night` 切换
- [ ] 发出 `PhaseChanged(GamePhase)` 信号
- [ ] 提供 `IsDay` / `IsNight` 只读属性供其他系统查询

**涉及文件：** `scripts/autoload/GameManager.cs`

### 2. 夜战词条占位（`Building` / `Soldier`）

- [ ] `Building` 增加 `HasNightCombat` 属性，默认 `false`
- [ ] `Soldier` 增加 `HasNightCombat` 属性，默认 `false`
- [ ] M1 不实现夜战单位，仅预留接口供 M4 使用

**涉及文件：** `scripts/nodes/Building.cs`, `scripts/nodes/Soldier.cs`

### 3. 夜晚休眠门控

- [ ] `Barracks`：监听 `PhaseChanged`；夜晚且 `!HasNightCombat` 时 `SpawnTimer.Stop()`，白天或夜战时 `Start()`
- [ ] `Soldier`：夜晚且 `!HasNightCombat` 时跳过 `_PhysicsProcess` 中的移动与攻击逻辑
- [ ] 阶段切换时，已在场上的士兵立即响应（无需等下一帧产兵）

**涉及文件：** `scripts/nodes/Barracks.cs`, `scripts/nodes/Soldier.cs`

### 4. 阶段 UI

- [ ] 主游戏顶部显示当前阶段文字：「白天」/「夜晚」
- [ ] 开发用按钮「切换昼夜」，点击调用 `GameManager.AdvancePhase()`
- [ ] `UIManager` 监听 `PhaseChanged` 更新显示

**涉及文件：** `scenes/main/main_game.tscn`, `scripts/autoload/UIManager.cs`

### 5. 数据层与目录骨架

- [ ] 创建 `scripts/systems/NightSystem.cs`（阶段切换时批量休眠/唤醒，可先薄封装）
- [ ] 创建 `scripts/systems/ShopSystem.cs`、`CardSystem.cs` 占位类
- [ ] 创建 `scripts/utils/GameConfig.cs`（白天/夜晚时长、初始金币）
- [ ] 创建 `resources/buildings/barracks.tres`（产出间隔、`HasNightCombat = false`）

### 6. 建筑工作特效

建筑完成一次工作时播放统一的视觉反馈，适用于产兵、攻击、生产资源等所有「工作」行为。

**表现（按顺序）：**

1. **充能变亮**：建筑从下往上逐渐变亮，亮度随进度上升
2. **工作完成**：完全变亮时，代表建筑完成一次工作
3. **完成跳动**：略微往上跳一下，再回到原位
4. **恢复常态**：整体亮度恢复正常，等待下一次工作

**实现要点：**

- [ ] 在 `Building` 基类提供 `PlayWorkEffect()`（或 `WorkPerformed` 信号 + 统一播放逻辑）
- [ ] 变亮：自下而上的亮度遮罩/渐变（Shader、`CanvasItemMaterial` 或分块 Sprite 叠色均可）
- [ ] 变亮时长与建筑工作间隔对齐（兵营 = `SpawnInterval`）；完全变亮时刻 = 工作触发时刻
- [ ] 跳动：完全变亮时用 `Tween` 做短暂 Y 轴位移（跳起 → 落回）
- [ ] 恢复：跳动结束后重置亮度/材质参数
- [ ] `Barracks` 产兵时调用工作特效（M1 唯一接入点；攻击/产资源建筑后续复用）

**涉及文件：** `scripts/nodes/Building.cs`, `scripts/nodes/Barracks.cs`, 建筑 Sprite 节点（或子节点材质）

### 7. 人工验证

- [ ] 标题 → 主游戏，默认显示「白天」，产兵与战斗正常
- [ ] 切换至「夜晚」，双方兵营停产、士兵停动
- [ ] 切回「白天」，产兵与战斗恢复
- [ ] 白天兵营产兵时，可见自下而上变亮 → 跳一下 → 恢复正常的完整特效
- [ ] 胜负结算与返回标题不受影响

## 不在 M1 范围（留给 M2）

- 夜晚打开商店购买卡牌
- 夜晚花钱修复建筑
- 手牌打出与卡牌 UI
- 夜战单位实际行为（`HasNightCombat = true`）

## 验收标准

- 游戏仅在白天 / 夜晚两阶段间切换
- 白天一切正常，与 M0 行为一致
- 夜晚无夜战词条的兵营与士兵全部休眠
- 兵营每次产兵时播放工作特效（自下而上变亮 → 跳动 → 恢复）
- 胜负判定与返回标题流程正常
