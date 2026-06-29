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
	public int MaxHealth { get; private set; } = 30;
	public int Damage { get; private set; } = 10;
	public float Speed { get; private set; } = 252f;
	public float AttackRange { get; private set; } = 90f;
	public float AttackCooldown { get; private set; } = 1f;
	public bool HasNightCombat { get; set; }

	private AttackBehavior _attackBehavior;
	private bool _statsPending;
	private NavigationAgent2D _navigationAgent;
	private float _attackTimer;
	private float _hitFlashTimer;
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
		MaxHealth = Data.Health;
		Health = Data.Health;
		Damage = Data.Damage;
		Speed = GameCoordinatesAdapter.GameUnitsToPixels(Data.Speed);
		AttackRange = GameCoordinatesAdapter.GameUnitsToPixels(Data.AttackRange);
		AttackCooldown = Data.AttackCooldown;
		HasNightCombat = Data.HasNightCombat;

		_attackBehavior = Data.AttackType == AttackType.Ranged
			? new RangedAttack(this)
			: new MeleeAttack(this);

		_statsPending = true;
	}

	private void ApplyPendingStats()
	{
		if (!_statsPending)
			return;
		_statsPending = false;

		float displaySize = GameCoordinatesAdapter.GameUnitsToPixels(Data.DisplaySize());
		uint color = Data.UnitColor;
		_baseSpriteModulate = new Color(
			((color >> 16) & 0xFF) / 255f,
			((color >> 8) & 0xFF) / 255f,
			(color & 0xFF) / 255f);

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
			circle.Radius = GameCoordinatesAdapter.GameUnitsToPixels(Data.CollisionRadius());
	}

	public void SetTarget(Soldier target)
	{
		if (target != null && target.IsAlive && target.IsPlayerUnit != IsPlayerUnit)
			_targetEnemy = target;
		else
			_targetEnemy = null;
	}

	public override void _Ready()
	{
		_navigationAgent = GetNode<NavigationAgent2D>("NavigationAgent");

		_sprite = GetNodeOrNull<Sprite2D>("Sprite");
		_collisionShape = GetNodeOrNull<CollisionShape2D>("CollisionShape");
		_sleepZEffect = GetNodeOrNull<SoldierSleepZEffect>("SleepZEffect");

		ApplyPendingStats();

		AreaEntered += OnAreaEntered;
		AreaExited += OnAreaExited;

		BattleManager battleManager = AdapterRegistry.Resolve<BattleManager>();
		if (battleManager != null)
			battleManager.Register(this);

		if (AdapterRegistry.Resolve<GameManager>() != null)
			AdapterRegistry.Resolve<GameManager>().PhaseChanged += OnPhaseChanged;

		UpdateSleepVisual();
	}

	public override void _ExitTree()
	{
		BattleManager battleManager = AdapterRegistry.Resolve<BattleManager>();
		if (battleManager != null)
			battleManager.Unregister(this);

		if (AdapterRegistry.Resolve<GameManager>() != null)
			AdapterRegistry.Resolve<GameManager>().PhaseChanged -= OnPhaseChanged;
	}

	public override void _Draw()
	{
		if (Data == null)
			return;

		float radius = GameCoordinatesAdapter.GameUnitsToPixels(Data.CollisionRadius());
		Color circleColor = IsPlayerUnit
			? new Color(0, 1, 0, 0.3f)
			: new Color(1, 0, 0, 0.3f);
		DrawCircle(Vector2.Zero, radius, circleColor);

		float barWidth = radius * 2f;
		float barHeight = 4f;
		float barY = -radius - 8f;
		float healthPercent = (float)Health / MaxHealth;

		DrawRect(new Rect2(-barWidth / 2f, barY, barWidth, barHeight), new Color(0.2f, 0.2f, 0.2f, 0.8f));
		DrawRect(new Rect2(-barWidth / 2f, barY, barWidth * healthPercent, barHeight), new Color(0.2f, 0.8f, 0.2f, 0.9f));
	}

	private void OnPhaseChanged(GameManager.GamePhase phase)
	{
		UpdateSleepVisual();
	}

	private bool IsActive => NightRules.CanUnitWork(HasNightCombat, AdapterRegistry.Resolve<IGameState>().IsDay);

	private bool IsSleeping =>
		IsAlive
		&& AdapterRegistry.Resolve<GameManager>()?.CurrentState == GameManager.GameState.Playing
		&& AdapterRegistry.Resolve<GameManager>().IsNight
		&& !AdapterRegistry.Resolve<GameManager>().IsPaused
		&& !HasNightCombat;

	private void UpdateSleepVisual()
	{
		if (_sprite != null && _hitFlashTimer <= 0f)
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

		float dt = (float)delta;

		if (_hitFlashTimer > 0f)
		{
			_hitFlashTimer -= dt;
			if (_hitFlashTimer <= 0f && _sprite != null)
				_sprite.Modulate = _baseSpriteModulate;
		}

		UpdateSleepVisual();
		if (!IsActive) return;

		if (_attackTimer > 0)
			_attackTimer -= dt;

		if (_targetEnemy != null && _targetEnemy.IsAlive)
		{
			float dist = GlobalPosition.DistanceTo(_targetEnemy.GlobalPosition);
			float myRadius = GameCoordinatesAdapter.GameUnitsToPixels(Data.CollisionRadius());
			float targetRadius = GameCoordinatesAdapter.GameUnitsToPixels(_targetEnemy.Data.CollisionRadius());
			if ((dist - myRadius - targetRadius) <= AttackRange)
			{
				if (_attackTimer <= 0 && _attackBehavior.TryExecute(_targetEnemy, dt))
					_attackTimer = AttackCooldown;
			}
			else
			{
				_navigationAgent.TargetPosition = _targetEnemy.GlobalPosition;
			}
		}
		else if (_targetCastle != null && _targetCastle.IsAlive)
		{
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
			_navigationAgent.TargetPosition = GetDestination();
		}

		// NavigationAgent2D-driven movement
		if (!_navigationAgent.IsNavigationFinished())
		{
			Vector2 next = _navigationAgent.GetNextPathPosition();
			Vector2 dir = (next - GlobalPosition).Normalized();
			GlobalPosition += dir * Speed * dt;
		}

		QueueRedraw();
	}

	private Vector2 GetDestination()
	{
		GameManager gm = AdapterRegistry.Resolve<GameManager>();
		if (gm == null)
		{
			Vector2 fallbackDir = IsPlayerUnit ? Vector2.Right : Vector2.Left;
			return GlobalPosition + fallbackDir * 2000;
		}

		Castle targetCastle = IsPlayerUnit ? gm.EnemyCastle : gm.PlayerCastle;
		if (targetCastle == null)
		{
			Vector2 fallbackDir = IsPlayerUnit ? Vector2.Right : Vector2.Left;
			return GlobalPosition + fallbackDir * 2000;
		}

		float frontX = IsPlayerUnit
			? targetCastle.GlobalPosition.X - 120f
			: targetCastle.GlobalPosition.X + targetCastle.GridColumns * targetCastle.CellSize + 120f;

		return new Vector2(frontX, GlobalPosition.Y);
	}

	public void TakeDamage(int amount)
	{
		if (!IsAlive) return;

		Health = CombatRules.ApplyDamage(Health, amount);
		_hitFlashTimer = 0.1f;
		if (_sprite != null)
			_sprite.Modulate = Colors.White;

		if (Health <= 0)
			Die();
	}

	private void Die()
	{
		IsAlive = false;
		_sleepZEffect?.SetSleeping(false);

		BattleManager battleManager = AdapterRegistry.Resolve<BattleManager>();
		if (battleManager != null)
			battleManager.Unregister(this);

		if (_collisionShape != null)
			_collisionShape.Disabled = true;

		Tween tween = CreateTween();
		tween.TweenProperty(this, "scale", Vector2.Zero, 0.25f);
		tween.Parallel().TweenProperty(this, "modulate:a", 0f, 0.25f);
		tween.TweenCallback(Callable.From(() => QueueFree()));
	}

	private void OnAreaEntered(Area2D area)
	{
		if (!IsAlive) return;

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
		Building building = area as Building;
		if (building != null && building.GetCastle() == _targetCastle)
		{
			_targetCastle = null;
			_targetBuilding = null;
		}
	}
}
