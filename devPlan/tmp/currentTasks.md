# 当前任务：全项目架构审查与渐进式重构

## 目标与边界

在不改变现有玩法、数值、存档格式和场景可见行为的前提下，清理领域逻辑、应用编排、Godot 表现和 UI 组件之间的边界。重构按可独立验证、可独立回退的小阶段推进；每一阶段先补行为特征测试，再迁移职责，禁止一次性重写。

本次优先处理已有证据明确的高耦合点：

- `Building.cs` 同时承担领域状态、产兵循环、仪式、陷阱、活体建筑、资源生产、派系效果、节点生命周期、音效和粒子表现。
- `UnitLogic.cs` 同时承担领域单位装配、移动同步、索敌桥接、死亡派系规则、投射物、音效和 Buff 文案表现。
- `UIManager.cs` 同时承担 UI 组合根、逐帧轮询、模态状态、Esc 优先级、输入分发和场景跳转。
- `InitManager.cs` 同时承担运行时装配、阶段用例编排、商店配置、自动保存和存档恢复。
- `GameManager.cs` 同时承担 Autoload 状态、DI 服务定位、阶段机、战报触发、阵营重置、存档 DTO 组装和结算。
- `main_game.tscn` 内直接铺设大量 UI 控件，面板缺少独立场景边界，复用和局部测试成本高。
- Domain 已做到独立项目、零 Godot 依赖、端口反转和较完整单测，这些基础保留；不因文件较大机械拆分类，不引入无业务含义的接口和包装层。

## 零、先修正确性缺陷与低风险契约偏差

以下问题不等待大规模重构，先以最小改动修复并建立回归保护：

- [x] 核对并补齐 `SaveStorage` 对 `SaveData` 全字段的持久化：仪式研习次数、已学仪式、地下城经验/技能等级、手牌 `ItemType/Attributes/IsPlaceable` 必须完整 round-trip
- [x] 为存档新增真实 SQLite 往返测试，覆盖旧存档兼容、仪式状态、运行时变化后的卡牌属性；禁止继续依赖 catalog 回填掩盖字段丢失
- [x] 补全 `IUnitHandle` 的传送能力契约，移除 `TrapService` 对具体 `Unit` 的类型强转，并用测试替身覆盖传送陷阱
- [x] 建立 `MatchPhase` 与战报 `BattlePhase` 的唯一映射函数和三条转换测试，消除 `Combat/Shop/Combine` 与 `Day/Night/Combine` 的分散手写映射
- [x] 将 `IVisualAssetRepository`、`IBuildingVisualRepository` 等端口移到拥有契约的内层项目；统一 Persistence 类 namespace

验收：存档所有字段往返不丢失；传送陷阱不依赖具体领域实现；阶段快照和回放共用同一映射；Domain 仍零 Godot 引用。
## 一、建立重构基线与依赖规则

- [x] 运行并记录 `dotnet build`、全部 Domain 单测、UID 校验和现有关键无头冒烟结果，确保当前分支可作为行为基线（见 `behaviorInventory.md` 基线表）
- [x] 为建筑生产/暂停/受损/摧毁/修复/仪式、单位生成/攻击/死亡、阶段切换、商店、存档恢复、UI 模态输入建立行为清单（见 `behaviorInventory.md`）
- [x] 给缺少自动保护的关键链路先补特征测试；Domain 行为放单元测试，Godot 装配和场景行为放无头 smoke，不把 Godot 类型引入 Domain 测试（Phase 零已补存档往返/传送/映射测试；其余缺口已列入 `behaviorInventory.md` 并挂到 Phase 二/三/四/五）
- [x] 增加架构约束检查：Domain 项目不得引用 Godot、Persistence、Godot Adapter；领域端口不得返回 Godot 类型；场景脚本不得直接访问 SQLite（`tools/check_architecture.py`，已接入 pre-commit）
- [x] 建立迁移期间的依赖图和职责表，明确每个状态的唯一写入者，避免新旧实现同时修改同一状态（`dependencyRules.md` + `behaviorInventory.md` 第三节）

