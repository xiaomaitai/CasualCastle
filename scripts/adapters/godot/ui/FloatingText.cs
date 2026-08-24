// 战场飘字：受击伤害与治疗数值上浮淡出
using Godot;

public partial class FloatingText : Node2D
{
	private const float RiseDistance = 42f;
	private const float LifeSeconds = 0.9f;
	private const float PopSeconds = 0.12f;
	private const float FadeDelaySeconds = 0.2f;
	private const int FontSize = 20;
	private const float LabelOffsetX = -18f;
	private const float LabelOffsetY = -26f;
	private static readonly Color DamageColor = new(1f, 0.85f, 0.35f, 1f);
	private static readonly Color HealColor = new(0.4f, 0.95f, 0.5f, 1f);

	public static FloatingText Spawn(Node parent, Vector2 position, int amount, bool isHeal)
	{
		FloatingText floating = new();
		parent.AddChild(floating);
		floating.Position = position;
		floating.Setup(amount.ToString(), isHeal);
		return floating;
	}

	private void Setup(string text, bool isHeal)
	{
		Label label = new();
		label.Text = text;
		label.AddThemeFontSizeOverride("font_size", FontSize);
		label.AddThemeColorOverride("font_color", isHeal ? HealColor : DamageColor);
		label.Position = new Vector2(LabelOffsetX, LabelOffsetY);
		label.ZIndex = 10;
		AddChild(label);
		Scale = Vector2.One * 1.25f;
		RunAnimation();
	}

	private void RunAnimation()
	{
		Tween tween = CreateTween();
		tween.TweenProperty(this, "scale", Vector2.One, PopSeconds);
		tween.Parallel().TweenProperty(this, "position", Position + new Vector2(0f, -RiseDistance), LifeSeconds);
		tween.Parallel().TweenProperty(this, "modulate:a", 0f, LifeSeconds - FadeDelaySeconds).SetDelay(FadeDelaySeconds);
		tween.TweenCallback(Callable.From(QueueFree));
	}
}
