// 单位逻辑节点：桥接领域单位与战场表现，并在死亡后保留逻辑尸体供其他系统使用
using System.Collections.Generic;
using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Building;
using CasualCastle.Domain.Shared;
using Godot;

public partial class UnitLogic : Node2D
{
	internal CasualCastle.Domain.Battle.Unit _unit;
	private NavigationPortAdapter _navPort;
	public IUnitHandle Handle => _unit;
	private IFieldUnitRepository _fieldRepo;
	private ITacticalQueries _tactical;
	private IRvoService _rvoService;
	private UnitEventRelay _eventRelay;
	private UnitVisual _visual;
	private UnitLifecycle _lifecycle;
	private CorpseService _corpseService;

	public bool IsPlayerUnit { get; set; }
	public bool IsAlive => _unit?.IsAlive ?? false;
	public int Health => _unit?.Health ?? 0;
	public int MaxHealth => _unit?.MaxHealth ?? 30;
	public int Damage => _unit?.Damage ?? 0;
	public float DisplaySize { get; private set; }
	public float CollisionRadius => _unit?.CollisionRadius ?? 50f;
	public AttackType AttackType { get; private set; }
	public DamageType DamageType => _unit?.DamageType ?? DamageType.Slash;
	public bool IsSelected { get; private set; }
	public float FuryDamageMultiplier => _unit != null
		? _unit.Skills.GetStatMultiplier("attack_damage_mult", _unit.GameCtx)
		: 1f;
	public float FuryAttackSpeedMultiplier => _unit != null
		? _unit.Skills.GetStatMultiplier("attack_speed_mult", _unit.GameCtx)
		: 1f;
	public float FuryMoveSpeedMultiplier => _unit != null
		? _unit.Skills.GetStatMultiplier("move_speed_mult", _unit.GameCtx)
		: 1f;
	public float AuraAttackSpeedMultiplier => _unit?.GameCtx.AuraAttackSpeedMultiplier ?? 1f;
	public float AuraMoveSpeedMultiplier => _unit?.GameCtx.AuraMoveSpeedMultiplier ?? 1f;
	public float AuraAttackDamageMultiplier => _unit?.GameCtx.AuraAttackDamageMultiplier ?? 1f;
	public float AuraRegenPerSecond => _unit?.GameCtx.AuraRegenPerSecond ?? 0f;
	public float AuraExecuteDamageMultiplier => _unit?.GameCtx.AuraExecuteDamageMultiplier ?? 1f;
	public int AuraArmorBonus => _unit?.GameCtx.AuraArmorBonus ?? 0;
	public bool IsFurious => _unit?.IsFurious ?? false;

	private bool _statsPending;
	private NavigationAgent2D _navigationAgent;
	private Node2D _body;
	private Vector2 _safeVelocity;
	private bool _isDead;
	private const float PushSyncEpsilonSq = 0.01f;

