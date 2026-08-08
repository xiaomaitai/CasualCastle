// 建筑节点核心：领域状态接口、血量、修复与网格绑定
using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Building;
using CasualCastle.Domain.Shared;
using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class Building : Area2D, IBuildingState, IBuildingTarget, IBuildingRef
{
	protected Castle CastleRef;

	protected int GridX;

	protected int GridY;

	private float _workSpeedMultiplier = 1f;

	private bool _visualApplied;

	private bool _isHordeBuilding;

	private Sprite2D _sprite;

	private ShaderMaterial _workMaterial;

	private Material _originalMaterial;

	private Tween _jumpTween;

	private Vector2 _spriteBasePosition;

	private float _workRequired;

	private float _workDone;

	private float _productionRate;

	private bool _workActive;

	private bool _workPaused;

	private bool _jumpTweenAwaitingResume;

	private BuildingStateIcon _stateIcon;

	private HordeAdjacencyIndicator _hordeIndicator;

	private Node2D _battlefield;

	private int _spawnCount;

	private bool _isPlayerBuilding;

	private GameManager _gameManager;

	private Shop _shop;

	private AdjacencyService _adjacencyService;

	private RitualService _ritualService;

	private ActiveRitual _activeRitual;

	private BuildingSystem _buildingSystemBacking;

	private BuildingSystem BuildingSys => _buildingSystemBacking ??= AdapterRegistry.Resolve<BuildingSystem>();

	[Signal]
	public delegate void HealthChangedEventHandler(int health, int maxHealth);

	public string TypeId { get; set; } = BuildingTypeIds.Barracks;

	public int MaxHealth { get; private set; }

	public int Health { get; private set; }

	public bool IsManuallyPaused { get; private set; }

	public bool IsCombineProhibited { get; private set; }

	public bool IsDestroyed => Health <= 0;

	public bool IsOperational => Health > 0 && !IsManuallyPaused;

	public bool IsRitualInProgress => _activeRitual != null;

	public string ActiveRitualId => _activeRitual?.RitualId;

	public float RitualRemainingSeconds => _activeRitual?.RemainingSeconds ?? 0f;

	public float RitualTotalSeconds => _activeRitual?.TotalSeconds ?? 0f;

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
			return tactical.HasEnemyOnBuilding(this);
		}
	}

	bool IBuildingRef.IsDestroyed => IsDestroyed;

	bool IBuildingRef.IsCore => BuildingSys.IsCoreBuilding(TypeId);

	bool IBuildingRef.IsEnemyOf(IUnitState unit)
	{
		if (CastleRef == null)
			return false;
		return CastleRef.IsPlayerCastle != unit.IsPlayerUnit;
	}

	float IBuildingRef.MinX => GameCoordinatesAdapter.PixelsToGameUnits(GlobalPosition.X - GetBuildingSize().X * 0.5f);

	float IBuildingRef.MinY => GameCoordinatesAdapter.PixelsToGameUnits(GlobalPosition.Y - GetBuildingSize().Y * 0.5f);

	float IBuildingRef.MaxX => GameCoordinatesAdapter.PixelsToGameUnits(GlobalPosition.X + GetBuildingSize().X * 0.5f);

	float IBuildingRef.MaxY => GameCoordinatesAdapter.PixelsToGameUnits(GlobalPosition.Y + GetBuildingSize().Y * 0.5f);

	IBuildingTarget IBuildingRef.BuildingTarget => this;

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
		SyncStateIconPosition();
	}

	public void SetManuallyPaused(bool paused)
	{
		if (BuildingSys.IsCoreBuilding(TypeId) || IsManuallyPaused == paused)
			return;

		IsManuallyPaused = paused;
		RefreshOperationalState();
	}

	public void SetCombineProhibited(bool prohibited)
	{
		if (BuildingSys.IsCoreBuilding(TypeId) || IsCombineProhibited == prohibited)
			return;

		IsCombineProhibited = prohibited;
		UpdateStateIcon();
	}

	public void ApplySnapshotState(int health, bool manuallyPaused, bool combineProhibited)
	{
		if (TypeId == BuildingTypeIds.CastleHeart)
			return;

		Health = Mathf.Clamp(health, 1, MaxHealth);
		EmitSignal(SignalName.HealthChanged, Health, MaxHealth);
		UpdateDamageVisual();

		IsManuallyPaused = manuallyPaused;
		IsCombineProhibited = combineProhibited;
		RefreshOperationalState();
	}

	public void ApplyHealthRatio(float ratio)
	{
		if (TypeId == BuildingTypeIds.CastleHeart)
			return;

		Health = Mathf.Clamp(Mathf.RoundToInt(MaxHealth * ratio), 1, MaxHealth);
		EmitSignal(SignalName.HealthChanged, Health, MaxHealth);
		UpdateDamageVisual();
	}

	public void InitFromType(string buildingType)
	{
		TypeId = buildingType;
		MaxHealth = BuildingSys.GetMaxHealth(buildingType);
		Health = MaxHealth;
		TryApplyVisual();
		UpdateDamageVisual();
	}

	public Castle GetCastle() => CastleRef;

	public void TakeDamage(int amount)
	{
		if (amount <= 0 || Health <= 0)
			return;

		Health = Mathf.Max(0, Health - amount);
		EmitSignal(SignalName.HealthChanged, Health, MaxHealth);
		UpdateDamageVisual();
		Sfx.Play(SfxEvents.BuildingHit);

		if (BuildingSys.IsCoreBuilding(TypeId) && CastleRef != null)
			_gameManager?.OnCastleHeartHealthChanged(CastleRef.IsPlayerCastle, Health, MaxHealth);

		if (Health <= 0)
			OnDestroyed();
		else if (BuildingSys.IsCoreBuilding(TypeId))
			PauseWork();
	}

	private void OnDestroyed()
	{
		Sfx.Play(SfxEvents.BuildingDestroy);
		if (TypeId != BuildingTypeIds.CastleHeart && CastleRef != null)
			CastleRef.ReleaseBuildingFootprint(this);

		if (TypeId != BuildingTypeIds.CastleHeart)
		{
			RefreshOperationalState();
			PlayDestructionEffect();
		}
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
		Health = MaxHealth;
		EmitSignal(SignalName.HealthChanged, Health, MaxHealth);
		UpdateDamageVisual();
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
		fieldRepo?.RegisterBuilding(this);

		_gameManager = AdapterRegistry.Resolve<GameManager>();
		_shop = AdapterRegistry.Resolve<Shop>();
		_adjacencyService = GameManager.Get<AdjacencyService>();
		_ritualService = GameManager.Get<RitualService>();
		

		_sprite = GetNodeOrNull<Sprite2D>("View/Sprite");
		TryApplyVisual();

		if (_gameManager != null)
			_gameManager.PhaseChanged += OnPhaseChanged;

		_stateIcon = new BuildingStateIcon();
		Node2D effects = GetNodeOrNull<Node2D>("Effects");
		effects?.AddChild(_stateIcon);
		_isHordeBuilding = BuildingSys.GetFactionId(TypeId) == FactionDef.HordeId;
		if (_isHordeBuilding)
		{
			_hordeIndicator = new HordeAdjacencyIndicator();
			effects?.AddChild(_hordeIndicator);
		}
		SyncStateIconPosition();
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
			UpdateWorkCycle();
		}
		else if (RitualRules.CanHostRitual(TypeId))
		{
			_battlefield = _gameManager?.Battlefield;
			if (_battlefield == null)
				_battlefield = GetTree().Root.GetNodeOrNull<Node2D>("MainGame/Battlefield");
		}
	}

	public override void _ExitTree()
	{
		TavernPartyService tavernParty = GameManager.Get<TavernPartyService>();
		tavernParty?.ReleaseTavern(this);

		IFieldUnitRepository fieldRepo = GameManager.Get<IFieldUnitRepository>();
		fieldRepo?.UnregisterBuilding(this);

		if (_gameManager != null)
			_gameManager.PhaseChanged -= OnPhaseChanged;
		CheatState.FastProductionChanged -= OnFastProductionChanged;
		StopWork();
	}

	public Vector2 GetBuildingSize()
	{
		CollisionShape2D shape = GetNodeOrNull<CollisionShape2D>("Logic/CollisionShape");
		if (shape?.Shape is RectangleShape2D rect)
			return rect.Size;
		return Vector2.Zero;
	}

}
