using Godot;

public partial class Building : Area2D
{
	[Export]
	public bool HasNightCombat = false;

	protected Castle CastleRef;
	protected int GridX;
	protected int GridY;

	private float _workSpeedMultiplier = 1f;
	private Color _baseModulate = Colors.White;
	private bool _visualApplied;

	private Sprite2D _sprite;
	private ShaderMaterial _workMaterial;
	private Material _originalMaterial;
	private Tween _jumpTween;
	private Vector2 _spriteBasePosition;
	private float _workRequired;
	private float _workDone;
	private bool _workActive;
	private bool _workPaused;
	private bool _jumpTweenAwaitingResume;
	private BuildingStateIcon _stateIcon;
	private Node2D _battlefield;
	private int _spawnCount;
	private bool _isPlayerBuilding;

	[Signal]
	public delegate void HealthChangedEventHandler(int health, int maxHealth);

	public string TypeId { get; set; } = "Barracks";
	public int MaxHealth { get; private set; }
	public int Health { get; private set; }
	public bool IsManuallyPaused { get; private set; }
	public bool IsFusionProhibited { get; private set; }
	public bool IsDestroyed => Health <= 0;
	public bool IsOperational => Health > 0 && !IsManuallyPaused;
	public bool ContributesToAdjacency => !IsDestroyed;
	public bool IsDamaged => Health < MaxHealth;
	public string DisplayName => BuildingSystem.GetDisplayName(TypeId);
	public int AnchorGridX => GridX;
	public int AnchorGridY => GridY;

	public bool HasEnemyOnTop
	{
		get
		{
			if (CastleRef == null)
				return false;

			bool isPlayerCastle = CastleRef.IsPlayerCastle;
			foreach (Area2D area in GetOverlappingAreas())
			{
				if (area is Soldier soldier && soldier.IsAlive && soldier.IsPlayerUnit != isPlayerCastle)
					return true;
			}
			return false;
		}
	}

	public Vector2I GetMainGridPosition()
	{
		Vector2I offset = BuildingSystem.GetMainCellOffset(TypeId);
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
		if (BuildingSystem.IsCoreBuilding(TypeId) || IsManuallyPaused == paused)
			return;

		IsManuallyPaused = paused;
		RefreshOperationalState();
	}

	public void SetFusionProhibited(bool prohibited)
	{
		if (BuildingSystem.IsCoreBuilding(TypeId) || IsFusionProhibited == prohibited)
			return;

		IsFusionProhibited = prohibited;
		UpdateStateIcon();
	}

	public void ApplySnapshotState(int health, bool manuallyPaused, bool fusionProhibited)
	{
		if (BuildingSystem.IsCoreBuilding(TypeId))
			return;

		Health = Mathf.Clamp(health, 1, MaxHealth);
		EmitSignal(SignalName.HealthChanged, Health, MaxHealth);
		UpdateDamageVisual();

		IsManuallyPaused = manuallyPaused;
		IsFusionProhibited = fusionProhibited;
		RefreshOperationalState();
	}

	public void InitFromType(string buildingType)
	{
		TypeId = buildingType;
		MaxHealth = BuildingSystem.GetMaxHealth(buildingType);
		Health = MaxHealth;
		HasNightCombat = BuildingSystem.GetHasNightCombat(buildingType);
		_baseModulate = BuildingSystem.GetSpriteModulate(buildingType);
		TryApplyVisual();
		UpdateDamageVisual();
	}

