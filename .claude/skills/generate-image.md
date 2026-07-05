当用户说 生成图片、生成素材、AI画图、画一张、生成图 时，使用此 skill。

## 图片规格文档

生图前必须先查阅项目素材规格：**`devPlan/design/assetSpecs.md`**，尺寸、目录结构、坐标换算均以该文档为准。

## 当前阶段：早期开发

liblib国内直连，不需要使用代理。

早期开发阶段仅使用性价比模型即可。

## Liblib.art

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

### WebUI 文生图模式

⚠️ 请求体必须包含 `templateUuid` 和 `generateParams`，参数在 `generateParams` 内部。

**已知可用的模板：**
| 参数 | 值 | 说明 |
|------|-----|------|
| templateUuid | `e10adc3949ba59abbe56e057f20f883e` | 通用文生图模板（SDK 示例） |
| checkPointId | `0ea388c7eb854be3ba3c6f65aac6bfd3` | 配套底模 |

如需换模型，去 https://www.liblib.art 创建生图模板，从模板 URL 中获取 `templateUuid`，从模型页面获取 `checkPointId`。

```bash
AUTH=$(LIBLIB_SIGN "/api/generate/webui/text2img")
curl -s -X POST "https://openapi.liblibai.cloud/api/generate/webui/text2img?AccessKey=$(echo $AUTH | cut -d'&' -f1)&Signature=$(echo $AUTH | cut -d'&' -f2)&Timestamp=$(echo $AUTH | cut -d'&' -f3)&SignatureNonce=$(echo $AUTH | cut -d'&' -f4)" \
  -H "Content-Type: application/json" \
  -d '{
    "templateUuid": "e10adc3949ba59abbe56e057f20f883e",
    "generateParams": {
        "checkPointId": "0ea388c7eb854be3ba3c6f65aac6bfd3",
        "prompt": "用户提供的提示词（英文）",
        "negativePrompt": "",
        "sampler": 15,
        "steps": 20,
        "cfgScale": 7,
        "width": 512,
        "height": 512,
        "imgCount": 1,
        "seed": -1,
        "restoreFaces": 0
    }
  }'
```

**generateParams 字段说明：**

| 字段 | 类型 | 必需 | 说明 |
|------|------|------|------|
| checkPointId | string | 是 | 底模 modelVersionUUID |
| prompt | string | 是 | 正向提示词（英文） |
| negativePrompt | string | 否 | 负向提示词 |
| sampler | int | 是 | 采样方法枚举（15 = DPM++ 2M SDE Karras） |
| steps | int | 是 | 采样步数（20-30） |
| cfgScale | float | 是 | 提示词引导系数（7） |
| width | int | 是 | 图片宽度 |
| height | int | 是 | 图片高度 |
| imgCount | int | 是 | 生成张数（1-4） |
| seed | int | 否 | 随机种子，-1 表示随机 |
| restoreFaces | int | 否 | 面部修复：0=关 1=开 |
| additionalNetwork | array | 否 | LoRA 列表，最多 5 个 |
| hiResFixInfo | object | 否 | 高分辨率修复参数 |

**响应：** `data.generateUuid` 是任务 ID，用于轮询。

### 轮询结果（WebUI）

⚠️ WebUI 文生图用 `/api/generate/webui/status`，ComfyUI 用 `/api/generate/comfy/status`。

```bash
AUTH=$(LIBLIB_SIGN "/api/generate/webui/status")
curl -s -X POST "https://openapi.liblibai.cloud/api/generate/webui/status?AccessKey=$(echo $AUTH | cut -d'&' -f1)&Signature=$(echo $AUTH | cut -d'&' -f2)&Timestamp=$(echo $AUTH | cut -d'&' -f3)&SignatureNonce=$(echo $AUTH | cut -d'&' -f4)" \
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

状态 5 时 `images[].imageUrl` 包含图片地址（有效期 7 天）。

**轮询策略：间隔 30 秒，最多 5 次（共 2.5 分钟）。** 超时后不要继续等，将任务信息记录到 `devPlan/tmp/unfetchedPicture.md`，然后告知用户稍后手动查询。

### 超时未完成 → 记录到 unfetchedPicture.md

轮询 5 次后仍非终态（status 非 5/6/7）时，将以下信息追加到 **`devPlan/tmp/unfetchedPicture.md`**：

```markdown
## [待查] {提示词摘要} — {记录时间}

