// 建筑节点：领域状态接口、产兵工作、仪式与表现
using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Building;
using CasualCastle.Domain.Shared;
using Godot;
using System.Collections.Generic;
using System.Linq;



public partial class Building : Area2D
{
	private BuildingBridge _bridge;

	public BuildingBridge Bridge => _bridge ??= new BuildingBridge(this, BuildingSys);

	protected Castle CastleRef;

	protected int GridX;

	protected int GridY;

	private readonly BuildingRuntime _runtime = new();

	private ProductionComponent _production;

	private RitualHostComponent _ritualHost;

	private BuildingView _view;

	private BuildingEffects _effects;

	private Node2D _battlefield;

	public Node2D Battlefield => _battlefield;

	private int _spawnCount;

	private bool _isPlayerBuilding;

	private GameManager _gameManager;

	private Shop _shop;

	private AdjacencyService _adjacencyService;

	private RitualService _ritualService;

	private readonly List<IBuildingComponent> _components = new();

	private LivingComponent _living;

	private BuildingSystem _buildingSystemBacking;

	private BuildingSystem BuildingSys => _buildingSystemBacking ??= AdapterRegistry.Resolve<BuildingSystem>();

	[Signal]
	public delegate void HealthChangedEventHandler(int health, int maxHealth);

	public string TypeId { get; set; } = BuildingTypeIds.Barracks;

	public int MaxHealth => _runtime.MaxHealth;

	public int Health => _runtime.Health;

	public bool IsManuallyPaused => _runtime.IsManuallyPaused;

	public bool IsCombineProhibited => _runtime.IsCombineProhibited;

	public bool IsDestroyed => Health <= 0;

	public bool IsOperational => Health > 0 && !IsManuallyPaused;

	public bool IsRitualInProgress => _runtime.IsRitualInProgress;

	public bool CanUseSpellBar()
	{
		bool isAltarUnlocked = _ritualService == null || _ritualService.IsDarkAltarUnlocked;
		return BuildingActionRules.CanUseSpellBar(TypeId, isAltarUnlocked);
	}

	public string ActiveRitualId => _runtime.ActiveRitual?.RitualId;

	public float RitualRemainingSeconds => _runtime.ActiveRitual?.RemainingSeconds ?? 0f;

	public float RitualTotalSeconds => _runtime.ActiveRitual?.TotalSeconds ?? 0f;

	public bool HasProductionCycle => _runtime.HasWorkCycle;

	public float ProductionProgress => _runtime.WorkRequired > 0f ? Mathf.Clamp(_runtime.WorkDone / _runtime.WorkRequired, 0f, 1f) : 0f;

	public float EffectiveProductionRate => _runtime.ProductionRate * _production.SpeedMultiplier * DungeonFactionBuffs.ProductionSpeedMultiplier;

	public float ProductionSecondsRemaining => EffectiveProductionRate > 0f ? (_runtime.WorkRequired - _runtime.WorkDone) / EffectiveProductionRate : 0f;

	public bool ContributesToAdjacency => !IsDestroyed;

	public bool IsDamaged => Health < MaxHealth;

	public string DisplayName => BuildingSys.GetDisplayName(TypeId);

	public int AnchorGridX => GridX;

	public int AnchorGridY => GridY;

	public bool IsPlayerOwned => _isPlayerBuilding;

	public bool HasEnemyOnTop
	{
		get
		{
			ITacticalQueries tactical = GameManager.Get<ITacticalQueries>();
			if (tactical == null || CastleRef == null)
				return false;
			return tactical.HasEnemyOnBuilding(Bridge);
		}
	}

	public Vector2I GetMainGridPosition()
	{
		Vector2I offset = BuildingSys.GetMainCellOffset(TypeId);
		return new Vector2I(AnchorGridX + offset.X, AnchorGridY + offset.Y);
	}

	public Vector2 GetMainCellLocalPosition()
	{
		Vector2I mainGrid = GetMainGridPosition();
		return CastleRef.GetCellCenter(mainGrid.X, mainGrid.Y);
	}

