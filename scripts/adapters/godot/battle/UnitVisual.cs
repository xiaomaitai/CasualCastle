// 单位视觉：血条、状态图标、受击闪白与翻转
using Godot;

public class UnitVisual
{
	private const float HitFlashDurationSeconds = 0.1f;
	private const string BodyNodePath = "View/Body";
	private const string BodyHealthNodePath = "View/BodyHealth";
	private const float AttackFps = 12f;
	private const float RunFps = 10f;
	private const float HurtFps = 8f;
	private const float DeathFps = 8f;
	private const float BodyHealthAboveHeadRatio = 0.75f;
	private const float BodyHealthWidthRatio = 1.15f;
	private static readonly Color StealthModulate = new(0.72f, 0.82f, 1f, 0.55f);
	private static readonly Color DimmedModulate = new(0.7f, 0.74f, 0.86f, 0.9f);

	private UnitCardView _card;
	private Node2D _body;
	private AnimatedSprite2D _bodyAnim;
	private HealthBarView _bodyHealth;
	private bool _bodyAnimated;
	private Color _baseBodyColor = Colors.White;
	private Color _portraitTint = Colors.White;
	private float _hitFlashTimer;
	private bool _isSleeping;

	public bool IsFlashing => _hitFlashTimer > 0f;
	public bool IsBodyAnimated => _bodyAnimated;

	public void Initialize(Node2D body)
	{
		_body = body;
		_card = body.GetNode<UnitCardView>("View/UnitCard");
		_bodyAnim = body.GetNodeOrNull<AnimatedSprite2D>(BodyNodePath);
		_bodyHealth = body.GetNodeOrNull<HealthBarView>(BodyHealthNodePath);
		ApplyBodyVisible(false);
	}

	public void ApplyStats(string typeId, float displaySize)
	{
		_portraitTint = Colors.White;
		_card.Configure(typeId, displaySize);
		ConfigureFrameBody(typeId, displaySize);
	}

	private void ConfigureFrameBody(string typeId, float displaySize)
	{
		if (_bodyAnim == null) return;
		string framesPath = $"res://assets/art/units/{typeId.ToLowerInvariant()}/frames";
		if (!ResourceLoader.Exists($"{framesPath}/run_00.png"))
		{
			_bodyAnimated = false;
			ApplyBodyVisible(false);
			_card.Visible = true;
			return;
		}
		_bodyAnimated = true;
		_bodyAnim.SpriteFrames = BuildFrames(framesPath);
		_bodyAnim.Centered = true;
		Texture2D runTex = GD.Load<Texture2D>($"{framesPath}/run_00.png");
		_bodyAnim.Offset = new Vector2(0f, -runTex.GetHeight() * 0.5f);
		float contentHeight = ComputeContentHeight(runTex);
		_bodyAnim.Scale = Vector2.One * (displaySize / Mathf.Max(1f, contentHeight));
		_bodyAnim.Position = Vector2.Zero;
		_bodyAnim.Visible = true;
		_card.Visible = false;
		ConfigureBodyHealth(displaySize);
	}

	private void ConfigureBodyHealth(float displaySize)
	{
		if (_bodyHealth == null) return;
		Sprite2D empty = _bodyHealth.GetNodeOrNull<Sprite2D>("Empty");
		if (empty == null || empty.Texture == null)
		{
			_bodyHealth.Visible = false;
			return;
		}
		float barWidth = empty.Texture.GetWidth();
		float scale = displaySize * BodyHealthWidthRatio / Mathf.Max(1f, barWidth);
		_bodyHealth.Scale = new Vector2(scale, scale * 1.1f);
		_bodyHealth.Position = new Vector2(0f, -displaySize * BodyHealthAboveHeadRatio);
		_bodyHealth.Visible = true;
	}

	private void ApplyBodyVisible(bool visible)
	{
		if (_bodyAnim != null) _bodyAnim.Visible = visible;
		if (_bodyHealth != null) _bodyHealth.Visible = visible;
	}

	private static SpriteFrames BuildFrames(string framesPath)
	{
		SpriteFrames frames = new SpriteFrames();
		AddAnimation(frames, "run", framesPath, RunFps, true);
		AddAnimation(frames, "attack", framesPath, AttackFps, false);
		AddAnimation(frames, "hurt", framesPath, HurtFps, false);
		AddAnimation(frames, "death", framesPath, DeathFps, false);
		return frames;
	}

