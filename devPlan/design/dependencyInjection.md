# 依赖注入设计 — CasualCastle

本文档描述 CasualCastle 项目的依赖注入（DI）架构设计。

---

## 1. 为什么是双层 DI

Godot 管理节点生命周期（场景树实例化、`_Ready`/`_ExitTree`），节点的构造函数参数由引擎控制，无法使用 Microsoft.Extensions.DependencyInjection 的构造函数注入。因此采用**双层 DI**：

- **第一层 MS DI**：管理纯 C# 服务（不继承 Godot 类型），支持标准 DI 语义
- **第二层 AdapterRegistry**：轻量服务定位器，管理 Godot 节点，节点自行注册/注销

两层各自覆盖不同的对象生命周期模型，通过工厂委托桥接。

---

## 2. 架构总览

```
┌─────────────────────────────────────────────────┐
│           CompositionRoot.Build()                │
│           (GameManager._Ready 中调用)              │
├─────────────────────────────────────────────────┤
│  MS DI (ServiceProvider)                         │
│  ┌─────────────────────────────────────────────┐ │
│  │ Domain 模块注册 (4 个 module 扩展方法)        │ │
│  │ IBattleReportRepository → BattleReportStorage│ │
│  │ IGameState → AdapterRegistry 工厂委托         │ │
│  └─────────────────────────────────────────────┘ │
│                       ↕ 工厂委托                  │
│  AdapterRegistry (静态 Dictionary<Type, object>)  │
│  ┌─────────────────────────────────────────────┐ │
│  │ GameManager        (同时注册为 IGameState)    │ │
│  │ DisplaySettingsManager                       │ │
│  │ BattleManager, BuildingSystem                │ │
│  │ BattleReportSystem, Hand, Shop               │ │
│  │ 动态节点 (Building) 按需 Register/Unregister   │ │
│  └─────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────┘
```

---

## 3. 启动与生命周期

### 3.1 启动流程

```
project.godot 启动
  → title_screen.tscn
  → 点击开始 → main_game.tscn
  → GameManager._Ready()                     [Autoload，最早加载]
      ├── Services = CompositionRoot.Build()  # 构建 MS DI 容器
      ├── AdapterRegistry.Register(this)      # 注册为 GameManager
      └── AdapterRegistry.Register<IGameState>(this)  # 注册为 IGameState

  场景树后续节点 ._Ready() 中：
      AdapterRegistry.Register(this)          # 注册自身
      AdapterRegistry.Resolve<T>()            # 解析依赖
```

### 3.2 注销

```
GameManager._ExitTree()
  → AdapterRegistry.Unregister<IGameState>(this)
  → AdapterRegistry.Unregister<GameManager>(this)
  → Services?.Dispose()
  → Instance = null
```

### 3.3 关键类

| 类 | 文件 | 职责 |
|----|------|------|
| `CompositionRoot` | `scripts/CompositionRoot.cs` | MS DI 容器构建入口 |
| `AdapterRegistry` | `scripts/adapters/godot/autoload/AdapterRegistry.cs` | Godot 节点服务定位器 |
| `GameManager` | `scripts/adapters/godot/autoload/GameManager.cs` | DI 根节点，持有 `ServiceProvider` |

---

## 4. MS DI 容器

### 4.1 构建入口

`CompositionRoot.Build()` 负责：

1. 调用 `GameDataLoader.Load()` 加载配置数据
2. 创建 `ServiceCollection`
3. 调用各 Domain 模块的注册扩展方法
4. 注册跨层桥接和持久化服务
5. 调用 `BuildServiceProvider()` 返回容器

