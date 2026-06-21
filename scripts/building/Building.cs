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

	public void BindToGrid(Castle castle, int gridX, int gridY)
	{
		CastleRef = castle;
		GridX = gridX;
		GridY = gridY;
	}

	public Castle GetCastle() => CastleRef;

	public bool CanWork => NightSystem.CanUnitWork(HasNightCombat);

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
	}
}
