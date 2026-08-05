#!/usr/bin/env python3
"""校验仓库内所有 uid 文本为 Godot 规范形式且引用可解析。"""
import argparse
import re
import sys
from pathlib import Path

BASE = 34
CHARS = "abcdefghijklmnopqrstuvwxy012345678"


def text_to_id(text):
    if not text.startswith("uid://"):
        return None
    uid = 0
    for ch in text[6:]:
        if "a" <= ch <= "z":
            uid = uid * BASE + ord(ch) - ord("a")
        elif "0" <= ch <= "9":
            uid = uid * BASE + ord(ch) - ord("0") + 25
        else:
            return None
    return uid & 0x7FFFFFFFFFFFFFFF


def id_to_text(uid):
    tmp = []
    while uid:
        tmp.append(CHARS[uid % BASE])
        uid //= BASE
    return "uid://" + "".join(reversed(tmp))


def canonical(text):
    uid = text_to_id(text)
    if uid is None:
        return None
    return id_to_text(uid)


def rel_res_path(path):
    return path.replace("\\", "/").removeprefix("res://")


def check_text(issues, file_key, text):
    canon = canonical(text)
    if canon is None:
        issues.add(f"{file_key}: 非法 uid 文本 {text}")
    elif canon != text:
        issues.add(f"{file_key}: 非规范 uid {text} -> {canon}")


def collect(project):
    ids = {}
    issues = set()
    for uid_file in project.rglob("*.uid"):
        if ".godot" in uid_file.parts or ".git" in uid_file.parts:
            continue
        text = uid_file.read_text(encoding="utf-8", errors="replace").strip()
        check_text(issues, str(uid_file.relative_to(project)), text)
        if text_to_id(text) is not None:
            resource = uid_file.with_suffix("")
            ids[text_to_id(text)] = rel_res_path(str(resource.relative_to(project)))
    for import_file in project.rglob("*.import"):
        if ".godot" in import_file.parts or ".git" in import_file.parts:
            continue
        content = import_file.read_text(encoding="utf-8", errors="replace")
        match = re.search(r'^uid="(uid://[^"]+)"', content, re.MULTILINE)
        if not match:
            continue
        text = match.group(1)
        source = import_file.with_suffix("")
        ids.setdefault(text_to_id(text), rel_res_path(str(source.relative_to(project))))
    for scene in list(project.rglob("*.tscn")) + list(project.rglob("*.tres")):
        if ".godot" in scene.parts or ".git" in scene.parts:
            continue
        rel = rel_res_path(str(scene.relative_to(project)))
        content = scene.read_text(encoding="utf-8", errors="replace")
        for match in re.finditer(r'uid="(uid://[^"]+)"', content):
            check_text(issues, rel, match.group(1))
        lines = content.splitlines()
        header_match = re.search(r'uid="(uid://[^"]+)"', lines[0]) if lines else None
        if header_match and text_to_id(header_match.group(1)) is not None:
            ids.setdefault(text_to_id(header_match.group(1)), rel)
    return ids, issues


def verify_references(project, ids, issues):
    for scene in list(project.rglob("*.tscn")) + list(project.rglob("*.tres")):
        if ".godot" in scene.parts or ".git" in scene.parts:
            continue
        rel = rel_res_path(str(scene.relative_to(project)))
        content = scene.read_text(encoding="utf-8", errors="replace")
        for match in re.finditer(r'\[ext_resource[^\]]*?uid="(uid://[^"]+)"[^\]]*?path="([^"]+)"', content):
            uid, path = match.group(1), match.group(2)
            uid_id = text_to_id(uid)
            if uid_id is None:
                continue
            target = rel_res_path(path)
            registered = ids.get(uid_id)
            if registered is None:
                issues.add(f"{rel}: 引用 uid {uid} 未登记到任何资源")
            elif registered != target:
                issues.add(f"{rel}: uid {uid} 指向 {target}，但登记为 {registered}")


def main():
    parser = argparse.ArgumentParser(description="校验 Godot uid 规范性")
    parser.add_argument("--project", default=".", help="项目根目录")
    args = parser.parse_args()
    project = Path(args.project).resolve()
    ids, issues = collect(project)
    verify_references(project, ids, issues)
    for issue in sorted(issues):
        print(f"[FAIL] {issue}")
    print(f"[{'FAIL' if issues else 'PASS'}] 共 {len(issues)} 个问题")
    sys.exit(1 if issues else 0)


if __name__ == "__main__":
    main()