验收：基线全部通过；每条待迁移主链路有可重复验证方式；依赖方向可自动检查；本阶段不改变运行行为。

## 二、补齐应用编排层，收拢组合根

目标结构：`Domain` 只表达业务模型与规则；轻量纯 C# Application 用例负责跨聚合/跨子域编排；Godot Adapter 只翻译节点、输入、帧时钟和信号；Persistence 只实现仓储。新增 `scripts/application/` 独立项目，但只放确有跨边界价值的 Coordinator/Mapper，不引入 CQRS、通用 EventBus 或每个操作一个空壳 Handler；同步解决方案、项目引用与架构文档。

- [ ] 把 `CompositionRoot` 保持为唯一 MS DI 注册入口；Godot 节点仅在场景组合根注入依赖，逐步停止业务代码中散落的 `GameManager.Get<T>()` 和 `AdapterRegistry.Resolve<T>()`
- [ ] 将 `InitManager` 拆为场景组合根与应用用例调用器：对局启动、阶段推进、夜间结算、商店刷新、自动保存分别由明确用例负责
- [ ] 将 `NightOrchestrator` 迁为纯 C# 应用用例，输入只包含领域数据/端口，不接收 `GameManager`、`Castle`、`Sfx` 等 Godot 对象
- [ ] 将 `GameSessionService` 从仓储透传器改为完整的保存/恢复用例；新增快照采集与恢复端口/映射器，移出 `GameManager.SaveGame` 和 `InitManager.LoadSaveIntoGame`
- [ ] 将阵营开局重置、阶段副作用、战报快照触发、结算删档收拢到对局应用服务，`GameManager` 只保留 Autoload 生命周期、阶段时钟投影和 Godot 信号
- [ ] 收紧服务解析为启动失败即暴露的必需依赖；取消正常运行路径中通过 null 兜底掩盖装配错误

验收：应用项目零 Godot 引用；`InitManager` 仅做装配和订阅；`GameManager` 不再组装存档 DTO、不直接编排阵营领域服务；阶段副作用由单一用例触发；现有存档可读取且 round-trip 等价。

## 三、UI 与前端场景组件化

目标结构：每个可独立显示/隐藏/交互的面板都是独立 `.tscn + C#` 组件，自己持有节点引用、信号绑定和清理；根 UI 只负责组合、全局模态栈和跨组件导航。

- [ ] 从 `main_game.tscn` 提取 HUD、手牌、商店、建筑工具、单位信息、法术栏、属性面板、暂停、设置、结算、作弊/运行信息为独立 UI 场景，公共视觉提为 Theme/StyleBox/可复用控件
- [ ] 每个组件用导出节点引用或场景唯一节点绑定自身控件，停止控制器从共享 `uiRoot` 按字符串路径抓取整棵 UI 树
- [ ] 用组件信号/显式接口传递“购买、选卡、关闭、暂停、设置”等用户意图；组件不得直接解析全局服务或调用其他组件内部节点
- [ ] 将 `UIManager.InitializeUI` 改为组件装配，将逐帧轮询改为事件驱动；仅动画、拖拽、世界空间跟随等确需每帧更新的组件保留 `_Process`
- [ ] 引入单一模态栈/输入路由，明确 Esc 和点击输入优先级；移除 `UIManager._Input` 中按组件硬编码的长链判断
- [ ] 将暂停/设置/商店/结算导致的输入屏蔽统一投影为 `UiInteractionState`，各组件订阅状态，避免多处分散调用 `SetInputBlocked`
- [ ] 将 TavernBubble 等世界空间表现移到对应战场表现系统，不由 UI 根动态查找 Battlefield 并创建
- [ ] 保证所有 UI 场景根节点与主 UI 位于 `CanvasLayer`，世界空间气泡等明确放在 Battlefield View 层，禁止混放

验收：新增/移除一个面板只改 UI 组合场景和注册，不修改中央输入长链；各组件可单独实例化 smoke；主场景不再内嵌大段面板节点；模态优先级、拖拽、暂停、设置和分辨率切换行为与基线一致。

## 四、重构 Building：领域状态、能力组件、表现三分离

