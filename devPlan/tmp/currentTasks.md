# 当前任务：Phase 二——补齐应用编排层，收拢组合根

## 目标与边界

在不改变现有玩法、数值、存档格式和场景可见行为的前提下，新增纯 C# Application 层承载跨聚合/跨子域编排；
Godot 适配层只翻译节点、输入、帧时钟和信号。不引入 CQRS、通用 EventBus 或每个操作一个空壳 Handler。

## 新增项目与文件

- `scripts/application/Application.csproj`（net8.0，零 Godot；引用 Shared/Building/Battle/History + DI.Abstractions）
- `scripts/application/ApplicationModule.cs`（AddApplication 注册扩展，CompositionRoot 仍是唯一 DI 注册入口）
- `scripts/application/GameSessionService.cs`（保存/恢复用例：迁自 adapters/godot/flow/GameSessionService.cs + GameManager.SaveGame + InitManager.LoadSaveIntoGame）
- `scripts/application/SaveDataMapper.cs`（IBuildingState/CardData ↔ SaveEntry 映射；旧档 catalog 回填迁入，依赖 DI 的 `IReadOnlyList<CardData>`）
- `scripts/application/NightCombineUseCase.cs`（夜间结算 + 复刻应用，迁自 NightOrchestrator）
- `scripts/application/PhaseTransitionUseCase.cs`（阶段副作用单一入口：快照/商店刷新/复刻/自动存档）
- `scripts/application/MatchSessionUseCase.cs`（对局启动副作用：阵营重置 + 战报 StartMatch）
- 删除：`scripts/adapters/godot/flow/NightOrchestrator.cs`、`scripts/adapters/godot/flow/GameSessionService.cs`
- 新增测试项目 `tests/CasualCastle.Application.Tests/`

## 新端口（domain 内，零 Godot）

- `domain/History/IBattleReportCapture.cs`：StartMatch(string) / CapturePhaseSnapshot(IReadOnlyList<IBuildingState>, int gold, IReadOnlyList<CardData>, int nightIndex, BattlePhase) / SaveCurrentReportDaily()；由 BattleReportSystem 实现
- `domain/Building/IGameSessionQuery.cs`：FactionId / Gold / CurrentNightIndex / PendingReplayReportId / PlayerBuildingStates / HandCards / CombineFactory
- `domain/Building/IGameSessionCommand.cs`：ClearNonCoreBuildings / PlaceRestoredBuilding / RestoreCoreBuilding / SetGold / ResetHand / AddRestoredCard / SetNightIndex / SetPendingReplayReportId / RefreshPlayerAdjacency（含合并后 Sfx）
- `domain/Building/IShopAccess.cs`：SetShopAvailable / RefreshOffers / RequestOpenShop / SetFaction
- `domain/Building/IBuildingState.cs`：补 `int Health { get; }`（Building 已有该属性）
- `domain/Shared/IGameSessionService.cs` 更新为：SaveGame(int slot) / LoadSaveData / HasSave / DeleteSave / RestoreSession(SaveData)
- 适配实现：`adapters/godot/flow/GameSessionBridge.cs`（实现 Query/Command/ShopAccess，InitManager 构造并注册 AdapterRegistry；CompositionRoot 用工厂委托注册到 MS DI）

## 分步实施（每步一个提交、一种职责迁移、全量验证通过再下一步）

- [x] 骨架：application 项目 + 注册进 sln/主 csproj（含 Compile Remove）+ check_architecture.py 增加 application 规则（零 Godot/零 SQLite/只引用 domain）+ dependencyRules 更新；构建与全部测试不变
- [x] 端口与桥接：5 个新端口 + IBuildingState.Health + BattleReportSystem 实现 IBattleReportCapture + GameSessionBridge + CompositionRoot 工厂注册；行为不变，6 个冒烟与主场景无头通过
- [x] 保存/恢复用例：GameSessionService 迁入 application + SaveDataMapper（含旧档回填）；删 GameManager.SaveGame/LoadSaveData/HasSave 与 InitManager.LoadSaveIntoGame/CreateRestoredCard/CopyItemFromCatalog/FindCatalogCard；PauseMenu/TitleScreen/InitManager 调用点切换；Application.Tests 覆盖保存/恢复/回填
- [x] 夜间结算用例：NightCombineUseCase（含 coreLevel 计算与复刻）；删 NightOrchestrator.cs 及其构造点；测试覆盖派系 coreLevel 与回调
- [x] 阶段副作用用例：PhaseTransitionUseCase；GameManager.OnPhaseChanged 只做「调用用例 + 发信号 + 音效」；InitManager 取消 PhaseChanged 订阅；测试覆盖快照映射、StartMatch 无快照、night>=1 自动存档、night 0 不存档
- [x] 对局启动用例：MatchSessionUseCase（阵营重置 + 战报 StartMatch）；GameManager.StartGameSession 委托；把正常路径上的 Get<T>()?.X 兜底收紧为 GetRequiredService；测试覆盖两派系重置与无操作派系
- [x] 收尾：文档同步（aiIndex/codeStructure/dependencyInjection/dependencyRules/behaviorInventory/cloudEnvironment）+ 勾选本清单 + 全量验证（build 0 警告、三测试套件、uid 校验、架构检查、6 冒烟、主场景无头 240 帧）

## 风险与行为保持要点

- Godot 信号 `PhaseChanged` 有 5 个单参订阅者（AudioManager/UnitLogic/Building/HudUiController/InitManager），签名保持不变；副作用改由 GameManager.OnPhaseChanged（机器事件第一个订阅者）直接调用用例，快照仍先于信号
- 副作用内部顺序保持：快照 → SetShopAvailable → 商店刷新/开门 → 夜间结算 → 复刻 → 自动存档；StartMatch（prev==next）不发快照
- GameSessionBridge 懒读 gm.PlayerCastle/Shop/Hand（与现行为一致），桥接在 InitManager._Ready 注册，早于任何用例解析

## 验收（沿用原计划）

应用项目零 Godot 引用；InitManager 仅做装配和订阅；GameManager 不再组装存档 DTO、不直接编排阵营领域服务；阶段副作用由单一用例触发；现有存档可读取且 round-trip 等价。