// 单位动画预览：加载剑士帧动画按状态循环播放，数字键切换
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
		SpriteFrames frames = new SpriteFrames();
		for (int i = 0; i < _states.Length; i++) AddAnimation(frames, _states[i]);
		return frames;
	}

	private void AddAnimation(SpriteFrames frames, string state)
	{
		frames.AddAnimation(state);
		frames.SetAnimationSpeed(state, _stateFps[state]);
		frames.SetAnimationLoop(state, state != "death");
		int index = 0;
		while (ResourceLoader.Exists(FramePath(state, index)))
		{
			frames.AddFrame(state, GD.Load<Texture2D>(FramePath(state, index)));
			index++;
		}
	}

	private static string FramePath(string state, int index) => $"{FramesDirectory}/{state}_{index:00}.png";
}
