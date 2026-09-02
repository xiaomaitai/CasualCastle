// 单位视觉：血条、状态图标、受击闪白与翻转
using Godot;
using System.Collections.Generic;

public class UnitVisual
{
	private const float HitFlashDurationSeconds = 0.1f;
	private const string BodyNodePath = "View/Body";
	private const string BodyHealthNodePath = "View/BodyHealth";
	private const float AttackFps = 12f;
	private const float RunFps = 10f;
	private const float HurtFps = 8f;
	private const float DeathFps = 8f;
	private const int HurtFrameLimit = 2;
	private const int GenericFrameLimit = 16;
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
		string folder = $"res://assets/art/units/{typeId.ToLowerInvariant()}/frames";
		string atlasPath = $"{folder}/atlas.png";
		if (!ResourceLoader.Exists(atlasPath))
		{
			_bodyAnimated = false;
			ApplyBodyVisible(false);
			_card.Visible = true;
			return;
		}
		_bodyAnimated = true;
		Texture2D atlas = GD.Load<Texture2D>(atlasPath);
		Dictionary<string, Rect2> regions = LoadRegions($"{folder}/atlas.json", out Vector2 cell);
		_bodyAnim.SpriteFrames = BuildFrames(atlas, regions);
		_bodyAnim.Centered = true;
		_bodyAnim.Offset = new Vector2(0f, -cell.Y * 0.5f);
		float contentHeight = regions.TryGetValue("run_00", out Rect2 run0)
			? ComputeRegionContentHeight(atlas, run0)
			: cell.Y;
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

	private static SpriteFrames BuildFrames(Texture2D atlas, Dictionary<string, Rect2> regions)
	{
		SpriteFrames frames = new SpriteFrames();
		AddAnimation(frames, atlas, regions, "run", RunFps, true, GenericFrameLimit);
		AddAnimation(frames, atlas, regions, "attack", AttackFps, false, GenericFrameLimit);
		AddAnimation(frames, atlas, regions, "hurt", HurtFps, false, HurtFrameLimit);
		AddAnimation(frames, atlas, regions, "death", DeathFps, false, GenericFrameLimit);
		return frames;
	}

	private static void AddAnimation(SpriteFrames frames, Texture2D atlas, Dictionary<string, Rect2> regions, string name, float fps, bool loop, int frameLimit)
	{
		if (!regions.ContainsKey($"{name}_00")) return;
		frames.AddAnimation(name);
		frames.SetAnimationSpeed(name, fps);
		frames.SetAnimationLoop(name, loop);
		int index = 0;
		while (index < frameLimit && regions.ContainsKey($"{name}_{index:00}"))
		{
			AtlasTexture frame = new AtlasTexture { Atlas = atlas, Region = regions[$"{name}_{index:00}"] };
			frames.AddFrame(name, frame);
			index++;
		}
	}

	private static Dictionary<string, Rect2> LoadRegions(string metaPath, out Vector2 cell)
	{
		Dictionary<string, Rect2> regions = new Dictionary<string, Rect2>();
		cell = Vector2.Zero;
		using FileAccess file = FileAccess.Open(metaPath, FileAccess.ModeFlags.Read);
		if (file == null) return regions;
		Variant root = Json.ParseString(file.GetAsText());
		if (root.VariantType == Variant.Type.Nil) return regions;
		Godot.Collections.Dictionary dict = root.AsGodotDictionary();
		Godot.Collections.Array cellArr = dict["cell"].AsGodotArray();
		cell = new Vector2((float)cellArr[0].AsDouble(), (float)cellArr[1].AsDouble());
		Godot.Collections.Dictionary frames = dict["frames"].AsGodotDictionary();
		foreach (string key in frames.Keys)
		{
			Godot.Collections.Array r = frames[key].AsGodotArray();
			regions[key] = new Rect2((float)r[0].AsDouble(), (float)r[1].AsDouble(), (float)r[2].AsDouble(), (float)r[3].AsDouble());
		}
		return regions;
	}

	private static float ComputeRegionContentHeight(Texture2D atlas, Rect2 region)
	{
		Image full = atlas.GetImage();
		Image sub = full.GetRegion(new Rect2I((int)region.Position.X, (int)region.Position.Y, (int)region.Size.X, (int)region.Size.Y));
		int top = sub.GetHeight();
		int bottom = 0;
		for (int y = 0; y < sub.GetHeight(); y++)
		{
			for (int x = 0; x < sub.GetWidth(); x++)
			{
				if (sub.GetPixel(x, y).A > 0f)
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
