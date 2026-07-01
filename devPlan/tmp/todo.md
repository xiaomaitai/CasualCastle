# 架构待办（TODO）

当前任务详见 `currentTasks.md`。这里是长期架构与技术债 backlog。

---

## 长期架构优化

### InitManager 职责拆分

`InitManager` 当前：手动装配 6+ 个域服务、注册到 AdapterRegistry、混合业务逻辑（`ResolveNightFusions`、`ApplyReplaySnapshot`）。

**方向：**
- 纯装配逻辑迁入 `CompositionRoot` 或专用 `SceneCompositionRoot`
- 业务逻辑（融合/重放）迁入各自域服务
- `InitManager` 仅保留 "场景节点初始化顺序编排" 职责

**时机：** 等 P2 入站端口完成后推进，避免与当前 DI 改造冲突。

---

### DI 系统简化：逐步收敛到 MS DI

当前双层 DI 并存，开发者需判断 "这个依赖走哪个容器"。

**方向：**
- 核心 adapter（`BattleManager`、`BuildingSystem` 等）通过工厂委托注册到 MS DI（参考 `IGameState` 模式）
- `AdapterRegistry` 最终只保留**动态实例**（每帧创建/销毁的节点）查找
- 考虑用 Source Generator 或约定注册减少手动维护

**时机：** 所有入站端口改造完成后推进，届时 MS DI 注册项增多，简化收益更明显。

---

### Hand / Shop 状态归属 Player 聚合

当前 `Hand` 和 `Shop` 各自管理状态（手牌、金币、随机种子），分散在多个类中。

**方向：**
- 新建 `Player` 聚合根，持有 `Gold`、`Hand`、`ShopOffers`
- `Hand` → 只保留 `TryPlaceCard` 等纯逻辑
- `Shop` → 只保留 `GenerateOffers` 等纯逻辑

**时机：** 低优先级，等游戏玩法稳定后再做聚合设计。

---

### Hand.CloneCard 绕过 CardData 构造器

`HandService.cs` 的 `CloneCard` 方法通过属性赋值创建 `CardData`，如果 `CardData` 未来添加不变性约束，此路径会绕过。

**方向：** `CardData` 添加 `Clone()` 方法或工厂方法，统一创建路径。

**时机：** 等 CardData 逻辑变复杂时处理。

---

### UnitSpatialService 物理计算归属

当前空间碰撞/推开逻辑放在 `Domain.Battle`。可讨论是否属于应用层（物理模拟是技术关注点）。

**方向：**
- 保持现状：空间计算是纯数学，不依赖外部框架，放 domain 层合理
- 不强制迁移，除非发现与 Godot 物理耦合的需求

---

### SoldierLogic 进一步拆分

P1 步骤 5/6 完成基本拆分后，长期可将 `SoldierLogic` 进一步拆为：
- 视觉层：`_Draw`、精灵缩放、受击闪烁
- 编排层：域服务调用、状态同步
- 生命周期层：死亡动画、QueueFree

**时机：** P1 完成后评估是否必要。
