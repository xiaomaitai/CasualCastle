// 攻击目标标记：受击位置短暂显示红色目标圈并淡出
using Godot;

public partial class TargetRing : Node2D
{
	private const float RingRadius = 26f;
	private const float RingLifeSeconds = 0.3f;
	private const int ArcPoints = 32;
	private const float EdgeWidth = 2.5f;
	private const float StartAlpha = 0.8f;
	private const float PopScale = 1.4f;
	private static readonly Color TargetColor = new(1f, 0.22f, 0.18f, 1f);

	public static TargetRing Spawn(Node parent, Vector2 position)
	{
		TargetRing ring = new();
		parent.AddChild(ring);
		ring.Position = position;
		ring.RunAnimation();
		return ring;
	}

	private void RunAnimation()
	{
		Scale = Vector2.One * PopScale;
		Modulate = new Color(TargetColor, StartAlpha);
		Tween tween = CreateTween();
		tween.TweenProperty(this, "scale", Vector2.One, RingLifeSeconds);
		tween.Parallel().TweenProperty(this, "modulate:a", 0f, RingLifeSeconds);
		tween.TweenCallback(Callable.From(QueueFree));
	}

	public override void _Draw()
	{
		DrawArc(Vector2.Zero, RingRadius, 0f, Mathf.Tau, ArcPoints, TargetColor, EdgeWidth);
	}
}
