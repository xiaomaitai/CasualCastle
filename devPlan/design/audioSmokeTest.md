# 音频冒烟测试说明

## 一、目的

`AudioSmoke`（`scenes/ui/dev_tools/audio_smoke.tscn` + `scripts/adapters/godot/dev/AudioSmoke.cs`）用于无头验证 v0.6 音频系统是否可用，避免每次改动后手动开游戏听一遍才能发现问题。覆盖 AudioManager 的 BGM 曲目池、SFX 事件、音频总线与音量写入路径。

## 二、校验项

| 编号 | 校验内容 | 检查方式 |
|------|---------|---------|
| 1 | Music / SFX 音频总线存在 | `AudioServer.GetBusIndex` |
| 2 | 15 首 BGM 资源可加载 | 逐个 `GD.Load<AudioStream>` |
| 3 | 31 个 SFX 事件资源可加载 | 逐个 `GD.Load<AudioStream>` |
| 4 | 标题/对局/结算曲目池切换 | `PlayTitleMusic` / `UpdateGameplayMusic` / `PlayResultMusic` |
| 5 | SFX 播放与防重入 | `PlaySfx` 连续调用多个事件 |
| 6 | 音量写入生效 | `SetMusicVolume(0.5)` 后读回总线 dB 对比 |
| 7 | 顺序播放开关 | `SetShuffleEnabled(false)` |

## 三、前置条件

1. **Godot 4.6.2 mono**：项目当前使用
   `C:\Program Files (x86)\Godot_v4.6.2-stable_mono_win64\Godot_v4.6.2-stable_mono_win64.exe`。
2. **.NET SDK 8.0**：`dotnet build CasualCastle.sln` 可成功（0 错误）。
3. **Godot 资源侧车**：新增/变更 `.cs`、`.tscn`、素材后，先执行 `godot-import` skill
   （`python tools\godot_import.py --project . --godot <Godot路径>`），否则脚本可能无法挂载。
4. **配置数据库**：`assets/data/config.db` 存在（clone 时由 `githooks/post-checkout` 从
   `config_db_dump.sql` 自动重建，无需手动处理）。
5. **音频资源**：`assets/audio/bgm/` 15 首与 `assets/audio/sfx/` 25 个文件及 `.import` 侧车在库内。

> 无头模式使用哑音频驱动，不依赖声卡，可正常执行播放路径。

## 四、执行方式

### 方式一：无头命令行（推荐，可脚本化/CI）

```powershell
cd C:\Users\zhouy\Documents\GodotProjects\CasualCastle
dotnet build CasualCastle.sln
python tools\godot_import.py --project . --godot "C:\Program Files (x86)\Godot_v4.6.2-stable_mono_win64\Godot_v4.6.2-stable_mono_win64.exe"
& "C:\Program Files (x86)\Godot_v4.6.2-stable_mono_win64\Godot_v4.6.2-stable_mono_win64.exe" --headless --path C:\Users\zhouy\Documents\GodotProjects\CasualCastle res://scenes/ui/dev_tools/audio_smoke.tscn --quit-after 600
```

- `--quit-after 600` 是保险丝：脚本正常会自行退出，防止脚本挂载失败时进程悬挂。
- 退出码 `0` = 全部通过；`1` = 存在失败项。

### 方式二：编辑器内运行

1. 打开 `scenes/ui/dev_tools/audio_smoke.tscn`
2. 按 `F6`（运行当前场景）
3. 观察输出面板中的 `SMOKE:` 行；脚本结束会自动退出运行

## 五、预期输出

```
SMOKE: buses ok
SMOKE: resources ok
SMOKE: audio manager ok
SMOKE: PASS
```

失败时输出（示例）：

```
SMOKE: FAIL BGM 资源缺失 bgm_title_3.ogg
SMOKE: FAIL 缺少音频总线 SFX
```

退出码为 1。退出时的 `resources still in use at exit` 是无头快速退出的正常提示，不是失败。

## 六、维护说明

- 新增 BGM 曲目或 SFX 事件时：先更新 `devPlan/design/audioRequirements.md` 清单，再同步
  `AudioSmoke.cs` 中的 `BgmFiles` / `SfxEventNames` 列表，最后执行 godot-import 并跑一次本测试。
- 测试只覆盖"资源存在 + 调用路径不抛错"，不评判音质与音量大小；听感验收仍需人工运行游戏。