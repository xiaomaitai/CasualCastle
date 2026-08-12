// 商店商品框：绘制黑金中世纪商品槽边框
using Godot;

public partial class OfferFrame : Control
{
	private const float FrameWidth = 2f;

	private static readonly Color FrameColor = new(0.63f, 0.46f, 0.22f, 0.9f);

	public override void _Draw()
	{
		Vector2 topRight = new Vector2(Size.X, 0f);
		Vector2 bottomRight = new Vector2(Size.X, Size.Y);
		Vector2 bottomLeft = new Vector2(0f, Size.Y);
		DrawLine(Vector2.Zero, topRight, FrameColor, FrameWidth);
		DrawLine(topRight, bottomRight, FrameColor, FrameWidth);
		DrawLine(bottomRight, bottomLeft, FrameColor, FrameWidth);
		DrawLine(bottomLeft, Vector2.Zero, FrameColor, FrameWidth);
	}
}