# CasualCastle 项目索引（AI 专用导航）

本文件是项目唯一导航入口，供 AI agent 快速定位文档、代码、场景、素材、测试与工具，避免全盘扫描文件系统。规则与约定见根目录 `AGENTS.md`。

使用方式：先按需求找到对应小节，只打开 1~2 个关键文件；不确定术语时先读 `devPlan/concepts/general.md`。

## 0. 速查（按需求）

| 我想… | 去这里 |
|---|---|
| 了解术语/游戏规则 | `devPlan/concepts/general.md`（派系概念见同目录各派系文档） |
| 查看当前任务/验收项 | `devPlan/tmp/currentTasks.md` |
| 登记/查看需人工实测项 | `devPlan/manualTestCases.md` |
| 查看架构 backlog | `devPlan/tmp/todo.md` |
| 查看行为清单与重构基线 | `devPlan/tmp/behaviorInventory.md` |
| 查看里程碑进度 | `devPlan/outline/c05Milestones.md` |
| 找某个功能模块的代码 | 本文件 §2.4 功能模块对照表 |
| 找场景/预制体 | 本文件 §3 |
| 找素材（卡图/建筑/音频） | 本文件 §4 |
| 找单元测试 | 本文件 §5 |
| 找开发工具脚本 | 本文件 §6 |
| 查实际游戏数值 | `assets/data/config.db` |

## 1. 文档导航（devPlan/）

### 概念文档（devPlan/concepts/）

| 文档 | 内容 | 何时读 |
|---|---|---|
| `devPlan/concepts/general.md` | 通用概念总纲：术语、阶段循环、地块/城堡/建筑/单位、隐身、组合与升级、战报、建筑卡与物品卡、战场、伤害类型与护甲、伤害元素、碰撞层、坐标系统、2.5D 渲染、派系总览 | 任何任务开始前 |
| `devPlan/concepts/alliance.md` | 联盟派系概念：王国军/冒险者公会/神殿卫士三流派、组合链、酒馆组队与鱼龙混杂、恩典 | 涉及联盟派系玩法时 |
| `devPlan/concepts/horde.md` | 部落派系概念：自由建造、空地邻接、五族与荒野盟约、升级规则 | 涉及部落派系玩法时 |
| `devPlan/concepts/necromancer.md` | 死灵法师派系概念：黑魔法、亡灵海领主光环、黑暗魔法、祭仪与仪式 | 涉及死灵法师玩法时 |
| `devPlan/concepts/dungeon.md` | 地下城派系概念：魔王宝座、舒适度、经验等级、四邪神流派 | 涉及地下城派系玩法时 |

### 总纲与运行时结构

| 文档 | 内容 | 何时读 |
|---|---|---|
| `devPlan/codeStructure.md` | 当前运行时代码结构：三层目录、依赖方向、双层 DI、系统清单、端口接口表、主要运行链路 | 定位代码或理解系统关系 |
| `devPlan/dataStructures.md` | 核心 C# 数据结构：卡牌、建筑、组合配方、战报快照、单位、科技树 | 涉及数据结构时；实际数值以 config.db 为准 |
| `devPlan/cloudEnvironment.md` | 运行环境与命令：本机（Windows/DSH）与云环境两套构建、测试、导入、启动游戏命令 | 需要跑构建、测试或游戏时 |
| `devPlan/manualTestCases.md` | 实测用例清单：需人工实机验证的项目（`MT-###`）、状态与维护规则 | 任务完成后登记实测项、用户验证时 |

### 里程碑与大纲（devPlan/outline/）

| 文档 | 内容 |
|---|---|
| `c01Overview.md` | 完整版玩法循环概述 |
| `c05Milestones.md` | 里程碑状态表（历史与验收记录，唯一进度来源） |
| `c06TechStack.md` | 技术栈与数据存储约定 |
| `c10BuildingArtPlan.md` | 建筑美术阶段计划（未完成，backlog 依据） |
| `*.png` | 界面线框：title、mainBattle、placement、nightShop、pause、gameOver、sceneFlow、battlefieldUi |

### 系统设计（devPlan/design/）

