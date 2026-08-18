# 冒烟测试编写与运行规范 — CasualCastle

本文档定义无头冒烟测试（`scenes/ui/dev_tools/*.tscn`，脚本位于 `scripts/adapters/godot/dev/`）的编写与运行契约，
目标是让冒烟测试**永远不会挂起卡死**：任何情况下都必须「打印 `SMOKE: PASS` 并以退出码 0 结束」，或
「打印 `SMOKE: FAIL ...` 并以退出码 1 结束」，二者必有其一。

历史事故（2026-08，两次）：
1. 冒烟脚本硬编码 `MainGame/UI/HandPanel` 路径，面板场景化后路径失效，`GetNode` 抛异常中断步骤逻辑，冒烟永不退出。
2. 新冒烟脚本未重新构建导致 C# 类缺失，场景根脚本实例化失败，`_Ready`（含看门狗）根本没执行，进程空转挂起。

两层事故暴露了「脚本内看门狗」的两个盲区：脚本加载失败时看门狗不启动；主循环卡死时 SceneTree 定时器永不触发。
因此防卡死必须是**四层防御**，从内到外：

## 一、防卡死四层机制（强制）

| 层 | 位置 | 覆盖的故障 | 实现 |
| --- | --- | --- | --- |
| L1 基类看门狗 | `SmokeTestBase.StartWatchdog(秒)`，`_Ready` 第一行调用 | 逻辑卡死但帧正常泵 | SceneTree 定时器超时 → 打印 FAIL → `Quit(1)` |
| L2 硬看门狗 | `SmokeTestBase` 内置线程定时器（看门狗秒数+30） | 主循环卡死、L1 定时器不触发 | 独立线程 `System.Environment.Exit(1)` 强制终止进程 |
| L3 引擎帧上限 | 运行器统一追加 `--quit-after` | 脚本加载失败/无退出路径但帧正常泵 | 引擎到帧数强制退出；无 SMOKE 行 → 判定失败 |
| L4 OS 硬超时 | 运行器统一用 GNU `timeout` 包裹 | 引擎级死锁等一切未知情况 | 到秒杀进程，退出码 124 → 判定失败 |

L1/L2 在 `SmokeTestBase` 内，脚本只需继承并在 `_Ready` 第一行 `StartWatchdog(秒)`；
L3/L4 由统一运行器提供（见第三节），**所有冒烟必须经运行器执行**。

## 二、编写规范（checklist）

1. 脚本继承 `SmokeTestBase`（`public partial class XxxSmoke : SmokeTestBase`）
2. `_Ready` 第一行 `StartWatchdog(秒)`：纯逻辑快速冒烟 30~60 秒；驱动完整昼夜/多帧交互的冒烟 180~300 秒
3. 断言一律 `Fail(描述)` 收集，不在检查函数里提前 return 或打印后继续
4. 步骤机类冒烟：`_Process` 中把步骤分发包进 `RunGuarded(AdvanceStep)`，任何步骤抛异常都会记录失败并退出
5. `_Ready` 里的检查序列逐条包 `RunGuarded(CheckXxx)`，单个检查抛异常不中断其余检查的报告
6. 需要场景节点时：UI 根组件经 `UIManager.Instance`/场景实例根/AdapterRegistry 等公开入口；
   其余节点一律 `RequireNode<T>`（缺失即记失败退出，绝不挂起）。
   禁止裸 `GetNode` 硬编码 `MainGame/UI/...` 路径——`tools/check_smoke_paths.py` 静态检查强制（已接入 pre-commit）。
7. 结束只经 `ReportAndQuit()`（幂等，首次生效），禁止自实现报告、禁止直接 `GetTree().Quit`
8. **新增/修改冒烟脚本后必须先 `dotnet build` 再用运行器执行**（事故 2 的直接教训）

## 三、运行规范（强制经运行器）

统一运行器：`sh tools/run_smoke.sh <场景路径.tscn>`

- 内部自动叠加 L3（`--quit-after`）与 L4（`timeout`）；可用 `GODOT_BIN`/`TIMEOUT_SEC`/`QUIT_AFTER` 环境变量覆盖默认值
- 判定契约：退出码 0 且输出含 `SMOKE: PASS` → `[PASS]`；其余（FAIL、无 SMOKE 行、超时 124、崩溃）→ `[FAIL]` 并摘录错误行
- 批量：每个冒烟独立进程，逐条调用运行器；**绝不裸跑 `godot --headless`，绝不无限等待**
- 当前冒烟清单与最新基线见 `devPlan/tmp/behaviorInventory.md`

## 四、反例与正例

反例（事故 1，曾卡死）：

    Control handPanel = GetNode<Control>("MainGame/UI/HandPanel");

反例（事故 2，曾卡死）：新增 UiSmoke.cs 后未 `dotnet build` 直接运行——场景根脚本类缺失，
`_Ready` 未执行、看门狗未启动。现在该情况由 L3/L4 兜底：帧上限退出后因无 `SMOKE: PASS` 判定失败。

正例：

    StartWatchdog(180f);                                  // _Ready 第一行
    Control handPanel = RequireNode<Control>(this, "MainGame/UI/Hand/HandPanel");

相关文档：`devPlan/design/audioSmokeTest.md`（音频冒烟专项）、`devPlan/design/devTools.md`（开发者工具）。
