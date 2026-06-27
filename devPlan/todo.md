# 架构待办（TODO）

长期架构与技术债 backlog。**当前进行中：** §2（详见 `currentTasks.md`）。

---

## 1. 六边形架构分层 ✅ 已完成

`scripts/` 已重组为三层：`domain/`、`ports/`、`adapters/`。验收见 `currentTasks.md`。

---

## 2. 真模块化与 DI ← 当前任务

保留三层，domain 拆为 4 个 C# 项目，DI 替代静态 Instance：

```
scripts/
├── domain/
│   ├── Domain.Shared    → 坐标、常量、枚举
│   ├── Domain.Building  → 建筑、邻接、商店、手牌、融合
│   ├── Domain.Battle    → 战斗、昼夜
│   └── Domain.History   → 战报、回放
├── ports/              → 接口（分散在各 domain 项目中）
└── adapters/           → Godot + 持久化
```

adapters 与主项目编译在一起，实现 domain 端口。`GameManager.Services` 作为 DI 入口。

**细化步骤见 `currentTasks.md`。**

---

## 3. 显示与缩放二次重构

游戏坐标已落地（每格 100 单位），进一步拆分显示职责，使核心域不出现像素。

---

## 4. 开发者模式

设置界面增加「开发者模式」开关，统一门控开发 UI 和快捷键。
