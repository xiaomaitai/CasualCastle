// 建筑行动环视图：围绕选中建筑绘制圆环底
using Godot;

public partial class BuildingActionRing : Control
{
	private const float Radius = 150f;
	private const float OuterThickness = 8f;
	private const float InnerThickness = 3f;
	private const int ArcPointCount = 96;

	private static readonly Color DarkColor = new(0.05f, 0.07f, 0.12f, 0.72f);
	private static readonly Color GoldColor = new(1f, 0.82f, 0.35f, 0.92f);

	public override void _Draw()
	{
		DrawArc(Vector2.Zero, Radius, 0f, Mathf.Tau, ArcPointCount, DarkColor, OuterThickness, true);
		DrawArc(Vector2.Zero, Radius, 0f, Mathf.Tau, ArcPointCount, GoldColor, InnerThickness, true);
	}
}