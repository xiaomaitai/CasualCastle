using Godot;

public partial class BgmPlayer : AudioStreamPlayer
{
	public override void _Ready()
	{
		Stream = GD.Load<AudioStream>("res://assets/audio/bgm/feel_good_island_loop.ogg");
		Finished += () => Play();
		Play();
	}
}
