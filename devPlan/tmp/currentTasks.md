# 当前任务：科技树数据库 + 可视化编辑器

## 背景

已将王国军五阶建筑表设计完成（19 座建筑，T1→T5，商店/组合双轨，跨线组合）。现在需要：
1. **数据入库** — 把建筑表内容写入 config.db
2. **可视化编辑器** — 能读数据库展示科技树，拖拽调整位置，右键连线，保存回写
3. **建筑素材库** — 右侧库面板，库↔表双向拖拽，新建建筑

设计文档见 `devPlan/design/humanRace.md` §3.2。

---

## 一、数据库设计

### 1.1 新建 race_defs 表

```sql
CREATE TABLE race_defs (
    id TEXT PRIMARY KEY,
    display_name TEXT NOT NULL,
    sort_order INTEGER NOT NULL DEFAULT 0
);
```

种子数据：王国军(kingdom)、冒险者公会(adventurer)、神殿卫士(temple)

### 1.2 新建 tech_tree_nodes 表

```sql
CREATE TABLE tech_tree_nodes (
    type_id TEXT PRIMARY KEY,
    race_id TEXT NOT NULL REFERENCES race_defs(id),
    tier INTEGER NOT NULL CHECK(tier BETWEEN 1 AND 5),
    col INTEGER NOT NULL DEFAULT 0,
    shop_available INTEGER NOT NULL DEFAULT 0,
    gold_cost INTEGER,
    shop_weight INTEGER,
    unlock_night INTEGER DEFAULT 0,
    display_name TEXT NOT NULL,
    unit_type_id TEXT,
    max_health INTEGER NOT NULL DEFAULT 200,
    spawn_interval REAL NOT NULL DEFAULT 10.0
);
```

- `type_id` = 建筑类型标识（如 Barracks、RoyalArmory），与 building_defs.type_id 对应
- `tier` = 1~5，对应建筑表五行（1=最弱底层，5=最强顶层）
- `col` = 0~N 水平列位置
- `shop_available` = 1 表示该建筑在商店中可直接购买，0 表示只能组合获得
- `gold_cost` / `shop_weight` = 商店购买时的费用和权重（shop_available=0 时为空）

### 1.3 连线关系 — 不建独立表

连线关系**不单独建表**，直接从 `combine_recipes` 表查询派生，相当于一个视图。新增/删除/修改连线都直接操作 `combine_recipes`，避免数据重复定义。

### 1.4 种子数据 — 王国军 19 节点

验收标准：
- [ ] race_defs 有 3 条种族记录
- [ ] tech_tree_nodes 有王国军 19 条记录（T1:5, T2:5, T3:5, T4:3, T5:1）
- [ ] 现有 building_defs 11 条迁移到新表
- [ ] combine_recipes 5 条保留不动（连线关系从此表查询）

---

## 二、领域模型

### 2.1 TechTreeNode 数据类

`scripts/domain/Building/TechTreeNode.cs`

```csharp
public class TechTreeNode
{
    public string TypeId { get; init; }
    public string RaceId { get; set; }
    public int Tier { get; set; }
    public int Col { get; set; }
    public bool ShopAvailable { get; set; }
    public int GoldCost { get; set; }
    public int ShopWeight { get; set; }
    public int UnlockNight { get; set; }
    public string DisplayName { get; set; }
    public string UnitTypeId { get; init; }
    public int MaxHealth { get; set; }
    public float SpawnInterval { get; set; }
}
```

### 2.2 RaceDef 数据类

`scripts/domain/Building/RaceDef.cs`

```csharp
public class RaceDef
{
    public string Id { get; init; }
    public string DisplayName { get; init; }
    public int SortOrder { get; init; }
}
```

### 2.3 ITechTreeRepository 接口

`scripts/domain/Building/ITechTreeRepository.cs`

```csharp
public interface ITechTreeRepository
{
    List<RaceDef> LoadRaces();
    List<TechTreeNode> LoadNodes(string raceId);
    List<CombineRecipe> LoadEdges(string raceId); // 从 combine_recipes 查询
    void SaveNodes(string raceId, List<TechTreeNode> nodes);
    void AddRecipe(CombineRecipe recipe);
    void RemoveRecipe(int recipeId);
    void SyncToGameTables(string raceId);
}
```

验收标准：
- [ ] 四个文件创建在 `scripts/domain/Building/` 下
- [ ] 纯 C# 数据类，零 Godot 依赖
- [ ] dotnet build 零 warning

---

## 三、持久化层

### 3.1 SqliteTechTreeRepository

