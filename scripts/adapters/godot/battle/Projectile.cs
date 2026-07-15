using Godot;

public partial class Projectile : Node2D
{
	private Vector2 _targetPos;
	private float _speed;
	private Sprite2D _sprite;

	public override void _Ready()
	{
		_sprite = new Sprite2D();
		AddChild(_sprite);
	}

	public void Launch(Vector2 from, Vector2 to, float speed, Texture2D texture)
	{
		GlobalPosition = from;
		_targetPos = to;
		_speed = speed;
		_sprite.Texture = texture;
		_sprite.Scale = Vector2.One * 1.5f;

		Vector2 dir = to - from;
		if (dir.Length() > 0.01f)
			_sprite.Rotation = dir.Angle();
	}

	public override void _Process(double delta)
	{
		Vector2 toTarget = _targetPos - GlobalPosition;
		float step = _speed * (float)delta;

		if (toTarget.Length() <= step)
		{
			QueueFree();
			return;
		}

		GlobalPosition += toTarget.Normalized() * step;
	}
}
