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

	public string TypeId { get; set; } = "Barracks";
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
		if (TypeId == "CastleHeart")
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
		if (TypeId == "CastleHeart")
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
		if (TypeId != "CastleHeart" && CastleRef != null)
			CastleRef.ReleaseBuildingFootprint(this);

		if (TypeId != "CastleHeart")
		{
			RefreshOperationalState();
			PlayDestructionEffect();
		}
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
		bool isNight = _gameManager != null && _gameManager.IsNight;
		return RepairRules.CanRepair(Health, MaxHealth, BuildingSys.IsCoreBuilding(TypeId),
			isPlayerOwned, HasEnemyOnTop, isPlaying, isNight);
	}

	private void Repair()
	{
		Health = MaxHealth;
		EmitSignal(SignalName.HealthChanged, Health, MaxHealth);
		UpdateDamageVisual();
		RefreshOperationalState();
	}

	public int GetRepairCost() => RepairRules.GetRepairCost(MaxHealth, Health, GameRules.RepairGoldPerHealth);

	public bool TryStartRitual(string ritualId)
	{
		if (_ritualService == null || _activeRitual != null || CastleRef == null)
			return false;
		if (!RitualRules.CanHostRitual(TypeId) || !IsPlayerOwned)
			return false;

		int coreLevel = RitualCircleRules.GetCoreLevel(CastleRef.GetBuildingStates());
		bool isCoreHosted = RitualRules.IsRitualCircleHost(TypeId);
		BlackMagicService blackMagic = GameManager.Get<BlackMagicService>();
		_activeRitual = _ritualService.TryStartRitual(
			ritualId, isCoreHosted, coreLevel,
			_shop.TrySpendGold,
			amount => blackMagic != null && blackMagic.TrySpend(amount));
		if (_activeRitual == null)
			return false;

		UpdateWorkCycle();
		UpdateStateIcon();
		return true;
	}

	private void TickRitual(float delta)
	{
		if (_activeRitual == null || _gameManager == null || _gameManager.CurrentState != GameManager.GameState.Playing)
			return;

		_ritualService.Tick(_activeRitual, delta);
		if (_activeRitual.IsComplete)
			CompleteRitual();
	}

	private void CompleteRitual()
	{
		RitualReward reward = _ritualService.GetReward(_activeRitual);
		ApplyRitualReward(reward);
		bool isCoreHosted = RitualRules.IsRitualCircleHost(TypeId);
		_activeRitual = null;
		if (isCoreHosted)
			CallDeferred(nameof(UpgradeCircleAfterRitual));
		else
			UpdateWorkCycle();
	}

	private void ApplyRitualReward(RitualReward reward)
	{
		if (reward == null)
			return;
		if (reward.GoldAmount > 0 && _shop != null)
			_shop.AddGold(reward.GoldAmount);
		if (!string.IsNullOrEmpty(reward.SummonUnitTypeId) && reward.SummonCount > 0)
			SpawnUnitType(reward.SummonUnitTypeId, reward.SummonCount);
		if (reward.UndeadMaxHealthBonus > 0)
			ApplyUndeadHealthBonus(reward.UndeadMaxHealthBonus);
	}

	private void ApplyUndeadHealthBonus(int amount)
	{
		UndeadFactionBuffs.AddUndeadMaxHealthBonus(amount);
		IFieldUnitRepository fieldRepo = GameManager.Get<IFieldUnitRepository>();
		if (fieldRepo == null)
			return;
		foreach (IUnitHandle unit in fieldRepo.AllUnits)
		{
			if (!unit.IsAlive || unit.IsPlayerUnit != _isPlayerBuilding || !UndeadRules.IsUndead(unit.Race))
				continue;
			unit.AddMaxHealthBonus(amount);
			if (unit is UnitLogic logic)
				logic.RefreshHealthVisual();
		}
	}

	private void UpgradeCircleAfterRitual()
	{
		string nextTypeId = RitualCircleRules.GetNextLevelTypeId(TypeId);
		if (nextTypeId == null)
		{
			UpdateWorkCycle();
			return;
		}

		CastleRef.ReplaceHeart(nextTypeId, Health);
		Sfx.Play(SfxEvents.Upgrade);
	}

	public bool CanWork
	{
		get
		{
			IGameState state = AdapterRegistry.Resolve<IGameState>();
			return IsOperational && !IsRitualInProgress
				&& (BuildingSys.GetHasNightCombat(TypeId) ? state.IsNight : state.IsDay);
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

	public override void _Process(double delta)
	{
		TickRitual((float)delta);
		if (!_workActive || _workPaused || !CanWork)
			return;

		_workDone += (float)delta * _productionRate * _workSpeedMultiplier;
		UpdateWorkEffectFromProgress();

		if (_workDone < _workRequired)
			return;

		_workDone -= _workRequired;
		PerformWork();
		PlayWorkJumpVisual();
	}

	protected virtual void OnPhaseChanged(GameManager.GamePhase phase)
	{
		UpdateWorkCycle();
	}

	private void OnFastProductionChanged()
	{
		SetWorkSpeedMultiplier(CheatState.FastProductionEnabled ? 10f : 1f);
	}

	protected void UpdateWorkCycle()
	{
		if (CanWork)
		{
			if (_workPaused)
				ResumeWork();
			else if (!_workActive)
				StartWorkCycle();
		}
		else
		{
			PauseWork();
		}
	}

	public void SetWorkSpeedMultiplier(float multiplier)
	{
		multiplier = Mathf.Max(0.1f, multiplier);
		if (Mathf.IsEqualApprox(_workSpeedMultiplier, multiplier))
			return;

		_workSpeedMultiplier = multiplier;
		UpdateHordeIndicator();
	}

	private void StartWorkCycle()
	{
		float productionRate = BuildingSys.GetProductionRate(TypeId);
		if (productionRate <= 0f)
			return;

		int unitCost = BuildingSys.GetUnitCost(TypeId);
		if (unitCost <= 0)
			return;

		_productionRate = productionRate;
		BeginWork(unitCost);
	}

	private void PerformWork()
	{
		SpawnUnits(1);
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

	private void SpawnUnitType(string unitTypeId, int count)
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

	protected void BeginWork(float workRequired)
	{
		_workRequired = workRequired;

		if (_workActive)
			return;

		_workActive = true;
		_workPaused = false;
		_workDone = 0f;
		SetProcess(true);
	}

	protected void PauseWork()
	{
		if (!_workActive || _workPaused)
			return;

		_workPaused = true;

		if (_jumpTween != null && _jumpTween.IsValid() && _jumpTween.IsRunning())
		{
			_jumpTween.Pause();
			_jumpTweenAwaitingResume = true;
		}
	}

	protected void ResumeWork()
	{
		if (!_workActive || !_workPaused)
			return;

		_workPaused = false;
		if (!CanWork)
			return;

		if (_jumpTweenAwaitingResume && _jumpTween != null && _jumpTween.IsValid())
		{
			_jumpTweenAwaitingResume = false;
			_jumpTween.Play();
		}
	}

	private void UpdateWorkEffectFromProgress()
	{
		if (_sprite == null || _workRequired <= 0f)
			return;

		EnsureWorkMaterial();
		_sprite.Material = _workMaterial;
		float progress = Mathf.Clamp(_workDone / _workRequired, 0f, 1f);
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
		_workMaterial.SetShaderParameter("brighten", 0.35f);
	}

	private void StopWork()
	{
		_workActive = false;
		_workPaused = false;
		_workDone = 0f;
		_jumpTweenAwaitingResume = false;
		SetProcess(false);
		CancelJumpTween();
		ResetWorkVisual();
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
				if (_workActive && !_workPaused)
					PauseWork();
			}
			else
			{
				StopWork();
			}
		}
		else
		{
			UpdateWorkCycle();
		}

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