`scripts/adapters/persistence/SqliteTechTreeRepository.cs`

- 构造函数打开 `res://assets/data/config.db`
- `LoadRaces()` 查询 race_defs 表
- `LoadNodes(raceId)` 查询 tech_tree_nodes WHERE race_id=@raceId ORDER BY tier, col
- `LoadEdges(raceId)` 查询 combine_recipes WHERE race_id=@raceId（通过 building 关联 race）
- `SaveNodes()` 对指定 race 先 DELETE 再批量 INSERT（事务包裹）

### 3.2 数据库迁移

- 运行时检测表是否存在（`SELECT name FROM sqlite_master`）
- 不存在则自动 CREATE TABLE
- 检测 tech_tree_nodes 是否为空且有 building_defs 数据 → 自动从现有表导入种子数据

### 3.3 SyncToGameTables

保存时将 tech_tree 数据同步到游戏运行时表：

| 目标表 | 同步逻辑 |
|--------|---------|
| `building_defs` | INSERT OR REPLACE：type_id, display_name, max_health, spawn_interval, unit_type_id, combine_tier(=tier-1) |
| `shop_catalog` | INSERT OR REPLACE：shop_available=1 的节点（id=type_id, name=display_name, cost=gold_cost, building_type=type_id, weight=shop_weight） |

验收标准：
- [ ] SqliteTechTreeRepository 完整实现 ITechTreeRepository
- [ ] 迁移逻辑在编辑器首次打开时自动触发
- [ ] SyncToGameTables 执行后游戏表数据与 tech_tree 表一致
- [ ] dotnet build 零 warning

---

## 四、编辑器场景

### 4.1 场景文件

`scenes/dev/tech_tree_editor.tscn`

```
TechTreeEditor (Control, fullscreen)
├── TopBar (HBoxContainer, 顶部固定 48px)
│   ├── RaceTabBar (TabBar, 3 个标签页)
│   ├── Spacer (Control, 弹性空间)
│   ├── SaveButton (Button, "保存")
│   └── CloseButton (Button, "关闭")
├── EditorArea (Control, 填充剩余空间)
│   ├── RowLabels (VBoxContainer, 左侧行标签 T1~T5)
│   └── CardCanvas (Control, 画布区 — 连线 Draw + 卡片容器)
│       └── [BuildingCard]* (程序化创建，非场景预置)
└── StatusBar (Label, 底部 24px)
```

### 4.2 布局规则

- 五条水平线表示 T1~T5（T1 底部，T5 顶部），等距分布
- 每行左侧显示 "T1" ~ "T5" 文本标签
- 建筑卡片放在对应行上，水平位置由 col 字段决定
- 卡片间距约 140px/列
- 连线在 CardCanvas 的 `_Draw()` 中绘制

验收标准：
- [ ] 场景文件创建在 scenes/dev/ 下
- [ ] 默认 1920×1080 正常显示
- [ ] 打开场景后能看到五条水平行线和行标签

---

## 五、编辑器控制器

### 5.1 TechTreeEditorController

`scripts/adapters/godot/dev/TechTreeEditorController.cs`

纯 C# 控制器（不继承 Godot 节点），模式参照 `ShopUiController`。

**构造：**
- 传入场景根 Control，用 `GetNode` 绑定所有子控件
- 创建 `SqliteTechTreeRepository` 实例
- 加载种族列表，默认选中王国军
- 调用 `LayoutCards()` 首次渲染

**核心方法：**

| 方法 | 功能 |
|------|------|
| `LoadRace(raceId)` | 从 repo 加载节点和连线，调用 LayoutCards |
| `LayoutCards()` | 清空画布，按 (tier, col) 排序创建 BuildingCard Control |
| `CreateCardView(node)` | 创建单个建筑卡片 Control：Panel + 名称Label + 费用Label + 产出Label |
| `DrawConnections()` | 在 CardCanvas._Draw() 回调中绘制所有连线箭头（从 combine_recipes 查询） |
| `Save()` | 调用 repo.SaveNodes + SyncToGameTables |
| `Dispose()` | 取消所有事件订阅 |

### 5.2 BuildingCard（建筑卡片）

每个卡片是一个 `Panel`（或 `Control`），包含：
- 背景颜色：绿色 = 商店可买(S)，蓝色 = 仅组合(U)
- 显示名称（如"皇家军营"）
- 费用标签（如"10g"，仅 S 类显示）
- 产出标签（如"→皇家禁卫"）
- 最小尺寸约 120×60 px
- Metadata 存储 TechTreeNode