	public void BindToGrid(Castle castle, int gridX, int gridY)
	{
		CastleRef = castle;
		GridX = gridX;
		GridY = gridY;
		_isPlayerBuilding = castle.IsPlayerCastle;
		_view?.SyncStateIconPosition();
	}

	public void SetManuallyPaused(bool paused)
	{
		if (BuildingSys.IsCoreBuilding(TypeId) || IsManuallyPaused == paused)
			return;

		_runtime.SetPaused(paused);
		RefreshOperationalState();
	}

	public void SetCombineProhibited(bool prohibited)
	{
		if (BuildingSys.IsCoreBuilding(TypeId) || IsCombineProhibited == prohibited)
			return;

		_runtime.SetCombineProhibited(prohibited);
		_view?.UpdateStateIcon();
	}

	public void ApplySnapshotState(int health, bool manuallyPaused, bool combineProhibited)
	{
		if (TypeId == BuildingTypeIds.CastleHeart)
			return;

		_runtime.Restore(Mathf.Clamp(health, 1, MaxHealth), MaxHealth, manuallyPaused, combineProhibited);
		_effects?.UpdateDamageVisual();
		RefreshOperationalState();
	}

	public void ApplyHealthRatio(float ratio)
	{
		if (TypeId == BuildingTypeIds.CastleHeart)
			return;

		_runtime.ApplyHealthRatio(ratio);
		_effects?.UpdateDamageVisual();
	}

	public void InitFromType(string buildingType)
	{
		TypeId = buildingType;
		_runtime.Restore(BuildingSys.GetMaxHealth(buildingType), BuildingSys.GetMaxHealth(buildingType), false, false, false);
		MountComponents();
		EnsureVisuals();
		_view.TryApplyVisual();
		_effects?.UpdateDamageVisual();
	}

	private void EnsureVisuals()
	{
		if (_view != null)
			return;

		_view = new BuildingView(this, BuildingSys);
		_effects = new BuildingEffects(this, BuildingSys);
		_view.Initialize();
		_effects.Initialize();
	}


	private void MountComponents()
	{
		_components.Clear();
		_living = null;

		if (TrapRules.IsTrap(TypeId))
			_components.Add(new TrapComponent(this));
		if (LivingBuildingRules.IsLivingBuilding(TypeId))
		{
			_living = new LivingComponent(this);
			_living.HealthRestored += amount =>
			{
				_runtime.Heal(amount);
				_effects?.UpdateDamageVisual();
			};
			_living.Upgraded += () => Sfx.Play(SfxEvents.Upgrade);
			_components.Add(_living);
		}
		if (ManaBuildingRules.IsManaProducer(TypeId))
			_components.Add(new ManaComponent(this));
		if (BuildingSys.GetFactionId(TypeId) == FactionDef.DungeonId)
			_components.Add(new PlagueComponent(this));
		if (CrystalMineRules.IsCrystalMine(TypeId))
			_components.Add(new CrystalMineComponent(this));
		if (TypeId == TrapRules.TreasureBox)
			_components.Add(new TreasureBoxComponent(this));
	}

	public Castle GetCastle() => CastleRef;

	public void TakeDamage(int amount)
	{
		if (amount <= 0 || Health <= 0)
			return;
		if (LivingBuildingRules.IsLivingBuilding(TypeId) && CastleRef != null
			&& !BuildingSys.IsCoreBuilding(TypeId))
		{
			ApplyLivingLinkedDamage(amount);
			return;
		}
		ApplyDamage(amount);
	}

	private void ApplyLivingLinkedDamage(int amount)
	{
		List<IBuildingState> neighbors = GetLivingNeighbors();
		if (neighbors.Count == 0)
		{
			ApplyDamage(amount);
			return;
		}
		int selfShare = amount / 2;
		int neighborShare = amount - selfShare;
		ApplyDamage(selfShare);
		int perNeighbor = neighborShare / neighbors.Count;
		int remainder = neighborShare - perNeighbor * neighbors.Count;
		for (int i = 0; i < neighbors.Count; i++)
		{
			int share = perNeighbor + (i < remainder ? 1 : 0);
			if (neighbors[i] is BuildingBridge neighbor && !neighbor.Building.IsDestroyed)
				neighbor.Building.ApplyDamage(share);
		}
	}

