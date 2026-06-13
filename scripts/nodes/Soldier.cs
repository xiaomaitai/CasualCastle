using Godot;
using System;

public partial class Soldier : Area2D
{
	[Export]
	public int Health = 30;

	[Export]
	public int Damage = 10;

	[Export]
	public float Speed = 80.0f;

	[Export]
	public float AttackRange = 30.0f;

	[Export]
	public float AttackCooldown = 1.0f;

	public bool IsPlayerUnit { get; set; }
	public bool IsAlive { get; private set; } = true;

	private Vector2 _moveDirection;
	private float _attackTimer = 0f;
	private Soldier _targetEnemy;
	private Sprite2D _sprite;
	private CollisionShape2D _collisionShape;

	public override void _Ready()
	{
		// 根据阵营决定移动方向
		_moveDirection = IsPlayerUnit ? Vector2.Right : Vector2.Left;

		// 获取子节点
		_sprite = GetNodeOrNull<Sprite2D>("Sprite");
		_collisionShape = GetNodeOrNull<CollisionShape2D>("CollisionShape");

		// 自动连接 Area2D 信号
		AreaEntered += OnAreaEntered;
		AreaExited += OnAreaExited;
	}

	public override void _Process(double delta)
	{
		if (!IsAlive) return;

		float dt = (float)delta;

		// 攻击冷却
		if (_attackTimer > 0)
		{
			_attackTimer -= dt;
		}

		if (_targetEnemy != null && _targetEnemy.IsAlive)
		{
			// 攻击范围内则攻击
			float dist = GlobalPosition.DistanceTo(_targetEnemy.GlobalPosition);
			if (dist <= AttackRange)
			{
				if (_attackTimer <= 0)
				{
					Attack(_targetEnemy);
					_attackTimer = AttackCooldown;
				}
			}
			else
			{
				// 追击敌人
				MoveToward(dt, _targetEnemy.GlobalPosition);
			}
		}
		else
		{
			_targetEnemy = null;
			// 向敌方方向移动
			MoveToward(dt, GlobalPosition + _moveDirection * 1000);
		}
	}

	private void MoveToward(float dt, Vector2 target)
	{
		Vector2 direction = (target - GlobalPosition).Normalized();
		GlobalPosition += direction * Speed * dt;
	}

	private void Attack(Soldier enemy)
	{
		enemy.TakeDamage(Damage);
	}

	public void TakeDamage(int amount)
	{
		if (!IsAlive) return;

		Health -= amount;
		if (Health <= 0)
		{
			Die();
		}
	}

	private void Die()
	{
		IsAlive = false;

		// 禁用碰撞
		if (_collisionShape != null)
			_collisionShape.Disabled = true;

		// 简单的死亡效果：隐藏后删除
		Modulate = new Color(1, 1, 1, 0.3f);
		var timer = GetTree().CreateTimer(0.5f);
		timer.Timeout += () => QueueFree();
	}

	private void OnAreaEntered(Area2D area)
	{
		if (!IsAlive) return;

		Soldier other = area as Soldier;
		if (other != null && other.IsAlive && other.IsPlayerUnit != IsPlayerUnit)
		{
			_targetEnemy = other;
		}
	}

	private void OnAreaExited(Area2D area)
	{
		Soldier other = area as Soldier;
		if (other == _targetEnemy)
		{
			_targetEnemy = null;
		}
	}
}
