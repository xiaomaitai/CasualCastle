using Godot;
using System;

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
	private Tween _workTween;
	private Vector2 _spriteBasePosition;
	private bool _workCycleActive;
	private bool _workCyclePaused;
	private bool _tweenAwaitingResume;
	private float _workInterval;
	private Action _workCallback;

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
		CancelWorkEffect();
	}

	protected virtual void OnPhaseChanged(GameManager.GamePhase phase)
	{
		UpdateWorkCycle();
	}

	protected void UpdateWorkCycle()
	{
		if (CanWork)
		{
			if (_workCyclePaused)
				ResumeWorkCycle();
			else if (!_workCycleActive)
				StartWorkCycle();
		}
		else
		{
			PauseWorkCycle();
		}
	}

	protected virtual void StartWorkCycle()
	{
	}

	protected void PauseWorkCycle()
	{
		if (!_workCycleActive || _workCyclePaused)
			return;

		_workCyclePaused = true;
		if (_workTween != null && _workTween.IsValid() && _workTween.IsRunning())
		{
			_workTween.Pause();
			_tweenAwaitingResume = true;
		}
	}

	private void ResumeWorkCycle()
	{
		if (!_workCycleActive || !_workCyclePaused)
			return;

		_workCyclePaused = false;
		if (!CanWork)
			return;

		if (_tweenAwaitingResume && _workTween != null && _workTween.IsValid())
		{
			_tweenAwaitingResume = false;
			_workTween.Play();
			return;
		}

		_tweenAwaitingResume = false;
		RunWorkCycle(_workInterval, _workCallback);
	}

	protected void BeginWorkCycle(float interval, Action onWorkComplete)
	{
		_workInterval = interval;
		_workCallback = onWorkComplete;
		if (_workCycleActive)
			return;

		_workCycleActive = true;
		_workCyclePaused = false;
		RunWorkCycle(interval, onWorkComplete);
	}

	private void RunWorkCycle(float interval, Action onWorkComplete)
	{
		if (!_workCycleActive)
			return;

		if (!CanWork)
		{
			PauseWorkCycle();
			return;
		}

		PlayWorkEffect(interval, () =>
		{
			if (!_workCycleActive)
				return;

			if (!CanWork)
			{
				PauseWorkCycle();
				return;
			}

			onWorkComplete?.Invoke();
			RunWorkCycle(interval, onWorkComplete);
		});
	}

	public void PlayWorkEffect(float duration, Action onWorkComplete)
	{
		CancelWorkEffectTween();
		_tweenAwaitingResume = false;

		if (_sprite == null || duration <= 0f)
		{
			PlayWorkJump(onWorkComplete);
			return;
		}

		EnsureWorkMaterial();
		_sprite.Material = _workMaterial;
		_workMaterial.SetShaderParameter("fill_amount", 0f);

		_workTween = CreateTween();
		_workTween.TweenMethod(
			Callable.From<float>(v => _workMaterial.SetShaderParameter("fill_amount", v)),
			0f, 1f, duration);
		_workTween.TweenCallback(Callable.From(() => PlayWorkJump(onWorkComplete)));
	}

	private void PlayWorkJump(Action onWorkComplete)
	{
		if (_sprite == null)
		{
			ResetWorkVisual();
			onWorkComplete?.Invoke();
			return;
		}

		const float jumpOffset = -6f;
		const float jumpUpDuration = 0.08f;
		const float jumpDownDuration = 0.1f;

		_workTween = CreateTween();
		_workTween.TweenProperty(_sprite, "position:y", _spriteBasePosition.Y + jumpOffset, jumpUpDuration)
			.SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
		_workTween.TweenProperty(_sprite, "position:y", _spriteBasePosition.Y, jumpDownDuration)
			.SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);
		_workTween.TweenCallback(Callable.From(() =>
		{
			ResetWorkVisual();
			onWorkComplete?.Invoke();
		}));
	}

	private void EnsureWorkMaterial()
	{
		if (_workMaterial != null)
			return;

		Shader shader = GD.Load<Shader>("res://assets/shaders/building_work.gdshader");
		_workMaterial = new ShaderMaterial { Shader = shader };
		_workMaterial.SetShaderParameter("brighten", 0.35f);
	}

	private void CancelWorkEffect()
	{
		_workCycleActive = false;
		_workCyclePaused = false;
		_tweenAwaitingResume = false;
		CancelWorkEffectTween();
		ResetWorkVisual();
	}

	private void CancelWorkEffectTween()
	{
		if (_workTween != null && _workTween.IsValid())
			_workTween.Kill();
		_workTween = null;
	}

	private void ResetWorkVisual()
	{
		if (_sprite == null)
			return;
		_sprite.Material = _originalMaterial;
		_sprite.Position = _spriteBasePosition;
	}
}