	private List<IBuildingState> GetLivingNeighbors()
	{
		List<IBuildingState> result = new();
		List<IBuildingState> states = CastleRef.GetBuildingStates();
		List<IAdjacencyBuilding> adjacent = states.OfType<IAdjacencyBuilding>().ToList();
		foreach (IAdjacencyBuilding neighbor in AdjacentRules.GetAdjacentBuildings(Bridge, adjacent, GameManager.Get<IBuildingRepository>()))
		{
			if (neighbor is IBuildingState state && !state.IsDestroyed && LivingBuildingRules.IsLivingBuilding(state.TypeId))
				result.Add(state);
		}
		return result;
	}

	private void ApplyDamage(int amount)
	{
		_runtime.TakeDamage(amount);
		_effects?.UpdateDamageVisual();
		Sfx.Play(SfxEvents.BuildingHit);

		if (BuildingSys.IsCoreBuilding(TypeId) && CastleRef != null)
			_gameManager?.OnCastleHeartHealthChanged(CastleRef.IsPlayerCastle, Health, MaxHealth);

		if (Health <= 0)
			OnDestroyed();
		else if (BuildingSys.IsCoreBuilding(TypeId))
			_production.Pause();
	}

	private void OnDestroyed()
	{
		Sfx.Play(SfxEvents.BuildingDestroy);
		if (TypeId != BuildingTypeIds.CastleHeart && CastleRef != null)
			CastleRef.ReleaseBuildingFootprint(this);

		if (TypeId != BuildingTypeIds.CastleHeart)
		{
			RefreshOperationalState();
			_effects?.PlayDestructionEffect();
			if (LivingBuildingRules.IsLivingBuilding(TypeId))
				ScheduleRegrow();
		}
	}

	private void ScheduleRegrow()
	{
		Castle castle = CastleRef;
		string typeId = TypeId;
		int gridX = GridX;
		int gridY = GridY;
		GetTree().CreateTimer(LivingBuildingRules.RegrowDelaySeconds).Timeout += () => LivingBuildingRegrow(castle, typeId, gridX, gridY);
	}

	private static void LivingBuildingRegrow(Castle castle, string typeId, int gridX, int gridY)
	{
		if (castle == null || !GodotObject.IsInstanceValid(castle))
			return;
		if (!LivingBuildingRules.HasLivingNeighbor(gridX, gridY, typeId, castle.GetBuildingStates(), GameManager.Get<IBuildingRepository>()))
			return;
		Building building = AdapterRegistry.Resolve<BuildingSystem>().CreateBuilding(typeId);
		if (building == null)
			return;
		building.BindToGrid(castle, gridX, gridY);
		if (!castle.PlaceBuilding(building, gridX, gridY, typeId))
		{
			building.QueueFree();
			return;
		}
		Sfx.Play(SfxEvents.Upgrade);
	}

	public bool TryRepair()
	{
		if (!CanRepair())
			return false;

		int cost = GetRepairCost();
		if (_shop?.TrySpendGold(cost) != true)
			return false;

		Repair();
		return true;
	}

	public bool CanRepair()
	{
		if (IsRitualInProgress)
			return false;
		bool isPlayerOwned = CastleRef != null && CastleRef.IsPlayerCastle;
		bool isPlaying = _gameManager?.CurrentState == GameManager.GameState.Playing;
		bool isShopPhase = _gameManager != null && _gameManager.IsShop;
		return RepairRules.CanRepair(Health, MaxHealth, BuildingSys.IsCoreBuilding(TypeId),
			isPlayerOwned, HasEnemyOnTop, isPlaying, isShopPhase);
	}

	private void Repair()
	{
		_runtime.Heal(MaxHealth);
		_effects?.UpdateDamageVisual();
		RefreshOperationalState();
	}

