using Godot;

public partial class CastleArea : Area2D
{
	[Export]
	public int GridColumns = 8;

	[Export]
	public int GridRows = 8;

	[Export]
	public int CellSize = 64;

	[Export]
	public int BlockSize = 60;

	[Export]
	public Color BlockColor = new Color(1, 1, 1, 0.06f);

	[Export]
	public Color AreaBgColor = new Color(1, 1, 1, 0.03f);

	public bool IsPlayerCastle { get; private set; }

	public override void _Ready()
	{
		Node2D parent = GetParent() as Node2D;
		IsPlayerCastle = parent != null && parent.Name == "PlayerSide";

		CollisionLayer = 4;
		CollisionMask = 0;

		var shape = new CollisionShape2D();
		var rect = new RectangleShape2D();
		rect.Size = new Vector2(GridColumns * CellSize, GridRows * CellSize);
		shape.Shape = rect;
		shape.Position = rect.Size / 2;
		AddChild(shape);
	}

	public override void _Draw()
	{
		int totalWidth = GridColumns * CellSize;
		int totalHeight = GridRows * CellSize;

		DrawRect(new Rect2(0, 0, totalWidth, totalHeight), AreaBgColor);

		int offset = (CellSize - BlockSize) / 2;

		for (int row = 0; row < GridRows; row++)
		{
			for (int col = 0; col < GridColumns; col++)
			{
				Vector2 position = new Vector2(col * CellSize + offset, row * CellSize + offset);
				DrawRect(new Rect2(position, new Vector2(BlockSize, BlockSize)), BlockColor);
			}
		}
	}
}
