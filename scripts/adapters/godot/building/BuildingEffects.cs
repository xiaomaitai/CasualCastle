// 建筑表现效果：工作跳动、受损闪色与摧毁粒子
using Godot;

public sealed class BuildingEffects
{
	private readonly Building _owner;

	private readonly BuildingSystem _buildingSystem;

	private Sprite2D _sprite;

	private Vector2 _spriteBasePosition;

	private Tween _jumpTween;

	private bool _jumpTweenAwaitingResume;

	public BuildingEffects(Building owner, BuildingSystem buildingSystem)
	{
		_owner = owner;
		_buildingSystem = buildingSystem;
	}

	public void Initialize()
	{
		_sprite = _owner.GetNodeOrNull<Sprite2D>("View/Sprite");
		if (_sprite != null)
			_spriteBasePosition = _sprite.Position;
	}

	public void UpdateDamageVisual()
	{
		if (_sprite == null)
			return;

		if (_owner.Health <= 0)
			_sprite.Modulate = new Color(0.55f, 0.55f, 0.6f, 0.75f);
		else if (_owner.IsDamaged)
			_sprite.Modulate = new Color(0.85f, 0.75f, 0.75f);
		else
			_sprite.Modulate = _buildingSystem.GetSpriteModulate(_owner.TypeId);
	}

	public void PlayWorkJumpVisual()
	{
		if (_sprite == null)
			return;

		CancelJumpTween();
		_jumpTweenAwaitingResume = false;

		const float jumpOffset = -6f;
		const float jumpUpDuration = 0.08f;
		const float jumpDownDuration = 0.1f;

		_jumpTween = _owner.CreateTween();
		_jumpTween.TweenProperty(_sprite, "position:y", _spriteBasePosition.Y + jumpOffset, jumpUpDuration)
			.SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
		_jumpTween.TweenProperty(_sprite, "position:y", _spriteBasePosition.Y, jumpDownDuration)
			.SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);
	}

	public void PauseJump()
	{
		if (_jumpTween != null && _jumpTween.IsValid() && _jumpTween.IsRunning())
		{
			_jumpTween.Pause();
			_jumpTweenAwaitingResume = true;
		}
	}

	public void ResumeJump()
	{
		if (_jumpTweenAwaitingResume && _jumpTween != null && _jumpTween.IsValid())
		{
			_jumpTweenAwaitingResume = false;
			_jumpTween.Play();
		}
	}

	public void StopJump()
	{
		_jumpTweenAwaitingResume = false;
		CancelJumpTween();
	}

	public void PlayDestructionEffect()
	{
		Node parent = _owner.GetParent();
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
		particles.GlobalPosition = _owner.GlobalPosition;
		particles.Emitting = true;
		parent.AddChild(particles);

		if (_sprite != null)
		{
			Tween tween = _owner.CreateTween();
			tween.TweenProperty(_sprite, "modulate:a", 0f, 0.3f);
		}

		Tween delayTween = _owner.CreateTween();
		delayTween.TweenInterval(0.7f);
		delayTween.TweenCallback(Callable.From(() =>
		{
			particles.QueueFree();
			_owner.QueueFree();
		}));
	}

	private void CancelJumpTween()
	{
		if (_jumpTween != null && _jumpTween.IsValid())
			_jumpTween.Kill();
		_jumpTween = null;
	}
}
