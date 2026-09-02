#!/usr/bin/env python3
"""Slice a character sprite sheet into per-animation transparent frames.

Detects the grid (row bands + per-row column segments) automatically, crops each
frame to its content box, converts the near-white background to transparency by
flood-filling from the image border, and writes each frame onto a uniform cell
canvas (bottom-center anchored) so the animation stays stable in AnimatedSprite2D.

Usage:
    python tools/slice_spritesheet.py --input <sheet.png> --output <dir> \
        --anims idle,attack,run,hurt,death --bg-thresh 235 --pad 4
"""
import argparse
import json
import os
from collections import deque
from PIL import Image


def is_bg(px, x, y, thresh):
    r, g, b = px[x, y][:3]
    return r >= thresh and g >= thresh and b >= thresh


def detect_row_bands(im, thresh, step=4, on=5, off=2):
    width, height = im.size
    px = im.load()
    row_has = []
    for y in range(height):
        cnt = 0
        for x in range(0, width, step):
            if not is_bg(px, x, y, thresh):
                cnt += 1
        row_has.append(cnt)
    bands = []
    inside = False
    start = 0
    for y in range(height):
        if row_has[y] > on and not inside:
            inside = True
            start = y
        elif row_has[y] <= off and inside:
            inside = False
            bands.append((start, y - 1))
    if inside:
        bands.append((start, height - 1))
    return bands


def detect_col_segments(im, y0, y1, thresh, step=3, on=1, gap=8):
    width, _ = im.size
    px = im.load()
    col_has = []
    for x in range(width):
        cnt = 0
        for y in range(y0, y1 + 1, step):
            if not is_bg(px, x, y, thresh):
                cnt += 1
        col_has.append(cnt)
    segs = []
    inside = False
    start = 0
    for x in range(width):
        if col_has[x] > on and not inside:
            inside = True
            start = x
        elif col_has[x] == 0 and inside:
            inside = False
            segs.append([start, x - 1])
    if inside:
        segs.append([start, width - 1])
    merged = []
    for s in segs:
        if merged and s[0] - merged[-1][1] <= gap:
            merged[-1][1] = s[1]
        else:
            merged.append(s)
    return merged


def tight_bbox(im, x0, x1, y0, y1, thresh):
    px = im.load()
    top = bottom = None
    for y in range(y0, y1 + 1):
        found = False
        for x in range(x0, x1 + 1, 2):
            if not is_bg(px, x, y, thresh):
                found = True
                break
        if found:
            if top is None:
                top = y
            bottom = y
    if top is None:
        return None
    return (x0, top, x1 + 1, bottom + 1)


def remove_bg(img, thresh):
    """Flood-fill near-white background from the border to alpha=0."""
    img = img.convert("RGBA")
    w, h = img.size
    px = img.load()
    visited = [[False] * w for _ in range(h)]
    queue = deque()
    for x in range(w):
        for y in (0, h - 1):
            if not visited[y][x] and is_bg(px, x, y, thresh):
                visited[y][x] = True
                queue.append((x, y))
    for y in range(h):
        for x in (0, w - 1):
            if not visited[y][x] and is_bg(px, x, y, thresh):
                visited[y][x] = True
                queue.append((x, y))
    while queue:
        x, y = queue.popleft()
        px[x, y] = (0, 0, 0, 0)
        for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
            nx, ny = x + dx, y + dy
            if 0 <= nx < w and 0 <= ny < h and not visited[ny][nx] and is_bg(px, nx, ny, thresh):
                visited[ny][nx] = True
                queue.append((nx, ny))
    for y in range(h):
        for x in range(w):
            if px[x, y][3] != 0:
                for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                    nx, ny = x + dx, y + dy
                    if 0 <= nx < w and 0 <= ny < h and px[nx, ny][3] == 0:
                        r, g, b, a = px[x, y]
                        if r >= thresh and g >= thresh and b >= thresh:
                            px[x, y] = (0, 0, 0, 0)
                        break
    return img


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--input", required=True)
    parser.add_argument("--output", required=True)
    parser.add_argument("--anims", default="idle,attack,run,hurt,death")
    parser.add_argument("--bg-thresh", type=int, default=235)
    parser.add_argument("--pad", type=int, default=4)
    parser.add_argument("--manifest", default=None)
    args = parser.parse_args()

    im = Image.open(args.input).convert("RGB")
    anims = [a.strip() for a in args.anims.split(",") if a.strip()]
    bands = detect_row_bands(im, args.bg_thresh)
    if len(bands) != len(anims):
        raise SystemExit(f"detected {len(bands)} rows but {len(anims)} anims given")

    os.makedirs(args.output, exist_ok=True)
    manifest = {"input": os.path.basename(args.input), "animations": {}}
    frames_written = 0

    for anim, (y0, y1) in zip(anims, bands):
        segs = detect_col_segments(im, y0, y1, args.bg_thresh)
        boxes = []
        for (x0, x1) in segs:
            bb = tight_bbox(im, x0, x1, y0, y1, args.bg_thresh)
            if bb:
                boxes.append(bb)
        if not boxes:
            continue
        cell_w = max(b[2] - b[0] for b in boxes) + 2 * args.pad
        cell_h = max(b[3] - b[1] for b in boxes) + 2 * args.pad
        manifest["animations"][anim] = {
            "row": [y0, y1], "cell": [cell_w, cell_h], "frames": []
        }
        for idx, bb in enumerate(boxes):
            x0, y0b, x1, y1b = bb
            crop = im.crop((x0, y0b, x1, y1b))
            frame = remove_bg(crop, args.bg_thresh)
            fw, fh = frame.size
            cell = Image.new("RGBA", (cell_w, cell_h), (0, 0, 0, 0))
            px = (cell_w - fw) // 2
            py = cell_h - args.pad - fh
            cell.paste(frame, (px, py), frame)
            name = f"{anim}_{idx:02d}.png"
            cell.save(os.path.join(args.output, name))
            manifest["animations"][anim]["frames"].append({
                "file": name, "src_box": [x0, y0b, x1, y1b], "cell_pos": [px, py]
            })
            frames_written += 1
        print(f"[{anim}] {len(boxes)} frames -> cell {cell_w}x{cell_h}")

    manifest_path = args.manifest or os.path.join(args.output, "manifest.json")
    with open(manifest_path, "w", encoding="utf-8") as f:
        json.dump(manifest, f, ensure_ascii=False, indent=2)
    print(f"total frames: {frames_written}")
    print(f"manifest: {manifest_path}")


if __name__ == "__main__":
    main()
