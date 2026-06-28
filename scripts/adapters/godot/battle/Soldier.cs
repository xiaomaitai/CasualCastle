using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Battle;
using Godot;
using System;

public partial class Soldier : Area2D
{
	public SoldierData Data { get; private set; } = new();
	public bool IsPlayerUnit { get; set; }
	public bool IsAlive { get; private set; } = true;

	public int Health { get; private set; } = 30;
	public int Damage { get; private set; } = 10;
	public float Speed { get; private set; } = 170f;
	public float AttackRange { get; private set; } = 60f;
	public float AttackCooldown { get; private set; } = 1f;
	public bool HasNightCombat { get; set; }

	private Vector2 _moveDirection;
	private float _attackTimer;
	private Soldier _targetEnemy;
	private Castle _targetCastle;
	private Building _targetBuilding;
	private Sprite2D _sprite;
	private CollisionShape2D _collisionShape;
	private SoldierSleepZEffect _sleepZEffect;
	private Color _baseSpriteModulate = Colors.White;

	public void InitializeFromStats(UnitStats stats)
	{
		Data = SoldierData.FromStats(stats);
		Health = Data.Health;
		Damage = Data.Damage;
		Speed = Data.Speed;
		AttackRange = Data.AttackRange;
		AttackCooldown = Data.AttackCooldown;
		HasNightCombat = Data.HasNightCombat;

		float displaySize = Data.DisplaySize();
		if (_sprite != null)
		{
			Texture2D texture = _sprite.Texture;
			if (texture != null)
			{
				float scale = displaySize / System.Math.Max(texture.GetWidth(), texture.GetHeight());
				_sprite.Scale = new Vector2(scale, scale);
			}
		}

		if (_collisionShape?.Shape is CircleShape2D circle)
			circle.Radius = Data.CollisionRadius();
	}

	public override void _Ready()
	{
		_moveDirection = IsPlayerUnit ? Vector2.Right : Vector2.Left;

		_sprite = GetNodeOrNull<Sprite2D>("Sprite");
		_collisionShape = GetNodeOrNull<CollisionShape2D>("CollisionShape");
		_sleepZEffect = GetNodeOrNull<SoldierSleepZEffect>("SleepZEffect");

		AreaEntered += OnAreaEntered;
		AreaExited += OnAreaExited;

		if (AdapterRegistry.Resolve<GameManager>() != null)
			AdapterRegistry.Resolve<GameManager>().PhaseChanged += OnPhaseChanged;

		UpdateSleepVisual();
	}

	public override void _ExitTree()
	{
		if (AdapterRegistry.Resolve<GameManager>() != null)
			AdapterRegistry.Resolve<GameManager>().PhaseChanged -= OnPhaseChanged;
	}

	public override void _Draw()
	{
		if (Data == null)
			return;
		float radius = Data.CollisionRadius();
		Color color = IsPlayerUnit
			? new Color(0, 1, 0, 0.3f)
			: new Color(1, 0, 0, 0.3f);
		DrawCircle(Vector2.Zero, radius, color);
	}

	private void OnPhaseChanged(GameManager.GamePhase phase)
	{
		UpdateSleepVisual();
	}

	private bool IsActive => AdapterRegistry.Resolve<NightSystem>()?.CanUnitWork(HasNightCombat) ?? true;

	private bool IsSleeping =>
		IsAlive
		&& AdapterRegistry.Resolve<GameManager>()?.CurrentState == GameManager.GameState.Playing
		&& AdapterRegistry.Resolve<GameManager>().IsNight
		&& !AdapterRegistry.Resolve<GameManager>().IsPaused
		&& !HasNightCombat;

	private void UpdateSleepVisual()
	{
		if (_sprite != null)
		{
			_sprite.Modulate = IsActive
				? _baseSpriteModulate
				: new Color(_baseSpriteModulate.R * 0.75f, _baseSpriteModulate.G * 0.8f, _baseSpriteModulate.B, 0.85f);
		}

		_sleepZEffect?.SetSleeping(IsSleeping);
	}

	public void SetBaseSpriteModulate(Color color)
	{
		_baseSpriteModulate = color;
		UpdateSleepVisual();
	}

	public override void _Process(double delta)
	{
		if (!IsAlive) return;
		if (AdapterRegistry.Resolve<GameManager>()?.CurrentState == GameManager.GameState.GameOver) return;

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

			if (_targetBuilding != null && _targetBuilding.IsDestroyed)
			{
				_targetCastle = null;
				_targetBuilding = null;
			}
			else if (_attackTimer <= 0)
			{
				if (_targetBuilding != null && _targetBuilding.Health > 0)
					_targetBuilding.TakeDamage(Damage);

				_attackTimer = AttackCooldown;
			}
		}
		else
		{
			_targetEnemy = null;
			MoveToward(dt, GlobalPosition + _moveDirection * 2000);
		}
	}

	private void MoveToward(float dt, Vector2 target)
	{
		Vector2 direction = (target - GlobalPosition).Normalized();
		GlobalPosition += direction * Speed * dt;
	}

	private void Attack(Soldier enemy)
	{
		int finalDamage = CombatRules.CalculateDamage(Damage, Data.DamageType, enemy.Data.ArmorType);
		enemy.TakeDamage(finalDamage);
	}

	public void TakeDamage(int amount)
	{
		if (!IsAlive) return;

		Health = CombatRules.ApplyDamage(Health, amount);
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
		SceneTreeTimer timer = GetTree().CreateTimer(0.5f);
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
		if (building != null && !building.IsDestroyed)
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