目标结构：`Building` 退化为 Godot 实体壳和组件宿主；Domain Building 聚合维护生命、暂停、组合限制、工作状态与领域不变量；Godot 能力组件负责把帧时钟/节点端口翻译给领域用例；`BuildingView`/`BuildingEffects` 独占精灵、材质、Tween、状态图标和粒子。

- [ ] 先为建筑生命、操作状态、生产进度、修复成本、摧毁/再生、仪式开始/完成补齐聚合级特征测试
- [ ] 引入纯领域 `BuildingRuntime`（最终命名以领域语言为准），封装 `Health/MaxHealth`、暂停、禁止组合、生产进度、活动仪式等状态，移除由 Adapter 随意写入领域状态的入口
- [ ] 将产兵时钟与生成请求拆为 `ProductionComponent`；领域只产生“应生成何种单位/数量”的结果，场景实例化和出生位置留在 Godot Adapter
- [ ] 将仪式宿主拆为 `RitualHostComponent`；消费金币/黑魔法、移除书、升级核心、召唤奖励通过应用端口执行，`Building` 不直接解析 `Shop/Hand/BlackMagicService`
- [ ] 将陷阱、宝箱、活体生长/再生/繁殖、法力生产、瘟疫、晶矿分别拆为按能力挂载的组件或策略；禁止继续向 `Building._Process` 添加类型分支
- [ ] 将共享生成流程统一到 `UnitSpawnService/Factory`，消除普通产兵、仪式召唤、死亡召唤的重复 `PackedScene` 加载与初始化逻辑
- [ ] 新建 `BuildingView` 与 `BuildingEffects`，迁移贴图、受损色、工作高亮、跳动、摧毁粒子、状态图标、部落邻接提示；逻辑组件不得直接播放 SFX 或操作 Sprite/Tween
- [ ] 将 `IBuildingState/IBuildingRef/IBuildingTarget` 的适配投影集中到一个桥接对象，避免 Godot 节点本身充当多个领域模型
- [ ] 更新 `Building.tscn` 为 Shell/Logic/View/Effects 组合，并逐类迁移；每迁移一种能力即删除旧分支，不长期保留双实现

验收：`Building.cs` 只负责节点生命周期、组件装配和极薄转发；业务能力新增不需要修改中央 `_Process`；表现类不写领域状态；Domain 单测覆盖所有迁移规则；所有产兵建筑、陷阱、仪式和活体建筑 smoke 行为与基线一致。

## 五、重构 Unit：保持聚合内聚，拆掉 Adapter 上帝类

- [ ] 先确认 `Domain.Battle.Unit` 的聚合边界：生命、目标、战斗状态、技能冷却和状态效果属于单位不变量，保留在聚合内部；仅将可独立演化的状态对象提为 `CombatState/StatusEffects/SkillRuntime`，不按行数机械拆服务
- [ ] 将 `Unit` 的大量公共 setter 收紧为意图方法，初始化数据改为不可变定义 + 运行时状态，防止 Adapter 绕过不变量
- [ ] 将派系共享资源建模为对局/阵营级资源池，移除每个 Unit 持有同一 `BlackMagicService/ManaService` Singleton 的错误所有权表达
- [ ] 移除聚合对全局静态阵营 Buff 的直接读取；通过对局上下文/效果快照注入，使多局测试、回放和存档恢复互不污染
- [ ] 将 `UnitLogic` 拆为领域桥接、Navigation/Motion、Targeting Adapter、UnitPresentation、DeathPresentation、ProjectilePresenter 等单一职责组件
- [ ] 将地下城死亡掉落、死灵黑魔法、酒馆解绑、尸体登记等死亡副作用改为领域事件 + 应用事件处理器；`UnitLogic.OnDied` 只转发事件和触发表现
- [ ] 统一单位生成工厂，替换 `Building` 与 `UnitLogic` 内的重复场景加载、属性初始化和阵营增益逻辑
- [ ] 投射物路径/纹理/音效映射集中为表现配置，领域攻击事件只携带攻击结果与表现所需语义，不引用素材路径
- [ ] Buff 展示从字符串拼接改为只读 `UnitPresentationModel`，领域状态不包含本地化文案