	private static void AddAnimation(SpriteFrames frames, string name, string framesPath, float fps, bool loop)
	{
		if (!ResourceLoader.Exists($"{framesPath}/{name}_00.png")) return;
		frames.AddAnimation(name);
		frames.SetAnimationSpeed(name, fps);
		frames.SetAnimationLoop(name, loop);
		int index = 0;
		while (ResourceLoader.Exists($"{framesPath}/{name}_{index:00}.png"))
		{
			frames.AddFrame(name, GD.Load<Texture2D>($"{framesPath}/{name}_{index:00}.png"));
			index++;
		}
	}

	private static float ComputeContentHeight(Texture2D tex)
	{
		Image image = tex.GetImage();
		int top = image.GetHeight();
		int bottom = 0;
		for (int y = 0; y < image.GetHeight(); y++)
		{
			for (int x = 0; x < image.GetWidth(); x++)
			{
				if (image.GetPixel(x, y).A > 0f)
				{
					if (y < top) top = y;
					if (y > bottom) bottom = y;
				}
			}
		}
		return bottom - top + 1;
	}

	public void PlayAnimation(string name)
	{
		if (!_bodyAnimated || _bodyAnim == null) return;
		if (_bodyAnim.Animation == name && _bodyAnim.IsPlaying()) return;
		_bodyAnim.Play(name);
	}

	public void SetMoving(bool moving)
	{
		if (!_bodyAnimated || _bodyAnim == null) return;
		if (moving)
		{
			_bodyAnim.Play("run");
		}
		else if (_bodyAnim.Animation == "run")
		{
			_bodyAnim.Stop();
		}
	}

	public void StartHitFlash()
	{
		_hitFlashTimer = HitFlashDurationSeconds;
		_card.SetPortraitTint(Colors.White);
		_card.SetStatus(UnitCardStatus.Hit);
		if (_bodyAnim != null) _bodyAnim.Modulate = Colors.White;
	}

	public void UpdateHitFlash(float dt)
	{
		if (_hitFlashTimer > 0f)
		{
			_hitFlashTimer -= dt;
			if (_hitFlashTimer <= 0f)
			{
				_card.SetPortraitTint(_portraitTint);
				_card.SetStatus(_isSleeping ? UnitCardStatus.Sleeping : UnitCardStatus.None);
				ApplyBodyModulate();
			}
		}
	}

	public void UpdateSleepVisual(bool isActive, bool isSleeping)
	{
		_isSleeping = isSleeping;
		_card.SetDimmed(!isActive);
		if (_hitFlashTimer <= 0f)
			_card.SetStatus(isSleeping ? UnitCardStatus.Sleeping : UnitCardStatus.None);
		ApplyBodyModulate();
	}

	public void SetBaseModulate(Color color)
	{
		_portraitTint = color;
		if (_hitFlashTimer <= 0f)
			_card.SetPortraitTint(color);
		_baseBodyColor = color;
		ApplyBodyModulate();
	}

	public void SetStealthVisual(bool stealthed)
	{
		_card.SetStealth(stealthed);
		_baseBodyColor = stealthed ? StealthModulate : Colors.White;
		ApplyBodyModulate();
	}

	private void ApplyBodyModulate()
	{
		if (_bodyAnim == null) return;
		if (_hitFlashTimer > 0f)
			_bodyAnim.Modulate = Colors.White;
		else
			_bodyAnim.Modulate = _baseBodyColor;
	}

	public void SetHealth(int current, int maximum)
	{
		float ratio = maximum > 0 ? current / (float)maximum : 0f;
		_card.SetHealth(current, maximum);
		if (_bodyHealth != null)
		{
			_bodyHealth.Fill = ratio;
			_bodyHealth.Ping(current >= maximum);
		}
	}

	public void SetSelected(bool selected)
	{
		_card.SetSelected(selected);
		_body.ZIndex = selected ? 20 : 0;
	}

	public void SetBuffs(string[] buffs)
	{
		_card.SetBuffs(buffs);
	}

	public void SetFlipH(bool flip)
	{
		_card.SetFlipH(flip);
		if (_bodyAnim != null) _bodyAnim.FlipH = flip;
	}
}