	private void TryApplyVisual()
	{
		Sprite2D sprite = _sprite ?? GetNodeOrNull<Sprite2D>("Sprite");
		if (sprite == null)
			return;

		if (!_visualApplied)
		{
			BuildingSystem.ApplyVisual(this);
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

		if (TypeId == "CastleHeart" && CastleRef != null)
			GameManager.Instance?.OnCastleHeartHealthChanged(CastleRef.IsPlayerCastle, Health, MaxHealth);

		if (Health <= 0)
			OnDestroyed();
		else if (TypeId == "CastleHeart")
			PauseWork();
	}

	private void OnDestroyed()
	{
		if (TypeId != "CastleHeart" && CastleRef != null)
			CastleRef.ReleaseBuildingFootprint(this);
		
		if (TypeId != "CastleHeart")
			RefreshOperationalState();
	}

	public bool TryRepair()
	{
		if (!CanRepair())
			return false;

		int cost = GetRepairCost();
		if (!ShopSystem.Instance?.TrySpendGold(cost) ?? false)
			return false;

		Repair();
		return true;
	}

	public bool CanRepair()
	{
		if (Health >= MaxHealth)
			return false;

		if (BuildingSystem.IsCoreBuilding(TypeId))
			return false;

		if (CastleRef == null || !CastleRef.IsPlayerCastle)
			return false;

		if (HasEnemyOnTop)
			return false;

		if (GameManager.Instance?.CurrentState != GameManager.GameState.Playing)
			return false;

		if (!GameManager.Instance.IsNight)
			return false;

		return true;
	}

	private void Repair()
	{
		Health = MaxHealth;
		EmitSignal(SignalName.HealthChanged, Health, MaxHealth);
		UpdateDamageVisual();
		RefreshOperationalState();
	}

	public int GetRepairCost() => (MaxHealth - Health) * GameConfig.RepairGoldPerHealth;

	public bool CanWork => IsOperational && NightSystem.CanUnitWork(HasNightCombat);

	public override void _Ready()
	{
		CollisionLayer = 4;
		CollisionMask = 2;

		_sprite = GetNodeOrNull<Sprite2D>("Sprite");
		TryApplyVisual();

		if (GameManager.Instance != null)
			GameManager.Instance.PhaseChanged += OnPhaseChanged;

		_stateIcon = new BuildingStateIcon();
		AddChild(_stateIcon);
		SyncStateIconPosition();
		RefreshOperationalState();

		if (BuildingSystem.GetSpawnInterval(TypeId) > 0f)
		{
			_battlefield = GetNodeOrNull<Node2D>("/root/MainGame/Battlefield");
			if (_battlefield == null)
				_battlefield = GetTree().Root.GetNodeOrNull<Node2D>("MainGame/Battlefield");
			UpdateWorkCycle();
		}
	}

	public override void _ExitTree()
	{
		if (GameManager.Instance != null)
			GameManager.Instance.PhaseChanged -= OnPhaseChanged;
		StopWork();
	}

	public override void _Process(double delta)
	{
		if (!_workActive || _workPaused || !CanWork)
			return;

		_workDone += (float)delta * _workSpeedMultiplier;
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
	}

	private void StartWorkCycle()
	{
		float workRequired = BuildingSystem.GetSpawnInterval(TypeId);
		if (workRequired > 0f)
			BeginWork(workRequired);
	}

	private void PerformWork()
	{
		SpawnUnits(1);
	}

	private void SpawnUnits(int count)
	{
		if (!CanWork || _battlefield == null || CastleRef == null || count <= 0)
			return;
		if (GameManager.Instance?.CurrentState == GameManager.GameState.GameOver)
			return;

		PackedScene soldierScene = GD.Load<PackedScene>("res://prefabs/Soldier.tscn");
		if (soldierScene == null)
			return;

		Vector2I marchDir = _isPlayerBuilding ? Vector2I.Right : Vector2I.Left;
		Vector2I spawnOffset = BuildingSystem.GetSpawnCellOffset(TypeId);
		int spawnGridX = GridX + spawnOffset.X;
		int spawnGridY = GridY + spawnOffset.Y;

		for (int i = 0; i < count; i++)
		{
			Vector2 spawnLocal = CastleRef.GetBuildingSpawnPosition(spawnGridX, spawnGridY, marchDir, _spawnCount);
			_spawnCount++;

			Soldier soldier = soldierScene.Instantiate<Soldier>();
			soldier.GlobalPosition = CastleRef.ToGlobal(spawnLocal);
			soldier.IsPlayerUnit = _isPlayerBuilding;
			BuildingSystem.ApplySoldierSpawnStats(TypeId, soldier);
			_battlefield.AddChild(soldier);
		}
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
			_sprite.Modulate = _baseModulate;
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
			AdjacentSystem.Instance?.RefreshCastle(castle);
	}

	private void SyncStateIconPosition()
	{
		if (_stateIcon == null || CastleRef == null)
			return;

		Vector2I mainOffset = BuildingSystem.GetMainCellOffset(TypeId);
		Vector2 mainCenter = CastleRef.GetCellCenter(GridX + mainOffset.X, GridY + mainOffset.Y);
		_stateIcon.Position = mainCenter - Position;
	}

	private void UpdateStateIcon()
	{
		if (_stateIcon == null || BuildingSystem.IsCoreBuilding(TypeId))
		{
			_stateIcon?.SetPaused(false);
			_stateIcon?.SetFusionProhibited(false);
			return;
		}

		if (IsManuallyPaused)
			_stateIcon.SetPaused(true);
		else
			_stateIcon.SetPaused(false);

		_stateIcon.SetFusionProhibited(IsFusionProhibited);
	}
}
