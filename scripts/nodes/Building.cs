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
	}

	protected virtual void OnPhaseChanged(GameManager.GamePhase phase)
	{
		UpdateWorkCycle();
	}

	protected void UpdateWorkCycle()
	{
		if (CanWork)
			StartWorkCycle();
		else
			StopWorkCycle();
	}

	protected virtual void StartWorkCycle()
	{
	}

	protected void StopWorkCycle()
	{
		_workCycleActive = false;
		CancelWorkEffect();
	}

	protected void BeginWorkCycle(float interval, Action onWorkComplete)
	{
		if (_workCycleActive) return;
		_workCycleActive = true;
		RunWorkCycle(interval, onWorkComplete);
	}

	private void RunWorkCycle(float interval, Action onWorkComplete)
	{
		if (!_workCycleActive || !CanWork)
		{
			_workCycleActive = false;
			return;
		}

		PlayWorkEffect(interval, () =>
		{
			if (!_workCycleActive || !CanWork)
			{
				_workCycleActive = false;
				return;
			}

			onWorkComplete?.Invoke();
			RunWorkCycle(interval, onWorkComplete);
		});
	}

	public void PlayWorkEffect(float duration, Action onWorkComplete)
	{
		CancelWorkEffectTween();

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
		if (_workMaterial != null) return;

		Shader shader = GD.Load<Shader>("res://assets/shaders/building_work.gdshader");
		_workMaterial = new ShaderMaterial { Shader = shader };
	}

	private void CancelWorkEffect()
	{
		_workCycleActive = false;
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
		if (_sprite == null) return;
		_sprite.Material = _originalMaterial;
		_sprite.Position = _spriteBasePosition;
	}
}
