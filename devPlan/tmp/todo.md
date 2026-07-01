# 架构待办（TODO）

当前任务详见 `currentTasks.md`。这里是长期架构与技术债 backlog。

---
## B10: Soldier 行为 DDD 重构

### 当前问题

- adapter 直接持有 `_domain` 聚合引用，绕过了入站 port
- `UnitRegistry.LoadFrom` 是 adapter 推数据入 domain，应该 domain 定义出站 port 让 adapter 实现
- domain 没有 Repository 接口

### 目标

- 入站 port：domain 定义 `ISoldierService`，adapter 不直接碰聚合
- 出站 port：domain 定义 `IUnitRepository`/`IBuildingRepository`，adapter 实现

---

## 架构评审：六边形架构与 DDD 纯度分析

### 一、当前架构偏离点

#### 1. 领域 Service 持有可变状态（核心问题）

DDD 中 Domain Service 应是**无状态的**，只封装跨聚合的业务逻辑。当前多个 Service 持有会话级可变状态：

| Service | 持有状态 | 实际充当的角色 |
|---------|---------|--------------|
| `UnitSpatialService` | `_playerUnits`, `_enemyUnits`, `_buildings`, `_grid` | 运行时 Repository + 空间索引 |
| `HandService` | `_hand` (手牌列表), `_selectedIndex` | Player 聚合的一部分 |
| `ShopService` | `_offers`, `Gold`, `_random`, `IsShopAvailable` | Player 聚合的一部分 + 商店实体 |
| `BattleReportService` | `_currentReport`, `_reportIdCounter` | 工作单元 / Session 状态 |
| `SoldierService` | 持有 `Soldier` 聚合引用 | 聚合外观（非真正 Service） |

这些类名为 "Service" 但实际上同时承担了**状态持有**和**业务逻辑**两个职责，违反了单一职责原则，也不是 DDD Service 的正确定义。

#### 2. Repository 模式缺失（运行时实体）

- 静态配置数据已有 `IUnitRepository` / `IBuildingRepository`（按 typeId 查询只读数据）
- 但**运行时实体**（活跃士兵、场上建筑）没有 Repository，其追踪逻辑散落在 `UnitSpatialService` 的 `Register`/`Unregister` 和 `BattleManager` 的 `_playerUnits`/`_enemyUnits` 中
- `UnitSpatialService` 既要维护实体集合（Repository 职责），又要做空间查询和碰撞（计算 Service 职责），职责混乱

#### 3. Adapter 直接创建并操作聚合

`SoldierLogic.cs:44`：
```csharp
SoldierService svc = new SoldierService();
```
- Adapter 层直接 `new` 领域对象，绕过 DI
- Adapter 读写聚合属性（`_service.GameX = ...`, `_service.TargetBuilding = ...`）——这些本应通过入站 Port 的方法调用来完成
- `ISoldierService` 接口暴露了大量 get/set 属性，本质上是对聚合的公开读写，不是有意义的业务操作

#### 4. 领域接口泄露 Godot 概念

`UnitSpatialService.IBuildingRef`：
```csharp
object NativeObject { get; }
object CastleObject { get; }
```
- `object` 类型的存在说明 domain 需要引用 adapter 层的对象（`Building`、`Castle`），但用 `object` 擦除了类型
- 这是六边形架构的"端口泄露"——domain 定义的接口隐含了对 adapter 实现细节的依赖

#### 5. 双层 DI 系统复杂度

- MS DI（`ServiceProvider`）只注册了 5 个服务
- `AdapterRegistry`（静态服务定位器）承担了大部分 Godot 节点的查找
- 两种 DI 机制并存，开发者需要判断"这个依赖应该走哪个容器"
- `AdapterRegistry` 本质是全局可变字典，是服务定位器反模式

#### 6. 入站 Port 接口设计问题

- `ISoldierService` 接口包含了聚合的所有读写属性（`GameX`, `GameY`, `Health`, `TargetBuilding` 等），是"数据桶"式的接口而非"行为"接口
- 入站 Port 应该暴露**业务操作**（如 `Attack()`, `MoveTo()`），而非内部状态的 getter/setter
- `ports/` 目录目前仅 1 个文件（`IBuildingFactory`），其他端口接口散落在各 domain 子项目中

### 二、能否改进

**可以改进**，但必须在 Gameplay 特性开发的间隙推进，且需要分阶段进行。核心思路：

1. **区分"状态"和"行为"的归属**——状态属于聚合/实体/Repository，行为属于 Domain Service
2. **引入运行时 Repository**——统一管理活跃实体的生命周期
3. **收窄入站 Port**——接口只暴露业务操作，不暴露内部状态
4. **消除端口泄露**——`IBuildingRef` 的 `object` 属性用正确的领域接口替代
5. **简化 DI**——逐步减少 `AdapterRegistry` 的使用，向 MS DI 收敛

### 三、改造方案

#### 阶段 1：无状态化 Domain Service + 引入运行时 Repository

**1a. 拆分 `UnitSpatialService`**

