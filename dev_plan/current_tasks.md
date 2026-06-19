# 当前任务

极简 MVP（M0）已于 2026-06-15 完成；体验增强（M0+：标题、BGM、结算）已完成。

**当前焦点：M1 白天 / 夜晚流程框架。** 阶段规则见 `concepts.md` §昼夜阶段，路线图见 `development_outline.md` §七。

## 进行中

M1 核心实现已完成，待游戏内人工验证。

## 待办（M1）

### 1. 阶段状态机（`GameManager`）

- [x] 新增 `GamePhase` 枚举：`Day`、`Night`（保留 `GameOver`）
- [x] 初始阶段设为 `Day`
- [x] 实现 `AdvancePhase()`：`Day` ↔ `Night` 切换
- [x] 发出 `PhaseChanged(GamePhase)` 信号
- [x] 提供 `IsDay` / `IsNight` 只读属性供其他系统查询
- [x] 按时间自动切换：白天 60s、夜晚 30s（`GameConfig`）

**涉及文件：** `scripts/autoload/GameManager.cs`, `scripts/utils/GameConfig.cs`

### 2. 夜战词条占位（`Building` / `Soldier`）

- [x] `Building` 增加 `HasNightCombat` 属性，默认 `false`
- [x] `Soldier` 增加 `HasNightCombat` 属性，默认 `false`
- [x] M1 不实现夜战单位，仅预留接口供 M4 使用

**涉及文件：** `scripts/nodes/Building.cs`, `scripts/nodes/Soldier.cs`

### 3. 夜晚休眠门控

- [x] `Barracks`：夜晚且 `!HasNightCombat` 时停止工作循环
- [x] `Soldier`：夜晚且 `!HasNightCombat` 时跳过 `_Process` 中的移动与攻击逻辑
- [x] 阶段切换时，已在场上的士兵立即响应

**涉及文件：** `scripts/nodes/Barracks.cs`, `scripts/nodes/Soldier.cs`, `scripts/systems/NightSystem.cs`

### 4. 阶段 UI

- [x] 主游戏顶部显示当前阶段文字：「白天」/「夜晚」
- [x] 显示阶段剩余时间倒计时
- [x] 开发用按钮「切换昼夜」，点击调用 `GameManager.AdvancePhase()`
- [x] `UIManager` 监听 `PhaseChanged` 更新显示

**涉及文件：** `scenes/main/main_game.tscn`, `scripts/autoload/UIManager.cs`

### 5. 数据层与目录骨架

- [x] 创建 `scripts/systems/NightSystem.cs`
- [x] 创建 `scripts/systems/ShopSystem.cs`、`CardSystem.cs` 占位类
- [x] 创建 `scripts/utils/GameConfig.cs`（白天 60s / 夜晚 30s、初始金币）
- [ ] 创建 `resources/buildings/barracks.tres`（产出间隔、`HasNightCombat = false`）

### 6. 建筑工作特效

- [x] `Building` 基类 `PlayWorkEffect()` + 自下而上变亮 Shader
- [x] 完全变亮时触发工作 + 跳动 + 恢复亮度
- [x] `Barracks` 以 `SpawnInterval` 为周期接入工作特效产兵

**涉及文件：** `scripts/nodes/Building.cs`, `scripts/nodes/Barracks.cs`, `assets/shaders/building_work.gdshader`

### 7. 人工验证

- [ ] 标题 → 主游戏，默认显示「白天」，产兵与战斗正常
- [ ] 白天倒计时 60s 后自动切至「夜晚」，兵营停产、士兵停动
- [ ] 夜晚 30s 后自动切回「白天」，产兵与战斗恢复
- [ ] 「切换昼夜」按钮可手动跳转阶段
- [ ] 白天兵营产兵时，可见自下而上变亮 → 跳一下 → 恢复正常的完整特效
- [ ] 胜负结算与返回标题不受影响

## 不在 M1 范围（留给 M2）

- 夜晚打开商店购买卡牌
- 夜晚花钱修复建筑
- 手牌打出与卡牌 UI
- 夜战单位实际行为（`HasNightCombat = true`）

## 验收标准

- 游戏仅在白天 / 夜晚两阶段间切换，按 60s / 30s 自动循环
- 白天一切正常，与 M0 行为一致
- 夜晚无夜战词条的兵营与士兵全部休眠
- 兵营每次产兵时播放工作特效（自下而上变亮 → 跳动 → 恢复）
- 胜负判定与返回标题流程正常
