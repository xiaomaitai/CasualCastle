// 单位逻辑核心：领域单位桥接、初始化与生命周期
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

}
