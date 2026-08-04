#!/usr/bin/env python3
import argparse
import base64
import hashlib
import hmac
import json
import os
import sqlite3
import sys
import time
import urllib.request
import uuid
from datetime import datetime, timezone

ACCESS_KEY = "VDg5YgVfesEiAiM1vRd_DA"
SECRET_KEY = "CuUdJC4wlAYeNALkHn5vKHjJgbIU0GTn"
BASE_URL = "https://openapi.liblibai.cloud"
DEFAULT_TEMPLATE = "bf085132c7134622895b783b520b39ff"
DEFAULT_CHECKPOINT = "75e0be0c93b34dd8baeec9c968013e0c"
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


def insert_task(conn, generate_uuid, spec, template, checkpoint, liblib_status=1):
    conn.execute(
        "INSERT INTO asset_gen_tasks "
        "(generate_uuid, status, prompt, negative_prompt, width, height, img_count, sampler, steps, cfg_scale, seed, template_uuid, checkpoint_id, liblib_status, submitted_at) "
        "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)",
        (
            generate_uuid,
            "submitted",
            spec.get("prompt", ""),
            spec.get("negative", ""),
            spec.get("width", 512),
            spec.get("height", 512),
            spec.get("img_count", 1),
            spec.get("sampler", 1),
            spec.get("steps", 30),
            spec.get("cfg", 4.0),
            spec.get("seed", -1),
            template,
            checkpoint,
            liblib_status,
            now_iso(),
        ),
    )
    conn.commit()
    return conn.execute("SELECT last_insert_rowid() AS id").fetchone()["id"]


def update_status(conn, generate_uuid, liblib_status):
    conn.execute(
        "UPDATE asset_gen_tasks SET liblib_status = ? WHERE generate_uuid = ?",
        (liblib_status, generate_uuid),
    )
    conn.commit()


def finish_task(conn, generate_uuid, image_url, local_path, error_msg=""):
    if error_msg:
        conn.execute(
            "UPDATE asset_gen_tasks SET status = 'failed', liblib_status = 6, error_msg = ?, completed_at = ? WHERE generate_uuid = ?",
            (error_msg, now_iso(), generate_uuid),
        )
    else:
        conn.execute(
            "UPDATE asset_gen_tasks SET status = 'completed', liblib_status = 5, image_url = ?, local_path = ?, completed_at = ? WHERE generate_uuid = ?",
            (image_url, local_path, now_iso(), generate_uuid),
        )
    conn.commit()


def cmd_submit(args):
    spec = {
        "prompt": args.prompt,
        "negative": args.negative or "",
        "width": args.width,
        "height": args.height,
        "sampler": args.sampler,
        "steps": args.steps,
        "cfg": args.cfg,
    }
    generate_params = {
        "checkPointId": args.checkpoint,
        "prompt": args.prompt,
        "negativePrompt": args.negative or "",
        "clipSkip": args.clip_skip,
        "sampler": args.sampler,
        "steps": args.steps,
        "cfgScale": args.cfg,
        "width": args.width,
        "height": args.height,
        "imgCount": 1,
        "randnSource": 0,
        "seed": -1,
    }
    payload = {"templateUuid": args.template, "generateParams": generate_params}
    result = request("/api/generate/webui/text2img", payload)
    if result.get("code") != 0:
        raise SystemExit(f"提交失败: {result}")
    generate_uuid = result["data"]["generateUuid"]
    conn = db_connect(args.db)
    db_id = insert_task(conn, generate_uuid, spec, args.template, args.checkpoint)
    conn.close()
    print(generate_uuid)
    print(f"db_id={db_id}")


def cmd_status(args):
    result = request("/api/generate/webui/status", {"generateUuid": args.generate_uuid})
    if result.get("code") != 0:
        raise SystemExit(f"查询失败: {result}")
    data = result["data"]
    print(f"generateStatus={data.get('generateStatus')}")
    for image in data.get("images") or []:
        print(image.get("imageUrl", ""))


