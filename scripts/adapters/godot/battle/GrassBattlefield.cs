// 战场地面：无缝底纹着色器 + 随机装饰层。
using Godot;

public partial class GrassBattlefield : Node2D
{
	private const float PatchFrequency = 0.10f;
	private static readonly Texture2D GrassTexture = SeamlessTileFactory.CreateGrassTexture();
	private static readonly Texture2D MudTexture = SeamlessTileFactory.CreateMudTexture();
	private static readonly Texture2D StoneTexture = SeamlessTileFactory.CreateStoneTexture();
	private const string FloorShaderCode = @"
shader_type canvas_item;
render_mode blend_mix;

uniform sampler2D grass_tex : source_color;
uniform sampler2D mud_tex : source_color;
uniform sampler2D stone_tex : source_color;
uniform vec2 tile_uv_scale = vec2(1.0, 1.0);
uniform float patch_freq = 0.1;

float hash21(vec2 p) {
	p = fract(p * vec2(234.34, 435.345));
	p += dot(p, p + 34.23);
	return fract(p.x * p.y);
}

float vnoise(vec2 p) {
	vec2 i = floor(p);
	vec2 f = fract(p);
	f = f * f * (3.0 - 2.0 * f);
	float a = hash21(i);
	float b = hash21(i + vec2(1.0, 0.0));
	float c = hash21(i + vec2(0.0, 1.0));
	float d = hash21(i + vec2(1.0, 1.0));
	return mix(mix(a, b, f.x), mix(c, d, f.x), f.y);
}

float fbm(vec2 p) {
	float value = 0.0;
	float amp = 0.5;
	for (int i = 0; i < 4; i++) {
		value += amp * vnoise(p);
		p *= 2.0;
		amp *= 0.5;
	}
	return value;
}

void fragment() {
	vec2 world_uv = UV * tile_uv_scale;
	vec2 tiled_uv = fract(world_uv);
	float patch = fbm(world_uv * patch_freq);
	vec3 grass = texture(grass_tex, tiled_uv).rgb;
	vec3 mud = texture(mud_tex, tiled_uv).rgb;
	vec3 stone = texture(stone_tex, tiled_uv).rgb;
	vec3 color = mix(grass, mud, smoothstep(0.55, 0.75, patch));
	color = mix(color, stone, smoothstep(0.82, 0.94, patch));
	COLOR = vec4(color, 1.0);
}
";

	public override void _Ready()
	{
		SetupFallbackBackground();
		SetupFloor();
		SetupDecorations();
	}

	private void SetupFallbackBackground()
	{
		ColorRect bg = new ColorRect();
		bg.Color = new Color(0.22f, 0.42f, 0.18f, 1f);
		bg.Position = new Vector2(-BattlefieldMetrics.OuterMargin, -BattlefieldMetrics.OuterMargin);
		bg.Size = WorldRectWithMargin();
		bg.ZIndex = -100;
		bg.MouseFilter = Control.MouseFilterEnum.Ignore;
		AddChild(bg);
	}

	private void SetupFloor()
	{
		ColorRect floorRect = new ColorRect();
		floorRect.Position = new Vector2(-BattlefieldMetrics.OuterMargin, -BattlefieldMetrics.OuterMargin);
		floorRect.Size = WorldRectWithMargin();
		floorRect.ZIndex = -10;
		floorRect.MouseFilter = Control.MouseFilterEnum.Ignore;
		floorRect.Material = CreateFloorMaterial();
		AddChild(floorRect);
	}

	private ShaderMaterial CreateFloorMaterial()
	{
		ShaderMaterial floorMaterial = new ShaderMaterial();
		Shader shader = new Shader();
		shader.Code = FloorShaderCode;
		floorMaterial.Shader = shader;
		floorMaterial.SetShaderParameter("grass_tex", GrassTexture);
		floorMaterial.SetShaderParameter("mud_tex", MudTexture);
		floorMaterial.SetShaderParameter("stone_tex", StoneTexture);
		floorMaterial.SetShaderParameter("tile_uv_scale", WorldRectWithMargin() / BattlefieldMetrics.TilePixelSize);
		floorMaterial.SetShaderParameter("patch_freq", PatchFrequency);
		return floorMaterial;
	}

	private void SetupDecorations()
	{
		AddChild(BattlefieldDecorationFactory.CreateGrassLayer(BattlefieldMetrics.WorldRect));
		AddChild(BattlefieldDecorationFactory.CreateStoneLayer(BattlefieldMetrics.WorldRect));
	}

	private Vector2 WorldRectWithMargin()
	{
		float width = BattlefieldMetrics.WorldWidth + BattlefieldMetrics.OuterMargin * 2f;
		float height = BattlefieldMetrics.WorldHeight + BattlefieldMetrics.OuterMargin * 2f;
		return new Vector2(width, height);
	}
}
