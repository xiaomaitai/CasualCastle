当用户说 抠图、去背景、移除背景、透明图、remove background 时，使用此 skill。

## 触发时机

抠图是**用户手动触发**的操作。生图完成后不要主动询问或执行抠图。RGB PNG 可直接用于游戏，用户筛选素材后自行决定哪些需要抠图。

## 前置条件

需要输入图片的公网 URL。若原图来自 `generate-image` skill 的生图结果，URL 可从 `assets/data/config.db` 的 `asset_gen_tasks` 表中查询：

```bash
sqlite3 assets/data/config.db "SELECT image_url FROM asset_gen_tasks WHERE local_path LIKE '%文件名%' ORDER BY submitted_at DESC LIMIT 1;"
```

完整素材管线见 `devPlan/design/assetSpecs.md` 中的「AI 素材管线」章节。

## 抠图工作流

使用 Liblib.art 的「最强抠图丨导入图片一键抠图」快捷应用（底层为 ComfyUI 背景移除工作流），将图片背景变为透明，输出 RGBA PNG。**抠图结果直接原地替换原文件。**

- 快捷应用模板 UUID：`4df2efa0f18d46dc9758803e478eb51c`
- 底层工作流 UUID：`574a938681da4e8ba2e1c24d55784edf`
- 工作流配置页面：https://www.liblib.art/apis/workflow?uuid=574a938681da4e8ba2e1c24d55784edf&modelInfoPath=9c298d00c4eb4c7c92b6d8c582352e58&from=feed

## Liblib.art API

liblib国内直连，不需要使用代理。

- Base URL: `https://openapi.liblibai.cloud`
- Auth: AccessKey + 签名（HMAC-SHA1）
- 已配置凭据：
  - AccessKey: `VDg5YgVfesEiAiM1vRd_DA`
  - SecretKey: `CuUdJC4wlAYeNALkHn5vKHjJgbIU0GTn`

### 签名生成（⚠️ Windows 用 `python`，macOS/Linux 用 `python3`）

```bash
LIBLIB_SIGN() {
  local URI="$1"
  local ACCESS_KEY="VDg5YgVfesEiAiM1vRd_DA"
  local SECRET_KEY="CuUdJC4wlAYeNALkHn5vKHjJgbIU0GTn"
  local TIMESTAMP=$(python -c "import time; print(int(time.time()*1000))")
  local NONCE=$(python -c "import uuid; print(str(uuid.uuid4()).replace('-',''))")
  local SIGN_CONTENT="${URI}&${TIMESTAMP}&${NONCE}"
  local SIGNATURE=$(python -c "
import hmac, hashlib, base64
digest = hmac.new('${SECRET_KEY}'.encode(), '${SIGN_CONTENT}'.encode(), hashlib.sha1).digest()
sig = base64.urlsafe_b64encode(digest).rstrip(b'=').decode()
print(sig)
")
  echo "${ACCESS_KEY}&${SIGNATURE}&${TIMESTAMP}&${NONCE}"
}
```

### 提交抠图任务

**端点：** `POST /api/generate/comfyui/app`

```bash
AUTH=$(LIBLIB_SIGN "/api/generate/comfyui/app")
curl -s -X POST "https://openapi.liblibai.cloud/api/generate/comfyui/app?AccessKey=$(echo $AUTH | cut -d'&' -f1)&Signature=$(echo $AUTH | cut -d'&' -f2)&Timestamp=$(echo $AUTH | cut -d'&' -f3)&SignatureNonce=$(echo $AUTH | cut -d'&' -f4)" \
  -H "Content-Type: application/json" \
  -d '{
    "templateUuid": "4df2efa0f18d46dc9758803e478eb51c",
    "generateParams": {
        "10": {
            "class_type": "LoadImage",
            "inputs": {
                "image": "输入图片的公网URL"
            }
        },
        "workflowUuid": "574a938681da4e8ba2e1c24d55784edf"
    }
  }'
```

**顶层字段说明：**