def cmd_poll(args):
    conn = db_connect(args.db)
    last = 1
    for attempt in range(1, args.max_attempts + 1):
        result = request("/api/generate/webui/status", {"generateUuid": args.generate_uuid})
        data = result.get("data") or {}
        last = data.get("generateStatus", last)
        update_status(conn, args.generate_uuid, last)
        print(f"attempt={attempt} generateStatus={last}")
        if last == 5:
            for image in data.get("images") or []:
                print(image.get("imageUrl", ""))
            conn.close()
            return 0
        if last in (6, 7):
            conn.close()
            return 1
        time.sleep(args.interval)
    conn.close()
    return 2


def cmd_download(args):
    urllib.request.urlretrieve(args.url, args.path)
    print(args.path)


def cmd_scale(args):
    from PIL import Image

    image = Image.open(args.path)
    size = (args.size, args.size)
    resized = image.resize(size, Image.LANCZOS)
    resized.save(args.path)
    print(f"{args.path} -> {size}")


def cmd_finish(args):
    conn = db_connect(args.db)
    finish_task(conn, args.generate_uuid, args.image_url, args.local_path)
    conn.close()
    print("done")


def cmd_batch(args):
    with open(args.spec, encoding="utf-8") as fh:
        specs = json.load(fh)
    conn = db_connect(args.db)
    pending = list(specs)
    active = []

    def submit_one(spec):
        template = spec.get("template", DEFAULT_TEMPLATE)
        checkpoint = spec.get("checkpoint", DEFAULT_CHECKPOINT) or ""
        generate_params = {
            "checkPointId": checkpoint,
            "prompt": spec["prompt"],
            "negativePrompt": spec.get("negative", ""),
            "clipSkip": spec.get("clip_skip", 2),
            "sampler": spec.get("sampler", 1),
            "steps": spec.get("steps", 30),
            "cfgScale": spec.get("cfg", 4.0),
            "width": spec.get("width", 512),
            "height": spec.get("height", 512),
            "imgCount": 1,
            "randnSource": 0,
            "seed": -1,
        }
        if spec.get("additional_network"):
            generate_params["additionalNetwork"] = spec["additional_network"]
        payload = {"templateUuid": template, "generateParams": generate_params}
        for retry in range(args.submit_retries + 1):
            try:
                result = request("/api/generate/webui/text2img", payload)
            except Exception as exc:
                print(f"[submit-error] {spec['name']}: {exc} (retry={retry})", flush=True)
                time.sleep(10 * (retry + 1))
                continue
            if result.get("code") == 0:
                generate_uuid = result["data"]["generateUuid"]
                db_id = insert_task(conn, generate_uuid, spec, template, checkpoint)
                active.append(
                    {
                        "spec": spec,
                        "uuid": generate_uuid,
                        "db_id": db_id,
                        "attempts": 0,
                        "last": 1,
                    }
                )
                print(f"[submitted] {spec['name']} {generate_uuid} db_id={db_id}", flush=True)
                return True
            if result.get("code") in (429, 100054):
                print(f"[rate-limit] {spec['name']}: {result.get('msg')} (retry={retry})", flush=True)
                time.sleep(10 * (retry + 1))
                continue
            print(f"[submit-failed] {spec['name']}: {result}", flush=True)
            return False
        print(f"[submit-giveup] {spec['name']}", flush=True)
        return False

    def handle_finished(task, outcome, image_url):
        spec = task["spec"]
        generate_uuid = task["uuid"]
        if outcome == "done" and image_url:
            os.makedirs(spec["save_dir"], exist_ok=True)
            local_path = os.path.join(spec["save_dir"], f"{spec['name']}_{task['db_id']}.png")
            try:
                urllib.request.urlretrieve(image_url, local_path)
            except Exception as exc:
                finish_task(conn, generate_uuid, image_url, "", f"download failed: {exc}")
                print(f"[download-failed] {spec['name']}: {exc}", flush=True)
                return
            if spec.get("scale_to"):
                try:
                    from PIL import Image

                    image = Image.open(local_path)
                    size = (spec["scale_to"], spec["scale_to"])
                    image.resize(size, Image.LANCZOS).save(local_path)
                except Exception as exc:
                    print(f"[scale-failed] {spec['name']}: {exc}", flush=True)
            finish_task(conn, generate_uuid, image_url, local_path)
            print(f"[completed] {local_path}", flush=True)
        elif outcome == "failed":
            finish_task(conn, generate_uuid, "", "", f"generateStatus={task['last']}")
            print(f"[failed] {spec['name']} {generate_uuid}", flush=True)
        else:
            print(f"[timeout] {spec['name']} {generate_uuid}", flush=True)

    while pending or active:
        while pending and len(active) < args.max_concurrent:
            submit_one(pending.pop(0))
            time.sleep(args.submit_delay)
        if not active:
            break
        time.sleep(args.interval)
        finished = []
        for task in active:
            task["attempts"] += 1
            try:
                result = request("/api/generate/webui/status", {"generateUuid": task["uuid"]})
            except Exception as exc:
                print(f"[poll-error] {task['spec']['name']}: {exc}", flush=True)
                if task["attempts"] >= args.max_attempts:
                    finished.append((task, "timeout", None))
                continue
            data = result.get("data") or {}
            task["last"] = data.get("generateStatus", task["last"])
            update_status(conn, task["uuid"], task["last"])
            if task["last"] == 5:
                images = data.get("images") or []
                image_url = images[0].get("imageUrl") if images else None
                finished.append((task, "done", image_url))
            elif task["last"] in (6, 7):
                finished.append((task, "failed", None))
            elif task["attempts"] >= args.max_attempts:
                finished.append((task, "timeout", None))
        for task, outcome, image_url in finished:
            active.remove(task)
            handle_finished(task, outcome, image_url)
    conn.close()