| 文档 | 内容 |
|---|---|
| `battleSystem.md` | 战斗系统核心规则 |
| `combineSystem.md` | 组合 + 升级（科技树入边判定） |
| `battleReport.md` | 战报录制与存储（M6） |
| `replayAi.md` | 敌方镜像复刻 AI（M6） |
| `uiDesign.md` | 界面布局与百分比坐标约定 |
| `unitCardDesign.md` | 单位卡牌 UI 设计 |
| `attributePanel.md` | 左下角属性面板展示规则与备选属性 |
| `dependencyInjection.md` | 双层 DI 设计 |
| `dependencyRules.md` | 分层依赖方向规则与自动检查说明 |
| `nodeStructure.md` | 实体节点 Logic/View/Effects 三层结构 |
| `navigationSystem.md` | 流场寻路 + 局部转向（SC2 式） |
| `assetSpecs.md` | 素材尺寸规格（以游戏单位定义） |
| `animationWorkflow.md` | 帧动画工作流（像素图集切片、Swordsman 试点、AnimatedSprite2D 接入；Spine 已弃用） |
| `audioRequirements.md` | 音频需求清单（v0.6 采购/实现唯一依据） |
| `audioSmokeTest.md` | 音频冒烟测试说明 |
| `smokeTestGuidelines.md` | 冒烟测试编写与运行规范（防卡死四层机制、PASS/FAIL 契约、统一运行器） |
| `devTools.md` | 开发者工具说明 |
| `godotMcpIntegration.md` | Godot MCP 接入方案（DSH 桥接）与冒烟简化评估 |
| `factions/allianceFaction.md`、`dungeonFaction.md`、`hordeFaction.md`、`necromancerFaction.md` | 四大派系设计 |

### 施工进度（devPlan/tmp/）

| 文档 | 内容 |
|---|---|
| `currentTasks.md` | 当前任务的细化步骤与验收项（全部完成则清空） |
| `todo.md` | 长期架构 backlog |

## 2. 代码导航（scripts/）

结构：`domain/`（纯 C# 业务规则，零 Godot）→ `adapters/godot/`（Godot 节点）与 `adapters/persistence/`（存储）。`scripts/domain/` 下每个子目录是独立 C# 项目；`scripts/CompositionRoot.cs` 构建 MS DI 容器。

### 2.1 domain — 核心域（改动必须带单测）

| 目录 | 职责 | 关键文件 |
|---|---|---|
| `domain/Shared/` | 坐标、常量、阶段状态机、存档模型、BGM 目录与选择规则 | `GameVector2.cs`、`GameCoordinateRules.cs`、`GameRules.cs`、`GameParameters.cs`、`BuildingTypeIds.cs`、`FactionIds.cs`、`MatchPhaseMachine.cs`、`SaveModels.cs`、`IVisualAssetRepository.cs`、`BgmDef.cs`、`BgmSelectionRules.cs` |
| `domain/Building/` | 建筑、卡牌、手牌、商店、组合、升级、仪式、邻接、科技树 | `BuildingData.cs`、`OccupancyGrid.cs`、`BuildingCellOccupancy.cs`、`CardRules.cs`、`Hand.cs`、`ShopRules.cs`、`CombineService.cs`、`UpgradeService.cs`、`RitualService.cs`、`AdjacencyService.cs`、`IBuildingVisualRepository.cs`、`BuildingVisualData.cs`、`TechTreeNode.cs` |
| `domain/Battle/` | 战斗、单位、技能、光环、隐身、尸体、流场寻路、黑魔法 | `CombatRules.cs`、`Unit.cs`、`UnitState.cs`、`UnitStats.cs`、`SkillService.cs`、`AuraService.cs`、`StealthService.cs`、`CorpseService.cs`、`FlowFieldService.cs`、`NavigationService.cs`、`BlackMagicService.cs` |
| `domain/History/` | 战报、回放、镜像复刻 | `BattleReportService.cs`、`ReportBuilder.cs`、`ReplayService.cs`、`MirrorRules.cs`、`IHistoryCoreLookup.cs` |

端口接口（`I*` 文件）按子域散落在上述目录中，如 `IFieldUnitRepository.cs`、`IBuildingPlacement.cs`、`IBattleReportRepository.cs`。

### 2.2a application — 纯 C# 应用用例层（零 Godot）

| 目录 | 职责 | 关键文件 |
| --- | --- | --- |
| `application/` | 跨聚合/跨子域编排用例与存档映射 | `GameSessionService.cs`（保存/恢复）、`NightCombineUseCase.cs`、`PhaseTransitionUseCase.cs`、`MatchSessionUseCase.cs`、`SaveDataMapper.cs`、`GameSessionPorts.cs`、`ApplicationModule.cs` |

