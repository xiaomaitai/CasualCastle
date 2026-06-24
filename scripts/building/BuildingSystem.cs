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

    private static readonly Dictionary<string, BuildingDefinition> Definitions = new()
    {
        ["CastleHeart"] = BuildingDefinition.Create(
            CastleHeartCells, new(0, 0), "城堡之心", GameConfig.CastleHeartMaxHealth,
            "res://prefabs/CastleHeart.tscn"),
        ["Barracks"] = BuildingDefinition.Create(
            Single, new(0, 0), "兵营", 100,
            "res://prefabs/Barracks.tscn",
            spawnInterval: 5f),
        ["ArcheryRange"] = BuildingDefinition.Create(
            ArcheryRangeCells, new(0, 0), "靶场", 120,
            "res://prefabs/ArcheryRange.tscn",
            spawnInterval: 6f, spawnCellOffset: new(1, 0),
            soldierDamage: 8, soldierAttackRange: 50f),
        ["Stable"] = BuildingDefinition.Create(
            StableCells, new(0, 1), "马厩", 150,
            "res://prefabs/Stable.tscn",
            spawnInterval: 5f, spawnCellOffset: new(1, 2),
            soldierSpeed: 120f),
    };

    private readonly struct BuildingDefinition
    {
        public Vector2I[] Footprint { get; init; }
        public Vector2I MainCellOffset { get; init; }
        public string DisplayName { get; init; }
        public int MaxHealth { get; init; }
        public string PrefabPath { get; init; }
        public float SpawnInterval { get; init; }
        public Vector2I SpawnCellOffset { get; init; }
        public int? SoldierDamage { get; init; }
        public float? SoldierAttackRange { get; init; }
        public float? SoldierSpeed { get; init; }

        public static BuildingDefinition Create(
            Vector2I[] footprint,
            Vector2I mainCellOffset,
            string displayName,
            int maxHealth,
            string prefabPath,
            float spawnInterval = 0f,
            Vector2I? spawnCellOffset = null,
            int? soldierDamage = null,
            float? soldierAttackRange = null,
            float? soldierSpeed = null)
        {
            return new BuildingDefinition
            {
                Footprint = footprint,
                MainCellOffset = mainCellOffset,
                DisplayName = displayName,
                MaxHealth = maxHealth,
                PrefabPath = prefabPath,
                SpawnInterval = spawnInterval,
                SpawnCellOffset = spawnCellOffset ?? Vector2I.Zero,
                SoldierDamage = soldierDamage,
                SoldierAttackRange = soldierAttackRange,
                SoldierSpeed = soldierSpeed,
            };
        }
    }

    private static BuildingDefinition GetDefinition(string buildingType)
    {
        if (Definitions.TryGetValue(buildingType, out BuildingDefinition definition))
            return definition;

        return Definitions["Barracks"];
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

    public static IReadOnlyList<Vector2I> GetFootprint(string buildingType) => GetDefinition(buildingType).Footprint;

    public static Vector2I GetMainCellOffset(string buildingType) => GetDefinition(buildingType).MainCellOffset;

    public static string GetDisplayName(string buildingType) => GetDefinition(buildingType).DisplayName;

    public static int GetMaxHealth(string buildingType) => GetDefinition(buildingType).MaxHealth;

    public static float GetSpawnInterval(string buildingType) => GetDefinition(buildingType).SpawnInterval;

    public static Vector2I GetSpawnCellOffset(string buildingType) => GetDefinition(buildingType).SpawnCellOffset;

    public static void ApplySoldierSpawnStats(string buildingType, Soldier soldier)
    {
        BuildingDefinition definition = GetDefinition(buildingType);
        if (definition.SoldierDamage.HasValue)
            soldier.Damage = definition.SoldierDamage.Value;
        if (definition.SoldierAttackRange.HasValue)
            soldier.AttackRange = definition.SoldierAttackRange.Value;
        if (definition.SoldierSpeed.HasValue)
            soldier.Speed = definition.SoldierSpeed.Value;
    }

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
        string prefabPath = GetDefinition(buildingType).PrefabPath;
        PackedScene scene = GD.Load<PackedScene>(prefabPath);
        return scene?.Instantiate<Building>();
    }
}