```csharp
// CompositionRoot.cs（简化）
public static ServiceProvider Build()
{
    GameDataLoader.Load();

    ServiceCollection services = new ServiceCollection();

    services.AddSingleton<IReadOnlyList<CardData>>(_ => GameDataLoader.ShopCatalog);
    services.AddDomainShared();
    services.AddDomainBuilding();
    services.AddDomainBattle();
    services.AddDomainHistory();

    // 跨层桥接：IGameState 委托到 AdapterRegistry
    services.AddSingleton<IGameState>(_ =>
        AdapterRegistry.Resolve<IGameState>()
        ?? throw new InvalidOperationException("IGameState not registered"));

    services.AddSingleton<IFieldUnitRepository, FieldUnitRepository>();
    services.AddSingleton<IBattleReportRepository, BattleReportStorage>();
    services.AddSingleton<IUnitRepository, SqliteUnitRepository>();
    services.AddSingleton<IBuildingRepository, SqliteBuildingRepository>();

    return services.BuildServiceProvider();
}
```

### 4.2 解析方式

```csharp
// 通过 GameManager 快捷方法
GameManager.Get<IBattleReportRepository>();

// 等价于
GameManager.Services.GetService<IBattleReportRepository>();
```

### 4.3 MS DI 已注册服务

| 服务接口 | 实现 | 生命周期 |
|----------|------|----------|
| `IReadOnlyList<CardData>` | `GameDataLoader.ShopCatalog`（工厂委托） | Singleton |
| `ShopRules` | `ShopRules` | Singleton |
| `AdjacencyService` | `AdjacencyService` | Singleton |
| `ICombatUseCase` | `UnitSpatialService` | Singleton |
| `IReplayUseCase` | `ReplayService` | Singleton |
| `IFieldUnitRepository` | `FieldUnitRepository` | Singleton |
| `IGameState` | `GameManager`（通过 AdapterRegistry 桥接） | Singleton（工厂委托） |
| `IBattleReportRepository` | `BattleReportStorage` | Singleton |
| `IUnitRepository` | `SqliteUnitRepository` | Singleton |
| `IBuildingRepository` | `SqliteBuildingRepository` | Singleton |

---

## 5. AdapterRegistry 服务定位器

### 5.1 实现

`AdapterRegistry` 是一个静态泛型字典包装，无外部依赖：

```csharp
public static class AdapterRegistry
{
    private static readonly Dictionary<Type, object> _instances = new();

    public static void Register<T>(T instance) where T : class
    {
        _instances[typeof(T)] = instance;
    }

    public static T Resolve<T>() where T : class
    {
        return _instances.TryGetValue(typeof(T), out object instance)
            ? (T)instance : null;
    }

    public static void Unregister<T>(T instance) where T : class
    {
        if (_instances.TryGetValue(typeof(T), out object existing) && existing == instance)
            _instances.Remove(typeof(T));
    }
}
```

### 5.2 设计原则

- **泛型 Key**：用 `typeof(T)` 做 key，天然支持同一实例注册为多个接口（如 `GameManager` 同时注册为 `GameManager` 和 `IGameState`）
- **Identity check 注销**：`Unregister` 校验实例引用，防止误删覆盖的同类型注册
- **null-safe 解析**：找不到返回 `null`，调用方自己处理（MS DI 中则抛异常，保证启动即报错）

### 5.3 注册表

| 系统 | 注册类型 | 注册时机 | 解析的依赖 |
|------|---------|---------|-----------|
| `GameManager` | `GameManager`, `IGameState` | `_Ready()` | MS DI |
| `BattleManager` | `BattleManager` | `_Ready()` | `IFieldUnitRepository`, `ICombatUseCase` (MS DI) |
| `DisplaySettingsManager` | `DisplaySettingsManager` | `_Ready()` | — |
| `BuildingSystem` | `BuildingSystem` | `_Ready()` | `IBuildingRepository` (MS DI) |
| `BattleReportSystem` | `BattleReportSystem` | `_Ready()` | `BattleReportService` (MS DI) |
| `Hand` | `Hand` | InitManager | `IBuildingPlacement` |
| `Shop` | `Shop` | InitManager | `ShopRules` (MS DI), `Hand` |
| `Building`（动态） | 不注册 | 实例化时 | `IFieldUnitRepository`, `GameManager`, `Shop`, `AdjacencyService` |
| `SoldierLogic`（动态） | 不注册 | 实例化时 | `IFieldUnitRepository` (MS DI), `GameManager`, `IGameState` |

