# 十一、技术栈

| 分类 | 技术 |
|------|------|
| 引擎 | Godot 4.6.2 (mono / C#) |
| 渲染 | Forward+（默认；云 VM 无 GPU 时回退 OpenGL3 软件渲染） |
| 物理 | Jolt Physics（2D 运行时碰撞仍用 Godot 内置物理，项目里 Jolt 配置为 `3d/physics_engine`） |
| 语言 | C# / .NET 8（`net8.0`） |
| 数据库 | SQLite（`Microsoft.Data.Sqlite`） |
| 测试 | xUnit（`tests/` 三个纯 C# 项目） |
| 素材与运行 | 美术素材走 Git LFS；命令见 `devPlan/cloudEnvironment.md` |

## 数据存储约定

- **配置数据**（建筑定义、单位属性、组合配方、伤害矩阵、商店卡池）：统一存储在 SQLite 数据库 `assets/data/config.db`
- **运行时存档**：SQLite 数据库，`user://saves/` 目录，一个存档一个 `.db` 文件
- **战报持久化**：已有 `BattleReportStorage`，存储格式不变
- **`.tres` 资源文件**：仅在 Godot 引擎要求时使用（如场景资源引用），非必要不用于配置数据存储
- **`.tscn` 预制体**：仅用于场景结构和节点层级，非必要不用于存储配置数据
