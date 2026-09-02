# Pack sliced frames into a single atlas texture with region metadata.
import argparse
import json
import os
import re
from PIL import Image


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--frames", required=True)
    parser.add_argument("--atlas", required=True)
    parser.add_argument("--cols", type=int, default=7)
    parser.add_argument("--anims", default="attack,attack_combo,run,hurt,death")
    args = parser.parse_args()

    anims = [a.strip() for a in args.anims.split(",") if a.strip()]
    anim_order = {a: i for i, a in enumerate(anims)}

    frames = []
    for name in sorted(os.listdir(args.frames)):
        m = re.match(r"^(.*?)_(\d{2})\.png$", name)
        if not m:
            continue
        anim, idx = m.group(1), int(m.group(2))
        if anim not in anim_order:
            continue
        frames.append((anim, idx, os.path.join(args.frames, name)))
    frames.sort(key=lambda t: (anim_order[t[0]], t[1]))

    imgs = [(anim, idx, Image.open(p).convert("RGBA")) for anim, idx, p in frames]
    cell_w, cell_h = imgs[0][2].size
    for (_, _, img) in imgs:
        if img.size != (cell_w, cell_h):
            raise SystemExit("non-uniform frame size: " + str(img.size))

    rows = (len(imgs) + args.cols - 1) // args.cols
    atlas = Image.new("RGBA", (args.cols * cell_w, rows * cell_h), (0, 0, 0, 0))
    regions = {}
    for i, (anim, idx, img) in enumerate(imgs):
        cx = (i % args.cols) * cell_w
        cy = (i // args.cols) * cell_h
        atlas.paste(img, (cx, cy), img)
        regions[f"{anim}_{idx:02d}"] = [cx, cy, cell_w, cell_h]

    os.makedirs(os.path.dirname(args.atlas), exist_ok=True)
    atlas.save(args.atlas)
    meta = {"atlas": os.path.basename(args.atlas), "cell": [cell_w, cell_h], "frames": regions}
    meta_path = os.path.splitext(args.atlas)[0] + ".json"
    with open(meta_path, "w", encoding="utf-8") as f:
        json.dump(meta, f, ensure_ascii=False, indent=2)
    print(f"atlas={args.atlas} size={atlas.size} frames={len(regions)} cols={args.cols}")
    print(f"meta={meta_path}")


if __name__ == "__main__":
    main()
