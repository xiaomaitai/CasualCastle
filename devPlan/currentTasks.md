# 当前任务

**当前焦点：B6 — SQLite 数据层（`devPlan/outline/c03BattleSystemRoadmap.md`）。**

---

## B6: SQLite 数据层

### 目标

将硬编码的游戏数据（`UnitRegistry`、`BuildingDefinitions`、`DamageMatrix`、`ShopRules`）迁移到 SQLite 数据库，统一数据管理。

### 数据库文件

`assets/data/game_data.db`（嵌入资源，随版本发布），通过 `res://` 路径加载。

### 表结构

```sql
CREATE TABLE unit_stats (
    type_id       TEXT PRIMARY KEY,
    size          INTEGER NOT NULL,  -- 0=Small 1=Medium 2=Large 3=Huge
    attack_type   INTEGER NOT NULL,  -- 0=Melee 1=Ranged
    damage_type   INTEGER NOT NULL,  -- 0=Normal 1=Pierce 2=Siege 3=Magic
    armor_type    INTEGER NOT NULL,  -- 0=Light 1=Heavy 2=Fortified 3=Beast
    health        INTEGER NOT NULL,
    damage        INTEGER NOT NULL,
    speed         REAL    NOT NULL,
    attack_range  REAL    NOT NULL,
    attack_cooldown REAL  NOT NULL,
    has_night_combat INTEGER NOT NULL DEFAULT 0,
    unit_color    INTEGER NOT NULL
);

CREATE TABLE building_defs (
    type_id           TEXT PRIMARY KEY,
    display_name      TEXT    NOT NULL,
    max_health        INTEGER NOT NULL,
    spawn_interval    REAL,
    spawn_cell_x      INTEGER DEFAULT 0,
    spawn_cell_y      INTEGER DEFAULT 0,
    unit_type_id      TEXT,
    has_night_combat  INTEGER DEFAULT 0,
    fusion_tier       INTEGER DEFAULT 0,
    is_core           INTEGER DEFAULT 0,
    footprint_json    TEXT    NOT NULL  -- JSON: [[0,0],[1,0]]
);

CREATE TABLE damage_matrix (
    damage_type INTEGER NOT NULL,
    armor_type  INTEGER NOT NULL,
    multiplier  REAL    NOT NULL,
    PRIMARY KEY (damage_type, armor_type)
);

CREATE TABLE shop_catalog (
    id            TEXT PRIMARY KEY,
    name          TEXT    NOT NULL,
    cost          INTEGER NOT NULL,
    building_type TEXT    NOT NULL
);

CREATE TABLE fusion_recipes (
    main_type_id     TEXT NOT NULL,
    material_type_id TEXT NOT NULL,
    material_count   INTEGER NOT NULL,
    result_type_id   TEXT NOT NULL,
    PRIMARY KEY (main_type_id, material_type_id)
);
```

### 加载流程

```
CompositionRoot.Build()
  └── GameDataLoader.Load("res://assets/data/game_data.db")
        ├── 打开 SQLite 连接（只读）
        ├── 读取 unit_stats → UnitRegistry 内存缓存
        ├── 读取 building_defs → BuildingDefinitions 内存缓存
        ├── 读取 damage_matrix → DamageMatrix 内存缓存
        ├── 读取 shop_catalog → ShopRules 内存缓存
        └── 读取 fusion_recipes → FusionRules 内存缓存
```

### 施工步骤

| 步骤 | 内容 |
|------|------|
| 1 | 添加 `Microsoft.Data.Sqlite` NuGet 包到主项目 |
| 2 | 创建 `assets/data/game_data.db`，建表+插入当前数据 |
| 3 | 新建 `scripts/adapters/persistence/GameDataLoader.cs` — 读取数据库填充缓存 |
| 4 | 改造 `UnitRegistry` — 移除硬编码，改为 `LoadFrom(DbDataReader)` |
| 5 | 改造 `BuildingDefinitions` — 同上 |
| 6 | 改造 `DamageMatrix` — 同上 |
| 7 | 改造 `ShopRules` — 同上 |
| 8 | 改造 `FusionRules` — 同上 |
| 9 | `CompositionRoot` 中调用 `GameDataLoader.Load()` |
| 10 | 验证：运行游戏，确认所有兵种/建筑/商店数据正确 |

### SQLite 工具链

- 编辑数据库：DB Browser for SQLite（免费，可视化编辑）
- C# 库：`Microsoft.Data.Sqlite`（微软官方，轻量，无需额外 native 依赖）
- 连接字符串：`Data Source=<path>`，只读模式打开

---

## 验收状态

- [x] `dotnet build` 0 错误
- [x] `dotnet test` 6/6 通过
- [x] 4 个 domain 项目零 `using Godot`
- [x] 战斗系统 B1-B5 + 碰撞 + 移动 完成
- [ ] B6 SQLite 数据层
