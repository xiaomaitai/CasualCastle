# 架构待办（TODO）

长期架构与技术债 backlog。**当前进行中：** §2（详见 `currentTasks.md`）。

---

## 1. 六边形架构分层 ✅ 已完成

`scripts/` 已重组为三层：`domain/`（核心域，零 Godot）、`ports/`（接口契约）、`adapters/`（godot/ + persistence/）。验收见 `currentTasks.md` §1-§6。

---

## 2. 真模块化与 DI ← 当前任务

每个模块是独立的 C# 项目，项目引用链 = 架构依赖图：

```
Shared ← Data ← Shop
   ↑       ↑       ↑
   ├─ Night ├─ Building ←─ Fusion
   ├─ Report           ←─ Replay
   └─ Battle

Godot → 所有 domain 项目
Game  → Godot（composition root）
``` 

12 个项目，每个用 `IServiceCollection` 扩展方法注册服务，`GameManager.Services` 作为 DI 容器入口。domain 项目零 Godot 引用。

**细化步骤见 `currentTasks.md` Phase 2A-2C。**

---

## 3. 显示与缩放二次重构

在整数**游戏坐标**（每格 100 单位）已落地的前提下，进一步拆分显示职责：

1. **核心域**：按游戏坐标声明业务所需的**逻辑尺寸**（占地、碰撞、产兵点等），不出现像素。
2. **防腐层（引擎适配）**：将源美术资源缩放/裁切到满足逻辑尺寸后再呈现。
3. **游戏内业务缩放**：相邻加成、融合 tier、Buff 等规则驱动的缩放。

目标：换图、换分辨率、换 UI 缩放策略时，不改核心域规则。

---

## 4. 开发者模式

在**设置界面**增加「开发者模式」开关（持久化至 `user://` 配置）：

| 状态 | 行为 |
| --- | --- |
| **关闭（默认）** | 隐藏开发向 UI；禁用开发向快捷键 |
| **开启** | 显示调试 UI、阶段跳过入口、P 键作弊产兵等 |

统一经「是否开发者模式」门控，避免散落 `#if DEBUG`。

---

## 参考

- `scripts/adapters/godot/core/GameCoordinates.cs`：坐标换算 shim（最终将删除）
- `scripts/adapters/godot/battle/UnitSpawn.cs`：产兵放置，已委托到 domain + adapter
