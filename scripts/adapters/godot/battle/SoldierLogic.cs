using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Battle;
using Godot;
using System;

public partial class SoldierLogic : Node2D
{
	internal ISoldierService _service;
	private UnitSpatialService _spatial;

	public bool IsPlayerUnit { get; set; }
	public bool IsAlive => _service?.IsAlive ?? false;
	public int Health => _service?.Health ?? 0;
	public int MaxHealth { get; private set; } = 30;
	public int Damage => _service?.Damage ?? 0;
	public float Speed { get; private set; } = 252f;
	public float AttackRange { get; private set; } = 90f;
	public float AttackCooldown { get; private set; } = 1f;
	public float VisionRange { get; private set; } = 250f;
	public bool HasNightCombat { get; set; }
	public float DisplaySize { get; private set; } = 125f;
	public float CollisionRadius => _service?.CollisionRadius ?? 50f;
	public AttackType AttackType { get; private set; }
	public DamageType DamageType { get; private set; }
	public ArmorType ArmorType { get; private set; } = ArmorType.Light;

	private uint _unitColor;
	private bool _statsPending;
	private NavigationAgent2D _navigationAgent;
	private float _hitFlashTimer;
	internal SoldierLogic _targetEnemy;
	private Sprite2D _sprite;
	private SoldierSleepZEffect _sleepZEffect;
	private Node2D _body;
	private Color _baseSpriteModulate = Colors.White;

	public void InitializeFromStats(UnitStats stats)
	{
		if (_service == null)
		{
			_body = GetParent<Node2D>();
			_navigationAgent = GetNode<NavigationAgent2D>("NavigationAgent");
			SoldierService svc = new SoldierService();
			_service = svc;
			_spatial = AdapterRegistry.Resolve<UnitSpatialService>();
			_spatial?.Register(svc);
		}
		_service.Initialize(stats, IsPlayerUnit);

		MaxHealth = stats.Health;
		Speed = GameCoordinatesAdapter.GameUnitsToPixels(stats.Speed);
		AttackRange = GameCoordinatesAdapter.GameUnitsToPixels(stats.AttackRange);
		AttackCooldown = stats.AttackCooldown;
		VisionRange = GameCoordinatesAdapter.GameUnitsToPixels(stats.VisionRange);
		HasNightCombat = stats.HasNightCombat;
		DisplaySize = stats.DisplaySize;
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

	public void SetTarget(SoldierLogic target)
	{
		_service?.SetEnemyTarget(target?._service);
	}

	public override void _Ready()
	{
		_body = GetParent<Node2D>();
		_sprite = _body?.GetNodeOrNull<Sprite2D>("View/Sprite");
		_sleepZEffect = _body?.GetNodeOrNull<SoldierSleepZEffect>("Effects/SleepZEffect");

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
		_spatial?.Unregister(_service);

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
		if (_spatial == null || _body == null) return;

		_service.GameX = GameCoordinatesAdapter.PixelsToGameUnits(_body.GlobalPosition.X);
		_service.GameY = GameCoordinatesAdapter.PixelsToGameUnits(_body.GlobalPosition.Y);

		(ISoldierService nearest, float edgeDist) = _spatial.FindNearestEnemy(_service);
		(object bld, object cstl) = _spatial.FindOverlappingBuilding(_service);
		_service.TargetBuilding = bld;
		_service.TargetCastle = cstl;

		_service.SetEnemyTarget(nearest);
		_service.UpdateTargeting(nearest, edgeDist);

		float marchX = IsPlayerUnit ? float.MaxValue : 0;
		float marchY = _service.GameY;
		(float newX, float newY) = _service.UpdateBehavior(dt, edgeDist, marchX, marchY);

		_body.GlobalPosition = new Vector2(
			GameCoordinatesAdapter.GameUnitsToPixels(newX),
			GameCoordinatesAdapter.GameUnitsToPixels(newY));

		if (_navigationAgent != null)
			_navigationAgent.AvoidanceEnabled = _service.State != SoldierState.Sieging
				&& !(_service.State == SoldierState.Fighting && edgeDist <= _service.AttackRange)
				&& !(_service.State == SoldierState.Retaliating && edgeDist <= _service.AttackRange);

		_body.QueueRedraw();
	}

	public void TakeDamage(int amount, SoldierLogic attacker = null)
	{
		if (!IsAlive) return;

		float atkX = 0, atkY = 0;
		if (attacker != null && attacker._body != null)
		{
			atkX = GameCoordinatesAdapter.PixelsToGameUnits(attacker._body.GlobalPosition.X);
			atkY = GameCoordinatesAdapter.PixelsToGameUnits(attacker._body.GlobalPosition.Y);
		}
		_service.TakeDamage(amount, attacker?._service, atkX, atkY);

		if (attacker != null && attacker.IsAlive)
			_spatial.PropagateRetaliation(_service, attacker._service);

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
