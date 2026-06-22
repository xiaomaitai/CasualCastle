using Godot;

public partial class ArcheryRange : Building
{
    [Export]
    public float SpawnInterval = 6.0f;

    public bool IsPlayerBuilding { get; private set; }

    private Node2D _battlefield;
    private int _spawnCount;

    public new void BindToGrid(Castle castle, int gridX, int gridY)
    {
        base.BindToGrid(castle, gridX, gridY);
        IsPlayerBuilding = castle.IsPlayerCastle;
    }

    public override void _Ready()
    {
        base._Ready();
        _battlefield = GetTree().Root.GetNodeOrNull<Node2D>("MainGame/Battlefield");
        UpdateWorkCycle();
    }

    protected override void StartWorkCycle()
    {
        BeginWork(GetWorkInterval(SpawnInterval));
    }

    protected override void PerformWork()
    {
        SpawnUnits(1);
    }

    private void SpawnUnits(int count)
    {
        if (!CanWork || _battlefield == null || CastleRef == null || count <= 0)
            return;
        if (GameManager.Instance?.CurrentState == GameManager.GameState.GameOver)
            return;

        PackedScene soldierScene = GD.Load<PackedScene>("res://prefabs/Soldier.tscn");
        if (soldierScene == null)
            return;

        Vector2I marchDir = IsPlayerBuilding ? Vector2I.Right : Vector2I.Left;
        int spawnGridX = GridX + 1;
        int spawnGridY = GridY;

        for (int i = 0; i < count; i++)
        {
            Vector2 spawnLocal = CastleRef.GetBuildingSpawnPosition(spawnGridX, spawnGridY, marchDir, _spawnCount);
            _spawnCount++;

            Soldier soldier = soldierScene.Instantiate<Soldier>();
            soldier.GlobalPosition = CastleRef.ToGlobal(spawnLocal);
            soldier.IsPlayerUnit = IsPlayerBuilding;
            soldier.AttackRange = 50f;
            soldier.Damage = 8;
            _battlefield.AddChild(soldier);
        }
    }
}
