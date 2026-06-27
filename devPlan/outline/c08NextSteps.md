# 八、下一步开发内容（立即行动）

**当前焦点：六边形架构分层（`todo.md` §1）。**

M6 战报与回放式敌方已验收。下一步按 `currentTasks.md` 推进架构重构首期：

1. **目录与边界**
   - 建立 `domain/`、`ports/`、`adapters/` 骨架
   - 明确核心域不依赖 Godot

2. **试点垂直切片**
   - 迁入游戏坐标与产兵点领域逻辑
   - Godot 适配层负责像素换算与 `UnitSpawn` 入树定位
   - 领域层单元测试

3. **文档同步**
   - `codeStructure.md`、`c05ProjectArchitecture.md`、`AGENTS.md`

**并行 backlog（非当前阻塞）：** `todo.md` §2 真模块化、§3 显示缩放、§4 设置内开发者模式开关。

---
