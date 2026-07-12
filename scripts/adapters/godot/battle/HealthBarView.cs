using Godot;

public partial class HealthBarView : Node2D
{
	private Sprite2D _empty;
	private Sprite2D _full;
	private bool _ready;
	private float _fill = 1f;
	private float _emptyWidth;
	private float _fullTexWidth;

	public override void _Ready()
	{
		_empty = GetNode<Sprite2D>("Empty");
		_full = GetNode<Sprite2D>("Full");
		_empty.Scale = new Vector2(1.8f, 1.8f);
		_full.Scale = new Vector2(1.8f, 1.8f);
		_emptyWidth = _empty.Texture.GetWidth() * _empty.Scale.X;
		_fullTexWidth = _full.Texture.GetWidth() * _full.Scale.X;
		_full.Centered = false;
		_full.Position = new Vector2(_emptyWidth, 0f);
		_ready = true;
		ApplyFill();
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

	private void ApplyFill()
	{
		float visibleFullWidth = Mathf.Max(0.0001f, _fill) * _emptyWidth;
		_full.Scale = new Vector2(visibleFullWidth / _fullTexWidth, 1.8f);
		_full.Position = new Vector2(_emptyWidth - visibleFullWidth, 0f);
	}
}
