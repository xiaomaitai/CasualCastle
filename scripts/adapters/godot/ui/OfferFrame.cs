// 商店商品槽边框绘制
using Godot;

public partial class OfferFrame : Control
{
	private const float DetailedFrameWidth = 2f;
	private const float CartoonFrameWidth = 5f;
	private static readonly Color DetailedFrameColor = new(0.63f, 0.46f, 0.22f, 0.9f);
	private static readonly Color Ink = new(0.025f, 0.035f, 0.045f, 1f);
	private static readonly Color PaleYellow = new(1f, 0.9f, 0.62f, 1f);
	private bool _cartoon;

	public void ApplyProfile(UiProfileKind kind)
	{
		_cartoon = kind == UiProfileKind.Cartoon;
		QueueRedraw();
	}

	public override void _Draw()
	{
		if (_cartoon)
			DrawCartoonFrame();
		else
			DrawDetailedFrame();
	}

	private void DrawCartoonFrame()
	{
		Vector2[] points = BuildHandDrawnOutline();
		DrawColoredPolygon(points, PaleYellow);
		Vector2[] closed = new Vector2[points.Length + 1];
		points.CopyTo(closed, 0);
		closed[^1] = points[0];
		DrawPolyline(closed, Ink, CartoonFrameWidth, true);
	}

	private void DrawDetailedFrame()
	{
		Vector2[] points = { Vector2.Zero, new(Size.X, 0f), Size, new(0f, Size.Y), Vector2.Zero };
		DrawPolyline(points, DetailedFrameColor, DetailedFrameWidth);
	}

	private Vector2[] BuildHandDrawnOutline()
	{
		float wobble = 2f + GetInstanceId() % 3;
		return new Vector2[]
		{
			new(3f, 5f), new(Size.X * 0.32f, 2f + wobble), new(Size.X * 0.67f, 5f - wobble), new(Size.X - 4f, 3f),
			new(Size.X - 2f, Size.Y * 0.34f), new(Size.X - 5f, Size.Y * 0.7f), new(Size.X - 3f, Size.Y - 4f),
			new(Size.X * 0.68f, Size.Y - 2f), new(Size.X * 0.31f, Size.Y - 5f), new(4f, Size.Y - 3f),
			new(2f, Size.Y * 0.65f), new(5f, Size.Y * 0.3f)
		};
	}
}