验收标准：
- [ ] 选中王国军后画布显示 19 张建筑卡片
- [ ] 卡片按 T1~T5 分五行排列
- [ ] 商店卡和升级卡颜色区分

---

## 六、拖拽与连线交互

### 6.1 左键拖拽 — 调整位置

1. 鼠标左键在卡片上按下 → 记录 `_draggedNode` 和起始列
2. 鼠标移动 → 计算水平偏移，按 140px/列 吸附，更新 `node.Col`，重排卡片
3. 鼠标释放 → 结束拖拽，卡片停留在新位置
4. 同一行内允许交换位置，不允许跨行

### 6.2 右键拖拽 — 创建连线

1. 鼠标右键在卡片 A 上按下 → 记录 `_connectionSource = A`
2. 拖动过程中 → 从 A 底部到鼠标位置绘制临时黄色连线
3. 鼠标在卡片 B 上释放 → 在 combine_recipes 表中新增配方（A→B），material_count 默认 2
4. 如果释放在空白处 → 取消
5. 如果 A==B → 忽略

### 6.3 连线的箭头绘制

在 CardCanvas 覆写 `_Draw()`：
- 遍历所有 CombineRecipe（从 repo 加载）
- 取 from 卡片底部中心 → to 卡片顶部中心
- 绘制三次贝塞尔曲线（控制点向下/上偏移）
- 终点绘制三角形箭头
- 跨线组合的连线用不同颜色（如橙色）区分

### 6.4 右键点击连线 — 删除

- 在 `_UnhandledInput` 中检测右键点击位置是否在连线附近（10px 容差）
- 弹出确认 → 从 `_edges` 列表中移除

### 6.5 双击卡片 — 编辑属性

弹出简单对话框（AcceptDialog 或自定义 Panel）：
- 名称（LineEdit）
- 费用（SpinBox）
- 商店可用（CheckBox）
- 权重（SpinBox）
- T4/T5 需要时可切换层级

验收标准：
- [ ] 左键拖拽建筑卡片，放手后位置更新
- [ ] 右键从 A 拖到 B，画面出现连线箭头
- [ ] 连线随卡片位置移动自动更新
- [ ] 右键点击连线可删除

---

## 七、种族切换与保存

### 7.1 种族标签切换

- TabBar 三个标签：王国军、冒险者公会、神殿卫士
- 切换时自动保存当前种族数据（防止丢失）
- 加载新种族的节点和连线
- 空种族显示空白画布 + 提示 "该种族暂无数据"

### 7.2 保存按钮

1. 收集当前画布上所有卡片的最新 col/tier 值
2. `_repo.SaveNodes(raceId, _nodes)`
3. `_repo.SyncToGameTables(raceId)`
4. 状态栏显示 "保存成功"（3 秒后消失）

### 7.3 关闭按钮

- `GetTree().ChangeSceneToFile("res://scenes/ui/title_screen.tscn")`
- 如果有未保存更改 → 弹出确认对话框

验收标准：
- [ ] 切换种族标签，不同种族数据互相独立
- [ ] 点击保存，config.db 数据更新
- [ ] 关闭再打开，上次保存的数据完整恢复
- [ ] SyncToGameTables 后游戏表数据正确

---

## 八、独立工具

编辑器作为**独立可执行工具**，不内置到游戏中。

### 8.1 启动方式

- 独立 Godot 场景，直接 `godot --path . res://scenes/dev/tech_tree_editor.tscn` 启动
- 或编辑器内按 F6 运行当前场景（`tech_tree_editor.tscn` 设为主场景）
- 不修改 `title_screen.tscn`，不添加 DevMode 按钮

验收标准：
- [ ] 在 Godot 编辑器中打开 tech_tree_editor.tscn 后按 F6 可直接运行
- [ ] 游戏内无法进入编辑器（编辑器场景不在游戏的场景切换路径中）

---

## 九、建筑素材库

### 9.1 概述

编辑器右侧设一个**建筑素材库面板**（Library Panel），列出所有可用建筑类型。库与科技树之间支持双向拖拽，建筑统一显示为简单方框 + 名称文本，后续再扩展美术资源配置。

### 9.2 场景布局更新

在 EditorArea 右侧增加库面板：

