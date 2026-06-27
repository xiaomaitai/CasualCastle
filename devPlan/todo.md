# 架构待办（TODO）

长期架构与技术债 backlog。**当前进行中：** §2（详见 `currentTasks.md`）。

---

## 1. 六边形架构分层 ✅ 已完成

`scripts/` 已重组为三层：`domain/`（核心域，零 Godot）、`ports/`（接口契约）、`adapters/`（godot/ + persistence/）。验收见 `currentTasks.md` §1-§6。

---

## 2. 真模块化与 DI ← 当前任务

用 **C# 项目拆分 + `Microsoft.Extensions.DependencyInjection`** 替代静态 `Instance` 单例：

- `CasualCastle.Domain` — 独立类库项目（纯 C#，无 Godot），含所有领域规则和端口接口
- `CasualCastle.Game` — Godot 主项目，引用 Domain，含所有适配器实现
- DI 容器在 `CompositionRoot` 中注册所有服务
- Godot 节点在 `_Ready()` 中从容器获取依赖

模块间经端口通信，依赖方向：`Game` → `Domain`，`Domain` 不引用任何外部项目。

**细化步骤见 `currentTasks.md` Phase 2A-2E。**

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