- **generateUuid**: `{UUID}`
- **提交时间**: `{时间}`
- **最后状态**: `{status} ({状态含义})`
- **模型**: `{templateUuid} / {checkPointId}`
- **提示词**: `{prompt}`
- **尺寸**: `{width}x{height}`
- **目标路径**: `{保存路径}`
```

### 如何手动查询未完成任务并下载

**查询状态：**
```bash
AUTH=$(LIBLIB_SIGN "/api/generate/webui/status")
curl -s -X POST "https://openapi.liblibai.cloud/api/generate/webui/status?AccessKey=$(echo $AUTH | cut -d'&' -f1)&Signature=$(echo $AUTH | cut -d'&' -f2)&Timestamp=$(echo $AUTH | cut -d'&' -f3)&SignatureNonce=$(echo $AUTH | cut -d'&' -f4)" \
  -H "Content-Type: application/json" \
  -d '{"generateUuid": "从 unfetchedPicture.md 中获取"}'
```

**若 status=5，下载图片：**
```bash
curl -s -o "{目标路径}" "{images[0].imageUrl}"
```

任务完成后从 `unfetchedPicture.md` 中删除对应记录。

### 模型选择

要使用不同模型，需在 liblib.art 网站创建对应模板：
1. 登录 https://www.liblib.art
2. 在线生图 → 选择模型和参数 → 保存为模板
3. 从模板页面 URL 获取 `templateUuid`
4. 从模型详情页获取 `checkPointId`（modelVersionUUID）

| 模型 | 单张成本 | 说明 |
|------|---------|------|
| Seedream 4.0 | ~¥0.01–0.05 | 通用高性价比，适合批量 UI/图标 |
| 星流 Star-3 Alpha | ~¥0.02–0.08 | 照片级真实感，适合写实风格单位 |
| LibDream | ~¥0.01–0.05 | 中文理解强，适合带文字的图 |

## 通用工作流

1. 用户要求生成图片时，先确认：
   - 提示词 → 如果用户给的描述太简单，帮助润色成更适合生图的英文 prompt
   - 尺寸 → 根据用途对照 `assetSpecs.md` 选择合适尺寸，如果没有合适尺寸，就建议并更新assetSpecs.md
   - 文件名 → 保存到 `assets/` 下的路径，具体根据assetSpec.md的规定
2. 生成昂贵高清图前向用户展示预估信息（模型、尺寸、保存路径），确认后开始生图。使用性价比模型时直接生图。
3. 调用 WebUI text2img API 提交任务，获取 `generateUuid`
4. 每隔 30 秒轮询 `/api/generate/webui/status`，最多 5 次（共 2.5 分钟）：
   - status=5 → 下载图片，告知用户路径和尺寸，询问是否需要调整
   - status=6/7 → 告知用户失败，检查 prompt 或参数后重试
   - 5 次轮询后仍非终态 → 将任务信息写入 `devPlan/tmp/unfetchedPicture.md`，告知用户稍后可手动查询
5. 下载完成后告知文件路径尺寸等信息，询问是否需要调整

## ComfyUI 工作流模式（高级）

如需使用 ComfyUI 工作流而非简单文生图：

1. 获取可用工作流列表：
```bash
AUTH=$(LIBLIB_SIGN "/api/generate/comfyui/app")
curl -s "https://openapi.liblibai.cloud/api/generate/comfyui/app?AccessKey=$(echo $AUTH | cut -d'&' -f1)&Signature=$(echo $AUTH | cut -d'&' -f2)&Timestamp=$(echo $AUTH | cut -d'&' -f3)&SignatureNonce=$(echo $AUTH | cut -d'&' -f4)"
```
2. 创建生图任务（需要 `workflowVersionId`）：
```bash
AUTH=$(LIBLIB_SIGN "/api/generate/comfyui/app")
curl -s -X POST "https://openapi.liblibai.cloud/api/generate/comfyui/app?AccessKey=$(echo $AUTH | cut -d'&' -f1)&Signature=$(echo $AUTH | cut -d'&' -f2)&Timestamp=$(echo $AUTH | cut -d'&' -f3)&SignatureNonce=$(echo $AUTH | cut -d'&' -f4)" \
  -H "Content-Type: application/json" \
  -d '{
    "workflowVersionId": "工作流版本ID",
    "prompt": "用户提供的提示词"
  }'
```
   - 响应中提取 `generateUuid`
3. 轮询结果（间隔 5 秒，最多等 5 分钟）：
```bash
AUTH=$(LIBLIB_SIGN "/api/generate/comfy/status")
curl -s -X POST "https://openapi.liblibai.cloud/api/generate/comfy/status?AccessKey=$(echo $AUTH | cut -d'&' -f1)&Signature=$(echo $AUTH | cut -d'&' -f2)&Timestamp=$(echo $AUTH | cut -d'&' -f3)&SignatureNonce=$(echo $AUTH | cut -d'&' -f4)" \
  -H "Content-Type: application/json" \
  -d '{"generateUuid": "GENERATE_UUID"}'
```
   - `generateStatus`: 1=等待, 2=执行中, 3=已生成, 4=审核中, 5=成功, 6=失败
   - 状态 5 时 `images[].imageUrl` 包含图片地址（有效期 7 天）
4. 下载图片到项目 `assets/` 目录