验收：Domain Unit 仍是业务内聚的聚合而不是贫血 DTO；其状态只能通过意图方法改变；`UnitLogic` 不含派系结算规则、素材路径和 Buff 文案；移动/碰撞与 View 变换互不影响；战斗和死亡测试、全兵种 smoke 与基线一致。

## 六、领域边界与 Infrastructure 隔离

- [ ] 区分领域模型、应用命令/结果、持久化 DTO 和 UI ViewModel，禁止同一个可变类跨四层复用
- [ ] 为 History 建立反腐败映射：适配器先把建筑、卡牌和经济状态映射为 History 自有 Snapshot，逐步移除 History 对 Building 项目的直接引用
- [ ] 将 `SaveData` 的构建/恢复映射集中到 Application mapper；Godot Adapter 只提供运行时快照端口，Persistence 只读写 DTO
- [ ] 抽取唯一的网格单元归属计算，供 `AdjacentRules` 与 `HordeAdjacencyRules` 共用，删除两套等价 cell-owner map
- [ ] 审核 `IBuildingRepository/IUnitRepository/ISkillRepository`：静态配置查询使用 Repository/Definition Catalog 语义，运行时实体集合使用专门端口，避免同名仓储混淆
- [ ] 审核所有端口方向和命名：UseCase 为入站、Repository/Gateway/Presenter 为出站；删除仅为 DI 而存在、没有边界价值的接口
- [ ] 将 Persistence 拆为可独立测试的纯 C# 项目；Godot 的 `res://`、`user://` 路径只在组合根解析后注入，不让存储实现调用 Godot API
- [ ] 收紧 MS DI 纯度：领域、Application、纯 Persistence 进入 MS DI；依赖 `SceneTreeTimer` 等 Godot 生命周期的实现留在场景 Adapter/Registry
- [ ] 给配置映射、存档版本兼容、空存档、旧存档迁移和战报 Snapshot 映射增加契约测试

验收：History 可消费自身 Snapshot 而非 Building 模型；Domain 不认识 SQLite、Godot 节点、资源路径或 Save DTO；Persistence 可在无 Godot 环境测试；相邻网格归属只有一个实现；旧存档兼容。
## 七、收尾、性能与文档同步

- [ ] 删除迁移后失效的静态单例入口、重复工厂、旧控制器和双轨代码，检查无未订阅信号、悬挂事件和场景路径引用
- [ ] 对高频路径做有证据的优化：缓存稳定依赖和资源、事件替代无意义轮询、批量空间查询；不为未测量问题引入对象池/ECS
- [ ] 运行 profiler 对比单位密集战斗、建筑密集、UI 打开/拖拽场景；记录帧耗、分配和节点数量，性能不得显著回退
- [ ] 更新 `AGENTS.md`、`aiIndex.md`、`codeStructure.md`、`dependencyInjection.md`、`nodeStructure.md` 和相关设计文档，内容以最终代码为准
- [ ] 运行完整 build、Domain tests、UID 校验、Godot 无头导入及关键 smoke；人工验证新局、读档、完整昼夜循环、四派系核心链路、结算返回标题
- [ ] 按阶段拆分提交；任何阶段失败时可回退该阶段，不依赖未提交的临时兼容层

验收：构建零 warning、全部测试通过、无头导入无脚本/资源错误、关键 smoke 通过；职责表与依赖图和代码一致；无永久兼容层；性能不低于基线。

## 执行顺序与停止条件

- [ ] 顺序固定为：正确性缺陷 → 基线 → 应用编排 → UI 场景化/输入路由 → Building → Unit → 领域边界与 Infrastructure → 收尾；UI 阶段只做行为保持的组件提取，数据源重构留到对应后端阶段
- [ ] 每阶段开始前再次细化到文件级迁移清单，并限制单次提交只包含一种职责迁移
- [ ] 若发现行为无测试、存档不兼容、领域规则归属存在两种同等合理解释，先停止迁移并补测试/确认模型，不以猜测继续
- [ ] 若拆分只减少文件行数，却增加双向依赖、全局解析或无业务语义接口，则撤销该拆分
