// 技能图标视图
using CasualCastle.Adapters.Persistence;
using CasualCastle.Domain.Battle;
using Godot;

public partial class SkillIconView : Control
{
	private const float IconSize = 58f;
	private const float GlyphRadius = 25f;
	private static readonly Color Background = new(0.025f, 0.03f, 0.032f, 1f);
	private static readonly Color BorderColor = new(0.72f, 0.51f, 0.22f, 1f);
	private static readonly Color CooldownShade = new(0.01f, 0.012f, 0.014f, 0.8f);
	private static readonly Color CooldownLine = new(0.96f, 0.76f, 0.34f, 1f);
	private static readonly Color Steel = new(0.82f, 0.87f, 0.93f, 1f);
	private static readonly Color Gold = new(0.92f, 0.76f, 0.32f, 1f);
	private static readonly Color Ice = new(0.62f, 0.86f, 1f, 1f);
	private static readonly Color Flame = new(0.95f, 0.46f, 0.2f, 1f);
	private static readonly Color FlameCore = new(0.98f, 0.85f, 0.36f, 1f);
	private static readonly Color Eye = new(0.76f, 0.9f, 1f, 1f);
	private static readonly Color Pupil = new(0.12f, 0.22f, 0.38f, 1f);
	private static readonly Color SlashRed = new(0.95f, 0.34f, 0.3f, 1f);
	private static readonly Color RaceGreen = new(0.5f, 0.86f, 0.42f, 1f);
	private static readonly Color RaceBrown = new(0.72f, 0.52f, 0.3f, 1f);
	private static readonly Color RaceOrange = new(0.96f, 0.66f, 0.3f, 1f);
	private static readonly Color RacePurple = new(0.72f, 0.52f, 0.96f, 1f);

	private string _skillId;
	private float _remaining;
	private float _total;
	private StyleBoxFlat _bg;
	private Texture2D _iconTexture;

	public override void _Ready()
	{
		CustomMinimumSize = new Vector2(IconSize, IconSize);
		SizeFlagsVertical = SizeFlags.ShrinkCenter;
		MouseFilter = MouseFilterEnum.Stop;
		CreateBackgroundStyle();
	}

	public void Configure(SkillDef skill)
	{
		_skillId = skill.Id;
		TooltipText = string.IsNullOrEmpty(skill.Description)
			? skill.DisplayName
			: $"{skill.DisplayName}\n{skill.Description}";
		_iconTexture = LoadSkillIcon(skill.Id);
		QueueRedraw();
	}

	public void SetCooldown(float remaining, float total)
	{
		if (Mathf.IsEqualApprox(_remaining, remaining) && Mathf.IsEqualApprox(_total, total))
			return;
		_remaining = remaining;
		_total = total;
		QueueRedraw();
	}

	public override void _Draw()
	{
		if (_bg == null)
			CreateBackgroundStyle();
		DrawStyleBox(_bg, new Rect2(Vector2.Zero, new Vector2(IconSize, IconSize)));
		if (_iconTexture != null)
			DrawTextureRect(_iconTexture, new Rect2(4f, 4f, IconSize - 8f, IconSize - 8f), false);
		else
			DrawSkillGlyph();
		if (_total > 0f && _remaining > 0f)
		{
			float progress = Mathf.Clamp(_remaining / _total, 0f, 1f);
			DrawCooldownSweep(progress);
		}
	}

	private static Texture2D LoadSkillIcon(string skillId)
	{
		IVisualAssetRepository repository = GameManager.Get<IVisualAssetRepository>();
		string iconUid = repository?.GetSkillIconUid(skillId) ?? "";
		if (string.IsNullOrEmpty(iconUid))
			return null;
		return GD.Load<Texture2D>(iconUid);
	}

	private void DrawSkillGlyph()
	{
		switch (_skillId)
		{
			case "stealth":
				DrawStealthGlyph();
				break;
			case "backstab":
				DrawBackstabGlyph();
				break;
			case "slowing_strike":
				DrawSnowflakeGlyph();
				break;
			case "motley_crew":
				DrawMotleyGlyph();
				break;
			case "berserker_rage":
				DrawRageGlyph();
				break;
			case "orc_rage_t1":
			case "orc_rage_t2":
				DrawRageGlyph();
				break;
			case "orc_rage_aura":
				DrawAuraGlyph();
				break;
			case "grace_shield":
				DrawGraceShieldGlyph();
				break;
			case "grace_cooldown":
				DrawGraceCooldownGlyph();
				break;
			case "grace_burn_t1":
			case "grace_burn_t2":
				DrawGraceBurnGlyph();
				break;
			case "fanatical_aura":
				DrawFanaticalAuraGlyph();
				break;
			case "shadow_curse":
				DrawShadowCurseGlyph();
				break;
			case "shadow_nova":
				DrawShadowNovaGlyph();
				break;
			default:
				DrawFallbackGlyph();
				break;
		}
	}

