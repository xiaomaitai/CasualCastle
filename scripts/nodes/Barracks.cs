using Godot;

public partial class Barracks : Building
{
	[Export]
	public float SpawnInterval = 5.0f;

	public bool IsPlayerBarracks { get; private set; }

	private Node2D _battlefield;
	private int _spawnCount;

	public new void BindToGrid(Castle castle, int gridX, int gridY)
	{
		base.BindToGrid(castle, gridX, gridY);
		IsPlayerBarracks = castle.IsPlayerCastle;
	}

	public override void _Ready()
	{
		base._Ready();

		_battlefield = GetNodeOrNull<Node2D>("/root/MainGame/Battlefield");
		if (_battlefield == null)
			_battlefield = GetTree().Root.GetNodeOrNull<Node2D>("MainGame/Battlefield");

		UpdateWorkCycle();
	}

	protected override void StartWorkCycle()
	{
		BeginWorkCycle(SpawnInterval, SpawnUnit);
	}

	private void SpawnUnit()
	{
		SpawnUnits(1);
	}

	public void SpawnUnits(int count)
	{
		if (!CanWork || _battlefield == null || CastleRef == null || count <= 0) return;
		if (GameManager.Instance?.CurrentState == GameManager.GameState.GameOver) return;

		PackedScene soldierScene = GD.Load<PackedScene>("res://prefabs/Soldier.tscn");
		if (soldierScene == null) return;

		Vector2I marchDir = IsPlayerBarracks ? Vector2I.Right : Vector2I.Left;

		for (int i = 0; i < count; i++)
		{
			Vector2 spawnLocal = CastleRef.GetBuildingSpawnPosition(GridX, GridY, marchDir, _spawnCount);
			_spawnCount++;

			Soldier soldier = soldierScene.Instantiate<Soldier>();
			soldier.GlobalPosition = CastleRef.ToGlobal(spawnLocal);
			soldier.IsPlayerUnit = IsPlayerBarracks;
			_battlefield.AddChild(soldier);
		}
	}
}
