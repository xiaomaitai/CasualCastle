# 当前任务：Phase 三——UI 与前端场景组件化

## 目标与边界

每个可独立显示/隐藏/交互的面板都是独立 `.tscn + C#` 组件，自己持有节点引用、信号绑定和清理；根 UI 只负责组合、全局模态栈和跨组件导航。本阶段只做行为保持的组件提取与输入路由，数据源重构留给对应后端阶段。

## 现状盘点（已核实）

- `scenes/main/main_game.tscn` 772 行：UI CanvasLayer 下内嵌 HUD/手牌/商店/建筑信息/暂停/结算/作弊/法术栏/行动环/出售确认等大量节点；`SettingsPanel` 与属性面板已独立成场景（ExtResource 13/24）
- `UIManager.cs` 356 行：组合 20 个控制器、逐帧轮询、Esc 硬编码长链、模态输入屏蔽分散调用、TavernBubble 动态挂到 Battlefield（层级错误）
- 面板控制器已是纯 C# 类（HudUiController/ShopUiController/HandUiController/BuildingInfoUiController/PauseMenuUiController/GameOverUiController/CheatMenuUiController/SpellBarUiController/BuildingActionRingController 等），各自从共享 uiRoot 按字符串路径抓取节点

## 分步实施（每步一个提交、行为保持、全量验证通过再下一步）

- [ ] 基础：`UiInteractionState`（纯 C# 状态投影：GameOver/任意模态/暂停/设置/商店）与 `UiModalStack`（按优先级压栈/弹栈/Esc 关闭栈顶）；UIManager 只保留注册式输入分发，删除 `_Input` 硬编码长链；SetInputBlocked 全部改经状态订阅；TavernBubble 挂载点移入 UI 层专用节点；冒烟与主场景无头通过
- [x] HUD 面板：`scenes/ui/hud.tscn`（血条/金币/黑魔法/阶段面板/菜单按钮/商店入口/工具按钮）+ HudUiController 改造为场景根脚本（导出节点引用）；UIManager 删除对应 InitializeUI 分支；冒烟通过
- [x] 手牌面板：`scenes/ui/hand_panel.tscn`（HandBackdrop + 10 槽位）+ HandUiController 场景化；拖拽行为保持；冒烟通过
- [x] 商店面板：`scenes/ui/shop_panel.tscn`（5 货架 + 刷新 + 关闭）+ ShopUiController 场景化；购买/拖放直放行为保持；冒烟通过
- [x] 建筑信息/单位信息面板：`scenes/ui/building_info_panel.tscn` + BuildingInfoUiController/UnitInfoUiController 场景化
- [x] 暂停/设置/结算面板：`scenes/ui/pause_menu.tscn`、`scenes/ui/game_over.tscn`（settings_panel 已独立）；PauseMenuUiController/GameOverUiController/SettingsUiController 场景化并接入模态栈
- [x] 作弊/运行信息面板（dev 区）：`scenes/ui/dev_tools/cheat_menu.tscn`、`runtime_info_panel.tscn` + CheatMenuUiController/RuntimeInfoPanelController/BgmInfoController 场景化
- [x] 法术栏/行动环/出售确认：`scenes/ui/spell_bar.tscn`、`scenes/ui/building_action_ring.tscn`（含出售确认）+ SpellBarUiController/BuildingActionRingController 场景化
- [x] 事件化收尾：UIManager `_Process` 中央轮询拆除（仅动画/拖拽/世界跟随保留自身 `_Process`）；购买/选卡/关闭/暂停/设置等用户意图改走组件信号或显式接口；主场景只剩面板实例引用
- [x] 冒烟与验收：每个新面板场景可单独实例化 smoke；新增 `ui_smoke.tscn` 或复用现有冒烟验证模态优先级/拖拽/暂停/设置/分辨率切换；行为清单与文档同步；勾选本清单

## 风险与行为保持要点

- Esc 优先级顺序保持：设置 → 作弊 → 运行信息 → 禁止组合 → 建筑管理 → 行动环 → 暂停关闭 → 手牌取消 → 商店取消拖拽 → 商店关闭 → 打开暂停
- 暂停打开时：商店关闭、手牌取消、拖拽取消、管理/禁止组合输入屏蔽；设置打开时管理工具屏蔽
- 所有面板根节点必须留在 UI CanvasLayer；TavernBubble 等世界空间表现放 Battlefield View 层专用节点，禁止混放
- 分辨率切换/百分比坐标行为与基线一致（uiDesign.md 约定）

