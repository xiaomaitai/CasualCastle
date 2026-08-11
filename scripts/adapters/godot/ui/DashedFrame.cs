// 商店商品槽虚线框：绘制细虚线边框
using Godot;

public partial class DashedFrame : Control
{
	private const float FrameWidth = 1f;
	private const float DashLength = 5f;

	private static readonly Color FrameColor = new(0.6f, 0.64f, 0.68f, 0.55f);

	public override void _Draw()
	{
		Vector2 topRight = new Vector2(Size.X, 0f);
		Vector2 bottomRight = new Vector2(Size.X, Size.Y);
		Vector2 bottomLeft = new Vector2(0f, Size.Y);
		DrawDashedLine(Vector2.Zero, topRight, FrameColor, FrameWidth, DashLength);
		DrawDashedLine(topRight, bottomRight, FrameColor, FrameWidth, DashLength);
		DrawDashedLine(bottomRight, bottomLeft, FrameColor, FrameWidth, DashLength);
		DrawDashedLine(bottomLeft, Vector2.Zero, FrameColor, FrameWidth, DashLength);
	}
}