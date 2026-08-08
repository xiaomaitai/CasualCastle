// 部落邻接指示器：加成格计数显示
using CasualCastle.Domain.Shared;
using Godot;

public partial class HordeAdjacencyIndicator : Node2D
{
	private const float BaseRadius = 42f;

	private int _adjacencyCellCount;

	public override void _Ready()
	{
		ZIndex = 14;
		Visible = false;
	}

	public void SetAdjacencyCellCount(int count)
	{
		_adjacencyCellCount = Mathf.Clamp(count, 0, GameRules.HordeAdjacencyMaxCells);
		Visible = _adjacencyCellCount > 0;
		QueueRedraw();
	}

	public override void _Draw()
	{
		if (_adjacencyCellCount <= 0)
			return;

		float intensity = _adjacencyCellCount / (float)GameRules.HordeAdjacencyMaxCells;
		Color ringColor = new(0.35f, 0.92f, 0.45f, 0.55f + 0.35f * intensity);
		DrawCircle(Vector2.Zero, BaseRadius, new Color(0.2f, 0.85f, 0.35f, 0.08f + 0.12f * intensity));
		DrawArc(Vector2.Zero, BaseRadius, 0f, Mathf.Tau, 48, ringColor, 3f);
		DrawArc(Vector2.Zero, BaseRadius + 9f, 0f, Mathf.Tau, 48, new Color(ringColor.R, ringColor.G, ringColor.B, ringColor.A * 0.55f), 2f);
	}
}
