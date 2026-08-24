// 选中范围预览：在单位脚下绘制攻击/治疗射程圈
using Godot;

public partial class RangePreview : Node2D
{
	private const int ArcPoints = 64;
	private const float EdgeWidth = 2f;
	private static readonly Color RangeFill = new(0.25f, 0.6f, 1f, 0.10f);
	private static readonly Color RangeEdge = new(0.35f, 0.7f, 1f, 0.45f);

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
