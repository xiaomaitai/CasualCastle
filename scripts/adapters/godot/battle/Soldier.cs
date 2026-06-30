using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Battle;
using Godot;
using System;
using DomainSoldier = CasualCastle.Domain.Battle.Soldier;

public partial class Soldier : Area2D
{
	internal DomainSoldier _domain;
	private UnitSpatialService _spatial;

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
	private bool _statsPending;
	private NavigationAgent2D _navigationAgent;
	private float _hitFlashTimer;
	internal Soldier _targetEnemy;
	private Sprite2D _sprite;
	private SoldierSleepZEffect _sleepZEffect;
	private Color _baseSpriteModulate = Colors.White;

	public void InitializeFromStats(UnitStats stats)
	{
		if (_domain == null)
		{
			_navigationAgent = GetNode<NavigationAgent2D>("Logic/NavigationAgent");
			_domain = new DomainSoldier();
			_spatial = AdapterRegistry.Resolve<UnitSpatialService>();
			_spatial?.Register(_domain);
		}
		_domain.Initialize(stats, IsPlayerUnit);

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
		_domain.TargetEnemy = target?._domain;
	}

	public override void _Ready()
	{
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
		_spatial?.Unregister(_domain);

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
		if (_spatial == null) return;

		_domain.GameX = GameCoordinatesAdapter.PixelsToGameUnits(GlobalPosition.X);
		_domain.GameY = GameCoordinatesAdapter.PixelsToGameUnits(GlobalPosition.Y);

		(DomainSoldier nearest, float edgeDist) = _spatial.FindNearestEnemy(_domain);
		(object bld, object cstl) = _spatial.FindOverlappingBuilding(_domain);
		_domain.TargetBuilding = bld;
		_domain.TargetCastle = cstl;

		_domain.TargetEnemy = nearest;
		_domain.UpdateTargeting(nearest, edgeDist);

		float marchX = IsPlayerUnit ? float.MaxValue : 0;
		float marchY = _domain.GameY;
		(float newX, float newY) = _domain.UpdateBehavior(dt, edgeDist, marchX, marchY);

		GlobalPosition = new Vector2(
			GameCoordinatesAdapter.GameUnitsToPixels(newX),
			GameCoordinatesAdapter.GameUnitsToPixels(newY));

		_navigationAgent.AvoidanceEnabled = _domain.State != SoldierState.Sieging
			&& !(_domain.State == SoldierState.Fighting && edgeDist <= _domain.AttackRange)
			&& !(_domain.State == SoldierState.Retaliating && edgeDist <= _domain.AttackRange);

		QueueRedraw();
	}

	public void TakeDamage(int amount, Soldier attacker = null)
	{
		if (!IsAlive) return;

		float atkX = attacker != null ? GameCoordinatesAdapter.PixelsToGameUnits(attacker.GlobalPosition.X) : 0;
		float atkY = attacker != null ? GameCoordinatesAdapter.PixelsToGameUnits(attacker.GlobalPosition.Y) : 0;
		_domain.TakeDamage(amount, attacker?._domain, atkX, atkY);
		Health = _domain.Health;

		if (attacker != null && attacker.IsAlive && _domain.TargetEnemy == attacker._domain)
			_spatial.PropagateRetaliation(_domain, attacker._domain);

		_hitFlashTimer = 0.1f;
		if (_sprite != null)
			_sprite.Modulate = Colors.White;

		if (!IsAlive)
		{
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
}
