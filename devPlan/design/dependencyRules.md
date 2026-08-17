# 依赖规则与自动检查 — CasualCastle

本文档定义项目分层的编译期依赖方向与自动检查方式，是 Phase 一基线的架构约束依据。
约束规则以 `tools/check_architecture.py` 的实行为准，本文件为说明。

## 编译期依赖方向

```
Domain.Shared
    ↑        ↑            ↑
Domain.Building  Domain.Battle  Domain.History(→Shared, Building)
    ↑
Application 纯 C# 项目（scripts/application/Application.csproj，跨聚合用例）
    ↑
Persistence 纯 C# 项目（scripts/adapters/persistence/Persistence.csproj，当前只含 SaveDatabase.cs）
    ↑
CasualCastle 主项目（Godot 适配层 + 存储适配的 Godot 侧文件）
    ↑
tests：CasualCastle.Domain.Tests（→ 四个 domain 项目）
       CasualCastle.Application.Tests（→ Application）
       CasualCastle.Persistence.Tests（→ Persistence + Shared）
```

单向无环：domain 项目之间只能向下引用；Persistence 纯 C# 项目只引用 domain；
主项目引用全部；测试项目只引用被测层。

## 规则

1. **domain 与 application 零 Godot**：四个 domain 项目与 application 项目不得引用 Godot 包，
   源码不得出现 Godot 类型（由此保证领域端口不会返回 Godot 类型）。
2. **domain 与 application 零 SQLite**：内层源码不得出现 SqliteConnection 等存储访问。
3. **场景脚本零 SQLite**：`scripts/adapters/godot/` 下运行时脚本不得直接访问 SQLite，
   配置数据只经 `scripts/adapters/persistence/` 读取。
   唯一例外：`scripts/adapters/godot/dev/` 开发者工具（按 AGENTS.md 约定，工具可直连 config.db）。
4. **纯 C# Persistence 项目**：`Persistence.csproj` 只编译其声明清单内的零 Godot 文件，
   且只引用 `scripts/domain/` 下的项目；Godot 的 res://、user:// 路径解析留在主项目组合根。
5. **domain 项目引用边界**：domain csproj 的 ProjectReference 不得指向 domain 之外。

## 自动检查

- `python tools/check_architecture.py`：校验以上全部规则，违规时输出 [FAIL] 并以非零码退出。
- 已接入 `githooks/pre-commit`：每次提交前自动执行，失败阻止提交。
- `python tools/check_uids.py`：独立校验 uid 文本规范性与引用可解析性。

## 迁移期间注意

- Phase 二已新增 `scripts/application/` 项目（只引用 domain 与端口，检查工具已覆盖）。
- Phase 六将更多存储文件迁入 Persistence 纯 C# 项目时，逐个加入 csproj 的 Compile Include 清单并保持零 Godot。
