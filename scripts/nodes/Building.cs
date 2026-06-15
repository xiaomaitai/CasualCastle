using Godot;

public partial class Building : Area2D
{
	[Export]
	public int CollisionSize = 56;

	protected Castle CastleRef;
	protected int GridX;
	protected int GridY;

	public void BindToGrid(Castle castle, int gridX, int gridY)
	{
		CastleRef = castle;
		GridX = gridX;
		GridY = gridY;
	}

	public Castle GetCastle() => CastleRef;

	public override void _Ready()
	{
		CollisionLayer = 4;
		CollisionMask = 0;

		CollisionShape2D shapeNode = GetNodeOrNull<CollisionShape2D>("CollisionShape");
		if (shapeNode?.Shape is RectangleShape2D rect)
		{
			rect.Size = new Vector2(CollisionSize, CollisionSize);
		}
	}
}