---

## 6. Domain 模块注册

每个 Domain 项目提供一个模块扩展方法，为未来的纯 C# 领域服务预留注册入口：

```csharp
public static class BattleModule
{
    public static IServiceCollection AddDomainBattle(this IServiceCollection services)
    {
        services.AddSingleton<ICombatUseCase, UnitSpatialService>();
        return services;
    }
}
```

| 模块 | 扩展方法 | 注册内容 |
|------|---------|---------|
| Shared | `AddDomainShared()` | 暂无 |
| Building | `AddDomainBuilding()` | `ShopRules`, `AdjacencyService` |
| Battle | `AddDomainBattle()` | `ICombatUseCase → UnitSpatialService` |
| History | `AddDomainHistory()` | `BattleReportService`, `IReplayUseCase → ReplayService` |

---

## 7. 跨层桥接设计

### 7.1 IGameState 桥接

`IGameState` 定义在 `Domain.Battle`，实现方是 Godot Autoload `GameManager`。由于 MS DI 构建时 AdapterRegistry 已就绪，使用**工厂委托**模式桥接：

```csharp
services.AddSingleton<IGameState>(_ =>
    AdapterRegistry.Resolve<IGameState>()
    ?? throw new InvalidOperationException("IGameState not registered"));
```

这保证了 domain 层代码通过 `IGameState` 接口获取游戏状态，而不依赖 Godot 类型。

### 7.2 MS DI → AdapterRegistry 桥接

某些 AdapterRegistry 节点需要消费 MS DI 服务（如 `BattleReportSystem` 需要 `IBattleReportRepository`）：

```csharp
// BattleReportSystem 中
var repo = GameManager.Get<IBattleReportRepository>();
```

方向始终是 **Adapter → MS DI**，domain 层不感知 AdapterRegistry。

### 7.3 出站端口 + Signal 桥接

当 domain 聚合需要**向外发出事件**（而非查询数据），使用"端口接口 + Signal Relay"模式：

```
domain 聚合
  → 调用 ISoldierEventPort.OnDamaged()     [端口接口，domain 定义]
      ↓
  SoldierEventRelay : Node, ISoldierEventPort   [adapter 层]
      → EmitSignal(Damaged)                     [Godot signal]
          ↓
  SoldierLogic.OnDamaged()                  [adapter 层 handler]
```

**规则：**

- **端口接口定义在 domain 项目**，零 Godot 引用，只声明事件方法签名
- **Relay 类定义在 adapter 项目**，继承 `Node` 实现端口接口，方法体只做两件事：存储数据（供 handler 读取）、发出 `[Signal]`
- **消费方**（通常是另一个 adapter 节点）在 `_Ready` 连接 relay 的 signal，在 handler 中执行 Godot 逻辑（视觉、音效、注销等）
- 端口方法参数若含非 GodotObject 类型（如 domain 接口），无法通过 signal 参数传递 → relay 存储引用，handler 从 relay 读取

**与场景 C 的区别：** 场景 C 是全局单例服务（如 `IGameState`），此模式是**每个领域实例独立持有端口**，relay 作为子节点挂在消费方下。

**当前实例：** `ISoldierEventPort` → `SoldierEventRelay` → `SoldierLogic`

---

## 8. 依赖方向规则