	private void DrawGraceShieldGlyph()
	{
		DrawColoredPolygon(new Vector2[]
		{
			new(28, 8),
			new(44, 15),
			new(44, 30),
			new(28, 42),
			new(12, 30),
			new(12, 15),
		}, Gold);
		DrawColoredPolygon(new Vector2[]
		{
			new(28, 14),
			new(38, 19),
			new(38, 29),
			new(28, 37),
			new(18, 29),
			new(18, 19),
		}, new Color(0.98f, 0.92f, 0.68f, 1f));
	}

	private void DrawGraceCooldownGlyph()
	{
		DrawCircle(new Vector2(28, 28), 15f, Ice);
		DrawArc(new Vector2(28, 28), 15f, 0f, Mathf.Tau, 32, new Color(0.4f, 0.55f, 0.7f, 1f), 2f);
		DrawLine(new Vector2(28, 28), new Vector2(28, 17), Steel, 2f);
		DrawLine(new Vector2(28, 28), new Vector2(36, 32), Steel, 2f);
	}

	private void DrawGraceBurnGlyph()
	{
		DrawColoredPolygon(new Vector2[]
		{
			new(28, 8),
			new(38, 24),
			new(33, 40),
			new(28, 36),
			new(23, 40),
			new(18, 24),
		}, Flame);
		DrawColoredPolygon(new Vector2[]
		{
			new(28, 17),
			new(34, 27),
			new(31, 36),
			new(28, 33),
			new(25, 36),
			new(22, 27),
		}, FlameCore);
	}

	private void DrawBackstabGlyph()
	{
		DrawColoredPolygon(new Vector2[]
		{
			new(28, 7),
			new(22, 29),
			new(34, 29),
		}, Steel);
		DrawRect(new Rect2(17, 31, 22, 3), Gold);
		DrawRect(new Rect2(25, 34, 6, 12), new Color(0.45f, 0.3f, 0.18f, 1f));
	}

	private void DrawSnowflakeGlyph()
	{
		Vector2 center = new(28, 28);
		for (int i = 0; i < 6; i++)
		{
			float angle = i * Mathf.Pi / 3f;
			Vector2 dir = new(Mathf.Cos(angle), Mathf.Sin(angle));
			DrawLine(center + dir * 5f, center + dir * 22f, Ice, 2f);
			DrawLine(center + dir * 13f, center + dir * 13f + Perp(dir) * 5f, Ice, 2f);
		}
		DrawCircle(center, 3f, Ice);
	}

	private void DrawMotleyGlyph()
	{
		DrawCircle(new Vector2(20, 18), 7f, RaceGreen);
		DrawCircle(new Vector2(36, 18), 7f, RaceBrown);
		DrawCircle(new Vector2(20, 34), 7f, RaceOrange);
		DrawCircle(new Vector2(36, 34), 7f, RacePurple);
	}

	private void DrawRageGlyph()
	{
		DrawColoredPolygon(new Vector2[]
		{
			new(28, 7),
			new(40, 27),
			new(34, 45),
			new(28, 41),
			new(22, 45),
			new(16, 27),
		}, Flame);
		DrawColoredPolygon(new Vector2[]
		{
			new(28, 18),
			new(35, 30),
			new(31, 40),
			new(28, 37),
			new(25, 40),
			new(21, 30),
		}, FlameCore);
	}

	private void DrawAuraGlyph()
	{
		Color green = new(0.45f, 0.9f, 0.5f, 1f);
		DrawCircle(new Vector2(28, 28), 11f, new Color(0.35f, 0.8f, 0.4f, 1f));
		DrawArc(new Vector2(28, 28), 18f, 0f, Mathf.Tau, 32, green, 3f);
		DrawArc(new Vector2(28, 28), 24f, 0f, Mathf.Tau, 32, new Color(green.R, green.G, green.B, 0.7f), 2f);
	}

