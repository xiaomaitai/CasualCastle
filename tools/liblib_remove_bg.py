#!/usr/bin/env python3
import argparse
import base64
import hashlib
import hmac
import json
import sqlite3
import time
import urllib.request
import uuid
from datetime import datetime, timezone

ACCESS_KEY = "VDg5YgVfesEiAiM1vRd_DA"
SECRET_KEY = "CuUdJC4wlAYeNALkHn5vKHjJgbIU0GTn"
BASE_URL = "https://openapi.liblibai.cloud"
TEMPLATE_UUID = "4df2efa0f18d46dc9758803e478eb51c"
WORKFLOW_UUID = "574a938681da4e8ba2e1c24d55784edf"
DEFAULT_DB = "assets/data/config.db"


def now_iso():
    return datetime.now(timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ")


def sign(uri):
    timestamp = str(int(time.time() * 1000))
    nonce = str(uuid.uuid4()).replace("-", "")
    content = f"{uri}&{timestamp}&{nonce}"
    digest = hmac.new(SECRET_KEY.encode(), content.encode(), hashlib.sha1).digest()
    signature = base64.urlsafe_b64encode(digest).rstrip(b"=").decode()
    return f"{ACCESS_KEY}&{signature}&{timestamp}&{nonce}"


def request(uri, payload, timeout=60):
    auth = sign(uri)
    parts = auth.split("&")
    url = (
        f"{BASE_URL}{uri}?AccessKey={parts[0]}&Signature={parts[1]}"
        f"&Timestamp={parts[2]}&SignatureNonce={parts[3]}"
    )
    data = json.dumps(payload).encode()
    req = urllib.request.Request(url, data=data, headers={"Content-Type": "application/json"})
    with urllib.request.urlopen(req, timeout=timeout) as resp:
        return json.loads(resp.read().decode())


def db_connect(db_path):
    conn = sqlite3.connect(db_path)
    conn.row_factory = sqlite3.Row
    return conn


def insert_task(conn, generate_uuid):
    conn.execute(
        "INSERT INTO asset_gen_tasks "
        "(generate_uuid, status, prompt, template_uuid, liblib_status, submitted_at) "
        "VALUES (?, 'submitted', 'remove_bg', ?, 1, ?)",
        (generate_uuid, TEMPLATE_UUID, now_iso()),
    )
    conn.commit()


def update_status(conn, generate_uuid, liblib_status):
    conn.execute(
        "UPDATE asset_gen_tasks SET liblib_status = ? WHERE generate_uuid = ?",
        (liblib_status, generate_uuid),
    )
    conn.commit()


def finish_task(conn, generate_uuid, image_url, local_path, error_msg=""):
    if error_msg:
        conn.execute(
            "UPDATE asset_gen_tasks SET status = 'failed', liblib_status = 6, image_url = ?, error_msg = ?, completed_at = ? WHERE generate_uuid = ?",
            (image_url, error_msg, now_iso(), generate_uuid),
        )
    else:
        conn.execute(
            "UPDATE asset_gen_tasks SET status = 'completed', liblib_status = 5, image_url = ?, local_path = ?, completed_at = ? WHERE generate_uuid = ?",
            (image_url, local_path, now_iso(), generate_uuid),
        )
    conn.commit()


def validate_rgba(path):
    from PIL import Image

    image = Image.open(path)
    mode = image.mode
    if mode != "RGBA":
        return False, f"不是 RGBA（{mode}）"
    alpha = image.getchannel("A")
    pixels = alpha.getdata()
    total = max(1, len(pixels))
    transparent = sum(1 for value in pixels if value == 0)
    semi = sum(1 for value in pixels if 0 < value < 255)
    ratio = transparent / total * 100
    return ratio > 5, f"尺寸={image.size} 模式={mode} 透明={ratio:.1f}% 半透明={semi / total * 100:.1f}%"


def main():
    parser = argparse.ArgumentParser(description="Liblib.art ComfyUI 抠图工具")
    parser.add_argument("image_url", help="输入图片的公网 URL")
    parser.add_argument("target_path", help="输出文件路径（原地替换原文件）")
    parser.add_argument("--interval", type=int, default=15)
    parser.add_argument("--max-attempts", type=int, default=10)
    parser.add_argument("--db", default=DEFAULT_DB)
    args = parser.parse_args()

    payload = {
        "templateUuid": TEMPLATE_UUID,
        "generateParams": {
            "10": {
                "class_type": "LoadImage",
                "inputs": {"image": args.image_url},
            },
            "workflowUuid": WORKFLOW_UUID,
        },
    }
    result = request("/api/generate/comfyui/app", payload)
    if result.get("code") != 0:
        raise SystemExit(f"提交失败: {result}")
    generate_uuid = result["data"]["generateUuid"]
    conn = db_connect(args.db)
    insert_task(conn, generate_uuid)
    print(f"generateUuid={generate_uuid}", flush=True)

    last = 1
    image_url = None
    for attempt in range(1, args.max_attempts + 1):
        result = request("/api/generate/comfy/status", {"generateUuid": generate_uuid})
        data = result.get("data") or {}
        last = data.get("generateStatus", last)
        update_status(conn, generate_uuid, last)
        print(f"attempt={attempt} generateStatus={last}", flush=True)
        if last == 5:
            for image in data.get("images") or []:
                if image.get("nodeId") == 17:
                    image_url = image.get("imageUrl")
                    break
            break
        if last in (6, 7):
            break
        time.sleep(args.interval)

    if not image_url:
        print(f"失败或超时: generateStatus={last} generateUuid={generate_uuid}")
        finish_task(conn, generate_uuid, "", "", f"generateStatus={last}")
        conn.close()
        raise SystemExit(1)

    urllib.request.urlretrieve(image_url, args.target_path)
    ok, message = validate_rgba(args.target_path)
    if not ok:
        finish_task(conn, generate_uuid, image_url, args.target_path, message)
        conn.close()
        raise SystemExit(f"验证失败: {message}")
    finish_task(conn, generate_uuid, image_url, args.target_path)
    conn.close()
    print(f"[completed] {args.target_path} {message}")


if __name__ == "__main__":
    main()
