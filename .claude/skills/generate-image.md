当用户说 生成图片、生成素材、AI画图、画一张、生成图 时，使用此 skill。

## 图片规格文档

生图前必须先查阅项目素材规格：**`devPlan/design/assetSpecs.md`**，尺寸、目录结构、坐标换算均以该文档为准。

## 当前阶段：早期开发 → 仅使用 Liblib.art

项目处于早期开发阶段，所有美术素材均为临时占位图，**只使用 Liblib.art**，无需 Leonardo AI。后续进入正式美术阶段再切换。

## Liblib.art

- Base URL: `https://openapi.liblibai.cloud`
- Auth: AccessKey + 签名（HMAC-SHA1）
- 已配置凭据：
  - AccessKey: `VDg5YgVfesEiAiM1vRd_DA`
  - SecretKey: `CuUdJC4wlAYeNALkHn5vKHjJgbIU0GTn`

### 签名生成

每次请求前，用以下 bash 函数生成签名：

```bash
LIBLIB_SIGN() {
  local URI="$1"
  local ACCESS_KEY="VDg5YgVfesEiAiM1vRd_DA"
  local SECRET_KEY="CuUdJC4wlAYeNALkHn5vKHjJgbIU0GTn"
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

### WebUI 文生图模式

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

### 模型选择

| 模型 | 单张成本 | 说明 |
|------|---------|------|
| Seedream 4.0 | ~¥0.01–0.05 | 通用高性价比，适合批量 UI/图标 |
| 星流 Star-3 Alpha | ~¥0.02–0.08 | 照片级真实感，适合写实风格单位 |
| LibDream | ~¥0.01–0.05 | 中文理解强，适合带文字的图 |

### 可用尺寸

常用尺寸（以 `assetSpecs.md` 为准）：

| 用途 | 参考尺寸 |
|------|---------|
| 建筑卡牌图 | 512×512 |
| 士兵/单位图 | 512×512 |
| 草地纹理（平铺） | 512×512 |
| 装饰草精灵 | 256×256 |
| 山/云背景 | 1920×540 |
| UI 图标 | 128×128 |

## 通用工作流

1. 用户要求生成图片时，先确认：
   - 提示词 → 如果用户给的描述太简单，帮助润色成更适合生图的英文 prompt
   - 尺寸 → 根据用途对照 `assetSpecs.md` 建议合适尺寸
   - 文件名 → 保存到 `assets/` 下的路径
2. 向用户展示预估信息（模型、尺寸、保存路径），确认后开始生图
3. 轮询等待结果，实时告知进度
4. 下载完成后告知文件路径，询问是否需要调整

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
3. 轮询结果（间隔 2 秒，最多等 5 分钟）：
```bash
AUTH=$(LIBLIB_SIGN "/api/generate/comfy/status")
curl -s -X POST "https://openapi.liblibai.cloud/api/generate/comfy/status?AccessKey=$(echo $AUTH | cut -d'&' -f1)&Signature=$(echo $AUTH | cut -d'&' -f2)&Timestamp=$(echo $AUTH | cut -d'&' -f3)&SignatureNonce=$(echo $AUTH | cut -d'&' -f4)" \
  -H "Content-Type: application/json" \
  -d '{"generateUuid": "GENERATE_UUID"}'
```
   - `generateStatus`: 1=等待, 2=执行中, 3=已生成, 4=审核中, 5=成功, 6=失败
   - 状态 5 时 `images[].imageUrl` 包含图片地址（有效期 7 天）
4. 下载图片到项目 `assets/` 目录

## 网络代理

Liblib 国内可直接访问。如遇网络问题：

```sh
export http_proxy=http://127.0.0.1:10809
export https_proxy=http://127.0.0.1:10809
```

---

## 后续阶段：Leonardo AI（旗舰 - 高品质大图）

> 当前阶段不使用。正式美术阶段时再启用。

- Base URL: `https://cloud.leonardo.ai/api/rest/v1`
- Auth: `Authorization: Bearer $LEONARDO_API_KEY`
- 环境变量未设置时，提示用户去 https://leonardo.ai/api 获取 API Key

### 生图流程

1. 创建生图任务：
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
2. 轮询结果（间隔 3 秒，最多等 3 分钟）：
```bash
curl -s "https://cloud.leonardo.ai/api/rest/v1/generations/$GENERATION_ID" \
  -H "Authorization: Bearer $LEONARDO_API_KEY"
```
3. 下载图片到项目 `assets/` 目录

### 旗舰模型

| 模型 | ID | 适用场景 |
|------|-----|---------|
| FLUX.2 Pro | `fullFlux2Pro` | 最高画质，适合主视觉、海报、概念图 |
| Phoenix | `b24e16ff-06e3-43eb-8d33-4416c2d75876` | Leonardo 自研旗舰，综合品质好 |
| Leonardo Lightning XL | `d69c8273-6b17-4a30-9b12-21b8e79b2c85` | 快速出图，性价比最高 |
