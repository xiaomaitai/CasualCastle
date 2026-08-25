// 战场飘字：受击伤害与治疗数值上浮淡出，按伤害类型着色，重击显示大号红字
using CasualCastle.Domain.Battle;
using Godot;

public partial class FloatingText : Node2D
{
	private const float RiseDistance = 42f;
	private const float LifeSeconds = 0.9f;
	private const float PopSeconds = 0.12f;
	private const float FadeDelaySeconds = 0.2f;
	private const int FontSize = 20;
	private const int CritFontSize = 28;
	private const float LabelOffsetX = -18f;
	private const float LabelOffsetY = -26f;
	private const float CritScale = 1.5f;
	private const float NormalScale = 1.25f;
	private static readonly Color DefaultDamageColor = new(1f, 0.85f, 0.35f, 1f);
	private static readonly Color HealColor = new(0.4f, 0.95f, 0.5f, 1f);
	private static readonly Color CritColor = new(1f, 0.25f, 0.25f, 1f);
	private static readonly Color SlashColor = new(0.55f, 0.72f, 1f, 1f);
	private static readonly Color PierceColor = new(1f, 0.62f, 0.3f, 1f);
	private static readonly Color MagicColor = new(0.72f, 0.42f, 1f, 1f);
	private static readonly Color SiegeColor = new(0.95f, 0.45f, 0.45f, 1f);
	private static readonly Color HolyColor = new(1f, 0.95f, 0.6f, 1f);
	private static readonly Color ChaosColor = new(1f, 0.3f, 0.72f, 1f);

	public static FloatingText Spawn(Node parent, Vector2 position, int amount, bool isHeal, bool isCrit = false, DamageType? damageType = null)
	{
		FloatingText floating = new();
		parent.AddChild(floating);
		floating.Position = position;
		floating.Setup(amount.ToString(), isHeal, isCrit, damageType);
		return floating;
	}

	private void Setup(string text, bool isHeal, bool isCrit, DamageType? damageType)
	{
		Label label = new();
		label.Text = text;
		label.AddThemeFontSizeOverride("font_size", isCrit ? CritFontSize : FontSize);
		label.AddThemeColorOverride("font_color", ResolveColor(isHeal, isCrit, damageType));
		label.Position = new Vector2(LabelOffsetX, LabelOffsetY);
		label.ZIndex = 10;
		AddChild(label);
		Scale = Vector2.One * (isCrit ? CritScale : NormalScale);
		RunAnimation();
	}

	private static Color ResolveColor(bool isHeal, bool isCrit, DamageType? damageType)
	{
		if (isHeal)
			return HealColor;
		if (isCrit)
			return CritColor;
		if (damageType == null)
			return DefaultDamageColor;
		switch (damageType.Value)
		{
			case DamageType.Slash:
				return SlashColor;
			case DamageType.Pierce:
				return PierceColor;
			case DamageType.Magic:
				return MagicColor;
			case DamageType.Siege:
				return SiegeColor;
			case DamageType.Holy:
				return HolyColor;
			case DamageType.Chaos:
				return ChaosColor;
			default:
				return DefaultDamageColor;
		}
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
