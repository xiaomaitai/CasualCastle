using Godot;

public partial class BuildingStateIcon : Node2D
{
	public enum IconType
	{
		None,
		Paused,
		Destroyed,
	}

	private IconType _iconType = IconType.None;

	public override void _Ready()
	{
		ZIndex = 16;
		Visible = false;
	}

	public void SetIcon(IconType type)
	{
		_iconType = type;
		Visible = type != IconType.None;
		QueueRedraw();
	}

	public override void _Draw()
	{
		switch (_iconType)
		{
			case IconType.Paused:
				DrawPauseIcon();
				break;
			case IconType.Destroyed:
				DrawHammerIcon();
				break;
		}
	}

	private void DrawPauseIcon()
	{
		Color color = new Color(0.88f, 0.94f, 1f, 0.95f);
		DrawRect(new Rect2(-10, -12, 5, 16), color);
		DrawRect(new Rect2(3, -12, 5, 16), color);
	}

	private void DrawHammerIcon()
	{
		Color head = new Color(0.95f, 0.78f, 0.35f, 0.95f);
		Color handle = new Color(0.72f, 0.52f, 0.28f, 0.95f);
		DrawRect(new Rect2(-12, -14, 16, 7), head);
		DrawRect(new Rect2(2, -6, 4, 16), handle);
	}
}
