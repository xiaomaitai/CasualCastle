# 架构待办（TODO）

长期架构与技术债 backlog。**当前进行中：** §2（详见 `currentTasks.md`）。

---

## 1. 六边形架构分层 ✅ 已完成

`scripts/` 已重组为三层：`domain/`（核心域）、`ports/`（接口契约）、`adapters/`（godot/ + persistence/）。验收见 `currentTasks.md` §1-§6。

---

## 2. 真模块化与 DI ← 当前任务

保留三层结构，domain 层内部拆为 9 个 C# 项目，DI 替代静态 Instance：

```
scripts/
├── domain/
│   ├── Domain.Shared    → 坐标、常量、枚举（零依赖）
│   ├── Domain.Data      → 卡牌、配方、建筑定义（→ Shared）
│   ├── Domain.Night     → 昼夜规则 + IGamePhase 端口（→ Shared）
│   ├── Domain.Shop      → 商店/手牌（→ Shared, Data）
│   ├── Domain.Building  → 占地/邻接（→ Shared, Data, Night）
│   ├── Domain.Fusion    → 融合配方匹配（→ Shared, Data, Building）
│   ├── Domain.Battle    → 战斗规则（→ Shared, Data, Night, Building）
│   ├── Domain.Report    → 战报构建（→ Shared, Data）
│   └── Domain.Replay    → 镜像坐标（→ Shared, Data, Building, Report）
├── ports/              → 接口（分散在各 domain 项目中）
└── adapters/           → Godot 实现 + 持久化
```

adapters 和主项目编译在一起，实现 domain 端口。`GameManager.Services` 作为 DI 入口。

**细化步骤见 `currentTasks.md`。**

---

## 3. 显示与缩放二次重构

在整数**游戏坐标**（每格 100 单位）已落地的前提下，进一步拆分显示职责：

1. **核心域**：按游戏坐标声明业务所需的**逻辑尺寸**（占地、碰撞、产兵点等），不出现像素。
2. **防腐层（引擎适配）**：将源美术资源缩放/裁切到满足逻辑尺寸后再呈现。
3. **游戏内业务缩放**：相邻加成、融合 tier、Buff 等规则驱动的缩放。

---

## 4. 开发者模式

在**设置界面**增加「开发者模式」开关（持久化至 `user://` 配置），统一经门控控制开发 UI 和快捷键。