```
TechTreeEditor (Control, fullscreen)
├── TopBar (...)
├── EditorArea (HBoxContainer, 填充剩余空间)
│   ├── LeftPanel (VBoxContainer)
│   │   ├── RowLabels (左侧行标签 T1~T5)
│   │   └── Spacer
│   ├── CardCanvas (Control, 画布区)
│   │   └── [BuildingCard]* (程序化创建)
│   └── LibraryPanel (VBoxContainer, 右侧固定 200px 宽)
│       ├── LibraryTitle (Label, "建筑素材库")
│       ├── NewBuildingButton (Button, "新建建筑")
│       └── LibraryItemList (ScrollContainer → VBoxContainer)
│           └── [LibraryItem]* (程序化创建)
└── StatusBar (Label, 底部 24px)
```

### 9.3 数据来源

库面板展示 `building_defs` 表中所有建筑类型。已在当前种族科技树中放置的建筑在库里灰显（不可拖入）。

### 9.4 库卡片（LibraryItem）

每个库卡片与科技树卡片相同样式——简单方框（Panel），内显示 `display_name`：
- 灰色背景 = 已放置在科技树中（不可拖拽）
- 白色背景 = 未放置，可拖拽
- 最小尺寸约 120×40 px

### 9.5 交互规则

#### 库 → 科技树（拖入）

1. 鼠标左键在库卡片上按下 → 开始拖拽，创建半透明跟随指针的预览
2. 拖到 CardCanvas 上 → 根据鼠标 Y 坐标自动确定 tier（吸附到最近行）
3. 释放 → 在 tech_tree_nodes 表中新增记录（type_id, race_id, tier, col=自动分配），刷新画布
4. 释放到空白处 → 取消
5. 已放置的建筑不可重复拖入

#### 科技树 → 库（拖出）

1. 鼠标左键在科技树卡片上按下并拖向库面板
2. 释放到 LibraryPanel 区域内 → 从 tech_tree_nodes 表删除该节点，刷新画布和库
3. 释放到其他位置 → 取消（卡片回到原位）
4. 删除节点时，遍历所有涉及该节点的 combine_recipes：
   - **该节点为 result** → 直接删除配方
   - **该节点为参与材料之一（main_type 或 material_type）**：
     - 配方只剩一个参与建筑 → 改为同线升级配方（material_type = 剩余建筑自身，count=2）
     - 配方还有其他参与建筑 → 移除被删节点，保留剩余材料关系
   - 示例：兵营+牧场→马厩，拖走兵营 → 配方改为 牧场+牧场→马厩

#### 科技树内拖拽（已有功能，不变）

在 CardCanvas 内左键拖拽 = 调整 col 位置（不行跨行）。

### 9.6 新建建筑

1. 点击"新建建筑"按钮 → 弹出对话框（AcceptDialog）
2. 输入 `type_id`（英文标识）和 `display_name`（中文名）
3. 确认 → INSERT 到 building_defs（默认 max_health=200, spawn_interval=10.0），在库中显示新卡片
4. type_id 重复时报错提示

### 9.7 后续扩展预留

- 库卡片后续支持拖入美术资源（图标/模型路径）
- 库卡片支持右键编辑属性（名称、生命值、产出间隔等）
- 当前阶段仅方框 + 名称

验收标准：
- [ ] 右侧库面板显示 building_defs 中所有建筑
- [ ] 已放置建筑灰显，未放置可拖拽
- [ ] 从库拖到画布，成功新增节点并刷新
- [ ] 从画布拖到库，成功删除节点并刷新
- [ ] 新建建筑后库中立即出现新卡片
- [ ] 拖拽过程中半透明预览跟随鼠标

---

## 实现顺序

| 阶段 | 内容 | 依赖 |
|------|------|------|
| 一 | 数据库建表 + 种子数据 | 无 |
| 二 | 领域模型 | 无 |
| 三 | 持久化层 | 一、二 |
| 四 | 编辑器场景（含库面板布局） | 无（纯布局） |
| 五 | 编辑器控制器 + 卡片渲染 + 库面板渲染 | 二、三、四 |
| 六 | 拖拽 + 连线交互 + 库↔表双向拖拽 | 五 |
| 七 | 保存 + 种族切换 | 三、六 |
| 八 | 独立工具启动 | 四 |
| 九 | 新建建筑对话框 | 三、五 |

---

## 待校对项

- [ ] 数据库表结构是否满足需求（列类型、默认值、约束）— 先保留，后续不够再加
- [ ] tech_tree_nodes 是否还需要其他字段（如 footprint_json、has_night_combat）— 先保留，后续不够再加
- [x] 编辑器交互方式是否符合预期（左键拖拽、右键连线、双击编辑）— 确认
- [x] 连线关系不建独立表，直接从 combine_recipes 查询派生 — 已修改
- [x] 编辑器作为独立工具，不内置到游戏中 — 已修改