### 2.2 adapters/godot — Godot 适配层

| 目录 | 职责 | 关键文件 |
|---|---|---|
| `autoload/` | 常驻节点：DI 根、阶段、初始化、显示设置 | `GameManager.cs`、`InitManager.cs`、`DisplaySettingsManager.cs`、`AdapterRegistry.cs` |
| `building/` | 城堡/建筑/放置/状态表现 | `Castle.cs`、`Building.cs`（薄壳：装配组件）、`BuildingBridge.cs`（IBuildingState/IBuildingRef/IBuildingTarget 投影）、`BuildingView.cs`/`BuildingEffects.cs`（表现）、`BuildingHealthBar.cs`（建筑受损耐久条）、`BuildingStateIcon.cs`、`ProductionComponent.cs`/`RitualHostComponent.cs`/`IBuildingComponent.cs` 与六能力组件（Trap/TreasureBox/Living/Mana/Plague/CrystalMine）、`TrapSourceFactory.cs`、`BuildingSystem.cs`、`CastlePlacementAdapter.cs` |
| `battle/` | 战场单位逻辑与表现、寻路网格源、弹道、统一生成 | `Unit.cs`、`UnitLogic.cs`、`UnitLifecycle.cs`、`UnitVisual.cs`、`UnitSpawn.cs`、`UnitSpawnService.cs`（产兵/仪式召唤共用）、`DeathSideEffects.cs`（死亡结算与掉落飘字）、`BattleManager.cs`、`NavGridSource.cs`、`FieldUnitRepository.cs`、`Projectile.cs` |
| `battlefield/` | 战场地块 | `BattlefieldTileMap.cs` |
| `battle_report/` | 战报快照/缓存/持久化 | `BattleReportSystem.cs`、`BuildingCoreLookup.cs` |
| `combine/` | 组合结果实例化 | `CombineBuildingFactory.cs` |
| `upgrade/` | 升级结果实例化 | `UpgradeBuildingFactory.cs` |
| `replay/` | 敌方复刻目标 | `ReplayTarget.cs` |
| `ui/` | UIManager 与各面板控制器 | `UIManager.cs`、`HudUiController.cs`、`UiFloatingText.cs`（HUD 金币飘字）、`ShopUiController.cs`、`HandUiController.cs`、`CardTooltipBuilder.cs`（卡牌悬停提示）、`BuildingManageUiController.cs`、`BuildingActionRingController.cs`、`BuildingActionRing.cs`、`PauseMenuUiController.cs`、`GameOverUiController.cs`、`SettingsUiController.cs` |
| `flow/` | 场景流程与会话桥接 | `TitleScreen.cs`、`MainGameController.cs`、`GameSessionBridge.cs`、`CameraController.cs` |
| `core/` | 战场尺寸换算 | `BattlefieldMetrics.cs` |
| `audio/` | 音频总线、BGM/SFX | `AudioManager.cs`、`Sfx.cs`、`SfxEvents.cs`、`UiSfx.cs` |
| `dev/` | 开发者工具与冒烟（不走单测） | `DevToolsMenu.cs`、`TechTreeEditorController.cs`、`DamageMatrixEditor.cs`、`ParameterEditor.cs`、`SmokeTestBase.cs`、`AudioSmoke.cs`、`UiSmoke.cs`、`AllianceBattleSmoke.cs`、`HordeBattleSmoke.cs`、`NecromancerSmoke.cs`、`NecromancerBattleSmoke.cs`、`BuildingActionRingSmoke.cs`、`DungeonSmoke.cs`、`RuntimeInfoPanelController.cs` |
| `cheat/` | 作弊状态 | `CheatState.cs` |

### 2.3 adapters/persistence — 存储

| 文件 | 职责 |
|---|---|
| `SqliteBuildingRepository.cs`、`SqliteUnitRepository.cs`、`SqliteSkillRepository.cs`、`SqliteTechTreeRepository.cs` | 读写 `assets/data/config.db` |
| `SaveDatabase.cs`（纯 C# 项目 `Persistence.csproj`） | 存档文件 SQLite 读写核心（路径注入、零 Godot） |
| `SaveStorage.cs` | 存档文件 Godot 侧入口（解析 `user://` 后委托 SaveDatabase） |
| `BattleReportStorage.cs` | 战报文件持久化 |
| `GameDataLoader.cs` | 数据加载 |

### 2.4 功能模块对照表（需求 → domain 规则 → adapter 实现）

