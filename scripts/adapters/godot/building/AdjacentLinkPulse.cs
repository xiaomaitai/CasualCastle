using Godot;

public partial class AdjacentLinkPulse : Node2D
{
    private const float Duration = 0.38f;
    private const float DisplayScale = 0.75f;

    private float _elapsed;
    private int _cellSize = 64;
    private ShaderMaterial _material;

    public void Configure(int cellSize)
    {
        _cellSize = cellSize;
    }

    public override void _Ready()
    {
        ZIndex = 20;

        float size = _cellSize * DisplayScale;
        var sprite = new Sprite2D
        {
            Centered = true,
            Texture = CreateWhiteTexture(),
            Scale = new Vector2(size, size),
        };

        Shader shader = GD.Load<Shader>("res://assets/shaders/adjacent_link_pulse.gdshader");
        _material = new ShaderMaterial { Shader = shader };
        sprite.Material = _material;
        AddChild(sprite);
        SetProcess(true);
    }

    public override void _Process(double delta)
    {
        _elapsed += (float)delta;
        float progress = Mathf.Clamp(_elapsed / Duration, 0f, 1f);
        _material.SetShaderParameter("progress", progress);

        if (_elapsed >= Duration)
            QueueFree();
    }

    private static ImageTexture CreateWhiteTexture()
    {
        Image image = Image.CreateEmpty(1, 1, false, Image.Format.Rgba8);
        image.SetPixel(0, 0, Colors.White);
        return ImageTexture.CreateFromImage(image);
    }
}
