# 十一、技术栈

| 分类 | 技术 |
|------|------|
| 引擎 | Godot 4.6 (C#) |
| 渲染 | Forward+ |
| 物理 | Jolt Physics |
| 语言 | C# / .NET |
| 数据库 | SQLite（`Microsoft.Data.Sqlite`） |

## 数据存储约定

- **配置数据**（建筑定义、单位属性、组合配方、伤害矩阵、商店卡池）：统一存储在 SQLite 数据库 `assets/data/config.db`
- **运行时存档**：SQLite 数据库，`user://saves/` 目录，一个存档一个 `.db` 文件
- **战报持久化**：已有 `BattleReportStorage`，存储格式不变
- **`.tres` 资源文件**：仅在 Godot 引擎要求时使用（如场景资源引用），非必要不用于配置数据存储
- **`.tscn` 预制体**：仅用于场景结构和节点层级，非必要不用于存储配置数据