| 字段 | 类型 | 必需 | 说明 |
|------|------|------|------|
| templateUuid | string | 是 | 快捷应用模板 UUID，固定为 `4df2efa0f18d46dc9758803e478eb51c` |
| generateParams | object | 是 | 工作流参数，以节点 ID 为 key |

**generateParams 字段说明：**

| 字段 | 类型 | 必需 | 说明 |
|------|------|------|------|
| `"10"` | object | 是 | 节点 ID=10 的参数（LoadImage 加载图像节点） |
| `"10".class_type` | string | 是 | 节点类型，固定 `"LoadImage"` |
| `"10".inputs.image` | string | 是 | 输入图片的公网 HTTP(S) URL |
| workflowUuid | string | 是 | 底层 ComfyUI 工作流 UUID，固定为 `574a938681da4e8ba2e1c24d55784edf` |

**响应：**

```json
{"code": 0, "data": {"generateUuid": "任务ID"}, "msg": ""}
```

### 轮询结果

**端点：** `POST /api/generate/comfy/status`

```bash
AUTH=$(LIBLIB_SIGN "/api/generate/comfy/status")
curl -s -X POST "https://openapi.liblibai.cloud/api/generate/comfy/status?AccessKey=$(echo $AUTH | cut -d'&' -f1)&Signature=$(echo $AUTH | cut -d'&' -f2)&Timestamp=$(echo $AUTH | cut -d'&' -f3)&SignatureNonce=$(echo $AUTH | cut -d'&' -f4)" \
  -H "Content-Type: application/json" \
  -d '{"generateUuid": "GENERATE_UUID"}'
```

**generateStatus 状态码：**

| 状态码 | 含义 |
|--------|------|
| 1 | 等待中 |
| 2 | 执行中 |
| 3 | 已生成 |
| 4 | 审核中 |
| 5 | 成功 |
| 6 | 失败 |
| 7 | 超时 |

状态 5 时 `images[]` 数组包含所有输出节点产出的图片。**抠图工作流有多个输出节点**，每个 `image` 对象包含：

| 字段 | 说明 |
|------|------|
| imageUrl | 图片地址（有效期 7 天） |
| nodeId | 输出该图的 ComfyUI 节点 ID |
| outputName | 输出节点名称（如 `"SaveImage"`） |
| auditStatus | 审核状态（3=通过） |

### 输出节点选择

抠图工作流产出的 4 张图中，只需保留 2 张 RGBA 透明图：

| nodeId | 模式 | 特征 | 用途 |
|--------|------|------|------|
| 16 | RGB | 无透明通道 | ❌ 跳过 |
| **17** | **RGBA** | **硬边抠图**（2% 半透明，边缘清晰） | ✅ **游戏素材首选** |
| 19 | RGB | 无透明通道 | ❌ 跳过 |
| 18 | RGBA | 软边羽化（大部分半透明，边缘柔和） | 可选（需柔和过渡时） |

**下载时优先取 nodeId=17（硬边抠图）**，这是适合游戏素材的干净切图。下载后验证 RGBA 并确认透明像素占比 > 5%。

**轮询策略：间隔 15 秒，最多 10 次（共 2.5 分钟）。** 抠图通常比生图更快。

### 任务记录（config.db）

所有抠图任务记录到 `assets/data/config.db` 的 `asset_gen_tasks` 表（与生图共用）。

**提交任务后立即 INSERT：**

```bash
NOW=$(date -u +"%Y-%m-%dT%H:%M:%SZ")
sqlite3 assets/data/config.db "
INSERT INTO asset_gen_tasks (generate_uuid, status, prompt, template_uuid, liblib_status, submitted_at)
VALUES ('$GEN_UUID', 'submitted', 'remove_bg', '4df2efa0f18d46dc9758803e478eb51c', 1, '$NOW');
"
```

**每次轮询后 UPDATE：**

```bash
sqlite3 assets/data/config.db "
UPDATE asset_gen_tasks SET liblib_status = $STATUS WHERE generate_uuid = '$GEN_UUID';
"
```

**任务成功时 UPDATE：**

