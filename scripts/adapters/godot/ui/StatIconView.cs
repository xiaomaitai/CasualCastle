// 属性区图标：按类型绘制剑/盾/抗性/金币小图标
using Godot;

public sealed partial class StatIconView : Control
{
	private const float IconSize = 20f;
	private const float BladeWidth = 2.5f;
	private const float GuardWidth = 2f;
	private static readonly Color Steel = new(0.82f, 0.87f, 0.93f, 1f);
	private static readonly Color SteelDark = new(0.45f, 0.55f, 0.7f, 1f);
	private static readonly Color Gold = new(0.92f, 0.76f, 0.32f, 1f);
	private static readonly Color GoldLight = new(0.98f, 0.92f, 0.55f, 1f);
	private static readonly Color Purple = new(0.72f, 0.52f, 0.96f, 1f);

	private StatIconKind _kind = StatIconKind.Attack;

	public override void _Ready()
	{
		CustomMinimumSize = new Vector2(IconSize, IconSize);
		MouseFilter = MouseFilterEnum.Stop;
	}

	public void Configure(StatIconKind kind)
	{
		_kind = kind;
		QueueRedraw();
	}

	public override void _Draw()
	{
		switch (_kind)
		{
			case StatIconKind.Attack:
				DrawAttackGlyph();
				break;
			case StatIconKind.Armor:
				DrawArmorGlyph();
				break;
			case StatIconKind.Resistance:
				DrawResistanceGlyph();
				break;
			case StatIconKind.Cost:
				DrawCostGlyph();
				break;
		}
	}

	private void DrawAttackGlyph()
	{
		DrawLine(new Vector2(16f, 4f), new Vector2(7f, 13f), Steel, BladeWidth);
		DrawLine(new Vector2(5f, 13f), new Vector2(16f, 13f), Gold, GuardWidth);
		DrawLine(new Vector2(11f, 13f), new Vector2(11f, 18f), Gold, GuardWidth);
	}

	private void DrawArmorGlyph()
	{
		DrawColoredPolygon(new Vector2[]
		{
			new(3f, 3f),
			new(17f, 3f),
			new(17f, 11f),
			new(10f, 17f),
			new(3f, 11f),
		}, Steel);
		DrawColoredPolygon(new Vector2[]
		{
			new(5f, 5f),
			new(15f, 5f),
			new(15f, 11f),
			new(10f, 15f),
			new(5f, 11f),
		}, SteelDark);
	}

	private void DrawResistanceGlyph()
	{
		DrawColoredPolygon(new Vector2[]
		{
			new(3f, 4f),
			new(17f, 4f),
			new(17f, 11f),
			new(10f, 17f),
			new(3f, 11f),
		}, Purple);
		DrawLine(new Vector2(15f, 3f), new Vector2(15f, 5f), Gold, 1.5f);
		DrawLine(new Vector2(13f, 4f), new Vector2(17f, 4f), Gold, 1.5f);
		DrawLine(new Vector2(15f, 5f), new Vector2(16f, 7f), Gold, 1.5f);
	}

	private void DrawCostGlyph()
	{
		DrawCircle(new Vector2(10f, 10f), 7f, Gold);
		DrawCircle(new Vector2(10f, 10f), 5f, GoldLight);
	}

	public enum StatIconKind
	{
		Attack,
		Armor,
		Resistance,
		Cost,
	}
}
