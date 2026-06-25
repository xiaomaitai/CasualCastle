using Godot;

public partial class BuildingStateIcon : Node2D
{
	public enum IconType
	{
		None,
		Paused,
	}

	private static readonly Vector2 DisplayScale = new(0.5f, 0.5f);

	private Sprite2D _sprite;
	private IconType _iconType = IconType.None;

	public override void _Ready()
	{
		ZIndex = 16;
		_sprite = new Sprite2D
		{
			Centered = true,
			Scale = DisplayScale,
		};
		AddChild(_sprite);
		Visible = false;
	}

	public void SetIcon(IconType type)
	{
		if (_iconType == type)
			return;

		_iconType = type;
		if (type == IconType.None)
		{
			_sprite.Texture = null;
			Visible = false;
			return;
		}

		_sprite.Texture = BuildingIcons.Pause;
		Visible = true;
	}
}