	public int GetRepairCost() => RepairRules.GetRepairCost(MaxHealth, Health, GameRules.RepairGoldPerHealth);

	public bool CanWork
	{
		get
		{
			IGameState state = AdapterRegistry.Resolve<IGameState>();
			return IsOperational && !IsRitualInProgress
				&& state.IsCombat;
		}
	}

	public override void _Ready()
	{
		IFieldUnitRepository fieldRepo = GameManager.Get<IFieldUnitRepository>();
		fieldRepo?.RegisterBuilding(Bridge);

		_gameManager = AdapterRegistry.Resolve<GameManager>();
		_shop = AdapterRegistry.Resolve<Shop>();
		_adjacencyService = GameManager.Get<AdjacencyService>();
		_ritualService = GameManager.Get<RitualService>();
		_runtime.HealthChanged += (health, maxHealth) => EmitSignal(SignalName.HealthChanged, health, maxHealth);
		_production = new ProductionComponent(this, _runtime, BuildingSys);
		_production.SpawnRequested += OnSpawnRequested;
		_ritualHost = new RitualHostComponent(this, _runtime, _ritualService, _gameManager);
		_ritualHost.CoreUpgradeRequested += () => CallDeferred(nameof(UpgradeCircleAfterRitual));
		

		EnsureVisuals();

		if (_gameManager != null)
			_gameManager.PhaseChanged += OnPhaseChanged;

		RefreshOperationalState();

		if (CheatState.FastProductionEnabled)
			SetWorkSpeedMultiplier(10f);
		CheatState.FastProductionChanged += OnFastProductionChanged;

		if (BuildingSys.GetProductionRate(TypeId) > 0f)
		{
			_battlefield = _gameManager?.Battlefield;
			if (_battlefield == null)
				_battlefield = GetNodeOrNull<Node2D>("/root/MainGame/Battlefield");
			if (_battlefield == null)
				_battlefield = GetTree().Root.GetNodeOrNull<Node2D>("MainGame/Battlefield");
			RefreshProductionCycle();
		}
		else if (RitualRules.CanHostRitual(TypeId))
		{
			_battlefield = _gameManager?.Battlefield;
			if (_battlefield == null)
				_battlefield = GetTree().Root.GetNodeOrNull<Node2D>("MainGame/Battlefield");
		}

		if (LivingBuildingRules.IsLivingBuilding(TypeId) || TrapRules.IsTrap(TypeId)
			|| ManaBuildingRules.IsManaProducer(TypeId) || CrystalMineRules.IsCrystalMine(TypeId))
			SetProcess(true);
	}

	public override void _ExitTree()
	{
		TavernPartyService tavernParty = GameManager.Get<TavernPartyService>();
		tavernParty?.ReleaseTavern(Bridge);

		IFieldUnitRepository fieldRepo = GameManager.Get<IFieldUnitRepository>();
		fieldRepo?.UnregisterBuilding(Bridge);

		if (_gameManager != null)
			_gameManager.PhaseChanged -= OnPhaseChanged;
		CheatState.FastProductionChanged -= OnFastProductionChanged;
		_production.Stop();
	}

	public Vector2 GetBuildingSize()
	{
		CollisionShape2D shape = GetNodeOrNull<CollisionShape2D>("Logic/CollisionShape");
		if (shape?.Shape is RectangleShape2D rect)
			return rect.Size;
		return Vector2.Zero;
	}


	public override void _Process(double delta)
	{
		float step = (float)delta;
		foreach (IBuildingComponent component in _components)
			component.Tick(step);
		_ritualHost.Tick(step);
		_production.Tick(step);
		_view?.UpdateWorkEffectFromProgress(HasProductionCycle, ProductionProgress);
	}

	protected virtual void OnPhaseChanged(MatchPhase phase)
	{
		RefreshProductionCycle();
	}

	private void OnFastProductionChanged()
	{
		SetWorkSpeedMultiplier(CheatState.FastProductionEnabled ? 10f : 1f);
	}

