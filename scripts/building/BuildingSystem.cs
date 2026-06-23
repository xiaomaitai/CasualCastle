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

    private static readonly Vector2I[] CastleHeartCells =
    {
        Vector2I.Zero,
        new(1, 0),
        new(0, 1),
        new(1, 1),
    };

    private static readonly Dictionary<string, BuildingShape> Shapes = new()
    {
        ["CastleHeart"] = BuildingShape.Create(CastleHeartCells, new(0, 0), "城堡之心", GameConfig.CastleHeartMaxHealth),
        ["Barracks"] = BuildingShape.Create(Single, new(0, 0), "兵营", 100),
        ["ArcheryRange"] = BuildingShape.Create(ArcheryRangeCells, new(0, 0), "靶场", 120),
        ["Stable"] = BuildingShape.Create(StableCells, new(0, 1), "马厩", 150),
    };

    private readonly struct BuildingShape
    {
        public Vector2I[] Footprint { get; init; }
        public Vector2I MainCellOffset { get; init; }
        public string DisplayName { get; init; }
        public int MaxHealth { get; init; }

        public static BuildingShape Create(Vector2I[] footprint, Vector2I mainCellOffset, string displayName, int maxHealth)
        {
            return new BuildingShape
            {
                Footprint = footprint,
                MainCellOffset = mainCellOffset,
                DisplayName = displayName,
                MaxHealth = maxHealth,
            };
        }
    }

    private static BuildingShape GetShape(string buildingType)
    {
        if (Shapes.TryGetValue(buildingType, out BuildingShape shape))
            return shape;

        return Shapes["Barracks"];
    }

    public override void _Ready()
    {
        Instance = this;
    }

    public override void _ExitTree()
    {
        if (Instance == this)
            Instance = null;
    }

    public static IReadOnlyList<Vector2I> GetFootprint(string buildingType) => GetShape(buildingType).Footprint;

    public static Vector2I GetMainCellOffset(string buildingType) => GetShape(buildingType).MainCellOffset;

    public static string GetDisplayName(string buildingType) => GetShape(buildingType).DisplayName;

    public static int GetMaxHealth(string buildingType) => GetShape(buildingType).MaxHealth;

    public static bool IsCoreBuilding(string buildingType) => buildingType == "CastleHeart";

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
        building.InitFromType(buildingType);
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
            "CastleHeart" => GD.Load<PackedScene>("res://prefabs/CastleHeart.tscn"),
            "ArcheryRange" => GD.Load<PackedScene>("res://prefabs/ArcheryRange.tscn"),
            "Stable" => GD.Load<PackedScene>("res://prefabs/Stable.tscn"),
            _ => GD.Load<PackedScene>("res://prefabs/Barracks.tscn"),
        };

        return scene?.Instantiate<Building>();
    }
}
