using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Battle;
using Godot;
using System;

public enum SoldierState
{
	Marching,
	Fighting,
	Retaliating,
	Sieging
}

public partial class Soldier : Area2D
{
	public bool IsPlayerUnit { get; set; }
	public bool IsAlive { get; private set; } = true;

	public int Health { get; private set; } = 30;
	public int MaxHealth { get; private set; } = 30;
	public int Damage { get; private set; } = 10;
	public float Speed { get; private set; } = 252f;
	public float AttackRange { get; private set; } = 90f;
	public float AttackCooldown { get; private set; } = 1f;
	public float VisionRange { get; private set; } = 250f;
	public bool HasNightCombat { get; set; }
	public float DisplaySize { get; private set; } = 125f;
	public float CollisionRadius { get; private set; } = 50f;
	public AttackType AttackType { get; private set; }
	public DamageType DamageType { get; private set; }
	public ArmorType ArmorType { get; private set; } = ArmorType.Light;

	private uint _unitColor;
	private AttackBehavior _attackBehavior;
	private bool _statsPending;
	private NavigationAgent2D _navigationAgent;
	private float _attackTimer;
	private float _hitFlashTimer;
	internal Soldier _targetEnemy;
	private Castle _targetCastle;
	internal Building _targetBuilding;
	private SoldierState _state;
	private Sprite2D _sprite;
	private SoldierSleepZEffect _sleepZEffect;
	private Color _baseSpriteModulate = Colors.White;

	public void InitializeFromStats(UnitStats stats)
	{
		MaxHealth = stats.Health;
		Health = stats.Health;
		Damage = stats.Damage;
		Speed = GameCoordinatesAdapter.GameUnitsToPixels(stats.Speed);
		AttackRange = GameCoordinatesAdapter.GameUnitsToPixels(stats.AttackRange);
		AttackCooldown = stats.AttackCooldown;
		VisionRange = GameCoordinatesAdapter.GameUnitsToPixels(stats.VisionRange);
		HasNightCombat = stats.HasNightCombat;
		DisplaySize = stats.DisplaySize;
		CollisionRadius = stats.CollisionRadius;
		_unitColor = stats.UnitColor;
		AttackType = stats.AttackType;
		DamageType = stats.DamageType;
		ArmorType = stats.ArmorType;

		_attackBehavior = stats.AttackType == AttackType.Ranged
			? new RangedAttack(this)
			: new MeleeAttack(this);

		_statsPending = true;
	}

