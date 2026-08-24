// 陷阱范围圈：选中陷阱时绘制触发范围
using Godot;

public partial class TrapRangeView : Node2D
{
	private const int ArcPoints = 64;
	private const float EdgeWidth = 2f;
	private static readonly Color RangeFill = new(1f, 0.45f, 0.2f, 0.10f);
	private static readonly Color RangeEdge = new(1f, 0.55f, 0.25f, 0.45f);

	private float _radiusPixels;

	public void Configure(float radiusPixels)
	{
		_radiusPixels = radiusPixels;
		QueueRedraw();
	}

	public override void _Draw()
	{
		DrawCircle(Vector2.Zero, _radiusPixels, RangeFill);
		DrawArc(Vector2.Zero, _radiusPixels, 0f, Mathf.Tau, ArcPoints, RangeEdge, EdgeWidth);
	}
}
