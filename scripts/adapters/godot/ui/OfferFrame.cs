// 商店商品框：围绕槽位绘制细灰边框
using Godot;

public partial class OfferFrame : Control
{
	private const float FrameWidth = 2f;

	private static readonly Color FrameColor = new(0.6f, 0.64f, 0.68f, 0.2f);

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