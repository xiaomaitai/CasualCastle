using Godot;

public partial class Building : Area2D
{
	[Export]
	public int CollisionSize = 56;

	[Export]
	public bool HasNightCombat = false;

	protected Castle CastleRef;
	protected int GridX;
	protected int GridY;

	private float _workSpeedMultiplier = 1f;

	private Sprite2D _sprite;
	private ShaderMaterial _workMaterial;
	private Material _originalMaterial;
	private Tween _jumpTween;
	private Vector2 _spriteBasePosition;
	private float _workInterval;
	private float _workElapsed;
	private bool _workActive;
	private bool _workPaused;
	private bool _jumpTweenAwaitingResume;
	private BuildingStateIcon _stateIcon;

	[Signal]
	public delegate void HealthChangedEventHandler(int health, int maxHealth);

	public string TypeId { get; set; } = "Barracks";
	public int MaxHealth { get; private set; }
	public int Health { get; private set; }
	public bool IsManuallyPaused { get; private set; }
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
		SyncStateIconPosition();
	}

	public void SetManuallyPaused(bool paused)
	{
		if (BuildingSystem.IsCoreBuilding(TypeId) || IsManuallyPaused == paused)
			return;

		IsManuallyPaused = paused;
		RefreshOperationalState();
	}

	public void InitFromType(string buildingType)
	{
		TypeId = buildingType;
		MaxHealth = BuildingSystem.GetMaxHealth(buildingType);
		Health = MaxHealth;
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

		if (TypeId == "CastleHeart" && CastleRef != null)
			GameManager.Instance?.OnCastleHeartHealthChanged(CastleRef.IsPlayerCastle, Health, MaxHealth);

		if (Health <= 0)
			OnDestroyed();
		else if (TypeId != "CastleHeart")
			RefreshOperationalState();
		else
			PauseWork();
	}

	private void OnDestroyed()
	{
		if (TypeId != "CastleHeart" && CastleRef != null)
			CastleRef.ReleaseBuildingFootprint(this);
		
		if (TypeId != "CastleHeart")
			RefreshOperationalState();
	}

	public void Repair()
	{
		if (Health >= MaxHealth)
			return;

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
		CollisionMask = 0;

		CollisionShape2D shapeNode = GetNodeOrNull<CollisionShape2D>("CollisionShape");
		if (shapeNode?.Shape is RectangleShape2D rect)
		{
			rect.Size = new Vector2(CollisionSize, CollisionSize);
		}

		_sprite = GetNodeOrNull<Sprite2D>("Sprite");
		if (_sprite != null)
		{
			_spriteBasePosition = _sprite.Position;
			_originalMaterial = _sprite.Material;
		}

		if (GameManager.Instance != null)
			GameManager.Instance.PhaseChanged += OnPhaseChanged;

		_stateIcon = new BuildingStateIcon();
		AddChild(_stateIcon);
		SyncStateIconPosition();
		RefreshOperationalState();
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

		_workElapsed += (float)delta;
		UpdateWorkEffectFromProgress();

		if (_workElapsed < _workInterval)
			return;

		_workElapsed = 0f;
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

	protected float GetWorkInterval(float baseInterval) => baseInterval / _workSpeedMultiplier;

	public void SetWorkSpeedMultiplier(float multiplier)
	{
		_workSpeedMultiplier = Mathf.Max(0.1f, multiplier);
		if (_workActive)
			RestartWorkCycle();
	}

	protected virtual void RestartWorkCycle()
	{
		StopWork();
		StartWorkCycle();
	}

	protected virtual void StartWorkCycle()
	{
	}

	protected virtual void PerformWork()
	{
	}

	protected void BeginWork(float interval)
	{
		_workInterval = interval;

		if (_workActive)
			return;

		_workActive = true;
		_workPaused = false;
		_workElapsed = 0f;
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
		if (_sprite == null || _workInterval <= 0f)
			return;

		EnsureWorkMaterial();
		_sprite.Material = _workMaterial;
		float progress = Mathf.Clamp(_workElapsed / _workInterval, 0f, 1f);
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
		_workElapsed = 0f;
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
			_sprite.Modulate = Colors.White;
	}

	private void RefreshOperationalState()
	{
		SyncStateIconPosition();
		UpdateStateIcon();

		if (!IsOperational)
			StopWork();
		else
			UpdateWorkCycle();

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
			_stateIcon?.SetIcon(BuildingStateIcon.IconType.None);
			return;
		}

		if (IsDestroyed)
			_stateIcon.SetIcon(BuildingStateIcon.IconType.Destroyed);
		else if (IsManuallyPaused)
			_stateIcon.SetIcon(BuildingStateIcon.IconType.Paused);
		else
			_stateIcon.SetIcon(BuildingStateIcon.IconType.None);
	}
}
