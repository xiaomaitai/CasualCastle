当新增/变更 Godot 项目素材（PNG、.tscn、.tres、.cs 等）后，使用此 skill 无头导入资源，自动生成 `.import`/`.uid` 侧车文件，无需用户手动打开 Godot 编辑器。

适用场景：

- generate-image / remove-bg 产出新 PNG 之后
- 新增或修改 .tscn / .tres / .cs 等需要 Godot 导入的资源之后
- git status 中出现缺少 `.import` / `.uid` 侧车的新素材时

## 定位 Godot 可执行文件

优先顺序：

1. 环境变量 `GODOT_BIN`
2. `Get-Command godot`（PATH 命中）
3. 常见安装目录：`C:\Program Files\Godot\`、`C:\Program Files (x86)\Godot*\`、`$env:LOCALAPPDATA\Programs\Godot\`
4. 桌面快捷方式 `Godot.lnk`（用 WScript.Shell 解析 TargetPath）

本项目当前使用：`C:\Program Files (x86)\Godot_v4.6.2-stable_mono_win64\Godot_v4.6.2-stable_mono_win64.exe`

## 导入命令（Windows PowerShell）

在项目根目录执行：

```powershell
& '<Godot 可执行文件路径>' --headless --import --path '<项目根目录>'
```

或使用 Codex 版自带脚本（`$CODEX_HOME/skills/godot-import`）：

```bash
python "$CODEX_HOME/skills/godot-import/scripts/godot_import.py" --project <项目根目录> [--godot <Godot 路径>] [--assets a.png b.tscn ...]
```

脚本自动定位 Godot、运行导入并校验侧车文件；`--assets` 传入的资源必须已有 `.import`（PNG）或 `.uid`（.cs/.tscn/.tres）。

## 验证

- 退出码为 0，日志含 `[ DONE ] reimport`
- 新增 PNG 已有 `.import` 侧车；新增 .cs/.tscn/.tres 已有 `.uid` 侧车
- 缺失时重新运行导入或检查 Godot 版本与项目版本是否匹配

## 注意

- `--path` / `--project` 必须指向含 `project.godot` 的目录
- Godot 版本应与项目匹配（本项目 4.6.x）
- `--headless` 无窗口运行，C# 项目同样适用
- 首次导入较慢属正常；`.godot/` 为缓存不提交，`.import` / `.uid` 需随资源一并提交
- 完成后汇报导入结果，不要要求用户手动打开 Godot 编辑器