def main():
    parser = argparse.ArgumentParser(description="Liblib.art WebUI 文生图工具")
    parser.add_argument("--db", default=DEFAULT_DB, help="config.db 路径")
    sub = parser.add_subparsers(dest="command", required=True)

    p_submit = sub.add_parser("submit")
    p_submit.add_argument("--prompt", required=True)
    p_submit.add_argument("--negative", default="")
    p_submit.add_argument("--width", type=int, default=512)
    p_submit.add_argument("--height", type=int, default=512)
    p_submit.add_argument("--template", default=DEFAULT_TEMPLATE)
    p_submit.add_argument("--checkpoint", default=DEFAULT_CHECKPOINT)
    p_submit.add_argument("--sampler", type=int, default=1)
    p_submit.add_argument("--steps", type=int, default=30)
    p_submit.add_argument("--cfg", type=float, default=4.0)
    p_submit.add_argument("--clip-skip", type=int, default=2)
    p_submit.set_defaults(func=cmd_submit)

    p_status = sub.add_parser("status")
    p_status.add_argument("generate_uuid")
    p_status.set_defaults(func=cmd_status)

    p_poll = sub.add_parser("poll")
    p_poll.add_argument("generate_uuid")
    p_poll.add_argument("--interval", type=int, default=30)
    p_poll.add_argument("--max-attempts", type=int, default=5)
    p_poll.set_defaults(func=cmd_poll)

    p_download = sub.add_parser("download")
    p_download.add_argument("url")
    p_download.add_argument("path")
    p_download.set_defaults(func=cmd_download)

    p_scale = sub.add_parser("scale")
    p_scale.add_argument("path")
    p_scale.add_argument("size", type=int)
    p_scale.set_defaults(func=cmd_scale)

    p_finish = sub.add_parser("finish")
    p_finish.add_argument("generate_uuid")
    p_finish.add_argument("--image-url", required=True)
    p_finish.add_argument("--local-path", required=True)
    p_finish.set_defaults(func=cmd_finish)

    p_batch = sub.add_parser("batch")
    p_batch.add_argument("spec")
    p_batch.add_argument("--interval", type=int, default=30)
    p_batch.add_argument("--submit-delay", type=float, default=5.0)
    p_batch.add_argument("--submit-retries", type=int, default=3)
    p_batch.add_argument("--max-concurrent", type=int, default=3)
    p_batch.add_argument("--max-attempts", type=int, default=5)
    p_batch.set_defaults(func=cmd_batch)

    args = parser.parse_args()
    args.func(args)


if __name__ == "__main__":
    main()