新建 `IFieldUnitRepository`（出站 Port，domain 定义）：
- `Register(ISoldierService)` / `Unregister(ISoldierService)`
- `GetAllPlayerUnits()` / `GetAllEnemyUnits()`
- `GetNearbyUnits(gameX, gameY, radius)`
- `FindNearestEnemy(soldier)`

新建 `FieldUnitRepository`（adapter 实现，MS DI 注册为 Singleton）：
- 接管当前 `UnitSpatialService` 的 `_playerUnits` / `_enemyUnits` / `_grid`
- 纯数据存储 + 空间查询，不包含业务逻辑

`UnitSpatialService` 改为无状态：
- 构造函数注入 `IFieldUnitRepository`
- `PushSoldiers(dt)`, `FindNearestEnemy(soldier)`, `PropagateRetaliation(...)` 等方法从 repository 获取数据，只做计算
- 不再持有 `_playerUnits` / `_enemyUnits` 列表

**1b. 拆分 `HandService` / `ShopService`**

方案 A（轻量）：将状态迁入 `Player` 聚合
- 新建 `Player` 聚合根，持有 `Gold`, `Hand`, `ShopOffers`
- `HandService` → 只留 `TryPlaceCard` 等纯逻辑
- `ShopService` → 只留 `GenerateOffers` 等纯逻辑

方案 B（渐进）：先不改聚合结构，只将 `HandService`/`ShopService` 重命名为 `Hand`/`Shop` 值对象，明确它们是"状态容器"而非"服务"

推荐方案 B 先行，后续再考虑 Player 聚合。

**1c. 统一 `SoldierService` 定位**

- `SoldierService` 当前是 `Soldier` 聚合的外观/代理
- 选项 1：删除 `SoldierService`，让 adapter 通过 `ISoldierService`（重命名为更准确的入站 Port）操作聚合
- 选项 2：保留 `SoldierService` 但重命名为 `SoldierFacade`，明确其角色不是 DDD Service

推荐选项 1，配合阶段 2 的入站 Port 改造。

#### 阶段 2：收窄入站 Port 接口

当前 `ISoldierService` 暴露了 15+ 个属性 getter/setter。应改为行为方法：

```csharp
// 当前（数据桶式）
float GameX { get; set; }
float GameY { get; set; }
SoldierState State { get; }

// 目标（行为式）
void MoveTo(float gameX, float gameY);
void MarchToward(float targetGameX, float targetGameY);
void EngageTarget(ISoldierService enemy);
void SiegeBuilding(IBuildingTarget building);
```

Adapter 不再直接写 `_service.GameX = ...`，而是调用 `_service.MoveTo(...)`。

**涉及改动：**
- `ISoldierService` 接口重设计（domain/Battle）
- `Soldier` 聚合配合调整（domain/Battle）
- `SoldierLogic._Process()` 改为调用行为方法（adapter）

#### 阶段 3：消除端口泄露

`UnitSpatialService.IBuildingRef` 中的 `NativeObject` 和 `CastleObject`：
- `NativeObject` 用于从 `building` adapter 提取 `IBuildingTarget` 接口 —— 应改为 `IBuildingRef` 直接提供 `TryGetBuildingTarget() → IBuildingTarget`
- `CastleObject` 用于让 `SoldierLogic` 获取 `Castle` 引用 —— 应通过新的出站 Port 或领域标识替代

#### 阶段 4：简化 DI 系统

- 将 `BattleManager` 等核心 adapter 通过工厂委托注册到 MS DI（类似 `IGameState` 的模式）
- 逐步将 `AdapterRegistry` 的注册项迁移到 MS DI
- 最终 `AdapterRegistry` 只保留**动态实例**（每帧创建/销毁的节点），作为轻量级查找表而非"第二 DI 系统"

### 四、优先级建议

| 优先级 | 改造项 | 理由 |
|--------|--------|------|
| **P0** | 拆分 `UnitSpatialService` + 引入 `IFieldUnitRepository` | 当前最严重的职责混乱，影响 B10 施工 |
| **P1** | 收窄 `ISoldierService` 入站 Port | 当前 adapter 直接操作聚合属性，与 B10 目标直接相关 |
| **P2** | 消除 `IBuildingRef` 的 `object` 泄露 | 让 domain 真正零 Godot |
| **P3** | `HandService`/`ShopService` 状态分离 | 与当前玩法迭代耦合较紧，可后续处理 |
| **P4** | DI 系统简化 | 不影响功能，纯架构优化 |

### 五、不做的事情

- **不引入 CQRS / Event Sourcing**：游戏对实时性要求高，事件溯源增加复杂度但不带来收益
- **不拆分更多微服务/进程**：这是单机游戏，多进程无意义
- **不追求"纯"DDD 聚合设计**：游戏中有大量跨聚合交互（士兵碰撞、反击传播），过度隔离聚合反而增加复杂度。核心目标是**Service 无状态 + 端口不泄露 + 聚合有边界**，不追求教科书式的 DDD
