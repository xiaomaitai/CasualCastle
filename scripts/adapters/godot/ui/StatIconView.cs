// 属性行图标：程序绘制战斗、移动与建筑信息符号
using Godot;

public sealed partial class StatIconView : Control
{
	private const float IconSize = 20f;
	private static readonly Color Steel = new(0.82f, 0.87f, 0.93f, 1f);
	private static readonly Color SteelDark = new(0.42f, 0.52f, 0.68f, 1f);
	private static readonly Color Gold = new(0.94f, 0.74f, 0.28f, 1f);
	private static readonly Color Purple = new(0.72f, 0.52f, 0.96f, 1f);
	private static readonly Color Cyan = new(0.46f, 0.82f, 0.95f, 1f);
	private static readonly Color Green = new(0.42f, 0.82f, 0.48f, 1f);

	private StatIconKind _kind;

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
			case StatIconKind.Attack: DrawAttack(); break;
			case StatIconKind.Armor: DrawShield(Steel); break;
			case StatIconKind.Resistance: DrawShield(Purple); break;
			case StatIconKind.Cost: DrawCoin(); break;
			case StatIconKind.Speed: DrawSpeed(); break;
			case StatIconKind.Range: DrawRange(); break;
			case StatIconKind.Cooldown: DrawClock(); break;
			case StatIconKind.Vision: DrawVision(); break;
			case StatIconKind.Shield: DrawShield(Cyan); break;
			case StatIconKind.Footprint: DrawFootprint(); break;
			case StatIconKind.Production: DrawProduction(); break;
			case StatIconKind.Building: DrawBuilding(); break;
			case StatIconKind.Unit: DrawUnit(); break;
		}
	}

	private void DrawAttack()
	{
		DrawLine(new Vector2(16f, 3f), new Vector2(6f, 13f), Steel, 2.5f);
		DrawLine(new Vector2(4f, 13f), new Vector2(14f, 13f), Gold, 2f);
		DrawLine(new Vector2(9f, 13f), new Vector2(9f, 18f), Gold, 2f);
	}

	private void DrawShield(Color color)
	{
		Vector2[] points = { new(3f, 3f), new(17f, 3f), new(16f, 12f), new(10f, 18f), new(4f, 12f) };
		DrawColoredPolygon(points, color);
		DrawPolyline(new Vector2[] { new(5f, 5f), new(15f, 5f), new(14f, 11f), new(10f, 15f) }, SteelDark, 1.5f);
	}

	private void DrawCoin()
	{
		DrawCircle(new Vector2(10f, 10f), 7f, Gold);
		DrawArc(new Vector2(10f, 10f), 5f, 0f, Mathf.Tau, 20, new Color(1f, 0.9f, 0.5f), 1.5f);
	}

	private void DrawSpeed()
	{
		DrawLine(new Vector2(2f, 7f), new Vector2(12f, 7f), Cyan, 2f);
		DrawLine(new Vector2(5f, 11f), new Vector2(15f, 11f), Cyan, 2f);
		DrawLine(new Vector2(8f, 15f), new Vector2(18f, 15f), Cyan, 2f);
	}

	private void DrawRange()
	{
		DrawArc(new Vector2(4f, 16f), 12f, -Mathf.Pi / 2f, 0f, 16, Steel, 2f);
		DrawArc(new Vector2(4f, 16f), 7f, -Mathf.Pi / 2f, 0f, 12, Cyan, 2f);
		DrawCircle(new Vector2(4f, 16f), 2f, Gold);
	}

	private void DrawClock()
	{
		DrawArc(new Vector2(10f, 10f), 7f, 0f, Mathf.Tau, 24, Steel, 2f);
		DrawLine(new Vector2(10f, 10f), new Vector2(10f, 5f), Gold, 2f);
		DrawLine(new Vector2(10f, 10f), new Vector2(14f, 12f), Gold, 2f);
	}

	private void DrawVision()
	{
		DrawArc(new Vector2(10f, 10f), 8f, 0f, Mathf.Pi, 18, Cyan, 2f);
		DrawArc(new Vector2(10f, 10f), 8f, Mathf.Pi, Mathf.Tau, 18, Cyan, 2f);
		DrawCircle(new Vector2(10f, 10f), 3f, Steel);
	}

	private void DrawFootprint()
	{
		DrawRect(new Rect2(2f, 2f, 7f, 7f), SteelDark);
		DrawRect(new Rect2(11f, 2f, 7f, 7f), Steel);
		DrawRect(new Rect2(2f, 11f, 7f, 7f), Steel);
		DrawRect(new Rect2(11f, 11f, 7f, 7f), SteelDark);
	}

	private void DrawProduction()
	{
		DrawArc(new Vector2(10f, 10f), 7f, -2.5f, 1.7f, 20, Green, 2f);
		DrawColoredPolygon(new Vector2[] { new(16f, 12f), new(18f, 17f), new(13f, 16f) }, Green);
	}

	private void DrawBuilding()
	{
		DrawRect(new Rect2(3f, 8f, 14f, 10f), SteelDark);
		DrawColoredPolygon(new Vector2[] { new(2f, 8f), new(10f, 2f), new(18f, 8f) }, Steel);
		DrawRect(new Rect2(8f, 12f, 4f, 6f), Gold);
	}

	private void DrawUnit()
	{
		DrawCircle(new Vector2(10f, 6f), 4f, Steel);
		DrawArc(new Vector2(10f, 18f), 8f, Mathf.Pi, Mathf.Tau, 18, Green, 3f);
	}

	public enum StatIconKind
	{
		Attack,
		Armor,
		Resistance,
		Cost,
		Speed,
		Range,
		Cooldown,
		Vision,
		Shield,
		Footprint,
		Production,
		Building,
		Unit,
	}
}