```bash
NOW=$(date -u +"%Y-%m-%dT%H:%M:%SZ")
sqlite3 assets/data/config.db "
UPDATE asset_gen_tasks SET status = 'completed', liblib_status = 5, image_url = '$IMAGE_URL', local_path = '$LOCAL_PATH', completed_at = '$NOW' WHERE generate_uuid = '$GEN_UUID';
"
```

**任务失败/超时时 UPDATE：**

```bash
NOW=$(date -u +"%Y-%m-%dT%H:%M:%SZ")
sqlite3 assets/data/config.db "
UPDATE asset_gen_tasks SET status = 'failed', liblib_status = $STATUS, completed_at = '$NOW', error_msg = '$ERROR' WHERE generate_uuid = '$GEN_UUID';
"
```

### 轮询超时处理

轮询 10 次后仍非终态：

```bash
NOW=$(date -u +"%Y-%m-%dT%H:%M:%SZ")
sqlite3 assets/data/config.db "
UPDATE asset_gen_tasks SET status = 'timeout', liblib_status = $LAST_STATUS, completed_at = '$NOW' WHERE generate_uuid = '$GEN_UUID';
"
```

### 手动查询未完成任务

```bash
sqlite3 assets/data/config.db "SELECT generate_uuid FROM asset_gen_tasks WHERE status IN ('submitted', 'timeout') AND liblib_status NOT IN (5, 6, 7) AND template_uuid = '4df2efa0f18d46dc9758803e478eb51c';"
```

## 结果验证

抠图完成后必须验证输出是否为 RGBA 透明图：

```bash
python -c "
from PIL import Image
import numpy as np
img = Image.open('输出文件路径')
print(f'尺寸: {img.size}, 模式: {img.mode}')
if img.mode == 'RGBA':
    alpha = np.array(img.split()[3])
    transparent_pct = (alpha == 0).sum() / alpha.size * 100
    semi_pct = ((alpha > 0) & (alpha < 255)).sum() / alpha.size * 100
    print(f'透明像素: {transparent_pct:.1f}%')
    print(f'半透明像素: {semi_pct:.1f}%')
    if transparent_pct < 1:
        print('⚠️ 警告：透明像素占比过低，可能抠图失败')
else:
    print('❌ 错误：输出不是 RGBA 格式，没有透明通道！')
"
```

如果输出是 RGB 而非 RGBA，说明下载了错误的输出节点图片——检查 `nodeId` 是否对应透明图输出节点。

## 通用工作流

1. 用户要求抠图时，确认：
   - 输入图片 → 从 `asset_gen_tasks` 表查原图的 Liblib.art URL（或用户提供的公网 URL）
   - 目标文件 → **原文件路径**（抠图结果直接替换原文件）
2. 调用 `/api/generate/comfyui/app` 提交抠图任务，获取 `generateUuid`
3. **立即 INSERT 记录到 `asset_gen_tasks` 表**
4. 每隔 15 秒轮询 `/api/generate/comfy/status`，最多 10 次：
   - status=5 → 下载 **nodeId=17**（硬边抠图 RGBA）的图片，**原地覆盖目标文件**，UPDATE 为 `completed`，告知用户
   - status=6/7 → UPDATE 为 `failed`，告知用户失败
   - 10 次后仍非终态 → UPDATE 为 `timeout`
5. 验证 nodeId=17 的输出为 RGBA 且透明像素占比 > 5%
6. 告知用户已完成替换，透明像素占比

## 注意事项

- 提交端点：`/api/generate/comfyui/app`（不是 `/api/generate/webui/text2img`）
- 轮询端点：`/api/generate/comfy/status`（不是 `/api/generate/webui/status`）
- 顶层使用 `templateUuid`（快捷应用 ID），`workflowUuid` 放在 `generateParams` 内部
- 节点参数以节点 ID 字符串为 key，内含 `class_type` 和 `inputs`
- 输入图片必须是公网 HTTP(S) URL。如果图片在本地，需先上传到图床或通过 Liblib.art 的其他接口获取公网 URL
- 抠图工作流输出多张图片（对应不同 SaveImage 节点），需选择 RGBA 透明图作为最终结果
- 费用约 10 积分/次
