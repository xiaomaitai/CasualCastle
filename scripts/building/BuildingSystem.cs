using Godot;
using System.Collections.Generic;

public partial class BuildingSystem : Node
{
    public static BuildingSystem Instance { get; private set; }

    private const string BuildingPrefabPath = "res://prefabs/Building.tscn";
    private const string PlaceholderTexturePath = "res://assets/art/placeholders/test_image.png";

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
            PlaceholderTexturePath,
            new Vector2(0.125f, 0.125f),
            new Color(1f, 0.82f, 0.35f),
            new Vector2(120f, 120f)),
        ["Barracks"] = BuildingDefinition.Create(
            Single, new(0, 0), "兵营", 100,
            PlaceholderTexturePath,
            new Vector2(0.0625f, 0.0625f),
            Colors.White,
            new Vector2(56f, 56f),
            spawnInterval: 5f),
        ["ArcheryRange"] = BuildingDefinition.Create(
            ArcheryRangeCells, new(0, 0), "靶场", 120,
            PlaceholderTexturePath,
            new Vector2(0.125f, 0.0625f),
            new Color(0.55f, 0.85f, 0.45f),
            new Vector2(124f, 56f),
            spawnInterval: 6f, spawnCellOffset: new(1, 0),
            soldierDamage: 8, soldierAttackRange: 50f),
        ["Stable"] = BuildingDefinition.Create(
            StableCells, new(0, 1), "马厩", 150,
            PlaceholderTexturePath,
            new Vector2(0.125f, 0.1875f),
            new Color(0.75f, 0.55f, 0.3f),
            new Vector2(124f, 188f),
            spawnInterval: 5f, spawnCellOffset: new(1, 2),
            soldierSpeed: 120f),
        ["WolfDen"] = BuildingDefinition.Create(
            Single, new(0, 0), "狼穴", 90,
            PlaceholderTexturePath,
            new Vector2(0.0625f, 0.0625f),
            new Color(0.55f, 0.35f, 0.75f),
            new Vector2(56f, 56f),
            spawnInterval: 6f,
            soldierDamage: 12,
            soldierSpeed: 95f,
            soldierHealth: 35,
            hasNightCombat: true,
            soldierSpriteModulate: new Color(0.75f, 0.55f, 0.95f)),
        ["BarracksT2"] = BuildingDefinition.Create(
            Single, new(0, 0), "强化兵营", 130,
            PlaceholderTexturePath,
            new Vector2(0.0625f, 0.0625f),
            new Color(0.85f, 0.9f, 1f),
            new Vector2(56f, 56f),
            spawnInterval: 4f,
            fusionTier: 1),
        ["WolfDenT2"] = BuildingDefinition.Create(
            Single, new(0, 0), "强化狼穴", 120,
            PlaceholderTexturePath,
            new Vector2(0.0625f, 0.0625f),
            new Color(0.65f, 0.45f, 0.85f),
            new Vector2(56f, 56f),
            spawnInterval: 5f,
            soldierDamage: 16,
            soldierSpeed: 95f,
            soldierHealth: 35,
            hasNightCombat: true,
            soldierSpriteModulate: new Color(0.8f, 0.6f, 1f),
            fusionTier: 1),
    };

    private readonly struct BuildingDefinition
    {
        public Vector2I[] Footprint { get; init; }
        public Vector2I MainCellOffset { get; init; }
        public string DisplayName { get; init; }
        public int MaxHealth { get; init; }
        public string TexturePath { get; init; }
        public Vector2 SpriteScale { get; init; }
        public Color SpriteModulate { get; init; }
        public Vector2 CollisionSize { get; init; }
        public string MaterialPath { get; init; }
        public float SpawnInterval { get; init; }
        public Vector2I SpawnCellOffset { get; init; }
        public int? SoldierDamage { get; init; }
        public float? SoldierAttackRange { get; init; }
        public float? SoldierSpeed { get; init; }
        public int? SoldierHealth { get; init; }
        public bool HasNightCombat { get; init; }
        public Color? SoldierSpriteModulate { get; init; }
        public int FusionTier { get; init; }

        public static BuildingDefinition Create(
            Vector2I[] footprint,
            Vector2I mainCellOffset,
            string displayName,
            int maxHealth,
            string texturePath,
            Vector2 spriteScale,
            Color spriteModulate,
            Vector2 collisionSize,
            float spawnInterval = 0f,
            Vector2I? spawnCellOffset = null,
            int? soldierDamage = null,
            float? soldierAttackRange = null,
            float? soldierSpeed = null,
            int? soldierHealth = null,
            bool hasNightCombat = false,
            Color? soldierSpriteModulate = null,
            int fusionTier = 0,
            string materialPath = null)
        {
            return new BuildingDefinition
            {
                Footprint = footprint,
                MainCellOffset = mainCellOffset,
                DisplayName = displayName,
                MaxHealth = maxHealth,
                TexturePath = texturePath,
                SpriteScale = spriteScale,
                SpriteModulate = spriteModulate,
                CollisionSize = collisionSize,
                MaterialPath = materialPath,
                SpawnInterval = spawnInterval,
                SpawnCellOffset = spawnCellOffset ?? Vector2I.Zero,
                SoldierDamage = soldierDamage,
                SoldierAttackRange = soldierAttackRange,
                SoldierSpeed = soldierSpeed,
                SoldierHealth = soldierHealth,
                HasNightCombat = hasNightCombat,
                SoldierSpriteModulate = soldierSpriteModulate,
                FusionTier = fusionTier,
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

    public static Color GetSpriteModulate(string buildingType) => GetDefinition(buildingType).SpriteModulate;

    public static float GetSpawnInterval(string buildingType) => GetDefinition(buildingType).SpawnInterval;

    public static Vector2I GetSpawnCellOffset(string buildingType) => GetDefinition(buildingType).SpawnCellOffset;

    public static bool GetHasNightCombat(string buildingType) => GetDefinition(buildingType).HasNightCombat;

    public static int GetFusionTier(string buildingType) => GetDefinition(buildingType).FusionTier;

    public static bool IsFusibleMaterial(string buildingType)
    {
        BuildingDefinition definition = GetDefinition(buildingType);
        return definition.FusionTier == 0
            && definition.Footprint.Length == 1
            && !IsCoreBuilding(buildingType);
    }

    public static void ApplySoldierSpawnStats(string buildingType, Soldier soldier)
    {
        BuildingDefinition definition = GetDefinition(buildingType);
        soldier.HasNightCombat = definition.HasNightCombat;
        if (definition.SoldierDamage.HasValue)
            soldier.Damage = definition.SoldierDamage.Value;
        if (definition.SoldierAttackRange.HasValue)
            soldier.AttackRange = definition.SoldierAttackRange.Value;
        if (definition.SoldierSpeed.HasValue)
            soldier.Speed = definition.SoldierSpeed.Value;
        if (definition.SoldierHealth.HasValue)
            soldier.Health = definition.SoldierHealth.Value;

        if (definition.SoldierSpriteModulate.HasValue)
            soldier.SetBaseSpriteModulate(definition.SoldierSpriteModulate.Value);
    }

    public static void ApplyVisual(Building building)
    {
        BuildingDefinition definition = GetDefinition(building.TypeId);

        Sprite2D sprite = building.GetNodeOrNull<Sprite2D>("Sprite");
        if (sprite != null)
        {
            Texture2D texture = GD.Load<Texture2D>(definition.TexturePath);
            if (texture != null)
                sprite.Texture = texture;

            sprite.Scale = definition.SpriteScale;
            sprite.Modulate = definition.SpriteModulate;

            if (!string.IsNullOrEmpty(definition.MaterialPath))
            {
                Material material = GD.Load<Material>(definition.MaterialPath);
                if (material != null)
                    sprite.Material = material;
            }
        }

        CollisionShape2D shapeNode = building.GetNodeOrNull<CollisionShape2D>("CollisionShape");
        if (shapeNode?.Shape is RectangleShape2D rect)
            rect.Size = definition.CollisionSize;
    }

    public static bool IsCoreBuilding(string buildingType) => buildingType == "CastleHeart";

    public static Building CreateBuilding(string buildingType)
    {
        Building building = InstantiateBuilding();
        if (building == null)
            return null;

        building.InitFromType(buildingType);
        return building;
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

        Building building = CreateBuilding(buildingType);
        if (building == null)
            return false;

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

    private static Building InstantiateBuilding()
    {
        PackedScene scene = GD.Load<PackedScene>(BuildingPrefabPath);
        return scene?.Instantiate<Building>();
    }
}
