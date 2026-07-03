using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Battle;
using Godot;

public partial class SoldierLogic : Node2D
{
	internal ISoldierService _service;
	public ISoldierService SoldierService => _service;
	private IFieldUnitRepository _fieldRepo;
	private SoldierEventRelay _eventRelay;
	private SoldierVisual _visual;
	private SoldierLifecycle _lifecycle;

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
	private Node2D _body;

	public void InitializeFromStats(UnitStats stats)
	{
		if (_service == null)
		{
			_body = GetParent<Node2D>();
			_navigationAgent = GetNode<NavigationAgent2D>("NavigationAgent");
			SoldierService svc = new SoldierService();
			_eventRelay = new SoldierEventRelay();
			AddChild(_eventRelay);
			svc.EventPort = _eventRelay;
			svc.NavPort = new NavigationPortAdapter(_navigationAgent);
			_service = svc;
			_fieldRepo = GameManager.Get<IFieldUnitRepository>();
			_visual = new SoldierVisual();
			_lifecycle = new SoldierLifecycle();
		}
		_service.Initialize(stats, IsPlayerUnit);
		if (_fieldRepo != null)
			_fieldRepo.Register(_service);

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

		_visual.ApplyStats(_unitColor, GameCoordinatesAdapter.GameUnitsToPixels(DisplaySize));
	}

	public void SetTarget(SoldierLogic target)
	{
		_service?.SetEnemyTarget(target?.SoldierService);
	}

	public override void _Ready()
	{
		_body = GetParent<Node2D>();
		_visual?.Initialize(_body);

		ApplyPendingStats();

		if (_service != null)
		{
			_service.MoveTo(
				GameCoordinatesAdapter.PixelsToGameUnits(_body.GlobalPosition.X),
				GameCoordinatesAdapter.PixelsToGameUnits(_body.GlobalPosition.Y));
		}


		if (AdapterRegistry.Resolve<GameManager>() != null)
			AdapterRegistry.Resolve<GameManager>().PhaseChanged += OnPhaseChanged;

		if (_eventRelay != null)
		{
			_eventRelay.Damaged += OnDamaged;
			_eventRelay.Died += OnDied;
		}

		UpdateSleepVisual();
	}

	public override void _ExitTree()
	{
		_fieldRepo?.Unregister(_service);

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
		_visual?.UpdateSleepVisual(IsActive, IsSleeping);
	}

	public void SetBaseSpriteModulate(Color color)
	{
		_visual?.SetBaseModulate(color);
		UpdateSleepVisual();
	}

	public override void _Process(double delta)
	{
		if (!IsAlive) return;
		if (AdapterRegistry.Resolve<GameManager>()?.CurrentState == GameManager.GameState.GameOver) return;

		float dt = (float)delta;

		_visual?.UpdateHitFlash(dt);

		UpdateSleepVisual();
		if (!IsActive) return;
		if (_fieldRepo == null || _body == null) return;

		(ISoldierService nearest, float edgeDist) = _fieldRepo.FindNearestEnemy(_service);

		IBuildingTarget buildingTarget = _fieldRepo.FindOverlappingBuilding(_service);
		if (buildingTarget != null)
			_service.SetBuildingTarget(buildingTarget);
		else
			_service.ClearBuildingTarget();

		_service.UpdateTargeting(nearest, edgeDist);

		float marchX, marchY;
		Vector2 marchDest = SelectTarget();
		marchX = GameCoordinatesAdapter.PixelsToGameUnits(marchDest.X);
		marchY = GameCoordinatesAdapter.PixelsToGameUnits(marchDest.Y);

		_service.UpdateBehavior(dt, edgeDist, marchX, marchY);

		_body.GlobalPosition = new Vector2(
			GameCoordinatesAdapter.GameUnitsToPixels(_service.GameX),
			GameCoordinatesAdapter.GameUnitsToPixels(_service.GameY));

		if (_navigationAgent != null)
			_navigationAgent.AvoidanceEnabled = _service.State == SoldierState.Marching;

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
		_service.TakeDamage(amount, attacker?.SoldierService, atkX, atkY);
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
		return new Vector2(_body.GlobalPosition.X + dir, _body.GlobalPosition.Y);
	}

	private void OnDamaged()
	{
		_visual?.StartHitFlash();
		ISoldierService attacker = _eventRelay.LastAttacker;
		if (attacker != null && attacker.IsAlive)
			_fieldRepo.PropagateRetaliation(_service, attacker);
	}

	private void OnDied()
	{
		_visual?.UpdateSleepVisual(false, false);

		_lifecycle?.PlayDeathAnimation(this, () =>
		{
			_fieldRepo?.Unregister(_service);
		});
	}
}
