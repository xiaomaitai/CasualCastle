using System.Collections.Generic;
using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Shared;
using Godot;

public partial class SoldierLogic : Node2D
{
	internal CasualCastle.Domain.Battle.Soldier _soldier;
	private NavigationPortAdapter _navPort;
	public ISoldierHandle Handle => _soldier;
	private IFieldUnitRepository _fieldRepo;
	private ITacticalQueries _tactical;
	private IRvoService _rvoService;
	private SoldierEventRelay _eventRelay;
	private SoldierVisual _visual;
	private SoldierLifecycle _lifecycle;

	public bool IsPlayerUnit { get; set; }
	public bool IsAlive => _soldier?.IsAlive ?? false;
	public int Health => _soldier?.Health ?? 0;
	public int MaxHealth => _soldier?.MaxHealth ?? 30;
	public int Damage => _soldier?.Damage ?? 0;
	public bool HasNightCombat => _soldier?.HasNightCombat ?? false;
	public float DisplaySize { get; private set; }
	public float CollisionRadius => _soldier?.CollisionRadius ?? 50f;
	public AttackType AttackType { get; private set; }
	public DamageType DamageType => _soldier?.DamageType ?? DamageType.Normal;
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
			_eventRelay.Attack += OnAttack;
			_soldier.EventPort = _eventRelay;
			_fieldRepo = GameManager.Get<IFieldUnitRepository>();
			_tactical = GameManager.Get<ITacticalQueries>();
			_rvoService = GameManager.Get<IRvoService>();
			_visual = new SoldierVisual();
			_lifecycle = new SoldierLifecycle();
		}
		_soldier.Initialize(stats, IsPlayerUnit);
		_soldier.SetDamageMatrix(GameManager.Get<DamageMatrix>());
		ISkillRepository skillRepo = GameManager.Get<ISkillRepository>();
		IReadOnlyList<SkillDef> skills = skillRepo.GetByUnitType(stats.TypeId);
		foreach (SkillDef skill in skills)
			_soldier.Skills.Add(skill);
		_rvoService.ConfigureRvo(_navPort, _soldier.CollisionRadius);

		if (_fieldRepo != null)
			_fieldRepo.Register(_soldier);

		DisplaySize = ComputeDisplaySize(stats.Size);
		AttackType = stats.AttackType;

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

		(ISoldierHandle nearest, float edgeDist) = _tactical.FindNearestEnemy(_soldier);

		IBuildingTarget buildingTarget = _tactical.FindOverlappingBuilding(_soldier);
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
			_tactical.PropagateRetaliation(_soldier, attacker);
	}

	private void OnDied()
	{
		_visual?.UpdateSleepVisual(false, false);

		_lifecycle?.PlayDeathAnimation(this, () =>
		{
			_fieldRepo?.Unregister(_soldier);
		});
	}

	private static System.Collections.Generic.Dictionary<string, Texture2D> _projectileTextures;

	private void OnAttack()
	{
		if (_body == null || _eventRelay == null)
			return;

		Node root = GetTree().CurrentScene;
		if (root == null)
			return;

		Projectile projectile = new Projectile();
		root.AddChild(projectile);

		Vector2 fromPos = _body.GlobalPosition;
		float targetPixelX = GameCoordinatesAdapter.GameUnitsToPixels(_eventRelay.LastAttackTargetX);
		float targetPixelY = GameCoordinatesAdapter.GameUnitsToPixels(_eventRelay.LastAttackTargetY);
		Vector2 toPos = new Vector2(targetPixelX, targetPixelY);

		string texturePath = GetProjectileTexturePath();
		Texture2D texture = LoadProjectileTexture(texturePath);
		if (texture == null)
			return;

		float speed = AttackType == AttackType.Melee ? GameRules.ProjectileSpeedMelee : GameRules.ProjectileSpeedRanged;
		projectile.Launch(fromPos, toPos, speed, texture);
	}

	private string GetProjectileTexturePath()
	{
		if (AttackType == AttackType.Ranged)
		{
			if (DamageType == DamageType.Magic)
				return "res://assets/art/projectiles/magic_ball.png";
			return "res://assets/art/projectiles/arrow.png";
		}

		if (DamageType == DamageType.Pierce)
			return "res://assets/art/projectiles/spear.png";
		return "res://assets/art/projectiles/sword.png";
	}

	private static Texture2D LoadProjectileTexture(string path)
	{
		if (_projectileTextures == null)
			_projectileTextures = new System.Collections.Generic.Dictionary<string, Texture2D>();

		if (_projectileTextures.TryGetValue(path, out Texture2D cached))
			return cached;

		Texture2D texture = GD.Load<Texture2D>(path);
		if (texture != null)
			_projectileTextures[path] = texture;
		return texture;
	}

	private static float ComputeDisplaySize(UnitSize size)
	{
		switch (size)
		{
			case UnitSize.Small: return 60f;
			case UnitSize.Medium: return 90f;
			case UnitSize.Large: return 120f;
			case UnitSize.Huge: return 180f;
			default: return 90f;
		}
	}
}
