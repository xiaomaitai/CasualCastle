// 血条视图：单位血量显示，满血时短暂显示后淡出
using Godot;

public partial class HealthBarView : Node2D
{
	private const float FadeDelaySeconds = 1.2f;
	private const float FadeSeconds = 0.6f;
	private const float VisibleAlpha = 1f;
	private const float HiddenAlpha = 0f;

	private Sprite2D _empty;
	private Sprite2D _full;
	private bool _ready;
	private float _fill = 1f;
	private float _emptyWidth;
	private float _fullTexWidth;
	private float _fullHeight;
	private float _emptyLeft;
	private bool _persistent;
	private Tween _fadeTween;

	public override void _Ready()
	{
		_empty = GetNode<Sprite2D>("Empty");
		_full = GetNode<Sprite2D>("Full");
		_empty.Centered = false;
		_full.Centered = false;
		_emptyWidth = _empty.Texture.GetWidth();
		_fullTexWidth = _full.Texture.GetWidth();
		_fullHeight = _full.Texture.GetHeight();
		_emptyLeft = -_emptyWidth / 2f;
		_empty.Position = new Vector2(_emptyLeft, -_empty.Texture.GetHeight() / 2f);
		_ready = true;
		ApplyFill();
	}

	public override void _ExitTree()
	{
		_fadeTween?.Kill();
	}

	public float Fill
	{
		set
		{
			_fill = Mathf.Clamp(value, 0f, 1f);
			if (_ready)
				ApplyFill();
		}
	}

	public void Ping(bool persistent)
	{
		_persistent = persistent;
		_fadeTween?.Kill();
		Modulate = new Color(1f, 1f, 1f, VisibleAlpha);
		if (_persistent)
			return;
		_fadeTween = CreateTween();
		_fadeTween.TweenInterval(FadeDelaySeconds);
		_fadeTween.TweenProperty(this, "modulate:a", HiddenAlpha, FadeSeconds);
	}

	private void ApplyFill()
	{
		float visibleFullWidth = Mathf.Max(0.0001f, _fill) * _emptyWidth;
		_full.Scale = new Vector2(visibleFullWidth / _fullTexWidth, 1f);
		_full.Position = new Vector2(_emptyLeft, -_fullHeight / 2f);
	}
}
