// 建筑耐久条：建筑受损时显示在建筑上方的耐久血条
using Godot;

public partial class BuildingHealthBar : Node2D
{
	private const int BarZIndex = 16;
	private const float BarHeightStretch = 1.1f;
	private const float AboveBuildingGapPixels = 6f;
	private const string HealthBarScenePath = "res://prefabs/HealthBar.tscn";

	private HealthBarView _bar;

	public void Configure(Vector2 buildingVisualSize)
	{
		ZIndex = BarZIndex;
		PackedScene scene = GD.Load<PackedScene>(HealthBarScenePath);
		_bar = scene.Instantiate<HealthBarView>();
		AddChild(_bar);
		Sprite2D empty = _bar.GetNodeOrNull<Sprite2D>("Empty");
		if (empty == null || empty.Texture == null)
			return;
		float scale = buildingVisualSize.X / empty.Texture.GetWidth();
		float barHalfHeight = empty.Texture.GetHeight() * scale * BarHeightStretch * 0.5f;
		_bar.Scale = new Vector2(scale, scale * BarHeightStretch);
		Position = new Vector2(0f, -buildingVisualSize.Y * 0.5f - AboveBuildingGapPixels - barHalfHeight);
		SetDamage(0, 0);
	}

	public void SetDamage(int health, int maxHealth)
	{
		if (_bar == null)
			return;
		if (health <= 0 || health >= maxHealth)
		{
			_bar.Visible = false;
			return;
		}
		_bar.Fill = health / (float)maxHealth;
		_bar.Ping(true);
		_bar.Visible = true;
	}
}
