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

	[Export]
	public bool HasNightCombat = false;

	public bool IsPlayerUnit { get; set; }
	public bool IsAlive { get; private set; } = true;

	private Vector2 _moveDirection;
	private float _attackTimer = 0f;
	private Soldier _targetEnemy;
	private Castle _targetCastle;
	private Building _targetBuilding;
	private Sprite2D _sprite;
	private CollisionShape2D _collisionShape;
	private SoldierSleepZEffect _sleepZEffect;

	public override void _Ready()
	{
		_moveDirection = IsPlayerUnit ? Vector2.Right : Vector2.Left;

		_sprite = GetNodeOrNull<Sprite2D>("Sprite");
		_collisionShape = GetNodeOrNull<CollisionShape2D>("CollisionShape");
		_sleepZEffect = GetNodeOrNull<SoldierSleepZEffect>("SleepZEffect");

		AreaEntered += OnAreaEntered;
		AreaExited += OnAreaExited;

		if (GameManager.Instance != null)
			GameManager.Instance.PhaseChanged += OnPhaseChanged;

		UpdateSleepVisual();
	}

	public override void _ExitTree()
	{
		if (GameManager.Instance != null)
			GameManager.Instance.PhaseChanged -= OnPhaseChanged;
	}

	private void OnPhaseChanged(GameManager.GamePhase phase)
	{
		UpdateSleepVisual();
	}

	private bool IsActive => NightSystem.CanUnitWork(HasNightCombat);

	private bool IsSleeping =>
		IsAlive
		&& GameManager.Instance?.CurrentState == GameManager.GameState.Playing
		&& GameManager.Instance.IsNight
		&& !GameManager.Instance.IsPaused
		&& !HasNightCombat;

	private void UpdateSleepVisual()
	{
		if (_sprite != null)
			_sprite.Modulate = IsActive ? Colors.White : new Color(0.75f, 0.8f, 1f, 0.85f);

		_sleepZEffect?.SetSleeping(IsSleeping);
	}

	public override void _Process(double delta)
	{
		if (!IsAlive) return;
		if (GameManager.Instance?.CurrentState == GameManager.GameState.GameOver) return;

		UpdateSleepVisual();
		if (!IsActive) return;

		float dt = (float)delta;

		if (_attackTimer > 0)
			_attackTimer -= dt;

		if (_targetEnemy != null && _targetEnemy.IsAlive)
		{
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
				MoveToward(dt, _targetEnemy.GlobalPosition);
			}
		}
		else if (_targetCastle != null && _targetCastle.IsAlive)
		{
			_targetEnemy = null;
			if (_attackTimer <= 0)
			{
				if (_targetBuilding != null && _targetBuilding.Health > 0)
					_targetBuilding.TakeDamage(Damage);

				_attackTimer = AttackCooldown;
			}
		}
		else
		{
			_targetEnemy = null;
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
			Die();
	}

	private void Die()
	{
		IsAlive = false;
		_sleepZEffect?.SetSleeping(false);

		if (_collisionShape != null)
			_collisionShape.Disabled = true;

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
			return;
		}

		Building building = area as Building;
		if (building != null)
		{
			Castle castle = building.GetCastle();
			if (castle != null && castle.IsAlive && castle.IsPlayerCastle != IsPlayerUnit)
			{
				_targetCastle = castle;
				_targetBuilding = building;
			}
		}
	}

	private void OnAreaExited(Area2D area)
	{
		Soldier other = area as Soldier;
		if (other == _targetEnemy)
			_targetEnemy = null;

		Building building = area as Building;
		if (building != null && building.GetCastle() == _targetCastle)
		{
			_targetCastle = null;
			_targetBuilding = null;
		}
	}
}
