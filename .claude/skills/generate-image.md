当用户说 生成图片、生成素材、AI画图、画一张、生成图 时，使用此 skill。

## 图片规格文档

生图前必须先查阅项目素材规格：**`devPlan/design/assetSpecs.md`**，尺寸、目录结构、坐标换算均以该文档为准。

## 分辨率规则

⚠️ **生成分辨率不得低于 512×512。** 低于此分辨率的 latent 空间太小（32×32），模型无法生成有意义内容，会产出色块乱码。

需要小于 512×512 的素材（如 256×256 的 grass_clump）时，**生成 512×512 或更大，然后缩放到目标尺寸**。缩放用 ImageMagick / Python PIL 均可。

## 当前阶段：早期开发

liblib国内直连，不需要使用代理。

早期开发阶段仅使用性价比模型即可。

## Liblib.art

- Base URL: `https://openapi.liblibai.cloud`（纯 API 网关，无文档页面；API 文档见飞书 Wiki）
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

⚠️ 请求体必须包含 `templateUuid` 和 `generateParams`。

**已知可用的模板：**

| 模型名称                 | LoRAId                           | 模型       | 模型描述                     |
| ------------------------ | -------------------------------- | ---------- | ---------------------------- |
| 简单线条-精致可爱二次元  | 491c449be47b41299bf0e9bae3e5cdba | F.1        | 底模，适合简洁线条二次元风格 |
| 平面游戏图标模型         | 39bfe441fd8641e78dbb57069b215fb3 | Qwen-Image | 适合平面化游戏图标           |
| 扁平卡通游戏图标         | 265c08134df245bcba2f6968701bdde8 | Qwen-Image | 适合扁平卡通风格图标         |
| 萌系Q版-可爱的粗线条画风 | b8262976e7d84e51b680053f500a86dd | F.1        | 适合Q版萌系粗线条角色/道具   |
| 粗描边卡通游戏道具设计   | a6939b5467de47a980de12aac988a557 | F.1        | 适合粗描边卡通道具/图标      |