	private void ApplyPendingStats()
	{
		if (!_statsPending)
			return;
		_statsPending = false;

		float displaySize = GameCoordinatesAdapter.GameUnitsToPixels(DisplaySize);
		_baseSpriteModulate = new Color(
			((_unitColor >> 16) & 0xFF) / 255f,
			((_unitColor >> 8) & 0xFF) / 255f,
			(_unitColor & 0xFF) / 255f);

		if (_sprite != null)
		{
			Texture2D texture = _sprite.Texture;
			if (texture != null)
			{
				float scale = displaySize / System.Math.Max(texture.GetWidth(), texture.GetHeight());
				_sprite.Scale = new Vector2(scale, scale);
			}
			_sprite.Position = new Vector2(0, -displaySize * 0.5f);
		}
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
		_navigationAgent = GetNode<NavigationAgent2D>("Logic/NavigationAgent");

		_sprite = GetNodeOrNull<Sprite2D>("View/Sprite");
		_sleepZEffect = GetNodeOrNull<SoldierSleepZEffect>("Effects/SleepZEffect");

		ApplyPendingStats();

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
		float radius = GameCoordinatesAdapter.GameUnitsToPixels(CollisionRadius);
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

		float myRadius = GameCoordinatesAdapter.GameUnitsToPixels(CollisionRadius);
		float edgeDist = float.MaxValue;

		BattleManager bm = AdapterRegistry.Resolve<BattleManager>();
		Building overlappingBuilding = bm?.FindOverlappingBuilding(this);

		if (overlappingBuilding != null)
		{
			_targetBuilding = overlappingBuilding;
			_targetCastle = overlappingBuilding.GetCastle();
		}
		else if (_targetBuilding != null && _targetCastle != null)
		{
			if (!_targetBuilding.IsDestroyed && _targetCastle.IsAlive)
			{
				Rect2 rect = BattleManager.GetBuildingRectStatic(_targetBuilding);
				if (!rect.HasPoint(GlobalPosition))
				{
					_targetBuilding = null;
					_targetCastle = null;
				}
			}
			else
			{
				_targetBuilding = null;
				_targetCastle = null;
			}
		}

		if (_targetEnemy != null && _targetEnemy.IsAlive)
		{
			float targetRadius = GameCoordinatesAdapter.GameUnitsToPixels(_targetEnemy.CollisionRadius);
			edgeDist = GlobalPosition.DistanceTo(_targetEnemy.GlobalPosition) - myRadius - targetRadius;
			_state = edgeDist <= VisionRange ? SoldierState.Fighting : SoldierState.Retaliating;
		}
		else if (_targetBuilding != null && !_targetBuilding.IsDestroyed
			&& _targetCastle != null && _targetCastle.IsAlive)
		{
			_state = SoldierState.Sieging;
		}
		else
		{
			_state = SoldierState.Marching;
		}

		switch (_state)
		{
			case SoldierState.Fighting:
			case SoldierState.Retaliating:
				if (edgeDist <= AttackRange)
				{
					_navigationAgent.AvoidanceEnabled = false;
					if (_attackTimer <= 0 && _attackBehavior.TryExecute(_targetEnemy, dt))
						_attackTimer = AttackCooldown;
				}
				else
				{
					_navigationAgent.AvoidanceEnabled = true;
					_navigationAgent.TargetPosition = _targetEnemy.GlobalPosition;
					MoveTowardTarget(dt);
				}
				break;

			case SoldierState.Sieging:
				_navigationAgent.AvoidanceEnabled = false;
				if (_attackTimer <= 0)
				{
					if (_targetBuilding.Health > 0)
						_targetBuilding.TakeDamage(Damage);
					_attackTimer = AttackCooldown;
				}
				break;

			case SoldierState.Marching:
				_navigationAgent.AvoidanceEnabled = true;
				_targetEnemy = null;
				_navigationAgent.TargetPosition = SelectTarget();
				MoveTowardTarget(dt);
				break;
		}

		QueueRedraw();
	}

	private void MoveTowardTarget(float dt)
	{
		Vector2 next = _navigationAgent.GetNextPathPosition();
		Vector2 dir = (next - GlobalPosition).Normalized();
		GlobalPosition += dir * Speed * dt;
	}

	private Vector2 SelectTarget()
	{
		GameManager gm = AdapterRegistry.Resolve<GameManager>();
		Castle targetCastle = IsPlayerUnit ? gm?.EnemyCastle : gm?.PlayerCastle;

		if (targetCastle != null && targetCastle.IsAlive)
			return targetCastle.Heart.GlobalPosition;

		if (targetCastle != null)
			return targetCastle.GlobalPosition + new Vector2(
				targetCastle.GridColumns * targetCastle.CellSize / 2f,
				targetCastle.GridRows * targetCastle.CellSize / 2f);

		float dir = IsPlayerUnit ? GameCoordinatesAdapter.PixelsPerCell : -GameCoordinatesAdapter.PixelsPerCell;
		return new Vector2(GlobalPosition.X + dir, GlobalPosition.Y);
	}

	public void TakeDamage(int amount, Soldier attacker = null)
	{
		if (!IsAlive) return;

		Health = CombatRules.ApplyDamage(Health, amount);
		_hitFlashTimer = 0.1f;
		if (_sprite != null)
			_sprite.Modulate = Colors.White;

		if (attacker != null && attacker.IsAlive && attacker.IsPlayerUnit != IsPlayerUnit)
		{
			float dist = GlobalPosition.DistanceTo(attacker.GlobalPosition);
			if (dist > VisionRange)
			{
				_targetEnemy = attacker;

				BattleManager bm = AdapterRegistry.Resolve<BattleManager>();
				if (bm != null)
					bm.PropagateRetaliation(this, VisionRange, attacker);
			}
		}

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

		Tween tween = CreateTween();
		tween.TweenProperty(this, "scale", Vector2.Zero, 0.25f);
		tween.Parallel().TweenProperty(this, "modulate:a", 0f, 0.25f);
		tween.TweenCallback(Callable.From(() => QueueFree()));
	}
}
