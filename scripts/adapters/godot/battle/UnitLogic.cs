// 单位逻辑节点：桥接领域单位与战场表现，并在死亡后保留逻辑尸体供其他系统使用
using CasualCastle.Adapters.Godot;
using CasualCastle.Adapters.Godot.Battle;
using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Building;
using CasualCastle.Domain.Shared;
using Godot;
using System.Collections.Generic;



public partial class UnitLogic : Node2D
{
	internal CasualCastle.Domain.Battle.Unit _unit;
	private RangePreview _rangePreview;

	public IUnitHandle Handle => _unit;

	public IUnitHandle LastAttacker => _eventRelay?.LastAttacker;

	private IFieldUnitRepository _fieldRepo;

	private ITacticalQueries _tactical;

	private UnitEventRelay _eventRelay;

	private UnitVisual _visual;

	private UnitPresentationModel _buffModel;

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

	private Node2D _body;

	private bool _isDead;

	private readonly ProjectilePresenter _projectiles = new();

	private DeathSideEffects _deathSideEffects;

	private const float BodySyncEpsilonSq = 0.01f;

	public void InitializeFromStats(UnitStats stats)
	{
		if (_unit == null)
		{
			_body = GetParent<Node2D>();
			_unit = new CasualCastle.Domain.Battle.Unit();
			_eventRelay = new UnitEventRelay();
			AddChild(_eventRelay);
			_eventRelay.Attack += OnAttack;
			_unit.EventPort = _eventRelay;
			_fieldRepo = GameManager.Get<IFieldUnitRepository>();
			_tactical = GameManager.Get<ITacticalQueries>();
			_visual = new UnitVisual();
			_lifecycle = new UnitLifecycle();
			_corpseService = GameManager.Get<CorpseService>();
		}
			_unit.Initialize(stats, IsPlayerUnit);
			_unit.SetDamageMatrix(GameManager.Get<DamageMatrix>());
			_unit.Tactical = _tactical;
			_unit.Resources = new FactionResources(GameManager.Get<BlackMagicService>(), GameManager.Get<ManaService>(), null);
			ISkillRepository skillRepo = GameManager.Get<ISkillRepository>();
		IReadOnlyList<SkillDef> skills = skillRepo.GetByUnitType(stats.TypeId);
		foreach (SkillDef skill in skills)
			_unit.Skills.Add(skill);

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

	public void SetComfort(ComfortService comfort)
	{
		if (_unit != null)
		{
			if (_unit.Resources == null)
				_unit.Resources = new FactionResources(null, null, null);
			_unit.Resources.Comfort = comfort;
		}
	}

	public void ConfigureDefender(float patrolCenterX, float patrolCenterY, float patrolRadius)
	{
		if (_unit == null)
			return;
		_unit.SetDefender(patrolCenterX, patrolCenterY, patrolRadius);
	}

	public void SetResting(bool resting)
	{
		if (_unit != null)
			_unit.SetResting(resting);
	}

	public void ApplyKingSkill(SkillDef skill)
	{
		if (_unit != null && skill != null)
			_unit.Skills.Add(skill);
	}

	public void RemoveKingSkill(string skillId)
	{
		if (_unit != null && !string.IsNullOrEmpty(skillId))
			_unit.Skills.Remove(skillId);
	}

	public void SetPatrolRadius(float radius)
	{
		if (_unit != null)
			_unit.SetPatrolRadius(radius);
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
			_eventRelay.Summoned += OnSummoned;
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
		if (_eventRelay != null)
			_eventRelay.Summoned -= OnSummoned;
	}

	private void OnPhaseChanged(MatchPhase phase)
	{
		UpdateSleepVisual();
	}

	private bool IsActive
	{
		get
		{
			IGameState state = AdapterRegistry.Resolve<IGameState>();
			return state.IsCombat;
		}
	}

	private bool IsSleeping =>
		IsAlive
		&& AdapterRegistry.Resolve<GameManager>()?.CurrentState == GameManager.GameState.Playing
		&& !AdapterRegistry.Resolve<GameManager>().IsPaused
		&& !IsActive;


	private const float FlipThresholdSpeed = 0.1f;
	private const float SmallDisplaySize = 60f;
	private const float MediumDisplaySize = 90f;
	private const float LargeDisplaySize = 120f;
	private const float HugeDisplaySize = 180f;
	public override void _PhysicsProcess(double delta)
	{
		if (!IsAlive) return;
		if (AdapterRegistry.Resolve<GameManager>()?.CurrentState == GameManager.GameState.GameOver) return;

		float dt = (float)delta;

		_visual?.UpdateHitFlash(dt);

		SyncBodyPosition();
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
	}

	private void SyncBodyPosition()
	{
		Vector2 target = new Vector2(
			GameCoordinatesAdapter.GameUnitsToPixels(_unit.GameX),
			GameCoordinatesAdapter.GameUnitsToPixels(_unit.GameY));
		if ((target - _body.GlobalPosition).LengthSquared() > BodySyncEpsilonSq)
			_body.GlobalPosition = target;
		if (Mathf.Abs(_unit.NavVelX) > FlipThresholdSpeed)
			_visual.SetFlipH(_unit.NavVelX < 0);
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
		if (_unit != null && _unit.IsDefender)
		{
			return new Vector2(
				GameCoordinatesAdapter.GameUnitsToPixels(_unit.PatrolCenterX),
				GameCoordinatesAdapter.GameUnitsToPixels(_unit.PatrolCenterY));
		}

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
		SpawnFloatingDamage();
		Sfx.Play(UnitSfx.GetHitSfxEvent(_eventRelay.LastAttacker));
		IUnitHandle attacker = _eventRelay.LastAttacker;
		if (attacker != null && attacker.IsAlive)
			_tactical.PropagateRetaliation(_unit, attacker);
	}

	private void SpawnFloatingDamage()
	{
		if (_body == null || _eventRelay == null)
			return;
		Node root = GetTree().CurrentScene;
		if (root == null || _eventRelay.LastDamageAmount <= 0)
			return;
		FloatingText.Spawn(root, _body.GlobalPosition, _eventRelay.LastDamageAmount, false);
	}

	private void OnDied()
	{
		_isDead = true;
		(_deathSideEffects ??= new DeathSideEffects(this)).Process();
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

		Vector2 fromPos = _body.GlobalPosition;
		Vector2 toPos = new Vector2(
			GameCoordinatesAdapter.GameUnitsToPixels(_eventRelay.LastHealTargetX),
			GameCoordinatesAdapter.GameUnitsToPixels(_eventRelay.LastHealTargetY));
		_projectiles.SpawnHeal(root, fromPos, toPos);
		SpawnFloatingHeal(root, toPos);
	}

	private void SpawnFloatingHeal(Node root, Vector2 position)
	{
		if (_eventRelay.LastHealAmount <= 0)
			return;
		FloatingText.Spawn(root, position, _eventRelay.LastHealAmount, true);
	}

	private void OnSummoned()
	{
		if (_isDead || _eventRelay == null || _body == null)
			return;
		GameManager gameManager = AdapterRegistry.Resolve<GameManager>();
		UnitSpawnService spawnService = AdapterRegistry.Resolve<UnitSpawnService>();
		if (gameManager == null || gameManager.Battlefield == null || spawnService == null)
			return;
		float pixelX = GameCoordinatesAdapter.GameUnitsToPixels(_eventRelay.LastSummonX);
		float pixelY = GameCoordinatesAdapter.GameUnitsToPixels(_eventRelay.LastSummonY);
		spawnService.SpawnUnitAt(gameManager.Battlefield, _eventRelay.LastSummonUnitType, new Vector2(pixelX, pixelY), IsPlayerUnit);
	}

	private void OnAttack()
	{
		if (_body == null || _eventRelay == null)
			return;

		Node root = GetTree().CurrentScene;
		if (root == null)
			return;

		Sfx.Play(UnitSfx.GetAttackSfxEvent(AttackType, DamageType));
		Vector2 fromPos = _body.GlobalPosition;
		float targetPixelX = GameCoordinatesAdapter.GameUnitsToPixels(_eventRelay.LastAttackTargetX);
		float targetPixelY = GameCoordinatesAdapter.GameUnitsToPixels(_eventRelay.LastAttackTargetY);
		Vector2 toPos = new Vector2(targetPixelX, targetPixelY);
		_projectiles.SpawnAttack(root, fromPos, toPos, AttackType, DamageType, AttackType == AttackType.Ranged);
		TargetRing.Spawn(root, toPos);
	}

	private static float ComputeDisplaySize(UnitSize size)
	{
		switch (size)
		{
			case UnitSize.Small: return SmallDisplaySize;
			case UnitSize.Medium: return MediumDisplaySize;
			case UnitSize.Large: return LargeDisplaySize;
			case UnitSize.Huge: return HugeDisplaySize;
			default: return MediumDisplaySize;
		}
	}


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
		if (_buffModel == null)
			_buffModel = new UnitPresentationModel();
		string[] buffs = _buffModel.GetBuffLabels(
			_unit != null && _unit.IsStealthed,
			_unit != null && _unit.ShieldHealth > 0,
			IsFurious,
			_unit != null && _unit.IsAmbushing,
			AuraAttackSpeedMultiplier,
			AuraMoveSpeedMultiplier,
			AuraAttackDamageMultiplier,
			AuraRegenPerSecond,
			AuraExecuteDamageMultiplier,
			AuraArmorBonus);
		_visual?.SetBuffs(buffs);
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
		UpdateRangePreview(selected);
	}

	private void UpdateRangePreview(bool selected)
	{
		if (_body == null)
			return;
		if (selected)
		{
			if (_rangePreview != null)
				return;
			float radius = GameCoordinatesAdapter.GameUnitsToPixels(Mathf.Max(_unit.AttackRange, _unit.HealRange));
			RangePreview preview = new RangePreview();
			preview.ZIndex = -1;
			preview.Configure(radius);
			_body.AddChild(preview);
			_rangePreview = preview;
		}
		else if (_rangePreview != null)
		{
			_rangePreview.QueueFree();
			_rangePreview = null;
		}
	}

}
