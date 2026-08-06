#!/usr/bin/env python3
"""建筑游戏纹理后处理：裁掉透明边距并按长边等比缩放，产出 _game.png。"""
import argparse
from pathlib import Path

from PIL import Image

ALPHA_THRESHOLD = 8


def trim_alpha_border(image):
    alpha = image.getchannel("A")
    mask = alpha.point(lambda value: 255 if value > ALPHA_THRESHOLD else 0)
    box = mask.getbbox()
    if not box:
        return image
    return image.crop(box)


def resize_long_edge(image, long_edge):
    width, height = image.size
    longest = max(width, height)
    if longest <= long_edge:
        return image
    scale = long_edge / longest
    new_size = (max(1, round(width * scale)), max(1, round(height * scale)))
    return image.resize(new_size, Image.LANCZOS)


def derive_output(input_path):
    if input_path.name.endswith("_cut.png"):
        name = input_path.name.replace("_cut.png", "_game.png")
        return input_path.with_name(name)
    return input_path.with_name(input_path.stem + "_game.png")


def main():
    parser = argparse.ArgumentParser(description="裁边 + 等比缩放，生成游戏纹理")
    parser.add_argument("input", help="抠图 RGBA PNG（_cut.png）")
    parser.add_argument("--long-edge", type=int, default=512, help="目标长边像素（2x2 建筑 512，1x1 建筑 256）")
    parser.add_argument("--out", default="", help="输出路径（缺省按 _game.png 后缀推导）")
    args = parser.parse_args()

    input_path = Path(args.input)
    image = Image.open(input_path).convert("RGBA")
    image = trim_alpha_border(image)
    image = resize_long_edge(image, args.long_edge)
    out_path = Path(args.out) if args.out else derive_output(input_path)
    image.save(out_path)
    print(f"[done] {out_path} 尺寸={image.size}")


if __name__ == "__main__":
    main()