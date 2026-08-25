// 碰撞体积可视叠加：以空心圆/空心矩形描边绘制单位与建筑碰撞形状
using Godot;

namespace CasualCastle.Adapters.Godot.Dev;

public partial class CollisionOverlay : Node2D
{
	public const int OverlayZIndex = 1000;
	public const float BorderWidth = 2f;
	public const int CircleArcPoints = 64;
	public static readonly Color FriendlyColor = new(0.2f, 1f, 0.3f, 0.9f);
	public static readonly Color EnemyColor = new(1f, 0.25f, 0.2f, 0.9f);

	private float _radiusPixels;
	private Vector2 _rectSize;
	private Color _color;

	public override void _Ready()
	{
		ZAsRelative = false;
		ZIndex = OverlayZIndex;
	}

	public void ConfigureCircle(float radiusPixels, Color color)
	{
		_radiusPixels = radiusPixels;
		_rectSize = Vector2.Zero;
		_color = color;
		QueueRedraw();
	}

	public void ConfigureRect(Vector2 sizePixels, Color color)
	{
		_rectSize = sizePixels;
		_radiusPixels = 0f;
		_color = color;
		QueueRedraw();
	}

	public override void _Draw()
	{
		if (_radiusPixels > 0f)
		{
			DrawArc(Vector2.Zero, _radiusPixels, 0f, Mathf.Tau, CircleArcPoints, _color, BorderWidth);
			return;
		}
		DrawRect(new Rect2(-_rectSize * 0.5f, _rectSize), _color, false, BorderWidth);
	}
}