```
              ┌─ 入站端口（IGameState 等）
              │  Adapter 实现，domain 消费
              │  编译期依赖：Adapter → Domain
              │  运行时调用：Domain → 端口 → Adapter
              │
┌─────────────┴────────────────────────────┐
│  Domain.Shared                            │
│  (零依赖，无 Godot)                        │
├──────────────────────────────────────────┤
│  Domain.Building  → Shared                │
│  Domain.Battle    → Shared, Building      │
│  Domain.History   → Shared, Building      │
│  (零 Godot，只依赖下层 domain)              │
├──────────────────────────────────────────┤
│  Adapters (Godot + persistence)           │
│  → 实现 domain 端口（入站 + 出站）           │
│  → 消费 MS DI + AdapterRegistry           │
│  (唯一可用 Godot API 的层)                 │
└─────────────┬────────────────────────────┘
              │  出站端口（ISoldierEventPort 等）
              │  Domain 定义，Adapter 实现为 Signal Relay
              │  编译期依赖：Adapter → Domain
              │  运行时调用：Domain 聚合 → 端口 → Relay → Signal → Handler
```

**编译期依赖始终单向**：Domain 定义接口，Adapter 引用并实现它们。Domain 不依赖任何外部项目。

**运行时调用双向**：
- **入站**（Adapter → Domain）：domain 端口被 adapter 实现后注入 MS DI，domain 通过接口调用（如 `IGameState`）
- **出站**（Domain → Adapter）：domain 聚合持有出站端口引用，调用后经 Relay 转为 Godot signal

- **Domain 零 Godot**：任何 domain 项目不得 `using Godot`
- **Adapter 实现端口**：入站端口直接实现，出站端口通过 Signal Relay 桥接
- **无循环依赖**：六项目编译期依赖 DAG 保证无环，运行时通过端口反转实现双向通信

---

## 9. 新增服务指南

### 场景 A：新增纯 C# 领域服务

在对应 Domain 模块的 `AddDomain*` 方法中注册：

```csharp
// 例：在 SharedModule.cs 中
public static IServiceCollection AddDomainShared(this IServiceCollection services)
{
    services.AddSingleton<ISomeService, SomeServiceImpl>();
    return services;
}
```

消费方通过 `GameManager.Get<ISomeService>()` 获取。

### 场景 B：新增 Godot 节点服务

1. 在节点 `_Ready()` 中 `AdapterRegistry.Register<T>(this)`
2. 在节点 `_ExitTree()` 中 `AdapterRegistry.Unregister<T>(this)`（如有动态销毁）
3. 消费方通过 `AdapterRegistry.Resolve<T>()` 获取
4. Autoload 节点额外调用 `AdapterRegistry.Register<InterfaceType>(this)` 注册其所实现的端口接口

### 场景 C：节点实现新入站端口，domain 需要消费

参考 `IGameState` 模式：
1. 在对应 Domain 项目中定义接口
2. Adapter 节点实现接口
3. 在 `CompositionRoot.Build()` 中添加工厂委托桥接

### 场景 D：domain 聚合需要发出事件（出站端口 + Signal）

参考 `ISoldierEventPort` 模式（详见 7.3）：
1. 在对应 Domain 项目中定义出站端口接口
2. 在 adapter 项目创建 Relay 类：继承 `Node`，实现端口接口，声明 `[Signal]`，方法体内存储数据 + `EmitSignal`
3. 消费方创建 Relay 实例作为子节点，将 relay 注入 domain 聚合的端口属性
4. 消费方在 `_Ready` 连接 relay 的 signal，在 handler 中执行 Godot 逻辑

---

## 10. 相关文件索引

| 文件 | 内容 |
|------|------|
| `scripts/CompositionRoot.cs` | MS DI 容器构建 |
| `scripts/adapters/godot/autoload/AdapterRegistry.cs` | 服务定位器实现 |
| `scripts/adapters/godot/autoload/GameManager.cs` | DI 根节点，`ServiceProvider` 持有者 |
| `scripts/domain/Shared/SharedModule.cs` | Shared 模块注册 |
| `scripts/domain/Building/BuildingModule.cs` | Building 模块注册 |
| `scripts/domain/Battle/BattleModule.cs` | Battle 模块注册 |
| `scripts/domain/History/HistoryModule.cs` | History 模块注册 |
| `devPlan/outline/c05ProjectArchitecture.md` | 项目架构（含 DI 概述和端口表） |
| `devPlan/codeStructure.md` | 代码结构（含系统清单） |
