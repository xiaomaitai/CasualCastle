using Godot;

public partial class BuildingStateIcon : Node2D
{
	private static readonly Vector2 DisplayScale = new(0.5f, 0.5f);
	private static readonly Vector2 OverlayOffset = new(14f, -8f);

	private Sprite2D _pauseSprite;
	private Sprite2D _combineProhibitSprite;
	private bool _paused;
	private bool _combineProhibited;

	public override void _Ready()
	{
		ZIndex = 16;
		_pauseSprite = new Sprite2D
		{
			Centered = true,
			Scale = DisplayScale,
		};
		_combineProhibitSprite = new Sprite2D
		{
			Centered = true,
			Scale = DisplayScale,
			Position = OverlayOffset,
		};
		AddChild(_pauseSprite);
		AddChild(_combineProhibitSprite);
		RefreshVisual();
	}

	public void SetPaused(bool paused)
	{
		if (_paused == paused)
			return;

		_paused = paused;
		RefreshVisual();
	}

	public void SetCombineProhibited(bool prohibited)
	{
		if (_combineProhibited == prohibited)
			return;

		_combineProhibited = prohibited;
		RefreshVisual();
	}

	private void RefreshVisual()
	{
		if (_pauseSprite == null || _combineProhibitSprite == null)
			return;

		if (_paused)
		{
			_pauseSprite.Texture = BuildingIcons.Pause;
			_pauseSprite.Visible = true;
		}
		else
		{
			_pauseSprite.Texture = null;
			_pauseSprite.Visible = false;
		}

		if (_combineProhibited)
		{
			_combineProhibitSprite.Texture = BuildingIcons.CombineProhibit;
			_combineProhibitSprite.Visible = true;
			_combineProhibitSprite.Position = _paused ? OverlayOffset : Vector2.Zero;
		}
		else
		{
			_combineProhibitSprite.Texture = null;
			_combineProhibitSprite.Visible = false;
		}

		Visible = _paused || _combineProhibited;
	}
}
