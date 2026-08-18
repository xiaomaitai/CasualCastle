// 建筑节点：领域状态接口、产兵工作、仪式与表现
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

	private readonly BuildingRuntime _runtime = new();

	private ProductionComponent _production;

	private RitualHostComponent _ritualHost;

	private bool _visualApplied;

	private bool _isHordeBuilding;

	private Sprite2D _sprite;

	private ShaderMaterial _workMaterial;

	private Material _originalMaterial;

	private Tween _jumpTween;

	private Vector2 _spriteBasePosition;

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

		_runtime.SetPaused(paused);
		RefreshOperationalState();
	}

	public void SetCombineProhibited(bool prohibited)
	{
		if (BuildingSys.IsCoreBuilding(TypeId) || IsCombineProhibited == prohibited)
			return;

		_runtime.SetCombineProhibited(prohibited);
		UpdateStateIcon();
	}

	public void ApplySnapshotState(int health, bool manuallyPaused, bool combineProhibited)
	{
		if (TypeId == BuildingTypeIds.CastleHeart)
			return;

		_runtime.Restore(Mathf.Clamp(health, 1, MaxHealth), MaxHealth, manuallyPaused, combineProhibited);
		UpdateDamageVisual();
		RefreshOperationalState();
	}

	public void ApplyHealthRatio(float ratio)
	{
		if (TypeId == BuildingTypeIds.CastleHeart)
			return;

		_runtime.ApplyHealthRatio(ratio);
		UpdateDamageVisual();
	}

	public void InitFromType(string buildingType)
	{
		TypeId = buildingType;
		_runtime.Restore(BuildingSys.GetMaxHealth(buildingType), BuildingSys.GetMaxHealth(buildingType), false, false, false);
		MountComponents();
		TryApplyVisual();
		UpdateDamageVisual();
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
				UpdateDamageVisual();
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
			if (neighbors[i] is Building neighbor && !neighbor.IsDestroyed)
				neighbor.ApplyDamage(share);
		}
	}

	private List<IBuildingState> GetLivingNeighbors()
	{
		List<IBuildingState> result = new();
		List<IBuildingState> states = CastleRef.GetBuildingStates();
		List<IAdjacencyBuilding> adjacent = states.OfType<IAdjacencyBuilding>().ToList();
		foreach (IAdjacencyBuilding neighbor in AdjacentRules.GetAdjacentBuildings(this, adjacent, GameManager.Get<IBuildingRepository>()))
		{
			if (neighbor is IBuildingState state && !state.IsDestroyed && LivingBuildingRules.IsLivingBuilding(state.TypeId))
				result.Add(state);
		}
		return result;
	}

	private void ApplyDamage(int amount)
	{
		_runtime.TakeDamage(amount);
		UpdateDamageVisual();
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
			PlayDestructionEffect();
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
		_runtime.HealthChanged += (health, maxHealth) => EmitSignal(SignalName.HealthChanged, health, maxHealth);
		_production = new ProductionComponent(this, _runtime, BuildingSys);
		_production.CyclesCompleted += OnProductionCyclesCompleted;
		_ritualHost = new RitualHostComponent(this, _runtime, _ritualService, _gameManager);
		_ritualHost.CoreUpgradeRequested += () => CallDeferred(nameof(UpgradeCircleAfterRitual));
		

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
		tavernParty?.ReleaseTavern(this);

		IFieldUnitRepository fieldRepo = GameManager.Get<IFieldUnitRepository>();
		fieldRepo?.UnregisterBuilding(this);

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
		UpdateWorkEffectFromProgress();
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
		UpdateHordeIndicator();
	}

	private void OnProductionCyclesCompleted(int cycles)
	{
		for (int i = 0; i < cycles; i++)
		{
			SpawnUnits(1);
			PlayWorkJumpVisual();
		}
	}

	private void SpawnUnits(int count)
	{
		if (!CanWork || _battlefield == null || CastleRef == null || count <= 0)
			return;
		if (_gameManager?.CurrentState == GameManager.GameState.GameOver)
			return;

		PackedScene unitScene = GD.Load<PackedScene>("res://prefabs/Unit.tscn");
		if (unitScene == null)
			return;

		for (int i = 0; i < count; i++)
		{
			Unit shell = unitScene.Instantiate<Unit>();
			UnitLogic unit = shell.GetNode<UnitLogic>("Logic");
			unit.IsPlayerUnit = _isPlayerBuilding;
			BuildingSys.ApplyUnitSpawnStats(TypeId, unit);
			ApplyFactionBuff(unit);
			UnitSpawn.PlaceUnit(
				_battlefield, CastleRef, shell,
				BuildingSys.GetFootprint(TypeId), GridX, GridY, _spawnCount);
			_spawnCount++;
			if (_isPlayerBuilding)
				AssignToTavern(unit);
		}
	}

	public void SpawnUnitType(string unitTypeId, int count)
	{
		if (_battlefield == null || CastleRef == null || count <= 0 || string.IsNullOrEmpty(unitTypeId))
			return;
		if (_gameManager?.CurrentState == GameManager.GameState.GameOver)
			return;

		PackedScene unitScene = GD.Load<PackedScene>("res://prefabs/Unit.tscn");
		if (unitScene == null)
			return;

		IUnitRepository unitRepo = GameManager.Get<IUnitRepository>();
		for (int i = 0; i < count; i++)
		{
			Unit shell = unitScene.Instantiate<Unit>();
			UnitLogic unit = shell.GetNode<UnitLogic>("Logic");
			unit.IsPlayerUnit = _isPlayerBuilding;
			unit.InitializeFromStats(unitRepo.Get(unitTypeId));
			ApplyFactionBuff(unit);
			UnitSpawn.PlaceUnit(
				_battlefield, CastleRef, shell,
				BuildingSys.GetFootprint(TypeId), GridX, GridY, _spawnCount);
			_spawnCount++;
			if (_isPlayerBuilding)
				AssignToTavern(unit);
		}
	}

	private static void ApplyFactionBuff(UnitLogic unit)
	{
		if (unit.Handle != null && UndeadRules.IsUndead(unit.Handle.Race) && UndeadFactionBuffs.MaxHealthBonus > 0)
			unit.Handle.AddMaxHealthBonus(UndeadFactionBuffs.MaxHealthBonus);
	}

	private void AssignToTavern(UnitLogic unit)
	{
		TavernPartyService tavernParty = GameManager.Get<TavernPartyService>();
		IFieldUnitRepository fieldRepo = GameManager.Get<IFieldUnitRepository>();
		if (tavernParty == null || fieldRepo == null)
			return;
		tavernParty.TryAssignTavern(unit.Handle, fieldRepo.AllBuildings);
	}

	public void OnWorkStarted()
	{
		SetProcess(true);
	}

	public void OnWorkPaused()
	{
		if (_jumpTween != null && _jumpTween.IsValid() && _jumpTween.IsRunning())
		{
			_jumpTween.Pause();
			_jumpTweenAwaitingResume = true;
		}
	}

	public void OnWorkResumed()
	{
		if (_jumpTweenAwaitingResume && _jumpTween != null && _jumpTween.IsValid())
		{
			_jumpTweenAwaitingResume = false;
			_jumpTween.Play();
		}
	}

	public void OnWorkStopped()
	{
		_jumpTweenAwaitingResume = false;
		SetProcess(false);
		CancelJumpTween();
		ResetWorkVisual();
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
		UpdateStateIcon();
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


	private const float WorkBrightenAmount = 0.35f;
	private void TryApplyVisual()
	{
		Sprite2D sprite = _sprite ?? GetNodeOrNull<Sprite2D>("View/Sprite");
		if (sprite == null)
			return;

		if (!_visualApplied)
		{
			BuildingSys.ApplyVisual(this);
			_visualApplied = true;
		}

		_sprite = sprite;
		_spriteBasePosition = sprite.Position;
		if (_originalMaterial == null)
			_originalMaterial = sprite.Material;
	}

	private void PlayDestructionEffect()
	{
		Node parent = GetParent();
		if (parent == null)
			return;

		Image img = Image.CreateEmpty(8, 8, false, Image.Format.Rgba8);
		for (int x = 0; x < 8; x++)
			for (int y = 0; y < 8; y++)
				img.SetPixel(x, y, new Color(1, 1, 1, 1));
		ImageTexture tex = ImageTexture.CreateFromImage(img);

		ParticleProcessMaterial mat = new ParticleProcessMaterial();
		mat.Gravity = new Vector3(0, 200, 0);
		mat.InitialVelocityMin = -80;
		mat.InitialVelocityMax = 80;
		mat.ScaleMin = 1.5f;
		mat.ScaleMax = 3.5f;
		mat.DampingMin = 2f;
		mat.DampingMax = 5f;
		mat.AngleMin = 0;
		mat.AngleMax = 360;
		mat.AngularVelocityMin = -360;
		mat.AngularVelocityMax = 360;
		mat.Color = new Color(0.5f, 0.45f, 0.4f, 1f);

		GpuParticles2D particles = new GpuParticles2D();
		particles.OneShot = true;
		particles.Explosiveness = 1f;
		particles.Amount = 16;
		particles.Lifetime = 0.6f;
		particles.ProcessMaterial = mat;
		particles.Texture = tex;
		particles.GlobalPosition = GlobalPosition;
		particles.Emitting = true;
		parent.AddChild(particles);

		if (_sprite != null)
		{
			Tween tween = CreateTween();
			tween.TweenProperty(_sprite, "modulate:a", 0f, 0.3f);
		}

		Tween delayTween = CreateTween();
		delayTween.TweenInterval(0.7f);
		delayTween.TweenCallback(Callable.From(() =>
		{
			particles.QueueFree();
			QueueFree();
		}));
	}

	private void UpdateWorkEffectFromProgress()
	{
		if (_sprite == null || _runtime.WorkRequired <= 0f)
			return;

		EnsureWorkMaterial();
		_sprite.Material = _workMaterial;
		float progress = Mathf.Clamp(_runtime.WorkDone / _runtime.WorkRequired, 0f, 1f);
		_workMaterial.SetShaderParameter("fill_amount", progress);
	}

	private void PlayWorkJumpVisual()
	{
		if (_sprite == null)
			return;

		CancelJumpTween();
		_jumpTweenAwaitingResume = false;

		const float jumpOffset = -6f;
		const float jumpUpDuration = 0.08f;
		const float jumpDownDuration = 0.1f;

		_jumpTween = CreateTween();
		_jumpTween.TweenProperty(_sprite, "position:y", _spriteBasePosition.Y + jumpOffset, jumpUpDuration)
			.SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
		_jumpTween.TweenProperty(_sprite, "position:y", _spriteBasePosition.Y, jumpDownDuration)
			.SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);
	}

	private void EnsureWorkMaterial()
	{
		if (_workMaterial != null)
			return;

		Shader shader = GD.Load<Shader>("res://assets/shaders/building_work.gdshader");
		_workMaterial = new ShaderMaterial { Shader = shader };
		_workMaterial.SetShaderParameter("brighten", WorkBrightenAmount);
	}

	private void CancelJumpTween()
	{
		if (_jumpTween != null && _jumpTween.IsValid())
			_jumpTween.Kill();
		_jumpTween = null;
	}

	private void ResetWorkVisual()
	{
		if (_sprite == null)
			return;
		_sprite.Material = _originalMaterial;
		_sprite.Position = _spriteBasePosition;
		UpdateDamageVisual();
	}

	private void UpdateDamageVisual()
	{
		if (_sprite == null)
			return;

		if (Health <= 0)
			_sprite.Modulate = new Color(0.55f, 0.55f, 0.6f, 0.75f);
		else if (IsDamaged)
			_sprite.Modulate = new Color(0.85f, 0.75f, 0.75f);
		else
			_sprite.Modulate = BuildingSys.GetSpriteModulate(TypeId);
	}

	private void RefreshOperationalState()
	{
		SyncStateIconPosition();
		UpdateStateIcon();

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

	private void SyncStateIconPosition()
	{
		if (_stateIcon == null || CastleRef == null)
			return;

		Vector2I mainOffset = BuildingSys.GetMainCellOffset(TypeId);
		Vector2 mainCenter = CastleRef.GetCellCenter(GridX + mainOffset.X, GridY + mainOffset.Y);
		_stateIcon.Position = mainCenter - Position;
		if (_hordeIndicator != null)
			_hordeIndicator.Position = mainCenter - Position;
	}

	private void UpdateHordeIndicator()
	{
		if (_hordeIndicator == null || CastleRef == null)
			return;

		List<IBuildingState> all = CastleRef.GetBuildingStates();
		List<IAdjacencyBuilding> domain = all.OfType<IAdjacencyBuilding>().ToList();
		int count = HordeAdjacencyRules.CountAdjacencyBonusCells(this, domain, GameManager.Get<IBuildingRepository>());
		_hordeIndicator.SetAdjacencyCellCount(count);
	}

	private void UpdateStateIcon()
	{
		if (_stateIcon == null || BuildingSys.IsCoreBuilding(TypeId))
		{
			_stateIcon?.SetPaused(false);
			_stateIcon?.SetCombineProhibited(false);
			return;
		}

		if (IsManuallyPaused)
			_stateIcon.SetPaused(true);
		else
			_stateIcon.SetPaused(false);

		_stateIcon.SetCombineProhibited(IsCombineProhibited);
	}

}
