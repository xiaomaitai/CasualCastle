#!/usr/bin/env python3
"""校验冒烟脚本不硬编码跨场景 UI 节点路径，防止节点迁移后 GetNode 抛异常导致冒烟卡死。"""
import re
import sys
from pathlib import Path

DEV_DIR = "scripts/adapters/godot/dev"
SMOKE_FILES = (
    "AudioSmoke.cs",
    "DungeonSmoke.cs",
    "NecromancerSmoke.cs",
    "NecromancerBattleSmoke.cs",
    "BuildingActionRingSmoke.cs",
    "PhaseCycleProbe.cs",
)
FORBIDDEN = re.compile(r'GetNode(?:OrNull)?<[^>]*>\("MainGame/')


def main():
    issues = []
    root = Path(".")
    for name in SMOKE_FILES:
        path = root / DEV_DIR / name
        if not path.exists():
            continue
        for number, line in enumerate(path.read_text(encoding="utf-8", errors="replace").splitlines(), 1):
            if FORBIDDEN.search(line):
                issues.append(f"{DEV_DIR}/{name}:{number}: 禁止硬编码 MainGame/UI 节点路径，改用 SmokeTestBase.RequireNode")
    for issue in issues:
        print(f"[FAIL] {issue}")
    print(f"[{'FAIL' if issues else 'PASS'}] 共 {len(issues)} 个问题")
    sys.exit(1 if issues else 0)


if __name__ == "__main__":
    main()
