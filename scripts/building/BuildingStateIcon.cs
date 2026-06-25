using Godot;

public partial class BuildingStateIcon : Node2D
{
	private static readonly Vector2 DisplayScale = new(0.5f, 0.5f);
	private static readonly Vector2 OverlayOffset = new(14f, -8f);

	private Sprite2D _pauseSprite;
	private Sprite2D _fusionProhibitSprite;
	private bool _paused;
	private bool _fusionProhibited;

	public override void _Ready()
	{
		ZIndex = 16;
		_pauseSprite = new Sprite2D
		{
			Centered = true,
			Scale = DisplayScale,
		};
		_fusionProhibitSprite = new Sprite2D
		{
			Centered = true,
			Scale = DisplayScale,
			Position = OverlayOffset,
		};
		AddChild(_pauseSprite);
		AddChild(_fusionProhibitSprite);
		RefreshVisual();
	}

	public void SetPaused(bool paused)
	{
		if (_paused == paused)
			return;

		_paused = paused;
		RefreshVisual();
	}

	public void SetFusionProhibited(bool prohibited)
	{
		if (_fusionProhibited == prohibited)
			return;

		_fusionProhibited = prohibited;
		RefreshVisual();
	}

	private void RefreshVisual()
	{
		if (_pauseSprite == null || _fusionProhibitSprite == null)
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

		if (_fusionProhibited)
		{
			_fusionProhibitSprite.Texture = BuildingIcons.FusionProhibit;
			_fusionProhibitSprite.Visible = true;
			_fusionProhibitSprite.Position = _paused ? OverlayOffset : Vector2.Zero;
		}
		else
		{
			_fusionProhibitSprite.Texture = null;
			_fusionProhibitSprite.Visible = false;
		}

		Visible = _paused || _fusionProhibited;
	}
}
