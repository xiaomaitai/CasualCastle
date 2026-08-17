#!/usr/bin/env python3
"""校验分层架构约束：domain 与 application 零 Godot/零 SQLite，依赖方向单向，运行时场景脚本不直接访问 SQLite。

唯一例外：scripts/adapters/godot/dev/ 开发者工具允许直连 config.db（按 AGENTS.md 约定）。
"""
import argparse
import re
import sys
from pathlib import Path

PURE_PROJECTS = (
    ("scripts/domain/Shared", "Shared.csproj"),
    ("scripts/domain/Building", "Building.csproj"),
    ("scripts/domain/Battle", "Battle.csproj"),
    ("scripts/domain/History", "History.csproj"),
    ("scripts/application", "Application.csproj"),
)
PERSISTENCE_DIR = "scripts/adapters/persistence"
GODOT_ADAPTER_DIR = "scripts/adapters/godot"
DEV_TOOLS_DIR = "scripts/adapters/godot/dev"
GODOT_TYPE = re.compile(
    r"\b(Godot|GodotObject|Node2D|Node3D|Area2D|Control|Sprite2D|Texture2D|"
    r"PackedScene|Vector2|Vector3|Color|Mathf|GD\.|ProjectSettings|FileAccess|DirAccess)\b"
)
SQLITE_TYPE = re.compile(r"\bSqlite(Connection|Command|DataReader|Parameter)\b|Microsoft\.Data\.Sqlite")


def source_text(path):
    lines = path.read_text(encoding="utf-8", errors="replace").splitlines()
    return "\n".join(line for line in lines if not line.strip().startswith("//"))


def cs_files(directory, project):
    root = project / directory
    return sorted(path for path in root.rglob("*.cs") if "obj" not in path.parts)


def collect(project):
    issues = set()

    for pure_dir, csproj_name in PURE_PROJECTS:
        csproj = project / pure_dir / csproj_name
        if not csproj.exists():
            issues.add(f"{pure_dir}: 缺少 {csproj_name}")
            continue
        text = csproj.read_text(encoding="utf-8", errors="replace")
        for ref in re.finditer(r'<ProjectReference Include="([^"]+)"', text):
            target = (csproj.parent / ref.group(1)).resolve().relative_to(project)
            if not str(target).replace("\\", "/").startswith("scripts/domain/"):
                issues.add(f"{pure_dir}: 项目引用越界 {target}")
        for pkg in re.finditer(r'<PackageReference Include="([^"]+)"', text):
            if "Godot" in pkg.group(1):
                issues.add(f"{pure_dir}: 包引用 {pkg.group(1)} 引入 Godot")

    for pure_dir, _ in PURE_PROJECTS:
        for path in cs_files(pure_dir, project):
            text = source_text(path)
            if GODOT_TYPE.search(text):
                issues.add(f"{path.relative_to(project)}: 纯 C# 内层代码引用 Godot 类型")
            if SQLITE_TYPE.search(text):
                issues.add(f"{path.relative_to(project)}: 纯 C# 内层代码直接访问 SQLite")

    for path in cs_files(GODOT_ADAPTER_DIR, project):
        if str(path.relative_to(project)).replace("\\", "/").startswith(DEV_TOOLS_DIR + "/"):
            continue
        if SQLITE_TYPE.search(source_text(path)):
            issues.add(f"{path.relative_to(project)}: Godot 适配层直接访问 SQLite")

    csproj = project / PERSISTENCE_DIR / "Persistence.csproj"
    if csproj.exists():
        text = csproj.read_text(encoding="utf-8", errors="replace")
        for inc in re.findall(r'<Compile Include="([^"]+)"', text):
            path = csproj.parent / inc
            if not path.exists():
                issues.add(f"Persistence: 声明的编译文件不存在 {inc}")
                continue
            if GODOT_TYPE.search(source_text(path)):
                issues.add(f"{path.relative_to(project)}: 纯 C# Persistence 引用 Godot 类型")
        for ref in re.finditer(r'<ProjectReference Include="([^"]+)"', text):
            target = (csproj.parent / ref.group(1)).resolve().relative_to(project)
            if not str(target).replace("\\", "/").startswith("scripts/domain/"):
                issues.add(f"Persistence: 项目引用越界 {target}")

    return issues


def main():
    parser = argparse.ArgumentParser(description="校验分层架构约束")
    parser.add_argument("--project", default=".", help="项目根目录")
    args = parser.parse_args()
    project = Path(args.project).resolve()
    issues = collect(project)
    for issue in sorted(issues):
        print(f"[FAIL] {issue}")
    print(f"[{'FAIL' if issues else 'PASS'}] 共 {len(issues)} 个问题")
    sys.exit(1 if issues else 0)


if __name__ == "__main__":
    main()