| 功能 | 领域规则 | 适配实现 |
|---|---|---|
| 战斗 | `domain/Battle/CombatRules.cs`、`Unit.cs`、`UnitState.cs` | `adapters/godot/battle/UnitLogic.cs`、`BattleManager.cs` |
| 技能/光环 | `domain/Battle/SkillService.cs`、`AuraService.cs` | `adapters/godot/ui/SkillIconView.cs` |
| 隐身 | `domain/Battle/StealthService.cs` | `adapters/godot/battle/UnitLogic.cs` |
| 建筑放置 | `domain/Building/OccupancyGrid.cs`、`BuildingData.cs` | `adapters/godot/building/Castle.cs`、`CastlePlacementAdapter.cs`、`BuildingSystem.cs` |
| 商店 | `domain/Building/Shop.cs`、`ShopRules.cs` | `adapters/godot/ui/ShopUiController.cs` |
| 手牌/打出 | `domain/Building/Hand.cs`、`CardRules.cs` | `adapters/godot/ui/HandUiController.cs` |
| 组合 | `domain/Building/CombineService.cs`、`CombineRules.cs` | `adapters/godot/combine/CombineBuildingFactory.cs`、`ui/BuildingActionRingController.cs`（手动组合） |
| 升级 | `domain/Building/UpgradeService.cs`、`UpgradeRules.cs` | `adapters/godot/upgrade/UpgradeBuildingFactory.cs` |
| 建筑行动环 | `domain/Building/UpgradeRules.cs`、`SellRules.cs`、`CombineRules.cs`、`BuildingActionRules.cs` | `adapters/godot/ui/BuildingActionRingController.cs` |
| 战报 | `domain/History/BattleReportService.cs`、`ReportBuilder.cs` | `adapters/godot/battle_report/BattleReportSystem.cs` |
| 敌方复刻 | `domain/History/ReplayService.cs`、`MirrorRules.cs` | `adapters/godot/replay/ReplayTarget.cs` |
| 阶段循环 | `domain/Shared/MatchPhaseMachine.cs` | `adapters/godot/autoload/GameManager.cs` |
| 存档 | `domain/Shared/SaveModels.cs`、`ISaveRepository.cs` | `adapters/persistence/SaveStorage.cs` |
| 音频 | — | `adapters/godot/audio/AudioManager.cs`、`Sfx.cs` |

## 3. 场景与预制体（scenes/、prefabs/）

| 路径 | 用途 |
|---|---|
| `scenes/ui/title_screen.tscn` | 标题界面（`project.godot` 主场景） |
| `scenes/main/main_game.tscn` | 主战场场景，系统挂载点 |
| `scenes/ui/settings_panel.tscn` | 设置面板 |
| `scenes/dev/tech_tree_editor.tscn` | 科技树编辑器 |
| `scenes/ui/dev_tools/building_editor.tscn` | 建筑编辑器（编辑建筑定义与派系库存） |
| `scenes/ui/dev_tools/*.tscn` | 开发者工具与冒烟场景（audio_smoke、ui_smoke、dungeon_smoke、damage_matrix_editor、parameter_editor、dev_tools_menu、runtime_info_panel、necromancer_smoke、necromancer_battle_smoke、alliance_battle_smoke、horde_battle_smoke、building_action_ring_smoke、phase_cycle_probe 等），冒烟一律经 `tools/run_smoke.sh` 运行 |
| `prefabs/Building.tscn`、`Castle.tscn`、`Unit.tscn`、`UnitCard.tscn`、`HealthBar.tscn`、`CardArt.tscn` | 动态实体预制体 |

## 4. 素材与数据（assets/）

