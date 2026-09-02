// 单位动画预览：加载剑士帧动画并按状态循环播放，数字键 1-5 切换
using Godot;
using System.Collections.Generic;

public partial class UnitAnimPreview : Node2D
{
	private const string FramesDirectory = "res://assets/art/units/swordsman/frames";
	private const float AttackFps = 12f;
	private const float ComboFps = 12f;
	private const float RunFps = 10f;
	private const float HurtFps = 8f;
	private const float DeathFps = 8f;

	private readonly string[] _states = { "attack", "attack_combo", "run", "hurt", "death" };
	private readonly Dictionary<string, float> _stateFps = new Dictionary<string, float>
	{
		{ "attack", AttackFps },
		{ "attack_combo", ComboFps },
		{ "run", RunFps },
		{ "hurt", HurtFps },
		{ "death", DeathFps }
	};

	private AnimatedSprite2D _sprite;
	private Label _hint;

	public override void _Ready()
	{
		_sprite = GetNode<AnimatedSprite2D>("AnimatedSprite");
		_hint = GetNode<Label>("Hint");
		_sprite.SpriteFrames = BuildFrames();
		PlayState(0);
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is not InputEventKey key) return;
		if (!key.Pressed || key.Echo) return;
		int index = (int)key.Keycode - (int)Key.Key1;
		if (index >= 0 && index < _states.Length) PlayState(index);
	}

	private void PlayState(int index)
	{
		_sprite.Play(_states[index]);
		_hint.Text = "剑士帧动画    当前: " + _states[index] + "    数字键 1-5 切换";
	}

	private SpriteFrames BuildFrames()
	{
		Texture2D atlas = GD.Load<Texture2D>($"{FramesDirectory}/atlas.png");
		Dictionary<string, Rect2> regions = LoadRegions($"{FramesDirectory}/atlas.json");
		SpriteFrames frames = new SpriteFrames();
		foreach (string state in _states) AddAnimation(frames, atlas, regions, state, _stateFps[state]);
		return frames;
	}

	private static void AddAnimation(SpriteFrames frames, Texture2D atlas, Dictionary<string, Rect2> regions, string name, float fps)
	{
		if (!regions.ContainsKey($"{name}_00")) return;
		frames.AddAnimation(name);
		frames.SetAnimationSpeed(name, fps);
		frames.SetAnimationLoop(name, name != "death");
		int index = 0;
		while (regions.ContainsKey($"{name}_{index:00}"))
		{
			AtlasTexture frame = new AtlasTexture { Atlas = atlas, Region = regions[$"{name}_{index:00}"] };
			frames.AddFrame(name, frame);
			index++;
		}
	}

	private static Dictionary<string, Rect2> LoadRegions(string metaPath)
	{
		Dictionary<string, Rect2> regions = new Dictionary<string, Rect2>();
		using FileAccess file = FileAccess.Open(metaPath, FileAccess.ModeFlags.Read);
		if (file == null) return regions;
		Variant root = Json.ParseString(file.GetAsText());
		if (root.VariantType == Variant.Type.Nil) return regions;
		Godot.Collections.Dictionary dict = root.AsGodotDictionary();
		Godot.Collections.Dictionary frames = dict["frames"].AsGodotDictionary();
		foreach (string key in frames.Keys)
		{
			Godot.Collections.Array r = frames[key].AsGodotArray();
			regions[key] = new Rect2((float)r[0].AsDouble(), (float)r[1].AsDouble(), (float)r[2].AsDouble(), (float)r[3].AsDouble());
		}
		return regions;
	}
}
