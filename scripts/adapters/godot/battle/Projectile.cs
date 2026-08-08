// 弹体表现：从攻击方飞向目标，命中后播放命中音效并销毁。
using Godot;

public partial class Projectile : Node2D
{
	private const float SpriteScale = 1.5f;
	private const float MinDirectionLength = 0.01f;
	private Vector2 _targetPos;
	private float _speed;
	private Sprite2D _sprite;

	public bool PlayHitSound { get; set; } = true;

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
		_sprite.Scale = Vector2.One * SpriteScale;

		Vector2 dir = to - from;
		if (dir.Length() > MinDirectionLength)
			_sprite.Rotation = dir.Angle();
	}

	public override void _Process(double delta)
	{
		Vector2 toTarget = _targetPos - GlobalPosition;
		float step = _speed * (float)delta;

		if (toTarget.Length() <= step)
		{
			if (PlayHitSound)
				Sfx.Play(SfxEvents.ProjectileHit);
			QueueFree();
			return;
		}

		GlobalPosition += toTarget.Normalized() * step;
	}
}