using Godot;

public partial class CardArtView : Node2D
{
	private const float MaskWidth = 636f;
	private const float MaskHeight = 531f;

	private Sprite2D _portrait;
	private bool _flipH;

	public override void _Ready()
	{
		_portrait = GetNode<Sprite2D>("ArtFrame/PortraitClip/Portrait");
	}

	public void SetPortrait(Texture2D texture)
	{
		float scaleX = MaskWidth / texture.GetWidth();
		float scaleY = MaskHeight / texture.GetHeight();
		float scale = Mathf.Max(scaleX, scaleY);
		float xScale = _flipH ? -scale : scale;
		_portrait.Scale = new Vector2(xScale, scale);
		_portrait.Texture = texture;
	}

	public void SetFlipH(bool flip)
	{
		if (_flipH == flip)
			return;
		_flipH = flip;
		_portrait.Scale = new Vector2(-_portrait.Scale.X, _portrait.Scale.Y);
	}

	public void SetPortraitTint(Color color)
	{
		_portrait.Modulate = color;
	}
}
