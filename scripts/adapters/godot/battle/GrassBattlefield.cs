using Godot;

public partial class GrassBattlefield : Node2D
{
	private const float WorldWidth = 1920f;
	private const float WorldHeight = 1080f;
	private const float Margin = 160f;
	private const float TilePixelSize = 256f;

	public override void _Ready()
	{
		SetupFallbackBackground();
		SetupGrassTile();
	}

	private void SetupFallbackBackground()
	{
		ColorRect bg = new ColorRect();
		bg.Color = new Color(0.22f, 0.42f, 0.18f, 1f);
		bg.Position = new Vector2(-Margin, -Margin);
		bg.Size = new Vector2(WorldWidth + Margin * 2, WorldHeight + Margin * 2);
		bg.ZIndex = -100;
		AddChild(bg);
	}

	private void SetupGrassTile()
	{
		if (!ResourceLoader.Exists("res://assets/art/battlefield/grass_tile.png"))
			return;

		Texture2D grassTex = GD.Load<Texture2D>("res://assets/art/battlefield/grass_tile.png");
		if (grassTex == null)
			return;

		ColorRect tileRect = new ColorRect();
		tileRect.Position = new Vector2(-Margin, -Margin);
		tileRect.Size = new Vector2(WorldWidth + Margin * 2, WorldHeight + Margin * 2);
		tileRect.ZIndex = -10;

		ShaderMaterial tileMat = new ShaderMaterial();
		Shader tileShader = new Shader();
		tileShader.Code = @"
shader_type canvas_item;
render_mode blend_mix;

uniform sampler2D grass_tex : source_color;
uniform vec2 tile_uv_scale = vec2(1.0, 1.0);

void fragment() {
	vec2 tiled_uv = fract(UV * tile_uv_scale);
	COLOR = texture(grass_tex, tiled_uv);
}
";
		tileMat.Shader = tileShader;
		tileMat.SetShaderParameter("grass_tex", grassTex);
		Vector2 rectSize = new Vector2(WorldWidth + Margin * 2, WorldHeight + Margin * 2);
		tileMat.SetShaderParameter("tile_uv_scale", rectSize / TilePixelSize);
		tileRect.Material = tileMat;
		AddChild(tileRect);
	}
}