	private void DrawStealthGlyph()
	{
		DrawCircle(new Vector2(28, 28), 11f, Eye);
		DrawArc(new Vector2(28, 28), 11f, 0f, Mathf.Tau, 32, new Color(0.3f, 0.55f, 0.8f, 1f), 2f);
		DrawCircle(new Vector2(28, 28), 4f, Pupil);
		DrawLine(new Vector2(16, 16), new Vector2(40, 40), SlashRed, 3f);
	}

	private void DrawFanaticalAuraGlyph()
	{
		Color purple = new(0.72f, 0.45f, 0.95f, 1f);
		DrawCircle(new Vector2(28, 28), 11f, new Color(0.5f, 0.3f, 0.8f, 1f));
		DrawArc(new Vector2(28, 28), 18f, 0f, Mathf.Tau, 32, purple, 3f);
		DrawArc(new Vector2(28, 28), 24f, 0f, Mathf.Tau, 32, new Color(purple.R, purple.G, purple.B, 0.7f), 2f);
	}

	private void DrawShadowCurseGlyph()
	{
		DrawCircle(new Vector2(28, 24), 9f, new Color(0.55f, 0.35f, 0.85f, 1f));
		DrawRect(new Rect2(24, 33, 8, 7), new Color(0.4f, 0.25f, 0.65f, 1f));
		DrawCircle(new Vector2(25, 22), 2.5f, RacePurple);
		DrawCircle(new Vector2(31, 22), 2.5f, RacePurple);
		DrawArc(new Vector2(28, 24), 15f, Mathf.Pi * 0.15f, Mathf.Pi * 0.85f, 16, RacePurple, 2f);
	}

	private void DrawShadowNovaGlyph()
	{
		Color purple = new(0.72f, 0.45f, 0.95f, 1f);
		for (int i = 0; i < 8; i++)
		{
			float angle = i * Mathf.Pi / 4f;
			Vector2 dir = new(Mathf.Cos(angle), Mathf.Sin(angle));
			DrawLine(new Vector2(28, 28) + dir * 8f, new Vector2(28, 28) + dir * 22f, purple, 3f);
		}
		DrawCircle(new Vector2(28, 28), 8f, new Color(0.5f, 0.3f, 0.8f, 1f));
	}

	private void DrawFallbackGlyph()
	{
		Font font = GetThemeDefaultFont();
		DrawString(font, new Vector2(0, 36), "?", HorizontalAlignment.Center, IconSize, 24, Steel);
	}

	private void DrawCooldownSweep(float progress)
	{
		Vector2 center = new(IconSize / 2f, IconSize / 2f);
		float startAngle = -Mathf.Pi / 2f;
		float endAngle = startAngle + progress * Mathf.Tau;
		DrawColoredPolygon(BuildSector(center, GlyphRadius, startAngle, endAngle), CooldownShade);
		Vector2 edge = center + new Vector2(Mathf.Cos(endAngle), Mathf.Sin(endAngle)) * GlyphRadius;
		DrawLine(center, edge, CooldownLine, 2f);
		if (_remaining > 0f)
		{
			Font font = GetThemeDefaultFont();
			DrawString(font, new Vector2(0, 36), _remaining.ToString("0.0"), HorizontalAlignment.Center, IconSize, 13, CooldownLine);
		}
	}

	private static Vector2[] BuildSector(Vector2 center, float radius, float startAngle, float endAngle)
	{
		float span = Mathf.Abs(endAngle - startAngle);
		int steps = Mathf.Max(2, (int)(span / (Mathf.Tau / 48f)));
		Vector2[] points = new Vector2[steps + 2];
		points[0] = center;
		for (int i = 0; i <= steps; i++)
		{
			float t = i / (float)steps;
			float angle = Mathf.Lerp(startAngle, endAngle, t);
			points[i + 1] = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
		}
		return points;
	}

	private static Vector2 Perp(Vector2 dir)
	{
		return new Vector2(-dir.Y, dir.X);
	}

	private void CreateBackgroundStyle()
	{
		_bg = new StyleBoxFlat();
		_bg.BgColor = Background;
		_bg.BorderColor = BorderColor;
		_bg.SetBorderWidthAll(2);
		_bg.SetCornerRadiusAll(3);
	}
}
