// 界面飘字：在 HUD 指定位置浮现并上浮淡出的数值提示
using Godot;

public partial class UiFloatingText : Node2D
{
	private const float RiseDistance = 34f;
	private const float LifeSeconds = 0.85f;
	private const float PopSeconds = 0.12f;
	private const float FadeDelaySeconds = 0.2f;
	private const float PopScale = 1.4f;
	private const int FontSize = 22;
	private const int OutlineSize = 5;
	private const float LabelOffsetX = -22f;
	private const float LabelOffsetY = -16f;
	private static readonly Color OutlineColor = new(0.05f, 0.05f, 0.07f, 1f);

	public static UiFloatingText Spawn(Node parent, Vector2 position, string text, Color color)
	{
		UiFloatingText floating = new();
		parent.AddChild(floating);
		floating.Position = position;
		floating.Setup(text, color);
		return floating;
	}

	private void Setup(string text, Color color)
	{
		Label label = new();
		label.Text = text;
		label.AddThemeFontSizeOverride("font_size", FontSize);
		label.AddThemeColorOverride("font_color", color);
		label.AddThemeColorOverride("font_outline_color", OutlineColor);
		label.AddThemeConstantOverride("outline_size", OutlineSize);
		label.Position = new Vector2(LabelOffsetX, LabelOffsetY);
		AddChild(label);
		Scale = Vector2.One * PopScale;
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