| **适用方向**  | **模板名称**                                                 | templateUuid                     | **备注**                                                     |
| ------------- | ------------------------------------------------------------ | -------------------------------- | ------------------------------------------------------------ |
| F.1文生图     | [F.1文生图 - 自定义完整参数](https://liblibai.feishu.cn/wiki/UAMVw67NcifQHukf8fpccgS5n6d#share-M89EdQ0ucoAR6HxyQyecrF98noh) | 6f7c4652458d4802969f8d089cf5b91f | Checkpoint默认为官方模型可用模型范围：基础算法F.1支持additional network |
| F.1图生图     | [F.1图生图 - 自定义完整参数](https://liblibai.feishu.cn/wiki/UAMVw67NcifQHukf8fpccgS5n6d#share-MbQ6dEVDvortZPxaPvOcBQQ9nAh) | 63b72710c9574457ba303d9d9b8df8bd | Checkpoint默认为官方模型可用模型范围：基础算法F.1支持additional network |
| 1.5和XL文生图 | [1.5和XL文生图 - 自定义完整参数](https://liblibai.feishu.cn/wiki/UAMVw67NcifQHukf8fpccgS5n6d#share-T6mBdy2NZo28r7xTXETcDxTEnAg) | e10adc3949ba59abbe56e057f20f883e | 可用模型范围：基础算法1.5，基础算法XL支持additional network，高分辨率修复和controlnet可通过自由拼接参数实现各类的文生图诉求 |
| 1.5和XL图生图 | [1.5和XL图生图 - 自定义完整参数](https://liblibai.feishu.cn/wiki/UAMVw67NcifQHukf8fpccgS5n6d#share-OsUddwS7zoIxNvxPM8EcGpUXnkc) | 9c7d531dc75f476aa833b3d452b8f7ad | 可用模型范围：基础算法1.5，基础算法XL支持additional network和controlnet可通过自由拼接参数实现各类的图生图和蒙版重绘诉求 |
| 局部重绘      | [Controlnet局部重绘](https://liblibai.feishu.cn/wiki/UAMVw67NcifQHukf8fpccgS5n6d#share-P2seddEU6opN2hxPyZKcfFpPneb) | b689de89e8c9407a874acd415b3aa126 | 提取自文生图完整参数支持additional network和controlnet不支持高分辨率修复（hiresfix） |
| 局部重绘      | [图生图局部重绘](https://liblibai.feishu.cn/wiki/UAMVw67NcifQHukf8fpccgS5n6d#share-MMB4dqZ33odraaxH8m6ckLDhn2f) | 74509e1b072a4c45a7f1843a963c8462 | 提取自图生图完整参数支持additionalNetwork不支持Controlnet    |
| 人物换脸      | [InstantID人像换脸](https://liblibai.feishu.cn/wiki/UAMVw67NcifQHukf8fpccgS5n6d#share-DNLudcOIGoAfpAxmo5wc8FTlnjf) | 7d888009f81d4252a7c458c874cd017f | 仅用于人像换脸注意人像参考图中的人物面部特征务必清晰         |
| Qwen-Image文生图 | [Qwen-Image文生图 - 自定义完整参数](https://liblibai.feishu.cn/wiki/UAMVw67NcifQHukf8fpccgS5n6d) | bf085132c7134622895b783b520b39ff | 可用模型范围：Qwen-Image。⚠️ **checkPointId 必传**，支持 additionalNetwork、controlNet、clipSkip、randnSource |
| Qwen-Image图生图 | Qwen-Image图生图 - 自定义完整参数 | — | 如需图生图请去 liblib.art 创建模板获取 templateUuid |



如需换模型，去 https://www.liblib.art 创建生图模板，从模板 URL 中获取 `templateUuid`，从模型页面获取 `checkPointId`。

```bash
AUTH=$(LIBLIB_SIGN "/api/generate/webui/text2img")
curl -s -X POST "https://openapi.liblibai.cloud/api/generate/webui/text2img?AccessKey=$(echo $AUTH | cut -d'&' -f1)&Signature=$(echo $AUTH | cut -d'&' -f2)&Timestamp=$(echo $AUTH | cut -d'&' -f3)&SignatureNonce=$(echo $AUTH | cut -d'&' -f4)" \
  -H "Content-Type: application/json" \
  -d '{
    "templateUuid": "test",
    "generateParams": {
        "checkPointId": "test",
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

**Qwen-Image 文生图示例（checkPointId 必传，cfgScale 默认 4.0）：**

```bash
AUTH=$(LIBLIB_SIGN "/api/generate/webui/text2img")
curl -s -X POST "https://openapi.liblibai.cloud/api/generate/webui/text2img?AccessKey=$(echo $AUTH | cut -d'&' -f1)&Signature=$(echo $AUTH | cut -d'&' -f2)&Timestamp=$(echo $AUTH | cut -d'&' -f3)&SignatureNonce=$(echo $AUTH | cut -d'&' -f4)" \
  -H "Content-Type: application/json" \
  -d '{
    "templateUuid": "bf085132c7134622895b783b520b39ff",
    "generateParams": {
        "checkPointId": "75e0be0c93b34dd8baeec9c968013e0c",
        "prompt": "用户提供的提示词（英文）",
        "negativePrompt": "",
        "clipSkip": 2,
        "sampler": 1,
        "steps": 30,
        "cfgScale": 4.0,
        "width": 768,
        "height": 1024,
        "imgCount": 1,
        "randnSource": 0,
        "seed": -1,
        "additionalNetwork": [],
        "controlNet": []
    }
  }'
```

**顶层字段说明：**

| 字段 | 类型 | 必需 | 说明 |
|------|------|------|------|
| templateUuid | string | 是 | 生图模板 UUID |

**generateParams 字段说明：**

| 字段 | 类型 | 必需 | 说明 |
|------|------|------|------|
| checkPointId | string | 否(模板) | 底模 modelVersionUUID。F.1 不使用；**Qwen-Image 必传**（默认 `75e0be0c93b34dd8baeec9c968013e0c`） |
| prompt | string | 是 | 正向提示词（英文） |
| negativePrompt | string | 否 | 负向提示词 |
| sampler | int | 是 | 采样方法枚举（15 = DPM++ 2M SDE Karras） |
| steps | int | 是 | 采样步数（20-30） |
| cfgScale | float | 是 | 提示词引导系数（7） |
| width | int | 是 | 图片宽度（**≥512**，小于 512 先生成大图再缩放） |
| height | int | 是 | 图片高度（**≥512**，小于 512 先生成大图再缩放） |
| imgCount | int | 是 | 生成张数（1-4） |
| seed | int | 否 | 随机种子，-1 表示随机 |
| restoreFaces | int | 否 | 面部修复：0=关 1=开 |
| additionalNetwork | array | 否 | LoRA 列表，最多 5 个。每个元素为对象，字段见下方 |
| hiResFixInfo | object | 否 | 高分辨率修复参数 |
| clipSkip | int | 否 | **Qwen-Image 专属。** CLIP 跳过层数（默认 2） |
| randnSource | int | 否 | **Qwen-Image 专属。** 噪声源：0=GPU，1=CPU |

**additionalNetwork 数组元素字段：**

| 字段 | 类型 | 必需 | 说明 |
|------|------|------|------|
| modelId | string | 是 | LoRA 模型版本 UUID |
| weight | float | 否 | LoRA 权重（-4.00 ~ +4.00，默认0.8） |

**additionalNetwork 示例：**
```json
"additionalNetwork": [
    { "modelId": "31360f2f031b4ff6b589412a52713fcf", "weight": 0.3 },
    { "modelId": "365e700254dd40bbb90d5e78c152ec7f", "weight": 0.6 }
]
```

**响应：** `data.generateUuid` 是任务 ID，用于轮询。提交任务后立即在 config.db 中 INSERT 一条记录（见下方"任务记录"）。

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

状态 5 时 `images[].imageUrl` 包含图片地址（有效期 7 天）。每次轮询后 UPDATE config.db 中的 `liblib_status`。

**轮询策略：间隔 30 秒，最多 5 次（共 2.5 分钟）。** 超时后不要继续等，将 config.db 中 status 更新为 `timeout`，告知用户稍后手动查询。

### 任务记录（config.db）

所有生图任务必须记录到 `assets/data/config.db` 的 `asset_gen_tasks` 表。

**提交任务后立即 INSERT：**

```bash
NOW=$(date -u +"%Y-%m-%dT%H:%M:%SZ")
sqlite3 assets/data/config.db "
INSERT INTO asset_gen_tasks (generate_uuid, status, prompt, negative_prompt, width, height, img_count, sampler, steps, cfg_scale, seed, template_uuid, checkpoint_id, liblib_status, submitted_at)
VALUES ('$GEN_UUID', 'submitted', '${PROMPT//\'/\'\'}', '${NEG_PROMPT//\'/\'\'}', $WIDTH, $HEIGHT, $IMG_COUNT, $SAMPLER, $STEPS, $CFG_SCALE, $SEED, '$TEMPLATE_UUID', '$CHECKPOINT_ID', 1, '$NOW');
"
```

**每次轮询后 UPDATE 状态：**

```bash
sqlite3 assets/data/config.db "
UPDATE asset_gen_tasks SET liblib_status = $STATUS WHERE generate_uuid = '$GEN_UUID';
"
```

**任务完成（成功）时 UPDATE：**

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

### 超时未完成 → 标记 timeout

轮询 5 次后仍非终态（status 非 5/6/7）时，更新数据库状态为 `timeout`：

```bash
NOW=$(date -u +"%Y-%m-%dT%H:%M:%SZ")
sqlite3 assets/data/config.db "
UPDATE asset_gen_tasks SET status = 'timeout', liblib_status = $LAST_STATUS, completed_at = '$NOW' WHERE generate_uuid = '$GEN_UUID';
"
```

告知用户任务超时，generate_uuid 已记录在 db 中，稍后可手动查询。

### 如何手动查询未完成任务

```bash
sqlite3 assets/data/config.db "SELECT generate_uuid FROM asset_gen_tasks WHERE status IN ('submitted', 'timeout') AND liblib_status NOT IN (5, 6, 7);"
```

获取 `generate_uuid` 后查询状态并下载：

**查询状态：**
```bash
AUTH=$(LIBLIB_SIGN "/api/generate/webui/status")
curl -s -X POST "https://openapi.liblibai.cloud/api/generate/webui/status?AccessKey=$(echo $AUTH | cut -d'&' -f1)&Signature=$(echo $AUTH | cut -d'&' -f2)&Timestamp=$(echo $AUTH | cut -d'&' -f3)&SignatureNonce=$(echo $AUTH | cut -d'&' -f4)" \
  -H "Content-Type: application/json" \
  -d '{"generateUuid": "从 db 中获取"}'
```

**若 status=5，下载图片并更新记录：**
```bash
curl -s -o "{目标路径}" "{images[0].imageUrl}"
NOW=$(date -u +"%Y-%m-%dT%H:%M:%SZ")
sqlite3 assets/data/config.db "
UPDATE asset_gen_tasks SET status = 'completed', liblib_status = 5, image_url = '$IMG_URL', local_path = '$LOCAL_PATH', completed_at = '$NOW' WHERE generate_uuid = '$GEN_UUID';
"
```

### 模型选择

**Qwen-Image 默认 checkPointId：** `75e0be0c93b34dd8baeec9c968013e0c`（Qwen-Image 官方底模，⚠️ 必传，不可省略）

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

### 查询模型版本信息

通过 `versionUuid`（即 checkPointId）查询模型名称、底模、是否可商用等：

```bash
AUTH=$(LIBLIB_SIGN "/api/model/version/get")
curl -s -X POST "https://openapi.liblibai.cloud/api/model/version/get?AccessKey=$(echo $AUTH | cut -d'&' -f1)&Signature=$(echo $AUTH | cut -d'&' -f2)&Timestamp=$(echo $AUTH | cut -d'&' -f3)&SignatureNonce=$(echo $AUTH | cut -d'&' -f4)" \
  -H "Content-Type: application/json" \
  -d '{"versionUuid": "模型版本UUID"}'
```

**响应字段：**

| 字段 | 说明 |
|------|------|
| versionName | 版本名称 |
| modelName | 模型名称 |
| baseAlgoName | 底模算法（F.1 / Qwen-Image 等） |
| commercialUse | 1=可商用，0=不可商用 |
| modelUrl | 模型详情页链接 |

## 提示词风格规范

⚠️ **所有生图提示词必须强调以下三个要素：**

1. **平面2D（flat 2D）** — 禁止立体感、阴影、渐变、3D 效果
2. **萌系（cute/moe style）** — Q版可爱风格，角色圆润、道具可爱化
3. **粗描边（thick outlines / bold lineart）** — 黑色或深色粗轮廓线，类似矢量插画

生成 prompt 时，始终在描述末尾或合适位置加入：`flat 2D, cute moe style, thick bold outlines, no shading, no gradient, simple flat colors`。如果用户给的描述太简单，帮助润色成英文 prompt 时必须融入这三个要素。

## 通用工作流

1. 用户要求生成图片时，先确认：
   - 提示词 → 如果用户给的描述太简单，帮助润色成更适合生图的英文 prompt。**必须融入平面2D、萌系、粗描边风格要素。**
   - 模型 → 从表格中选择checkpointId，也就是模型id。
   - 尺寸 → 根据用途对照 `assetSpecs.md` 选择合适尺寸。**若目标尺寸 < 512，先生成 ≥512 的图再缩放。** 如果没有合适尺寸，就建议并更新 assetSpecs.md
   - 文件名 → 保存到 `assets/` 下的路径，具体根据 assetSpec.md 的规定
2. 生成昂贵高清图前向用户展示预估信息（模型、尺寸、保存路径），确认后开始生图。使用性价比模型时直接生图。
3. 调用 WebUI text2img API 提交任务，获取 `generateUuid`
4. **立即 INSERT 一条记录到 `asset_gen_tasks` 表**
5. 每隔 30 秒轮询 `/api/generate/webui/status`，最多 5 次（共 2.5 分钟）：
   - **每次轮询后 UPDATE `liblib_status`**
   - status=5 → 下载图片，UPDATE 为 `completed`，告知用户路径和尺寸，询问是否需要调整
   - status=6/7 → UPDATE 为 `failed`，告知用户失败，检查 prompt 或参数后重试
   - 5 次轮询后仍非终态 → UPDATE 为 `timeout`，告知用户 generate_uuid 已记录在 db，稍后可手动查询
6. 下载完成后告知文件路径尺寸等信息，询问是否需要调整
7. **如果需要缩放到目标尺寸，下载后立即缩放并保存**
8. **不要主动询问或执行抠图。** RGB PNG 可直接用于游戏，用户会在筛选素材后手动要求抠图

## 后续步骤

生图产出的 RGB PNG 可直接用于游戏。用户筛选素材后如需透明背景，使用 **`remove-bg` skill**（`.claude/skills/remove-bg.md`）抠图并原地替换。详见 `devPlan/design/assetSpecs.md` 中的「AI 素材管线」章节。
