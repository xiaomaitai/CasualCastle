// 建筑行动环视图：按全局界面Profile绘制环形边界
using Godot;

public partial class BuildingActionRing : Control
{
	private const float Radius = 150f;
	private const float OuterThickness = 8f;
	private const float InnerThickness = 3f;
	private const float CartoonThickness = 6f;
	private const int ArcPointCount = 96;
	private static readonly Color Ink = new(0.025f, 0.035f, 0.045f, 1f);
	private static readonly Color DetailedDark = new(0.05f, 0.07f, 0.12f, 0.72f);
	private static readonly Color DetailedAccent = new(1f, 0.82f, 0.35f, 0.92f);
	private bool _cartoon;

	public void ApplyProfile(UiProfileKind kind)
	{
		_cartoon = kind == UiProfileKind.Cartoon;
		QueueRedraw();
	}

	public override void _Draw()
	{
		if (_cartoon)
			DrawPolyline(BuildHandDrawnRing(), Ink, CartoonThickness, true);
		else
			DrawDetailedRing();
	}

	private void DrawDetailedRing()
	{
		DrawArc(Vector2.Zero, Radius, 0f, Mathf.Tau, ArcPointCount, DetailedDark, OuterThickness, true);
		DrawArc(Vector2.Zero, Radius, 0f, Mathf.Tau, ArcPointCount, DetailedAccent, InnerThickness, true);
	}

	private Vector2[] BuildHandDrawnRing()
	{
		Vector2[] points = new Vector2[ArcPointCount + 1];
		float phase = GetInstanceId() % 7;
		for (int i = 0; i <= ArcPointCount; i++)
		{
			float angle = i * Mathf.Tau / ArcPointCount;
			float wobble = Mathf.Sin(angle * 5f + phase) * 2.8f + Mathf.Sin(angle * 9f) * 1.4f;
			points[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (Radius + wobble);
		}
		return points;
	}
}