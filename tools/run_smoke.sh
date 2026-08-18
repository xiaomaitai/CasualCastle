#!/bin/sh
# 冒烟测试统一运行器：多层防卡死（OS 硬超时 + 引擎帧上限 + 基类看门狗），
# 只有「退出码 0 且输出含 SMOKE: PASS」才算通过，其余（FAIL/无输出/超时/崩溃）一律失败。
set -u

SCENE="$1"
GODOT_BIN="${GODOT_BIN:-}"
QUIT_AFTER="${QUIT_AFTER:-180000}"
TIMEOUT_SEC="${TIMEOUT_SEC:-600}"

if [ -z "$SCENE" ]; then
    echo "用法: tools/run_smoke.sh <场景路径.tscn>" >&2
    exit 2
fi

if [ -z "$GODOT_BIN" ]; then
    for candidate in \
        "Godot_v4.6.2-stable_mono_win64_console.exe" \
        "Godot_v4.6.2-stable_mono_win64.exe" \
        "godot"; do
        if command -v "$candidate" >/dev/null 2>&1; then
            GODOT_BIN="$candidate"
            break
        fi
        win_path="/c/Program Files (x86)/Godot_v4.6.2-stable_mono_win64/$candidate"
        if [ -x "$win_path" ]; then
            GODOT_BIN="$win_path"
            break
        fi
    done
fi

if [ -z "$GODOT_BIN" ]; then
    echo "[FAIL] 未找到 Godot，请设置 GODOT_BIN 环境变量" >&2
    exit 2
fi

OUT="$(mktemp)"
timeout "$TIMEOUT_SEC" "$GODOT_BIN" --headless --path . --quit-after "$QUIT_AFTER" "$SCENE" >"$OUT" 2>&1
CODE=$?

if [ "$CODE" -eq 0 ] && grep -q "SMOKE: PASS" "$OUT"; then
    echo "[PASS] $SCENE"
    rm -f "$OUT"
    exit 0
fi

echo "[FAIL] $SCENE（退出码=$CODE）"
grep -E "SMOKE: FAIL|SMOKE: 硬|ERROR|Exception" "$OUT" | head -20
rm -f "$OUT"
exit 1
