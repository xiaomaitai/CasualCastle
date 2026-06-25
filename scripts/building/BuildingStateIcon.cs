using Godot;

public partial class BuildingStateIcon : Node2D
{
	public enum IconType
	{
		None,
		Paused,
		Destroyed,
		RepairBlocked,
	}

	private const float DisplayScale = 0.5f;

	private Sprite2D _baseSprite;
	private Sprite2D _overlaySprite;
	private IconType _iconType = IconType.None;

	public override void _Ready()
	{
		ZIndex = 16;
		_baseSprite = new Sprite2D
		{
			Centered = true,
			Scale = new Vector2(DisplayScale, DisplayScale),
		};
		_overlaySprite = new Sprite2D
		{
			Centered = true,
			Scale = new Vector2(DisplayScale, DisplayScale),
			Texture = BuildingIcons.Prohibit,
			Visible = false,
		};
		AddChild(_baseSprite);
		AddChild(_overlaySprite);
		Visible = false;
	}

	public void SetIcon(IconType type)
	{
		if (_iconType == type)
			return;

		_iconType = type;
		if (type == IconType.None)
		{
			_baseSprite.Texture = null;
			_overlaySprite.Visible = false;
			Visible = false;
			return;
		}

		_baseSprite.Texture = type switch
		{
			IconType.Paused => BuildingIcons.Pause,
			IconType.Destroyed => BuildingIcons.Repair,
			IconType.RepairBlocked => BuildingIcons.Repair,
			_ => null,
		};
		_overlaySprite.Visible = type == IconType.RepairBlocked;
		Visible = true;
	}
}
