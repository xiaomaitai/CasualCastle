# 当前任务：Phase 七——收尾、性能与文档同步

## 已完成（前六阶段全部提交）

- Phase 零：存档往返 + 持久化项目 + 视觉资源端口迁移 domain
- Phase 一：领域层（Shared/Building/Battle/History）
- Phase 二：应用层（GameSessionService/UseCases/Ports）
- Phase 三：UI 与前端场景组件化（9 面板独立场景 + UiInteractionState/UiInputRouter）
- Phase 四：Building 重构（BuildingRuntime 聚合+605 单测、生产/仪式/六能力组件、UnitSpawnService、BuildingView/Effects/Bridge、Building.tscn Shell/Logic/View/Effects）
- Phase 五：Unit 重构（SkillRuntime/StatusEffects/FactionResources/GameContext 注入、setter 收紧、UnitLogic 拆 ProjectilePresenter/UnitSfx/DeathSideEffects/UnitPresentationModel；单测 612）
- Phase 六：领域边界与 Infrastructure 隔离（反腐败映射/存档映射集中/唯一网格归属/端口方向审核/Persistence 纯 C# 项目/MS DI 纯度/契约测试；单测 612+5+24；24 个非规范 uid 侧车规范化）

## Phase 七 分步实施（每步一个提交、行为保持、全量验证通过再下一步）

- [ ] 7.1 全量构建 0 警告、全测通过、uid/arch/smoke-path/headless smoke 与主场景无头全绿（构建/单测/uid/arch/smoke-path 已验证；headless smoke 与主场景无头待 Godot 环境）
- [ ] 7.2 文档与索引同步（aiIndex/codeStructure/nodeStructure/dependency* 等）

## 验收

Phase 六 验收通过：History 消费自身 Snapshot；Domain 不认识 SQLite/节点/资源路径/Save DTO；相邻网格归属只有一个实现；旧存档兼容。

## 后续阶段大纲

（Phase 七 完成后，后续功能方向见 devPlan/outline/c05Milestones.md「后续方向」）