## 验收（沿用原计划）

新增/移除一个面板只改 UI 组合场景和注册，不修改中央输入长链；各组件可单独实例化 smoke；主场景不再内嵌大段面板节点；模态优先级、拖拽、暂停、设置和分辨率切换行为与基线一致。

## 后续阶段大纲（四~七）

### 四、重构 Building：领域状态、能力组件、表现三分离（文件级清单）

现状（已核实）：Building.cs 1214 行；_Process 依次调度 TickTrap/TickLivingGrowth/TickBreeding/TickManaProduction/TickPlagueSpread/TickCrystalMine/TickTreasureBox/TickRitual + 生产循环；表现方法 TryApplyVisual/UpdateWorkEffectFromProgress/UpdateDamageVisual/UpdateHordeIndicator/UpdateStateIcon；BuildingSystem.cs 241 行负责配置查询与创建。

- [ ] 4.1 领域聚合 `domain/Building/BuildingRuntime.cs`（纯 C#）：封装 Health/MaxHealth、手动暂停、禁止组合、生产循环（激活/暂停/进度/速率/Tick 产出循环数）、活动仪式（复用 ActiveRitual）、摧毁判定与事件；配套 `BuildingRuntimeTests.cs` 覆盖：伤害/修复、暂停/恢复、禁止组合、摧毁、生产进度与循环产出、仪式开始/计时完成、快照恢复（验收：领域规则先有单测保护）
- [ ] 4.2 状态迁移：Building.cs 的 Health/MaxHealth/暂停/禁止组合/生产字段/_activeRitual 全部改为代理 BuildingRuntime；删重复字段与散落写入点；公共属性与信号语义逐项对齐；全部 smoke 与主场景无头通过（每迁移一项删除旧写入，不留双轨）
- [x] 4.3 生产组件 `adapters/godot/building/ProductionComponent.cs`：迁 StartWorkCycle/PerformWork/UpdateWorkCycle/SetWorkSpeedMultiplier 与 _Process 生产段；组件持有 runtime 引用并发出 `CyclesCompleted(cycles)` 事件，Building 只订阅并实例化（`SpawnRequested(unitTypeId, count)` 待 4.6 统一生成时升级）
- [x] 4.4 仪式宿主 `RitualHostComponent.cs`：迁 TickRitual/ApplyRitualReward/ApplyUndeadHealthBonus；金币/黑魔法消耗、移除书、核心升级经领域服务端口（RitualService/Shop/Hand），组件不含素材路径与表现
- [ ] 4.5 能力组件六件套（adapter）：`TrapComponent`（TickTrap）、`TreasureBoxComponent`（TickTreasureBox）、`LivingComponent`（TickLivingGrowth/TickLivingRegen/TickBreeding/TryBreedFleshNest）、`ManaComponent`（TickManaProduction）、`PlagueComponent`（TickPlagueSpread）、`CrystalMineComponent`（TickCrystalMine）；InitFromType 按能力挂载进组件列表，_Process 遍历列表，删除全部类型分支
- [ ] 4.6 统一生成 `adapters/godot/battle/UnitSpawnService.cs`：普通产兵、仪式召唤（Phase 五再接入死亡召唤）共用 PackedScene 加载与初始化；Building.SpawnUnits/SpawnUnitType 迁移；ProductionComponent 事件升级为 `SpawnRequested(unitTypeId, count)`
- [ ] 4.7 表现分离 `BuildingView.cs`（贴图/材质/高亮/状态图标/部落邻接指示）与 `BuildingEffects.cs`（工作跳动/受损闪色/摧毁粒子）；Building 只转发事件，表现类不写领域状态
- [ ] 4.8 桥接与场景：`IBuildingState/IBuildingRef/IBuildingTarget` 投影集中到一个桥接对象（Building 不再同时实现三接口的散落投影）；`Building.tscn` 改 Shell/Logic/View/Effects 组合；文档同步与全量验收
- 验收（沿用）：Building.cs 只负责节点生命周期、组件装配和极薄转发；业务能力新增不需要修改中央 _Process；表现类不写领域状态；Domain 单测覆盖所有迁移规则；所有产兵建筑、陷阱、仪式和活体建筑 smoke 与基线一致


### 五、重构 Unit：保持聚合内聚，拆掉 Adapter 上帝类

