# 当前任务：Phase 五——重构 Unit：保持聚合内聚，拆掉 Adapter 上帝类

## 已完成（四阶段全部提交）

- Phase 零：存档往返 + 持久化项目 + 视觉资源端口迁移到 domain
- Phase 一：领域层（Shared/Building/Battle/History）
- Phase 二：应用层（GameSessionService/UseCases/Ports）
- Phase 三：UI 与前端场景组件化（9 面板独立场景 + UiInteractionState/UiInputRouter）
- Phase 四：Building 重构（BuildingRuntime 聚合[605 domain 单测]、ProductionComponent/RitualHostComponent/六能力组件/UnitSpawnService/BuildingView/BuildingEffects/BuildingBridge、Building.tscn Shell/Logic/View/Effects）

## Phase 五 分步实施（每步一个提交、行为保持、全量验证通过再下一步）

- [x] 5.1 确认 `Domain.Battle.Unit` 聚合边界；仅将可独立演化状态提为可独立演化组件：`SkillRuntime`（技能冷却状态机）与 `StatusEffects`（减速/中毒/诅咒/战斗计时/伏击/再生），`CombatState` 对应现有 `UnitState` 枚举 + `SetAmbushing` 内部意图；不按行数机械拆服务（+7 单测 @612）
- [x] 5.2 大量公共 setter 收紧为意图方法，初始化数据改不可变定义 + 运行时状态（Health/MaxHealth 改 private set；IsDefender/Patrol*/IsResting → `SetDefender/SetPatrolRadius/SetResting`；初始化经不可变 `UnitStats`）
- [x] 5.3 派系共享资源建模为对局/阵营级资源池，移除 Unit 持有 BlackMagicService/ManaService Singleton 所有权（新建 `FactionResources` 池对象承载 BlackMagic/Mana/Comfort，Unit 经 `Resources` 端口访问；资源不在每帧重建的 GameContext 上）
- [x] 5.4 移除聚合对全局静态阵营 Buff 的直接读取；经对局上下文/效果快照注入（`UndeadDamageMultiplier` 注入 `GameContext`，SkillService 构建快照，Unit 不再直接读 `UndeadFactionBuffs`）
- [ ] 5.5 `UnitLogic` 拆为领域桥接、Navigation/Motion、Targeting Adapter、UnitPresentation、DeathPresentation、ProjectilePresenter 等单一职责组件（已抽 `ProjectilePresenter.cs`、`UnitSfx.cs` 音效映射；Navigation 已有 NavigationPortAdapter；其余待续）
- [ ] 5.6 死亡副作用改为领域事件 + 应用事件处理器；`OnDied` 只转发事件和触发表现（已抽 `DeathSideEffects.cs`：黑魔法 OnUnitDied/地下城经验/血魔召唤经应用侧处理器；OnDied 改薄转发）
- [ ] 5.7 统一单位生成工厂，替换 Building 与 UnitLogic 内重复加载/初始化/增益逻辑（4.6 已建 UnitSpawnService，此处接入死亡召唤）
- [ ] 5.8 投射物路径/纹理/音效映射集中为表现配置，领域攻击事件不引用素材路径
- [ ] 5.9 Buff 展示改只读 `UnitPresentationModel`，领域状态不含本地化文案

## 验收

Domain Unit 状态只能通过意图方法改变；UnitLogic 不含派系结算规则、素材路径和 Buff 文案；战斗和死亡测试、全兵种 smoke 与基线一致。

## 后续阶段大纲

### 六、领域边界与 Infrastructure 隔离

- [ ] 区分领域模型、应用命令/结果、持久化 DTO 和 UI ViewModel，禁止同一个可变类跨四层复用
- [ ] History 建立反腐败映射：适配器先映射为 History 自有 Snapshot，移除对 Building 项目的直接引用
- [ ] `SaveData` 构建/恢复映射集中到 Application mapper（Phase 二已完成大部分），Godot Adapter 只提供运行时快照端口
- [ ] 抽取唯一网格单元归属计算，供 `AdjacentRules` 与 `HordeAdjacencyRules` 共用
- [ ] 审核 `IBuildingRepository/IUnitRepository/ISkillRepository`：静态配置查询用 Repository/Definition Catalog 语义
- [ ] 审核端口方向和命名：UseCase 入站、Repository/Gateway/Presenter 出站；删除无边界价值接口
- [ ] Persistence 拆为可独立测试纯 C# 项目（Phase 零已建 Persistence.csproj，继续迁入剩余存储实现）
- [ ] 收紧 MS DI 纯度：依赖 Godot 生命周期的实现留在场景 Adapter/Registry

### 七、收尾、性能与文档同步

- [ ] 全量构建 0 警告、全测通过、uid/arch/smoke-path/headless smoke 与主场景无头全绿
- [ ] 文档与索引同步（aiIndex/codeStructure/nodeStructure/dependency* 等）