	public void InitializeFromStats(UnitStats stats)
	{
		if (_unit == null)
		{
			_body = GetParent<Node2D>();
			_navigationAgent = GetNode<NavigationAgent2D>("NavigationAgent");
			_navigationAgent.VelocityComputed += OnVelocityComputed;
			_navPort = new NavigationPortAdapter(_navigationAgent);
			_unit = new CasualCastle.Domain.Battle.Unit(_navPort);
			_eventRelay = new UnitEventRelay();
			AddChild(_eventRelay);
			_eventRelay.Attack += OnAttack;
			_unit.EventPort = _eventRelay;
			_fieldRepo = GameManager.Get<IFieldUnitRepository>();
			_tactical = GameManager.Get<ITacticalQueries>();
			_rvoService = GameManager.Get<IRvoService>();
			_visual = new UnitVisual();
			_lifecycle = new UnitLifecycle();
			_corpseService = GameManager.Get<CorpseService>();
		}
			_unit.Initialize(stats, IsPlayerUnit);
			_unit.SetDamageMatrix(GameManager.Get<DamageMatrix>());
			_unit.Tactical = _tactical;
			_unit.BlackMagic = GameManager.Get<BlackMagicService>();
			ISkillRepository skillRepo = GameManager.Get<ISkillRepository>();
		IReadOnlyList<SkillDef> skills = skillRepo.GetByUnitType(stats.TypeId);
		foreach (SkillDef skill in skills)
			_unit.Skills.Add(skill);
		_rvoService.ConfigureRvo(_navPort, _unit.CollisionRadius);

		if (_fieldRepo != null)
			_fieldRepo.Register(_unit);

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
			_unit.TypeId,
			GameCoordinatesAdapter.GameUnitsToPixels(DisplaySize));
		_visual.SetHealth(Health, MaxHealth);
	}

	public void SetTarget(UnitLogic target)
	{
		_unit?.SetEnemyTarget(target?.Handle);
	}

	public void RefreshHealthVisual()
	{
		_visual?.SetHealth(Health, MaxHealth);
	}

	public override void _Ready()
	{
		_body = GetParent<Node2D>();
		_visual?.Initialize(_body);

		ApplyPendingStats();

		if (_unit != null)
		{
			_unit.SetPosition(
				GameCoordinatesAdapter.PixelsToGameUnits(_body.GlobalPosition.X),
				GameCoordinatesAdapter.PixelsToGameUnits(_body.GlobalPosition.Y));
		}


		if (AdapterRegistry.Resolve<GameManager>() != null)
			AdapterRegistry.Resolve<GameManager>().PhaseChanged += OnPhaseChanged;

		if (_eventRelay != null)
		{
			_eventRelay.Damaged += OnDamaged;
			_eventRelay.Died += OnDied;
			_eventRelay.Healed += OnHealed;
		}

		UpdateSleepVisual();
	}

	public override void _ExitTree()
	{
		if (!_isDead)
			_fieldRepo?.Unregister(_unit);

		if (AdapterRegistry.Resolve<GameManager>() != null)
			AdapterRegistry.Resolve<GameManager>().PhaseChanged -= OnPhaseChanged;

		if (_eventRelay != null)
			_eventRelay.Healed -= OnHealed;
	}

	private void OnPhaseChanged(GameManager.GamePhase phase)
	{
		UpdateSleepVisual();
	}

	private bool IsActive
	{
		get
		{
			IGameState state = AdapterRegistry.Resolve<IGameState>();
			bool isNightUnit = _unit != null && _unit.HasNightCombat;
			return isNightUnit ? state.IsNight : state.IsDay;
		}
	}

    private bool IsSleeping =>
        IsAlive
        && AdapterRegistry.Resolve<GameManager>()?.CurrentState == GameManager.GameState.Playing
        && !AdapterRegistry.Resolve<GameManager>().IsPaused
        && !IsActive;

	private void UpdateSleepVisual()
	{
		_visual?.UpdateSleepVisual(IsActive, IsSleeping);
	}

	private void UpdateStealthVisual()
	{
		bool stealthed = _unit != null && _unit.IsStealthed;
		_visual?.SetStealthVisual(stealthed);
		UpdateBuffs();
	}

	private void UpdateBuffs()
	{
		List<string> buffs = new();
		if (_unit != null && _unit.IsStealthed)
			buffs.Add("隐身");
		if (_unit != null && _unit.ShieldHealth > 0)
			buffs.Add("护盾");
		if (IsFurious)
			buffs.Add("狂怒");
		if (_unit != null && _unit.IsAmbushing)
			buffs.Add("伏击");
		if (AuraAttackSpeedMultiplier > 1.01f)
			buffs.Add($"攻速光环 +{Mathf.RoundToInt((AuraAttackSpeedMultiplier - 1f) * 100f)}%");
		if (AuraMoveSpeedMultiplier > 1.01f)
			buffs.Add($"移速光环 +{Mathf.RoundToInt((AuraMoveSpeedMultiplier - 1f) * 100f)}%");
		if (AuraAttackDamageMultiplier > 1.01f)
			buffs.Add($"攻击光环 +{Mathf.RoundToInt((AuraAttackDamageMultiplier - 1f) * 100f)}%");
		if (AuraRegenPerSecond > 0f)
			buffs.Add("回血光环");
		if (AuraExecuteDamageMultiplier > 1.01f)
			buffs.Add("斩杀光环");
		if (AuraArmorBonus > 0)
			buffs.Add($"护甲光环 +{AuraArmorBonus}");
		_visual?.SetBuffs(buffs.ToArray());
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

		Vector2 pushedPixelPos = new Vector2(
			GameCoordinatesAdapter.GameUnitsToPixels(_unit.GameX),
			GameCoordinatesAdapter.GameUnitsToPixels(_unit.GameY));
		if ((pushedPixelPos - _body.GlobalPosition).LengthSquared() > PushSyncEpsilonSq)
			_body.GlobalPosition = pushedPixelPos;

		UpdateSleepVisual();
		UpdateStealthVisual();
		if (!IsActive) return;
		if (_fieldRepo == null || _body == null) return;

		(IUnitHandle nearest, _) = _tactical.FindNearestEnemy(_unit);

		IBuildingTarget buildingTarget = _tactical.FindOverlappingBuilding(_unit);
		if (buildingTarget != null)
			_unit.SetBuildingTarget(buildingTarget);
		else
			_unit.ClearBuildingTarget();

		_unit.UpdateTargeting(nearest);

		float marchX, marchY;
		Vector2 marchDest = SelectTarget();
		marchX = GameCoordinatesAdapter.PixelsToGameUnits(marchDest.X);
		marchY = GameCoordinatesAdapter.PixelsToGameUnits(marchDest.Y);

		_unit.UpdateBehavior(dt, marchX, marchY);

		Vector2 currentPixelPos = _body.GlobalPosition;
		Vector2 nextPathPos = _navigationAgent.GetNextPathPosition();
		Vector2 direction = nextPathPos - currentPixelPos;
		float pixelSpeed = GameCoordinatesAdapter.GameUnitsToPixels(_unit.GetCurrentSpeed());
		Vector2 desiredVelocity = direction.Length() > 0.001f
			? direction.Normalized() * pixelSpeed
			: Vector2.Zero;
		_navigationAgent.Velocity = desiredVelocity;

		Vector2 moveVelocity = _safeVelocity != Vector2.Zero ? _safeVelocity : desiredVelocity;
		_body.GlobalPosition += moveVelocity * dt;

		if (Mathf.Abs(moveVelocity.X) > 0.1f)
			_visual.SetFlipH(moveVelocity.X < 0);

		_unit.SetPosition(
			GameCoordinatesAdapter.PixelsToGameUnits(_body.GlobalPosition.X),
			GameCoordinatesAdapter.PixelsToGameUnits(_body.GlobalPosition.Y));

	}

	private void OnVelocityComputed(Vector2 safeVelocity)
	{
		_safeVelocity = safeVelocity;
	}

	public void TakeDamage(int amount, UnitLogic attacker = null, bool isAoe = false)
	{
		if (!IsAlive) return;

		float atkX = 0, atkY = 0;
		if (attacker != null && attacker._body != null)
		{
			atkX = GameCoordinatesAdapter.PixelsToGameUnits(attacker._body.GlobalPosition.X);
			atkY = GameCoordinatesAdapter.PixelsToGameUnits(attacker._body.GlobalPosition.Y);
		}
		_unit.TakeDamage(amount, attacker?.Handle, atkX, atkY, isAoe);
	}

	private Vector2 SelectTarget()
	{
			IBuildingRef tavern = _unit.AssignedTavern;
			if (tavern != null)
			{
				(float centerX, float centerY) = TavernPartyService.GetTavernStandTarget(_unit, tavern);
			return new Vector2(
				GameCoordinatesAdapter.GameUnitsToPixels(centerX),
				GameCoordinatesAdapter.GameUnitsToPixels(centerY));
			}

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
		Sfx.Play(GetHitSfxEvent(_eventRelay.LastAttacker));
		IUnitHandle attacker = _eventRelay.LastAttacker;
		if (attacker != null && attacker.IsAlive)
			_tactical.PropagateRetaliation(_unit, attacker);
	}

	private string GetAttackSfxEvent()
	{
		if (AttackType == AttackType.Melee)
		{
			switch (DamageType)
			{
				case DamageType.Slash:
					return SfxEvents.MeleeSlash;
				case DamageType.Pierce:
					return SfxEvents.MeleePierce;
				default:
					return SfxEvents.MeleeAttack;
			}
		}

		switch (DamageType)
		{
			case DamageType.Pierce:
				return SfxEvents.LaunchArrow;
			case DamageType.Magic:
			case DamageType.Shadow:
				return SfxEvents.LaunchMagic;
			default:
				return SfxEvents.ProjectileLaunch;
		}
	}

	private static string GetHitSfxEvent(IUnitHandle attacker)
	{
		if (attacker == null)
			return SfxEvents.Hit;

		switch (attacker.DamageType)
		{
			case DamageType.Slash:
				return SfxEvents.HitSlash;
			case DamageType.Pierce:
				return SfxEvents.HitPierce;
			case DamageType.Magic:
			case DamageType.Shadow:
				return SfxEvents.HitMagic;
			default:
				return SfxEvents.Hit;
		}
	}

	private void OnDied()
	{
		_isDead = true;
		Shop shop = AdapterRegistry.Resolve<Shop>();
		if (shop != null && shop.FactionId == FactionDef.NecromancerId)
			GameManager.Get<BlackMagicService>()?.OnUnitDied(IsPlayerUnit, _unit?.Race);
		_visual?.UpdateSleepVisual(false, false);
		_lifecycle?.PlayDeathAnimation(_body);
		Sfx.Play(SfxEvents.UnitDeath);
		TavernPartyService tavernParty = GameManager.Get<TavernPartyService>();
		tavernParty?.RemoveOnDeath(_unit);
		_corpseService?.ScheduleCorpseRemoval(_unit);
	}

	private void OnHealed()
	{
		if (_body == null || _eventRelay == null)
			return;

		Node root = GetTree().CurrentScene;
		if (root == null)
			return;

		Projectile projectile = new Projectile();
		root.AddChild(projectile);
		projectile.SetModulate(new Color(0.5f, 1f, 0.6f));
		projectile.PlayHitSound = false;

		Vector2 fromPos = _body.GlobalPosition;
		Vector2 toPos = new Vector2(
			GameCoordinatesAdapter.GameUnitsToPixels(_eventRelay.LastHealTargetX),
			GameCoordinatesAdapter.GameUnitsToPixels(_eventRelay.LastHealTargetY));
		Texture2D texture = LoadProjectileTexture("res://assets/art/projectiles/magic_ball.png");
		if (texture == null)
			return;

		projectile.Launch(fromPos, toPos, GameRules.HealProjectileSpeed, texture);
	}

	private static System.Collections.Generic.Dictionary<string, Texture2D> _projectileTextures;

	private void OnAttack()
	{
		if (_body == null || _eventRelay == null)
			return;

		Node root = GetTree().CurrentScene;
		if (root == null)
			return;

		Sfx.Play(GetAttackSfxEvent());
		Projectile projectile = new Projectile();
		root.AddChild(projectile);
		projectile.PlayHitSound = AttackType == AttackType.Ranged;

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
			if (DamageType == DamageType.Magic || DamageType == DamageType.Shadow)
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
