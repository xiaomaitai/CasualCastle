# 冒烟测试编写与运行规范 — CasualCastle

本文档定义无头冒烟测试（`scenes/ui/dev_tools/*.tscn`，脚本位于 `scripts/adapters/godot/dev/`）的编写与运行契约，
目标是让冒烟测试**永远不会挂起卡死**：任何情况下都必须「打印 `SMOKE: PASS` 并以退出码 0 结束」，或
「打印 `SMOKE: FAIL ...` 并以退出码 1 结束」，二者必有其一。

背景：2026-08 曾发生冒烟卡死事故——冒烟脚本硬编码 `MainGame/UI/HandPanel` 节点路径，手牌面板场景化后路径失效，
`GetNode` 抛异常中断了步骤逻辑，冒烟既未通过也未退出。以下三层机制从代码上杜绝此类问题复发。

## 一、防卡死三层机制（强制）

1. **看门狗（Watchdog）**：`SmokeTestBase.StartWatchdog(秒)` 必须在 `_Ready` 第一行启动。
   超时未退出则打印 `SMOKE: FAIL 看门狗超时` 并强制 `Quit(1)`。
   任何意外（异常吞掉、死循环、等待条件永不成立）都无法让冒烟挂起。
2. **统一报告（ReportAndQuit）**：所有结果经 `SmokeTestBase.ReportAndQuit()` 输出；该方法幂等（只首次生效），
   有失败时逐条打印 `SMOKE: FAIL ...` 并以 1 退出，否则打印 `SMOKE: PASS` 并以 0 退出。
   脚本内禁止再自实现 ReportAndQuit，禁止直接 `GetTree().Quit`。
3. **安全节点访问（RequireNode）**：禁止在冒烟脚本中用 `GetNode` 硬编码跨场景 UI 节点路径
   （尤其 `MainGame/UI/...`——UI 面板正在场景化，路径随时可能失效）。
   一律使用 `SmokeTestBase.RequireNode<T>(this, path)`：节点缺失时记录失败、统一报告并抛异常中止当前步骤，
   不会静默空引用，也不会挂起。该规则由 `tools/check_smoke_paths.py` 静态检查强制（已接入 pre-commit）。

## 二、编写规范（checklist）

1. 脚本继承 `SmokeTestBase`（`public partial class XxxSmoke : SmokeTestBase`）
2. `_Ready` 第一行 `StartWatchdog(秒)`：纯逻辑快速冒烟 30~60 秒；驱动完整昼夜/多帧交互的冒烟 180~300 秒
3. 断言一律 `Fail(描述)` 收集，不在检查函数里提前 return 或打印后继续
4. 步骤机类冒烟：`_Process` 中把步骤分发包进 `RunGuarded(AdvanceStep)`，任何步骤抛异常都会记录失败并退出
5. `_Ready` 里的检查序列逐条包 `RunGuarded(CheckXxx)`，单个检查抛异常不中断其余检查的报告
6. 需要场景节点时：UI 根组件经 `UIManager.Instance`/场景实例根/AdapterRegistry 等公开入口；
   其余节点一律 `RequireNode<T>`；路径变更后同步更新（缺失会在 1 帧内以 FAIL 暴露，绝不挂起）
7. 结束只经 `ReportAndQuit()`

## 三、运行规范

单条（仓库根目录，Windows 用 console 版 Godot）：

    godot --headless --path . scenes/ui/dev_tools/<smoke>.tscn

自动判定契约（CI/批量脚本必须遵守）：

| 结果 | 判定 |
| --- | --- |
| 退出码 0 且输出含 `SMOKE: PASS` | 通过 |
| 退出码 1 且输出含 `SMOKE: FAIL` | 失败（含看门狗超时） |
| 无 SMOKE 行、进程崩溃、超时未退出 | 失败 |

批量运行时每条冒烟独立进程，外层加硬超时兜底（bash：`timeout 600 godot ...`；Windows 由任务框架限制时长），
**超时一律视为失败，绝不无限等待**。当前冒烟清单与最新基线见 `devPlan/tmp/behaviorInventory.md`。

## 四、反例与正例

反例（曾导致卡死）：

    Control handPanel = GetNode<Control>("MainGame/UI/HandPanel");
    // 面板移入 Hand 场景后路径失效 → 抛异常 → 冒烟永不退出

正例：

    Control handPanel = RequireNode<Control>(this, "MainGame/UI/Hand/HandPanel");

相关文档：`devPlan/design/audioSmokeTest.md`（音频冒烟专项）、`devPlan/design/devTools.md`（开发者工具）。