	public void RefreshProductionCycle()
	{
		_production.RefreshCycle();
	}

	public void SetWorkSpeedMultiplier(float multiplier)
	{
		_production.SetSpeedMultiplier(multiplier);
		_view?.UpdateHordeIndicator();
	}

	private void OnSpawnRequested(string unitTypeId, int count)
	{
		for (int i = 0; i < count; i++)
		{
			SpawnService?.SpawnProduction(this, 1);
			_effects?.PlayWorkJumpVisual();
		}
	}

	public int NextSpawnIndex()
	{
		int index = _spawnCount;
		_spawnCount++;
		return index;
	}

	private UnitSpawnService SpawnService => AdapterRegistry.Resolve<UnitSpawnService>();

	public void OnWorkStarted()
	{
		SetProcess(true);
	}

	public void OnWorkPaused()
	{
		_effects?.PauseJump();
	}

	public void OnWorkResumed()
	{
		_effects?.ResumeJump();
	}

	public void OnWorkStopped()
	{
		SetProcess(false);
		_effects?.StopJump();
		_view?.ResetWorkVisual();
		_effects?.UpdateDamageVisual();
	}


	public bool TryStartRitual(string ritualId)
	{
		if (_ritualService == null || _runtime.IsRitualInProgress || CastleRef == null)
			return false;
		if (!RitualRules.CanHostRitual(TypeId, _ritualService.IsDarkAltarUnlocked) || !IsPlayerOwned)
			return false;

		int coreLevel = RitualCircleRules.GetCoreLevel(CastleRef.GetBuildingStates());
		bool isCoreHosted = RitualRules.IsRitualCircleHost(TypeId);
		BlackMagicService blackMagic = GameManager.Get<BlackMagicService>();
		CardData book = null;
		if (RitualRules.IsStudyRitual(ritualId))
			book = FindUnlearnedBook();
		ActiveRitual started = _ritualService.TryStartRitual(
			ritualId, isCoreHosted, coreLevel,
			_shop.TrySpendGold,
			amount => blackMagic != null && blackMagic.TrySpend(amount),
			book?.Id, book == null ? null : DarkMagicBookRules.GetRitualId(book));
		if (started == null)
			return false;

		_runtime.StartRitual(ritualId, started.TotalSeconds, isCoreHosted, started.BookCardId, started.BookRitualId);
		RefreshProductionCycle();
		_view?.UpdateStateIcon();
		return true;
	}

	private CardData FindUnlearnedBook()
	{
		Hand hand = AdapterRegistry.Resolve<Hand>();
		if (hand == null)
			return null;

		foreach (CardData card in hand.Cards)
		{
			string ritualId = DarkMagicBookRules.GetRitualId(card);
			if (ritualId != null && !_ritualService.IsLearned(ritualId))
				return card;
		}
		return null;
	}

	private void UpgradeCircleAfterRitual()
	{
		string nextTypeId = RitualCircleRules.GetNextLevelTypeId(TypeId);
		if (nextTypeId == null)
		{
			RefreshProductionCycle();
			return;
		}

		CastleRef.ReplaceHeart(nextTypeId, Health);
		Sfx.Play(SfxEvents.Upgrade);
	}


	private void RefreshOperationalState()
	{
		_view?.SyncStateIconPosition();
		_view?.UpdateStateIcon();

		if (!IsOperational)
		{
			if (IsManuallyPaused)
			{
				if (_runtime.HasWorkCycle && !_runtime.WorkPaused)
					_production.Pause();
			}
			else
			{
				_production.Stop();
			}
		}
		else
		{
			RefreshProductionCycle();
		}

		if (IsOperational && (LivingBuildingRules.IsLivingBuilding(TypeId) || TrapRules.IsTrap(TypeId)
			|| ManaBuildingRules.IsManaProducer(TypeId) || CrystalMineRules.IsCrystalMine(TypeId)))
			SetProcess(true);

		Castle castle = CastleRef;
		if (castle != null)
			_adjacencyService.RefreshCastle(castle.GetBuildingStates());
	}

}
