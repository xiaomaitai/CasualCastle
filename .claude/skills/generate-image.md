当用户说 生成图片、生成素材、AI画图、画一张、生成图 时，使用此 skill。

## 图片规格文档

生图前必须先查阅项目素材规格：**`devPlan/design/assetSpecs.md`**，尺寸、目录结构、坐标换算均以该文档为准。

## 路由规则

根据用户描述的图片用途，选择调用哪个 API：

| 用途 | API | 模型 |
|------|-----|------|
| 高清大屏素材、主视觉图、艺术图、海报、概念图 | **Leonardo AI** (旗舰) | FLUX.2 Pro 或 Phoenix |
| 小图标、纹理、普通单位图、UI 元素、批量素材 | **Liblib.art** (性价比) | Star-3 Alpha 或 LibDream |

如果用户没有明确说明用途，询问用户这张图是做什么用的，再决定用哪个 API。

## 模型成本对比

Leonardo AI 使用 **token 制**（API Basic $9/月 含 3,500 token），Liblib.art 使用 **积分制**（免费 300 点/天，VIP ¥35/月 含 15,000 点）。以下为单张参考成本：

| 模型 | 单张估算 | 说明 |
|------|---------|------|
| **FLUX.2 Pro** (Leonardo) | ~$0.03–0.05 | 最高画质，token 消耗不透明（约 10–16 token），适合主视觉/海报 |
| **Phoenix** (Leonardo) | ~$0.01–0.03 | Leonardo 自研旗舰，综合品质好，适合概念图 |
| **Lightning XL** (Leonardo) | ~$0.01–0.02 | 快速出图，Alchemy V2 下 10 token/张，性价比最高 |
| **Seedream 4.0** (Liblib) | ~¥0.01–0.05 | 通用高性价比，适合批量 UI/图标 |
| **Star-3 Alpha** (Liblib) | ~¥0.02–0.08 | 照片级真实感，适合写实风格单位 |
| **LibDream** (Liblib) | ~¥0.01–0.05 | 中文理解强，适合带文字图 |
| **F.1 Kontext** (Liblib) | ¥0.29/张 (Pro) | 明确定价，适合图像编辑/微调 |

> Leonardo 新用户有 $5 免费额度（永不过期）；Liblib.art 新用户有 500 试用积分（7 天有效）。

## Leonardo AI（旗舰 - 高品质大图）

- Base URL: `https://cloud.leonardo.ai/api/rest/v1`
- Auth: `Authorization: Bearer $LEONARDO_API_KEY`（API Key 从用户处获取，请用户设置环境变量 `LEONARDO_API_KEY`）
- 环境变量未设置时，提示用户去 https://leonardo.ai/api 获取 API Key，然后设置 `LEONARDO_API_KEY`

### 生图流程

1. 与用户确认：提示词、尺寸（默认 1536×1024 横版）、张数（默认 1）
2. 调用价格预估（可选）：
```bash
curl -s -X POST "https://cloud.leonardo.ai/api/rest/v1/pricing-calculator" \
  -H "Authorization: Bearer $LEONARDO_API_KEY" \
  -H "Content-Type: application/json" \
  -d '{"prompt":"...", "modelId":"...", "width":1536, "height":1024, "num_images":1}'
```
3. 创建生图任务：
```bash
curl -s -X POST "https://cloud.leonardo.ai/api/rest/v1/generations" \
  -H "Authorization: Bearer $LEONARDO_API_KEY" \
  -H "Content-Type: application/json" \
  -d '{
    "prompt": "用户提供的提示词",
    "negative_prompt": "负面提示词（可选）",
    "modelId": "b24e16ff-06e3-43eb-8d33-4416c2d75876",
    "width": 1536,
    "height": 1024,
    "num_images": 1,
    "guidance_scale": 7,
    "enhancePrompt": true,
    "alchemy": true
  }'
```
   - 响应中提取 `sdGenerationJob.generationId`
4. 轮询结果（间隔 3 秒，最多等 3 分钟）：
```bash
curl -s "https://cloud.leonardo.ai/api/rest/v1/generations/$GENERATION_ID" \
  -H "Authorization: Bearer $LEONARDO_API_KEY"
```
   - 当 `generations_by_pk.generated_images[].url` 有值时，生图完成
5. 下载图片到项目 `assets/` 目录：
```bash
curl -sL "IMAGE_URL" -o "assets/用户指定文件名.png"
```

### 旗舰模型 ID

| 模型 | ID | 单张成本 | 适用场景 |
|------|-----|---------|---------|
| FLUX.2 Pro | `fullFlux2Pro` | ~$0.03–0.05 | 最高画质，适合主视觉、海报、概念图 |
| Phoenix | `b24e16ff-06e3-43eb-8d33-4416c2d75876` | ~$0.01–0.03 | Leonardo 自研旗舰，综合品质好 |
| Leonardo Lightning XL | `d69c8273-6b17-4a30-9b12-21b8e79b2c85` | ~$0.01–0.02 | 快速出图，性价比最高 |

### 可用尺寸

宽度和高度必须是 8 的倍数，范围 32-1536：
- 横版 16:9 → 1536×864
- 横版 3:2 → 1536×1024
- 方形 → 1024×1024
- 竖版 2:3 → 1024×1536

## Liblib.art（性价比 - 小图批量）

- Base URL: `https://openapi.liblibai.cloud`
- Auth: AccessKey + 签名（HMAC-SHA1）
- 环境变量：`LIBLIB_ACCESS_KEY` 和 `LIBLIB_SECRET_KEY`
- 未设置时，提示用户去 https://openapi.liblibai.cloud 获取密钥对

### 签名生成

每次请求前，用以下 bash 函数生成签名：

