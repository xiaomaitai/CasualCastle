# 当前任务：Phase 六——领域边界与 Infrastructure 隔离

## 已完成（前五阶段全部提交）

- Phase 零：存档往返 + 持久化项目 + 视觉资源端口迁移 domain
- Phase 一：领域层（Shared/Building/Battle/History）
- Phase 二：应用层（GameSessionService/UseCases/Ports）
- Phase 三：UI 与前端场景组件化（9 面板独立场景 + UiInteractionState/UiInputRouter）
- Phase 四：Building 重构（BuildingRuntime 聚合+605 单测、生产/仪式/六能力组件、UnitSpawnService、BuildingView/Effects/Bridge、Building.tscn Shell/Logic/View/Effects）
- Phase 五：Unit 重构（SkillRuntime/StatusEffects/FactionResources/GameContext 注入、setter 收紧、UnitLogic 拆 ProjectilePresenter/UnitSfx/DeathSideEffects/UnitPresentationModel；单测 612）

## Phase 六 分步实施（每步一个提交、行为保持、全量验证通过再下一步）

- [ ] 6.1 区分领域模型、应用命令/结果、持久化 DTO 与 UI ViewModel，禁止同一个可变类跨四层复用
- [x] 6.2 History 建立反腐败映射：适配器先映射为 History 自有 Snapshot，移除对 Building 项目的直接引用（History 已零 Building 引用；`IHistoryCoreLookup` 端口 + `BuildingCoreLookup` 桥；战报采集端口改收自身 Snapshot）
- [ ] 6.3 `SaveData` 构建/恢复映射集中到 Application mapper（Phase 二已完成大部分），Godot Adapter 只提供运行时快照端口
- [x] 6.4 抽取唯一网格单元归属计算：`AdjacentRules` 与 `HordeAdjacencyRules` 共用 `BuildingCellOccupancy` 一套实现
- [ ] 6.5 审核 `IBuildingRepository/IUnitRepository/ISkillRepository`：静态配置查询用 Repository/Definition Catalog 语义
- [ ] 6.6 审核端口方向和命名：UseCase 入站、Repository/Gateway/Presenter 出站；删除无边界价值接口
- [ ] 6.7 Persistence 拆为可独立测试纯 C# 项目（Phase 零已建 Persistence.csproj，继续迁入剩余存储实现）
- [ ] 6.8 收紧 MS DI 纯度：依赖 Godot 生命周期的实现留在场景 Adapter/Registry
- [x] 6.9 契约测试：配置映射、存档版本兼容、空存档、旧存档迁移、战报 Snapshot 映射（新增 `ToCardData_RestoresPlaceableCardFields`/`OldSaveWithoutItemType_BackfillsFromCatalog`；空/典型存档恢复与快照映射由 GameSession/SaveDataMapper/ReportBuilder/ReportService 测试覆盖）

## 验收

History 消费自身 Snapshot；Domain 不认识 SQLite/节点/资源路径/Save DTO；相邻网格归属只有一个实现；旧存档兼容。

## 后续阶段大纲

### 七、收尾、性能与文档同步

- [ ] 全量构建 0 警告、全测通过、uid/arch/smoke-path/headless smoke 与主场景无头全绿
- [ ] 文档与索引同步（aiIndex/codeStructure/nodeStructure/dependency* 等）
