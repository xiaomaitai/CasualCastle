using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Battle;
using Godot;

public partial class SoldierLogic : Node2D
{
	internal CasualCastle.Domain.Battle.Soldier _soldier;
	private NavigationPortAdapter _navPort;
	public ISoldierHandle Handle => _soldier;
	private IFieldUnitRepository _fieldRepo;
	private IRvoService _rvoService;
	private SoldierEventRelay _eventRelay;
	private SoldierVisual _visual;
	private SoldierLifecycle _lifecycle;

	public bool IsPlayerUnit { get; set; }
	public bool IsAlive => _soldier?.IsAlive ?? false;
	public int Health => _soldier?.Health ?? 0;
	public int MaxHealth { get; private set; } = 30;
	public int Damage => _soldier?.Damage ?? 0;
	public float Speed { get; private set; } = 252f;
	public float AttackRange { get; private set; } = 90f;
	public float AttackCooldown { get; private set; } = 1f;
	public float VisionRange { get; private set; } = 250f;
	public bool HasNightCombat { get; set; }
	public float DisplaySize { get; private set; } = 125f;
	public float CollisionRadius => _soldier?.CollisionRadius ?? 50f;
	public AttackType AttackType { get; private set; }
	public DamageType DamageType { get; private set; }
	public ArmorType ArmorType { get; private set; } = ArmorType.Light;
	public bool IsSelected { get; private set; }

	private bool _statsPending;
	private NavigationAgent2D _navigationAgent;
	private Node2D _body;
	private Vector2 _safeVelocity;

	public void InitializeFromStats(UnitStats stats)
	{
		if (_soldier == null)
		{
			_body = GetParent<Node2D>();
			_navigationAgent = GetNode<NavigationAgent2D>("NavigationAgent");
			_navigationAgent.VelocityComputed += OnVelocityComputed;
			_navPort = new NavigationPortAdapter(_navigationAgent);
			_soldier = new CasualCastle.Domain.Battle.Soldier(_navPort);
			_eventRelay = new SoldierEventRelay();
			AddChild(_eventRelay);
			_soldier.EventPort = _eventRelay;
			_fieldRepo = GameManager.Get<IFieldUnitRepository>();
			_rvoService = GameManager.Get<IRvoService>();
			_visual = new SoldierVisual();
			_lifecycle = new SoldierLifecycle();
		}
		_soldier.Initialize(stats, IsPlayerUnit);
		_rvoService.ConfigureRvo(_navPort, _soldier.CollisionRadius);

		if (_fieldRepo != null)
			_fieldRepo.Register(_soldier);

		MaxHealth = stats.Health;
		Speed = GameCoordinatesAdapter.GameUnitsToPixels(stats.Speed);
		AttackRange = GameCoordinatesAdapter.GameUnitsToPixels(stats.AttackRange);
		AttackCooldown = stats.AttackCooldown;
		VisionRange = GameCoordinatesAdapter.GameUnitsToPixels(stats.VisionRange);
		HasNightCombat = stats.HasNightCombat;
		DisplaySize = stats.DisplaySize;
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

		_visual.ApplyStats(
			_soldier.TypeId,
			GameCoordinatesAdapter.GameUnitsToPixels(DisplaySize));
		_visual.SetHealth(Health, MaxHealth);
	}

	public void SetTarget(SoldierLogic target)
	{
		_soldier?.SetEnemyTarget(target?.Handle);
	}

	public override void _Ready()
	{
		_body = GetParent<Node2D>();
		_visual?.Initialize(_body);

		ApplyPendingStats();

		if (_soldier != null)
		{
			_soldier.SetPosition(
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
		_fieldRepo?.Unregister(_soldier);

		if (AdapterRegistry.Resolve<GameManager>() != null)
			AdapterRegistry.Resolve<GameManager>().PhaseChanged -= OnPhaseChanged;
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

	public void SetSelected(bool selected)
	{
		IsSelected = selected;
		_visual.SetSelected(selected);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!IsAlive) return;
		if (AdapterRegistry.Resolve<GameManager>()?.CurrentState == GameManager.GameState.GameOver) return;

		float dt = (float)delta;

		_visual?.UpdateHitFlash(dt);

		UpdateSleepVisual();
		if (!IsActive) return;
		if (_fieldRepo == null || _body == null) return;

		(ISoldierHandle nearest, float edgeDist) = _fieldRepo.FindNearestEnemy(_soldier);

		IBuildingTarget buildingTarget = _fieldRepo.FindOverlappingBuilding(_soldier);
		if (buildingTarget != null)
			_soldier.SetBuildingTarget(buildingTarget);
		else
			_soldier.ClearBuildingTarget();

		_soldier.UpdateTargeting(nearest, edgeDist);

		float marchX, marchY;
		Vector2 marchDest = SelectTarget();
		marchX = GameCoordinatesAdapter.PixelsToGameUnits(marchDest.X);
		marchY = GameCoordinatesAdapter.PixelsToGameUnits(marchDest.Y);

		_soldier.UpdateBehavior(dt, edgeDist, marchX, marchY);

		Vector2 currentPixelPos = _body.GlobalPosition;
		Vector2 nextPathPos = _navigationAgent.GetNextPathPosition();
		Vector2 direction = nextPathPos - currentPixelPos;
		float pixelSpeed = GameCoordinatesAdapter.GameUnitsToPixels(_soldier.Speed);
		Vector2 desiredVelocity = direction.Length() > 0.001f
			? direction.Normalized() * pixelSpeed
			: Vector2.Zero;
		_navigationAgent.Velocity = desiredVelocity;

		Vector2 moveVelocity = _safeVelocity != Vector2.Zero ? _safeVelocity : desiredVelocity;
		_body.GlobalPosition += moveVelocity * dt;

		if (Mathf.Abs(moveVelocity.X) > 0.1f)
			_visual.SetFlipH(moveVelocity.X < 0);

		_soldier.SetPosition(
			GameCoordinatesAdapter.PixelsToGameUnits(_body.GlobalPosition.X),
			GameCoordinatesAdapter.PixelsToGameUnits(_body.GlobalPosition.Y));

	}

	private void OnVelocityComputed(Vector2 safeVelocity)
	{
		_safeVelocity = safeVelocity;
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
		_soldier.TakeDamage(amount, attacker?.Handle, atkX, atkY);
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
		_visual.SetHealth(Health, MaxHealth);
		_visual?.StartHitFlash();
		ISoldierHandle attacker = _eventRelay.LastAttacker;
		if (attacker != null && attacker.IsAlive)
			_fieldRepo.PropagateRetaliation(_soldier, attacker);
	}

	private void OnDied()
	{
		_visual?.UpdateSleepVisual(false, false);

		_lifecycle?.PlayDeathAnimation(this, () =>
		{
			_fieldRepo?.Unregister(_soldier);
		});
	}
}
