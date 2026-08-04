// 战场世界尺寸与城堡布局共享常量。
using Godot;

public static class BattlefieldMetrics
{
	public const float WorldWidth = 3840f;
	public const float WorldHeight = 2160f;
	public const float OuterMargin = 160f;
	public const float CastleGridPixels = 800f;
	public const float HorizontalSideMargin = 170f;
	public const float BottomSideMargin = 150f;
	public const float PlayerSideX = HorizontalSideMargin;
	public const float EnemySideX = WorldWidth - HorizontalSideMargin - CastleGridPixels;
	public const float SideY = WorldHeight - BottomSideMargin - CastleGridPixels;

	public static Rect2 WorldRect => new Rect2(0f, 0f, WorldWidth, WorldHeight);
}