| 路径 | 内容与命名 |
|---|---|
| `assets/art/cards/` | 单位卡图 `{Name}_{id}.png`（如 `Archer_24.png`）；`card_base`/`card_art_frame`/`card_art_mask` 为卡面框架 |
| `assets/art/buildings/` | 建筑图：`{Name}_128_cut.png`（裁边透明原图）与 `{Name}_128_game.png`（游戏用） |
| `assets/art/units/` | 单位立绘 |
| `assets/art/units/swordsman_sheet_out.png` + `swordsman/frames/` | 剑士帧动画图集与切片帧（attack/attack_combo/run/hurt/death） |
| `assets/art/skills/` | 技能图标 `{skill_name}_{id}.png` |
| `assets/art/projectiles/` | 弹道：arrow、spear、sword、magic_ball |
| `assets/art/tiles/` | 地块贴图 `cell_dirt_01..04`、`kenney_medieval_rts` 图集 |
| `assets/art/placeholders/` | 占位图 `test_image.png` |
| `assets/art/references/` | 美术参考图 |
| `assets/audio/bgm/` | BGM：`bgm_{派系}_{day|night}_{n}.ogg`、`bgm_title_*`、`bgm_result_*`；编号/曲名/风格（舒缓/激昂）/Godot 资源 uid 入库 `bgm_defs`，按 uid 引用，舒缓曲目仅第一天 |
| `assets/audio/sfx/` | SFX：`sfx_{事件}.ogg` |
| `assets/data/config.db` | 权威游戏数据（SQLite：建筑、单位、技能、商店、科技树、配方、物品、BGM 目录 `bgm_defs`）；`config_db_dump.sql` 由 pre-commit 自动生成，勿手改 |
| `assets/shaders/` | 建筑工作/邻接脉冲 shader |
| `assets/dev/` | shader 预览场景 |

素材规格见 `devPlan/design/assetSpecs.md`；新素材产出或变更后须用 `tools/godot_import.py` 生成 `.import`/`.uid`。

## 5. 测试（tests/）

- `CasualCastle.Domain.Tests/`：单测文件按被测类命名（`{Xxx}Tests.cs`），`BattleTestFakes.cs` 为公共测试替身；运行 `dotnet test tests/CasualCastle.Domain.Tests/CasualCastle.Domain.Tests.csproj`
- `CasualCastle.Persistence.Tests/`：存档 SQLite 真实文件往返测试（引用纯 C# Persistence 项目）；运行 `dotnet test tests/CasualCastle.Persistence.Tests/CasualCastle.Persistence.Tests.csproj`
- `CasualCastle.Application.Tests/`：应用用例测试（保存/恢复、阶段流转、夜间结算、开局重置）；运行 `dotnet test tests/CasualCastle.Application.Tests/CasualCastle.Application.Tests.csproj`
- 约定：domain 改动必须带单测；bug 修复写 `RealBug` 测试；测试文件零注释

## 6. 工具（tools/）

| 脚本 | 用途 |
|---|---|
| `tools/liblib_gen.py` | AI 生图（Liblib.art） |
| `tools/liblib_remove_bg.py` | 抠图（ComfyUI 去背景） |
| `tools/godot_import.py` | Godot 无头导入，生成 `.import`/`.uid` |
| `tools/slice_spritesheet.py` | 像素角色图集切片为逐帧动画（自动网格识别 + 白底转透明 + 统一单元格） |
| `tools/pack_sprite_atlas.py` | 把切片帧打包为单张图集 + `atlas.json` 区域元数据 |
| `tools/build_game_texture.py` | 建筑贴图裁边/缩放构建 |
| `tools/check_uids.py` | 校验 `.uid` 文件 |
| `tools/check_architecture.py` | 校验分层架构约束（domain 零 Godot/零 SQLite、依赖方向），已接入 pre-commit |
| `tools/check_smoke_paths.py` | 校验冒烟脚本不硬编码 MainGame/UI 节点路径，已接入 pre-commit |
| `tools/run_smoke.sh` | 冒烟统一运行器（四层防卡死：基类看门狗 + 线程硬看门狗 + --quit-after + OS timeout），PASS 判定契约 |
| `tools/run_game.ps1` | 本机启动 Godot：编辑器未运行时一并启动，已运行时只启动游戏 |

## 7. 入口与运行链路

- 启动：`project.godot` → `scenes/ui/title_screen.tscn` → `scenes/main/main_game.tscn`
- Autoload：`GameManager`、`DisplaySettingsManager`、`AudioManager`
- 主要链路：白天产兵战斗 → 夜晚商店（购卡/修复）→ 白天开始应用敌方复刻与自动存档 → 阶段循环；任一方城堡血量归零 → 结算（保存战报）
- 构建/测试/导入/运行命令见 `devPlan/cloudEnvironment.md`

## 8. 使用提示

- 找规则：先读 `devPlan/concepts/general.md` 定位术语（派系概念见 `devPlan/concepts/` 对应文档），再读对应 design 文档
- 找代码：用 §2.4 对照表定位 domain 与 adapter，只打开关键文件，避免整目录扫描
- 文档与实际数据冲突时，以 `assets/data/config.db` 为准
- `_deprecated/` 内容除非用户主动要求，不读取