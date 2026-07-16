# 架构待办（TODO）

## 王国军 T3 皇家兵种

五条组合链的终点。设计见 `devPlan/design/humanRace.md` 第 3 章。

- 5 座 T3 皇家建筑：皇家军营、皇家壁垒、皇家射场、皇家马场、皇家游骑
- 5 种皇家兵种：皇家禁卫、皇家盾卫、皇家神射手、皇家骑士、皇家斥候
- 组合链：T2+T2→T3（一座 T3 消耗 4 座 T1）
- 需扩展 combine_tier 字段支持 2

## 冒险者公会（人类流派 2/3）

多种族佣兵联盟，公会产人 + 酒馆组队。设计见 `devPlan/design/humanRace.md` 第 4 章。

- **4 座公会建筑**（T1→T2）：游侠公会→风行圣殿（精灵）、战士公会→山丘要塞（矮人）、盗贼行会→暗影公会（半身人）、术士高塔→龙火塔（龙裔）
- **1 座特殊建筑**：酒馆→冒险者公会（T1→T2），组队集结中心
- 8 种兵种：4 种族 × 2 阶
- 酒馆组队逻辑：公会产出的人前往最近酒馆 → 凑齐 N 个不同种族 → 全队出发
- T1 酒馆 N=4，T2 公会 N=6
- 新系统：种族标签字段 → 需扩展 unit_stats 表
- 新系统：酒馆组队逻辑（路线规划 + 种族去重 + 满员触发）
- 核心机制：鱼龙混杂——单位附近友军种族种类越多，闪避率越高
- 新系统：闪避机制（概率无视攻击）→ 需修改战斗伤害计算
- 商店卡池新增 5 种卡（4 公会 + 酒馆），总卡池从 3 种→8 种
- 后续可扩展更多种族公会（兽人、侏儒、人类、蜥蜴人）

## 神殿卫士（人类流派 3/3）

治疗续航型流派。设计见 `devPlan/design/humanRace.md` 第 5 章。

- 3 座 T1：圣所、圣泉、裁判所
- 3 座 T2：圣殿、圣光圣所、裁决厅
- 6 种兵种：圣盾兵→圣骑士、牧师→大主教、审判官→惩戒者
- 新系统：治疗单位 AI（牧师不攻击，改为治疗周围友军）
- 核心机制：恩典——受到治疗触发各兵种特有额外效果（护盾/冷却缩减/范围伤害）
- 商店卡池新增 3 种卡，总卡池从 8 种→11 种

---

## 架构合规问题（六边形架构 + DDD 审查）

以下问题按严重程度排列：

### 1. BuildingSystem 硬编码建筑视觉配置 — `adapters/godot/building/BuildingSystem.cs:32-154` ✅

已修复：视觉配置迁移到 `config.db` → `building_defs` 表（`texture_path`、`sprite_modulate_r/g/b/a`、`material_path`），`BuildingData` 新增对应字段，`BuildingSystem` 移除 `Visuals` 字典和 `VisualDef` 结构体，改为从 `IBuildingRepository` 读取。

### 2. Domain 层魔法数字 — `domain/Battle/SkillService.cs:7-8`、`domain/Battle/SkillSet.cs:63-78` ✅

已修复：`NearbyAllyRadius`/`TargetIsolatedRadius` 移到 `GameRules.SkillNearbyAllyRadius`/`.SkillTargetIsolatedRadius`；闪避率映射移到 `GameRules.DodgeChanceByAllyRaceCount[]`。

### 3. GameManager 包含过多编排逻辑 — `adapters/godot/autoload/GameManager.cs` ✅

已修复：提取 `IGameSessionService` 端口（`domain/Shared/`）和 `GameSessionService` 适配器（`adapters/godot/flow/`），`GameManager` 的 `SaveGame`/`LoadSaveData`/`HasSave` 通过端口委托，不再直接调用 `ISaveRepository`。

### 4. Building.cs 过大（610 行）混合多种职责 — `adapters/godot/building/Building.cs` ✅

已修复：提取 `RepairRules`（`domain/Building/`），维修条件判断和费用计算移到领域层；工作循环状态机核心已在 `NightRules.CanUnitWork`，剩余为 Godot 视觉/节点管理（属于 adapter 合理职责）。

### 5. SoldierLogic 属性重复 — `adapters/godot/battle/SoldierLogic.cs` ✅

已修复：删除 `Speed`/`AttackRange`/`VisionRange`/`AttackCooldown`/`ArmorType` 只写死代码；`MaxHealth`/`HasNightCombat`/`DamageType` 改为委托 `_soldier`，消除双重数据源。

### 6. GameManager.CanUnitWork 与 NightRules.CanUnitWork 逻辑重复 ✅

已修复：`GameManager.CanUnitWork` 为零调用方死代码，已删除。`Building` 和 `SoldierLogic` 均已统一走 `NightRules.CanUnitWork`。

### 7. FieldUnitRepository 未使用的常量 — `adapters/persistence/FieldUnitRepository.cs:9` ✅

已删除。

### 8. Castle.cs 硬编码贴图路径 — `adapters/godot/building/Castle.cs:66-71` ✅

已有 `[Export] public Texture2D[] CellTextures`，硬编码路径仅作为未赋值时的 fallback。

### 9. BuildingSystem 静态门面绕过 DI — `adapters/godot/building/BuildingSystem.cs` ✅

已修复：移除 `static Instance` 和全部 `static` 数据访问方法。`IBuildingRepository`、`IUnitRepository` 在 `_Ready()` 中通过 `GameManager.Get<T>()` 注入为字段，依赖关系显式可见。所有调用方改为通过 `AdapterRegistry.Resolve<BuildingSystem>()` 获取实例。

### 10. InitManager / NightOrchestrator 直接操作 Godot 节点 ✅

已修复：`ClearNonCoreBuildings` 逻辑提取至 `Castle`，消除 InitManager 与 ReplayTarget 重复。`NightOrchestrator` 改为构造函数注入 `IBuildingRepository`、`IReplayUseCase`，不再内部调用 `GameManager.Get<T>()`。`LoadSaveIntoGame` 改为实例方法。

### 11. IBuildingRef.CastleRef 类型泄露 — `domain/Battle/IBuildingRef.cs:12` ✅

已修复：移除 `IBuildingRef.CastleRef` 属性（零调用方死代码），端口接口不再暴露 opaque object。

### 12. FieldUnitRepository 归类错误 — `adapters/persistence/FieldUnitRepository.cs` ✅

已修复：文件移至 `adapters/godot/battle/`，namespace 改为 `CasualCastle.Adapters.Godot.Battle`。

### 13. Adapter 之间直接耦合 — `adapters/godot/dev/TechTreeEditorController.cs:4,56` ✅

已修复：`TechTreeEditorController` 改为构造函数注入 `ITechTreeRepository`，`TechTreeEditorScene` 从 DI 获取并传入。`CastlePlacementAdapter` 的 lazy resolve 受 Godot 初始化顺序约束（PlayerCastle 在构造时尚不存在），暂时保留。

### 14. IBuildingRepository 默认接口方法含领域逻辑 — `domain/Building/IBuildingRepository.cs:24-29` ✅

已修复：移除 `IsCombinableMaterial` 默认实现，逻辑移至 `CombineRules.IsCombinableMaterial` 静态方法。
