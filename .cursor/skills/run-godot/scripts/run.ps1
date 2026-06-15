param(
    [string]$GodotDir = "C:\Program Files (x86)\Godot_v4.6.2-stable_mono_win64",
    [string]$ProjectPath = (Resolve-Path (Join-Path $PSScriptRoot "..\..\..\..")).Path,
    [string]$MainScene = "res://scenes/ui/title_screen.tscn",
    [int]$EditorStartupSeconds = 3
)

function Get-GodotExecutable {
    param([string]$Dir)

    if (-not (Test-Path $Dir)) {
        throw "Godot 安装目录不存在: $Dir"
    }

    $exe = Get-ChildItem -Path $Dir -Filter "Godot*.exe" -ErrorAction Stop | Select-Object -First 1
    if (-not $exe) {
        throw "在 $Dir 中找不到 Godot*.exe"
    }

    return $exe.FullName
}

function Test-GodotEditorRunning {
    param([string]$GodotExe, [string]$Project)

    $processName = [System.IO.Path]::GetFileNameWithoutExtension($GodotExe)
    $normalizedProject = $Project.Replace('/', '\').TrimEnd('\')

    Get-CimInstance Win32_Process -Filter "Name = '$processName.exe'" -ErrorAction SilentlyContinue |
        Where-Object {
            $_.CommandLine -match '--editor' -and
            $_.CommandLine -match [regex]::Escape($normalizedProject)
        }
}

$godot = Get-GodotExecutable -Dir $GodotDir
$editorRunning = Test-GodotEditorRunning -GodotExe $godot -Project $ProjectPath

if (-not $editorRunning) {
    Write-Host "未检测到编辑器，正在启动 Godot 编辑器..."
    Start-Process -FilePath $godot -ArgumentList @('--editor', '--path', $ProjectPath) -WorkingDirectory $ProjectPath
    Start-Sleep -Seconds $EditorStartupSeconds
} else {
    Write-Host "编辑器已在运行，跳过启动编辑器。"
}

Write-Host "正在启动游戏..."
Start-Process -FilePath $godot -ArgumentList @('--path', $ProjectPath, $MainScene) -WorkingDirectory $ProjectPath
