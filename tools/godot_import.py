#!/usr/bin/env python3
"""Godot 资源无头导入：运行 --headless --import 生成 .import/.uid 侧车文件。"""
import argparse
import os
import shutil
import subprocess
import sys
from pathlib import Path


def find_godot(override):
    if override and Path(override).is_file():
        return str(Path(override).resolve())
    env_bin = os.environ.get("GODOT_BIN")
    if env_bin and Path(env_bin).is_file():
        return env_bin
    found = shutil.which("godot")
    if found:
        return found
    roots = [
        Path("C:/Program Files/Godot"),
        Path("C:/Program Files (x86)"),
        Path(os.environ.get("LOCALAPPDATA", "")) / "Programs" / "Godot",
    ]
    for root in roots:
        if not root.exists():
            continue
        pattern = "Godot*/Godot*.exe" if "x86" in str(root) else "Godot*.exe"
        candidates = sorted(root.glob(pattern), reverse=True)
        if candidates:
            return str(candidates[0])
    return None


def verify_sidecars(project, assets):
    missing = []
    for rel in assets:
        path = project / rel
        if not path.exists():
            continue
        if not Path(str(path) + ".import").exists() and not Path(str(path) + ".uid").exists():
            missing.append(rel)
    if missing:
        print("[WARN] 缺少侧车文件: " + ", ".join(missing))
        return 1
    return 0


def main():
    parser = argparse.ArgumentParser(description="Godot 资源无头导入")
    parser.add_argument("--project", default=".", help="Godot 项目根目录（含 project.godot）")
    parser.add_argument("--godot", default="", help="Godot 可执行文件路径（缺省自动查找）")
    parser.add_argument("--assets", nargs="*", default=[], help="需要验证侧车文件的资源相对路径")
    args = parser.parse_args()

    project = Path(args.project).resolve()
    if not (project / "project.godot").is_file():
        print(f"[ERROR] 未找到 project.godot: {project}")
        sys.exit(1)

    godot = find_godot(args.godot)
    if not godot:
        print("[ERROR] 未找到 Godot。请用 --godot 指定路径或设置 GODOT_BIN 环境变量。")
        sys.exit(1)

    print(f"[info] Godot: {godot}")
    print(f"[info] Project: {project}")
    code = subprocess.run([godot, "--headless", "--import", "--path", str(project)]).returncode
    if code != 0:
        print(f"[ERROR] 导入失败，退出码 {code}")
        sys.exit(code)

    check_script = Path(__file__).resolve().parent / "check_uids.py"
    check = subprocess.run([sys.executable, str(check_script), "--project", str(project)])
    if check.returncode != 0:
        print("[ERROR] UID 校验未通过，请先规范化非规范 uid 文本")
        sys.exit(check.returncode)

    if args.assets:
        sys.exit(verify_sidecars(project, args.assets))
    print("[done] 导入完成")


if __name__ == "__main__":
    main()