```bash
LIBLIB_SIGN() {
  local URI="$1"
  local ACCESS_KEY="${LIBLIB_ACCESS_KEY}"
  local SECRET_KEY="${LIBLIB_SECRET_KEY}"
  local TIMESTAMP=$(python3 -c "import time; print(int(time.time()*1000))")
  local NONCE=$(python3 -c "import uuid; print(str(uuid.uuid4()).replace('-',''))")
  local SIGN_CONTENT="${URI}&${TIMESTAMP}&${NONCE}"
  local SIGNATURE=$(python3 -c "
import hmac, hashlib, base64
digest = hmac.new('${SECRET_KEY}'.encode(), '${SIGN_CONTENT}'.encode(), hashlib.sha1).digest()
sig = base64.urlsafe_b64encode(digest).rstrip(b'=').decode()
print(sig)
")
  echo "${ACCESS_KEY}&${SIGNATURE}&${TIMESTAMP}&${NONCE}"
}
```

### 生图流程（ComfyUI 工作流模式）

1. 与用户确认：提示词、工作流、尺寸、张数
2. 获取可用工作流列表：
```bash
AUTH=$(LIBLIB_SIGN "/api/generate/comfyui/app")
curl -s "https://openapi.liblibai.cloud/api/generate/comfyui/app?AccessKey=$(echo $AUTH | cut -d'&' -f1)&Signature=$(echo $AUTH | cut -d'&' -f2)&Timestamp=$(echo $AUTH | cut -d'&' -f3)&SignatureNonce=$(echo $AUTH | cut -d'&' -f4)"
```
3. 创建生图任务（需要用户提供 `workflowVersionId`）：
```bash
AUTH=$(LIBLIB_SIGN "/api/generate/comfyui/app")
curl -s -X POST "https://openapi.liblibai.cloud/api/generate/comfyui/app?AccessKey=$(echo $AUTH | cut -d'&' -f1)&Signature=$(echo $AUTH | cut -d'&' -f2)&Timestamp=$(echo $AUTH | cut -d'&' -f3)&SignatureNonce=$(echo $AUTH | cut -d'&' -f4)" \
  -H "Content-Type: application/json" \
  -d '{
    "workflowVersionId": "用户选择的工作流版本ID",
    "prompt": "用户提供的提示词"
  }'
```
   - 响应中提取 `generateUuid`
4. 轮询结果（间隔 2 秒，最多等 5 分钟）：
```bash
AUTH=$(LIBLIB_SIGN "/api/generate/comfy/status")
curl -s -X POST "https://openapi.liblibai.cloud/api/generate/comfy/status?AccessKey=$(echo $AUTH | cut -d'&' -f1)&Signature=$(echo $AUTH | cut -d'&' -f2)&Timestamp=$(echo $AUTH | cut -d'&' -f3)&SignatureNonce=$(echo $AUTH | cut -d'&' -f4)" \
  -H "Content-Type: application/json" \
  -d '{"generateUuid": "GENERATE_UUID"}'
```
   - `generateStatus`: 1=等待, 2=执行中, 3=已生成, 4=审核中, 5=成功, 6=失败
   - 状态 5 时 `images[].imageUrl` 包含图片地址（有效期 7 天）
5. 下载图片到项目 `assets/` 目录

### WebUI 文生图模式（更简单的接口）

如果不使用 ComfyUI 工作流，可以用更简单的 text2img 接口：

```bash
AUTH=$(LIBLIB_SIGN "/api/generate/webui/text2img")
curl -s -X POST "https://openapi.liblibai.cloud/api/generate/webui/text2img?AccessKey=$(echo $AUTH | cut -d'&' -f1)&Signature=$(echo $AUTH | cut -d'&' -f2)&Timestamp=$(echo $AUTH | cut -d'&' -f3)&SignatureNonce=$(echo $AUTH | cut -d'&' -f4)" \
  -H "Content-Type: application/json" \
  -d '{
    "prompt": "用户提供的提示词",
    "model_name": "Seedream 4.0",
    "width": 512,
    "height": 512,
    "steps": 30,
    "cfg_scale": 7
  }'
```

### 性价比模型

| 模型 | 单张成本 | 说明 |
|------|---------|------|
| Seedream 4.0 | ~¥0.01–0.05 | 通用高性价比 |
| 星流 Star-3 Alpha | ~¥0.02–0.08 | 照片级真实感 |
| LibDream | ~¥0.01–0.05 | 中文理解强，适合带文字的图 |
| F.1 Kontext | ¥0.29/张 (Pro) / ¥0.58/张 (Max) | 高级图像编辑，明确定价 |

## 通用工作流

1. 用户要求生成图片时，先确认：
   - 图片用途 → 决定用哪个 API
   - 提示词 → 如果用户给的描述太简单，帮助润色成更适合生图的英文 prompt
   - 尺寸 → 根据用途建议合适尺寸
   - 文件名 → 保存到 `assets/` 下的文件名
2. 向用户展示预估信息（API、模型、尺寸），确认后开始生图
3. 轮询等待结果，实时告知进度
4. 下载完成后告知文件路径，询问是否需要调整

## API Key 配置

使用前请设置以下环境变量：

```sh
# Leonardo AI（must-have - 高清大图）
export LEONARDO_API_KEY="你的API Key"

# Liblib.art（可选 - 小图批量）
export LIBLIB_ACCESS_KEY="你的AccessKey"
export LIBLIB_SECRET_KEY="你的SecretKey"
```

## 网络代理

如果有网络问题，在执行 curl 前设置代理：

```sh
export http_proxy=http://127.0.0.1:10809
export https_proxy=http://127.0.0.1:10809
```