- [ ] 确认 `Domain.Battle.Unit` 聚合边界；仅将可独立演化状态提为 `CombatState/StatusEffects/SkillRuntime`，不按行数机械拆服务
- [ ] 大量公共 setter 收紧为意图方法，初始化数据改不可变定义 + 运行时状态
- [ ] 派系共享资源建模为对局/阵营级资源池，移除 Unit 持有 BlackMagicService/ManaService Singleton 所有权
- [ ] 移除聚合对全局静态阵营 Buff 的直接读取；经对局上下文/效果快照注入
- [ ] `UnitLogic` 拆为领域桥接、Navigation/Motion、Targeting Adapter、UnitPresentation、DeathPresentation、ProjectilePresenter 等单一职责组件
- [ ] 死亡副作用改为领域事件 + 应用事件处理器；`OnDied` 只转发事件和触发表现
- [ ] 统一单位生成工厂，替换 Building 与 UnitLogic 内重复加载/初始化/增益逻辑
- [ ] 投射物路径/纹理/音效映射集中为表现配置，领域攻击事件不引用素材路径
- [ ] Buff 展示改只读 `UnitPresentationModel`，领域状态不含本地化文案
- 验收：Domain Unit 状态只能通过意图方法改变；UnitLogic 不含派系结算规则、素材路径和 Buff 文案；战斗和死亡测试、全兵种 smoke 与基线一致

### 六、领域边界与 Infrastructure 隔离

- [ ] 区分领域模型、应用命令/结果、持久化 DTO 和 UI ViewModel，禁止同一个可变类跨四层复用
- [ ] History 建立反腐败映射：适配器先映射为 History 自有 Snapshot，移除对 Building 项目的直接引用
- [ ] `SaveData` 构建/恢复映射集中到 Application mapper（Phase 二已完成大部分），Godot Adapter 只提供运行时快照端口
- [ ] 抽取唯一网格单元归属计算，供 `AdjacentRules` 与 `HordeAdjacencyRules` 共用
- [ ] 审核 `IBuildingRepository/IUnitRepository/ISkillRepository`：静态配置查询用 Repository/Definition Catalog 语义
- [ ] 审核端口方向和命名：UseCase 入站、Repository/Gateway/Presenter 出站；删除无边界价值接口
- [ ] Persistence 拆为可独立测试纯 C# 项目（Phase 零已建 Persistence.csproj，继续迁入剩余存储实现）
- [ ] 收紧 MS DI 纯度：依赖 Godot 生命周期的实现留在场景 Adapter/Registry
- [ ] 给配置映射、存档版本兼容、空存档、旧存档迁移和战报 Snapshot 映射增加契约测试
- 验收：History 消费自身 Snapshot；Domain 不认识 SQLite/节点/资源路径/Save DTO；相邻网格归属只有一个实现；旧存档兼容

### 七、收尾、性能与文档同步

- [ ] 删除迁移后失效的静态单例入口、重复工厂、旧控制器和双轨代码，检查无未订阅信号、悬挂事件和场景路径引用
- [ ] 高频路径做有证据的优化；不为未测量问题引入对象池/ECS
- [ ] profiler 对比单位密集战斗、建筑密集、UI 打开/拖拽场景，性能不得显著回退
- [ ] 更新 AGENTS.md、aiIndex.md、codeStructure.md、dependencyInjection.md、nodeStructure.md 和相关设计文档
- [ ] 完整 build、Domain tests、UID 校验、无头导入及关键 smoke；人工验证新局、读档、完整昼夜循环、四派系核心链路、结算返回标题
- [ ] 按阶段拆分提交；任何阶段失败时可回退该阶段
- 验收：构建零 warning、全部测试通过、无头导入无脚本/资源错误、关键 smoke 通过；职责表与依赖图和代码一致；性能不低于基线

## 执行顺序与停止条件

- [ ] 顺序固定为：正确性缺陷（已完成）→ 基线（已完成）→ 应用编排（已完成）→ UI 场景化/输入路由（进行中）→ Building → Unit → 领域边界与 Infrastructure → 收尾
- [ ] 每阶段开始前再次细化到文件级迁移清单，并限制单次提交只包含一种职责迁移
- [ ] 若发现行为无测试、存档不兼容、领域规则归属存在两种同等合理解释，先停止迁移并补测试/确认模型，不以猜测继续
- [ ] 若拆分只减少文件行数，却增加双向依赖、全局解析或无业务语义接口，则撤销该拆分