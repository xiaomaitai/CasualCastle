using CasualCastle.Domain.Battle;
using Godot;

public partial class Projectile : Area2D
{
	private const float Speed = 600f;
	private const float HitRadius = 10f;

	private Soldier _target;
	private int _damage;
	private DamageType _damageType;
	private Soldier _attacker;

	public Projectile()
	{
		CircleShape2D shape = new CircleShape2D();
		shape.Radius = HitRadius;
		CollisionShape2D collision = new CollisionShape2D();
		collision.Shape = shape;
		AddChild(collision);

		Sprite2D sprite = new Sprite2D();
		sprite.Modulate = new Color(1, 0.9f, 0.3f);
		sprite.Scale = new Vector2(0.3f, 0.3f);
		AddChild(sprite);
	}

	public void Launch(Vector2 origin, Soldier target, int damage, DamageType damageType, Soldier attacker)
	{
		GlobalPosition = origin;
		_target = target;
		_damage = damage;
		_damageType = damageType;
		_attacker = attacker;

		AreaEntered += OnAreaEntered;
	}

	public override void _Process(double delta)
	{
		if (_target == null || !_target.IsAlive)
		{
			QueueFree();
			return;
		}

		float dt = (float)delta;
		Vector2 direction = (_target.GlobalPosition - GlobalPosition).Normalized();
		GlobalPosition += direction * Speed * dt;

		if (GlobalPosition.DistanceTo(_target.GlobalPosition) <= HitRadius)
			HitTarget();
	}

	private void OnAreaEntered(Area2D area)
	{
		Soldier soldier = area as Soldier;
		if (soldier != null && soldier == _target)
			HitTarget();
	}

	private void HitTarget()
	{
		if (_target == null || !_target.IsAlive)
			return;

		int finalDamage = CombatRules.CalculateDamage(_damage, _damageType, _target.Data.ArmorType);
		_target.TakeDamage(finalDamage, _attacker);
		QueueFree();
	}
}
