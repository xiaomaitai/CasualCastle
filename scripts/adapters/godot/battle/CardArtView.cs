using Godot;

public partial class CardArtView : Node2D
{
	private Sprite2D _portrait;

	public override void _Ready()
	{
		_portrait = GetNode<Sprite2D>("ArtFrame/PortraitClip/Portrait");
	}

	public void SetPortraitTint(Color color)
	{
		_portrait.Modulate = color;
	}
}
