using Godot;

public partial class CardArtView : Node2D
{
	private const float MaskWidth = 636f;
	private const float MaskHeight = 531f;

	private Sprite2D _portrait;

	public override void _Ready()
	{
		_portrait = GetNode<Sprite2D>("ArtFrame/PortraitClip/Portrait");
	}

	public void SetPortrait(Texture2D texture)
	{
		float scaleX = MaskWidth / texture.GetWidth();
		float scaleY = MaskHeight / texture.GetHeight();
		float scale = Mathf.Max(scaleX, scaleY);
		_portrait.Scale = Vector2.One * scale;
		_portrait.Texture = texture;
	}

	public void SetPortraitTint(Color color)
	{
		_portrait.Modulate = color;
	}
}
