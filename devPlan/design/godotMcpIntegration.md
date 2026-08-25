# Godot MCP 接口接入（DSH ↔ Godot）

> 对应 `devPlan/tmp/todo.md`「Godot MCP 接口接入」。本文记录选型、兼容性验证、DSH 接入方式，以及「冒烟测试能否改用 MCP 简化」的评估结论。

## 1. 选型

- **仓库**：[Coding-Solo/godot-mcp](https://github.com/Coding-Solo/godot-mcp)（v0.1.1，MIT，TypeScript，Node MCP server，stdio transport）
- 社区活跃（多个 fork 扩展，如 RTS agentic testing / Android 导出校验），是主流社区方案。
- 依赖：Node.js + `GODOT_PATH` 指向 Godot 可执行文件（可省略，server 会自动探测）。

## 2. 兼容性验证（2026，本机）

- 环境：Windows + Godot **4.6.2.stable.mono**（本项目版本）。
- 结果：**14 个工具全部发现**；`get_godot_version` 实测返回 `4.6.2.stable.mono.official.71f334935` ✓；`get_project_info` 实测正确解析 CasualCastle 项目（36 scenes / 393 scripts / 165 assets）✓。
- 结论：与 Godot 4.6 / .NET 项目**兼容**，可作为 DSH 的 MCP server。

### 工具清单（14 个）

| 分类 | 工具 | 说明 |
|---|---|---|
| 进程控制 | `launch_editor` / `run_project` / `get_debug_output` / `stop_project` | 启动编辑器 / 运行项目 / 取实时输出 / 停止 |
| 项目信息 | `get_godot_version` / `list_projects` / `get_project_info` | 版本、项目列表、项目结构 |
| 场景操作 | `create_scene` / `add_node` / `load_sprite` / `save_scene` | 经 `--headless --script godot_operations.gd` 无头操作场景文件 |
| 网格/UID | `export_mesh_library` / `get_uid` / `update_project_uids` | MeshLibrary 导出、UID 查询与重存（Godot 4.4+） |

## 3. DSH 接入方式（用户级配置，无需管理员）

DSH **内置** MCP 客户端桥接插件 `@deepseek-ai/dsh-mcp-client`（`@deepseek-ai/dsh` 的正式依赖，stdio / streamable-http 两种传输）。工具以 `mcp__<serverName>__<rawName>` 形式注册为原生工具（与 Claude Code / Codex 命名一致）。

接入点在 **home 级用户补丁层 `$DSH_HOME/cordis.patch.yml`**（即 `~/.dsh/cordis.patch.yml`，叠加在所有 profile 之上，无需管理员权限、不碰安装目录）。已写入：

```yaml
- insert:
    - id: mcp-godot
      name: '@deepseek-ai/dsh-mcp-client'
      config:
        serverName: godot
        transport: stdio
        command: node
        args: ['C:/Users/zhouy/.dsh/mcp-servers/godot-mcp/build/index.js']
        env:
          GODOT_PATH: 'C:\Program Files (x86)\Godot_v4.6.2-stable_mono_win64\Godot_v4.6.2-stable_mono_win64_console.exe'
```

已用 `dsh --profile web --dump-config` 验证：合成配置树含 `mcp-godot` 实例（serverName/stdio/路径/env 正确）。

**生效条件**：重启 DSH Desktop（启动时加载补丁栈）。重启会中断当前会话，会话可恢复。

## 4. 冒烟测试能否改用 MCP 简化 —— 结论：不能替代，保留 `run_smoke.sh`

| 现有冒烟方案要求 | godot-mcp 能力 | 结论 |
|---|---|---|
| 无头运行（`--headless`） | `run_project` 固定 `-d --path`，**不支持 `--headless`**（仅场景操作工具走 headless） | ❌ 无头/CI 不可用 |
| 超时保护（`--quit-after` + OS timeout） | 无超时参数，只能 `stop_project` 手动停 | ❌ 防卡死缺失 |
| 进程隔离（8 冒烟互不影响） | 单 `activeProcess`，一次只能跑一个；MCP server 长驻 = 单点故障 | ❌ 可靠性下降 |
| 退出码 + `SMOKE: PASS` 契约 | `get_debug_output` 可读输出，但无退出码判定 | ⚠️ 部分可用 |

**结论**：冒烟测试**保持现状**（`tools/run_smoke.sh` + 游戏内断言 + 四层防卡死 + 退出码契约）。MCP 的 `run_project`/`get_debug_output` 仅适合交互式调试（人工盯运行），不构成对现有自动化的简化。

## 5. MCP 的适用场景（开发辅助，非冒烟）

- **编辑器操作**：agent 直接 `create_scene` / `add_node` / `load_sprite` / `save_scene` 改场景，替代人工点按——可与现有 `godot-import` skill、动画工作流构建、参数编辑器（todo）打通。
- **UID 管理**：`get_uid` / `update_project_uids` 与仓库 `tools/check_uids.py` 互补（注意 MCP `get_uid` 的 `filePath` 路径解析有已知小问题，实测 `res://` 前缀文件报「File does not exist」）。
- **运行时输出**：`get_debug_output` 对 MCP 启动的进程可取实时 stdout/stderr。

## 6. 状态与待办

- ✅ 选型 + 兼容性验证（本机 Godot 4.6.2 mono）
- ✅ 冒烟简化评估（结论：不替代）
- ✅ 部署 godot-mcp 至 `~/.dsh/mcp-servers/godot-mcp`（稳定路径，14 工具实测可用）
- ✅ 用户级接入：`~/.dsh/cordis.patch.yml` 已写入 `mcp-godot` 实例，`dsh --dump-config` 验证合成正确（无需管理员）
- ⏳ **激活**：重启 DSH Desktop → 工具以 `mcp__godot__*` 生效，重启后验证

## 7. 重启后验证清单

1. 重启 DSH Desktop（完整退出再启动；home patch 新增不触发 HMR，必须重启）
2. 新会话中应出现 `mcp__godot__*` 工具（14 个：`get_godot_version` / `get_project_info` / `run_project` / `get_debug_output` / `get_uid` 等）
3. 调用 `mcp__godot__get_godot_version` → 返回 `4.6.2.stable.mono.official.71f334935` 即连通
4. 冒烟仍走 `tools/run_smoke.sh`（MCP 不替代，见 §4）
