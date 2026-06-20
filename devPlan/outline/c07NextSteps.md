# 七、下一步开发内容（立即行动）

**当前焦点：M1 白天 / 夜晚流程框架。** 阶段规则见 `concepts.md` §昼夜阶段。

建议按以下顺序推进，每完成一项即可人工验证：

1. **扩展 `GameManager` 阶段枚举**
   - 新增 `GamePhase`：`Day`、`Night`（保留 `GameOver`）
   - 提供 `AdvancePhase()`（Day ↔ Night 切换）与 `PhaseChanged` 信号
   - 初始阶段为 `Day`

2. **夜晚休眠门控**
   - `Barracks`：夜晚且 `HasNightCombat == false` 时停止 `SpawnTimer`
   - `Soldier`：夜晚且 `HasNightCombat == false` 时停止移动与攻击（原地休眠）
   - `Building` / `Soldier` 增加 `HasNightCombat` 属性，M1 默认均为 `false`

3. **阶段 UI 最小实现**
   - 主游戏顶部显示「白天」/「夜晚」
   - 开发用按钮「切换昼夜」手动推进（后续再换自动计时）

4. **建立数据层目录**
   - `resources/buildings/barracks.tres`（含产兵间隔、`HasNightCombat` 等）
   - `scripts/utils/GameConfig.cs`（阶段时长、初始金币等常量）

5. **创建 `scripts/systems/` 空壳**
   - `NightSystem.cs`：集中处理阶段切换时的休眠/唤醒
   - `ShopSystem.cs`、`CardSystem.cs` 仅占位类，M2 再填充

**完成 M1 的验收标准：**

- 从标题进入游戏后，可看到当前阶段（白天/夜晚）
- 点击「切换昼夜」能在 Day ↔ Night 间切换
- 白天：双方兵营产兵、士兵战斗，与 M0 一致
- 夜晚：双方兵营停止产兵、士兵停止行动（原地休眠）
- 胜负判定与返回标题流程不受影响

**M2 在此基础上追加（本阶段不做）：**

- 夜晚打开商店购买卡牌
- 夜晚花钱修复建筑

---
