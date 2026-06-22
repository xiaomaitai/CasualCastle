using Godot;
using System.Collections.Generic;

public partial class BuildingSystem : Node
{
    public static BuildingSystem Instance { get; private set; }

    [Signal]
    public delegate void BuildingPlacedEventHandler(Castle castle, Building building, string buildingType);

    private static readonly Vector2I[] Single = { Vector2I.Zero };
    private static readonly Vector2I[] ArcheryRangeCells = { Vector2I.Zero, new(1, 0) };
    private static readonly Vector2I[] StableCells =
    {
        Vector2I.Zero,
        new(0, 1),
        new(0, 2),
        new(1, 2),
    };

    public override void _Ready()
    {
        Instance = this;
    }

    public override void _ExitTree()
    {
        if (Instance == this)
            Instance = null;
    }

    public static IReadOnlyList<Vector2I> GetFootprint(string buildingType) => buildingType switch
    {
        "ArcheryRange" => ArcheryRangeCells,
        "Stable" => StableCells,
        _ => Single,
    };

    public static Vector2I GetMainCellOffset(string buildingType)
    {
        IReadOnlyList<Vector2I> cells = GetFootprint(buildingType);
        if (cells.Count == 1)
            return cells[0];

        int minX = cells[0].X;
        int maxX = cells[0].X;
        int minY = cells[0].Y;
        int maxY = cells[0].Y;
        foreach (Vector2I cell in cells)
        {
            minX = Mathf.Min(minX, cell.X);
            maxX = Mathf.Max(maxX, cell.X);
            minY = Mathf.Min(minY, cell.Y);
            maxY = Mathf.Max(maxY, cell.Y);
        }

        float centerX = (minX + maxX) / 2f;
        float centerY = (minY + maxY) / 2f;
        Vector2I best = cells[0];
        float bestDistSq = float.MaxValue;
        foreach (Vector2I cell in cells)
        {
            float dx = cell.X - centerX;
            float dy = cell.Y - centerY;
            float distSq = dx * dx + dy * dy;
            if (distSq < bestDistSq)
            {
                bestDistSq = distSq;
                best = cell;
            }
        }

        return best;
    }

    public bool CanPlace(Castle castle, string buildingType, int anchorX, int anchorY)
    {
        if (castle == null)
            return false;

        foreach (Vector2I offset in GetFootprint(buildingType))
        {
            if (!castle.IsCellPassable(anchorX + offset.X, anchorY + offset.Y))
                return false;
        }

        return true;
    }

    public bool TryPlace(Castle castle, string buildingType, int anchorX, int anchorY)
    {
        if (castle == null || !castle.IsPlayerCastle)
            return false;

        if (!CanPlace(castle, buildingType, anchorX, anchorY))
            return false;

        Building building = InstantiateBuilding(buildingType);
        if (building == null)
            return false;

        building.TypeId = buildingType;
        building.BindToGrid(castle, anchorX, anchorY);
        if (!castle.PlaceBuilding(building, anchorX, anchorY, buildingType))
        {
            building.QueueFree();
            return false;
        }

        EmitSignal(SignalName.BuildingPlaced, castle, building, buildingType);
        AdjacentSystem.Instance?.OnBuildingPlaced(castle, building);
        return true;
    }

    private static Building InstantiateBuilding(string buildingType)
    {
        PackedScene scene = buildingType switch
        {
            "ArcheryRange" => GD.Load<PackedScene>("res://prefabs/ArcheryRange.tscn"),
            "Stable" => GD.Load<PackedScene>("res://prefabs/Stable.tscn"),
            _ => GD.Load<PackedScene>("res://prefabs/Barracks.tscn"),
        };

        return scene?.Instantiate<Building>();
    }
}
