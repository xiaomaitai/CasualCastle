using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Building;
using CasualCastle.Domain.Shared;
using CasualCastle.Adapters.Godot;
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
        Vector2I.Zero, new(0, 1), new(0, 2), new(1, 2),
    };
    private static readonly Vector2I[] CastleHeartCells =
    {
        Vector2I.Zero, new(1, 0), new(0, 1), new(1, 1),
    };

    private readonly struct VisualDef
    {
        public Vector2I[] Footprint { get; init; }
        public string TexturePath { get; init; }
        public Vector2 SpriteScale { get; init; }
        public Color SpriteModulate { get; init; }
        public Vector2 CollisionSize { get; init; }
        public string MaterialPath { get; init; }
    }

    private static readonly Dictionary<string, VisualDef> Visuals = new()
    {
        ["CastleHeart"] = new()
        {
            Footprint = CastleHeartCells,
            TexturePath = PlaceholderTexturePath,
            SpriteScale = new(0.125f, 0.125f),
            SpriteModulate = new Color(1f, 0.82f, 0.35f),
            CollisionSize = new(120f, 120f),
        },
        ["Barracks"] = new()
        {
            Footprint = Single,
            TexturePath = PlaceholderTexturePath,
            SpriteScale = new(0.0625f, 0.0625f),
            SpriteModulate = Colors.White,
            CollisionSize = new(56f, 56f),
        },
        ["ArcheryRange"] = new()
        {
            Footprint = ArcheryRangeCells,
            TexturePath = PlaceholderTexturePath,
            SpriteScale = new(0.125f, 0.0625f),
            SpriteModulate = new Color(0.55f, 0.85f, 0.45f),
            CollisionSize = new(124f, 56f),
        },
        ["Stable"] = new()
        {
            Footprint = StableCells,
            TexturePath = PlaceholderTexturePath,
            SpriteScale = new(0.125f, 0.1875f),
            SpriteModulate = new Color(0.75f, 0.55f, 0.3f),
            CollisionSize = new(124f, 188f),
        },
        ["WolfDen"] = new()
        {
            Footprint = Single,
            TexturePath = PlaceholderTexturePath,
            SpriteScale = new(0.0625f, 0.0625f),
            SpriteModulate = new Color(0.55f, 0.35f, 0.75f),
            CollisionSize = new(56f, 56f),
        },
        ["BarracksT2"] = new()
        {
            Footprint = Single,
            TexturePath = PlaceholderTexturePath,
            SpriteScale = new(0.0625f, 0.0625f),
            SpriteModulate = new Color(0.85f, 0.9f, 1f),
            CollisionSize = new(56f, 56f),
        },
        ["WolfDenT2"] = new()
        {
            Footprint = Single,
            TexturePath = PlaceholderTexturePath,
            SpriteScale = new(0.0625f, 0.0625f),
            SpriteModulate = new Color(0.65f, 0.45f, 0.85f),
            CollisionSize = new(56f, 56f),
        },
    };

    private AdjacentSystem _adjacentSystem;

    public override void _Ready()
    {
        Instance = this;
        AdapterRegistry.Register<BuildingSystem>(this);
        _adjacentSystem = AdapterRegistry.Resolve<AdjacentSystem>();
    }

    public override void _ExitTree()
    {
        if (Instance == this)
        {
            AdapterRegistry.Unregister<BuildingSystem>(this);
            Instance = null;
        }
    }

    public static IReadOnlyList<Vector2I> GetFootprint(string buildingType)
    {
        IReadOnlyList<GridCellOffset> domain = BuildingDefinitions.GetFootprint(buildingType);
        Vector2I[] result = new Vector2I[domain.Count];
        for (int i = 0; i < domain.Count; i++)
            result[i] = new Vector2I(domain[i].X, domain[i].Y);
        return result;
    }

    public static Vector2I GetMainCellOffset(string buildingType)
    {
        GridCellOffset offset = BuildingDefinitions.GetMainCellOffset(buildingType);
        return new Vector2I(offset.X, offset.Y);
    }

    public static string GetDisplayName(string buildingType) => BuildingDefinitions.GetDisplayName(buildingType);
    public static int GetMaxHealth(string buildingType) => BuildingDefinitions.GetMaxHealth(buildingType);
    public static float GetSpawnInterval(string buildingType) => BuildingDefinitions.GetSpawnInterval(buildingType);

    public static Vector2I GetSpawnCellOffset(string buildingType)
    {
        GridCellOffset offset = BuildingDefinitions.GetSpawnCellOffset(buildingType);
        return new Vector2I(offset.X, offset.Y);
    }

    public static bool GetHasNightCombat(string buildingType) => BuildingDefinitions.GetHasNightCombat(buildingType);
    public static int GetFusionTier(string buildingType) => BuildingDefinitions.GetFusionTier(buildingType);
    public static bool IsCoreBuilding(string buildingType) => BuildingDefinitions.IsCoreBuilding(buildingType);
    public static bool IsFusibleMaterial(string buildingType) => BuildingDefinitions.IsFusibleMaterial(buildingType);

    private static VisualDef GetVisual(string buildingType)
    {
        if (Visuals.TryGetValue(buildingType, out VisualDef def))
            return def;
        return Visuals["Barracks"];
    }

    public static Color GetSpriteModulate(string buildingType) => GetVisual(buildingType).SpriteModulate;

    public static void ApplySoldierSpawnStats(string buildingType, Soldier soldier)
    {
        string unitTypeId = BuildingDefinitions.GetUnitTypeId(buildingType);
        UnitStats stats = UnitRegistry.Get(unitTypeId);
        soldier.InitializeFromStats(stats);
    }

    public static void ApplyVisual(Building building)
    {
        VisualDef visual = GetVisual(building.TypeId);
        Sprite2D sprite = building.GetNodeOrNull<Sprite2D>("Sprite");
        if (sprite != null)
        {
            Texture2D texture = GD.Load<Texture2D>(visual.TexturePath);
            if (texture != null) sprite.Texture = texture;
            sprite.Scale = visual.SpriteScale;
            sprite.Modulate = visual.SpriteModulate;
            if (!string.IsNullOrEmpty(visual.MaterialPath))
            {
                Material material = GD.Load<Material>(visual.MaterialPath);
                if (material != null) sprite.Material = material;
            }
        }

        CollisionShape2D shapeNode = building.GetNodeOrNull<CollisionShape2D>("CollisionShape");
        if (shapeNode?.Shape is RectangleShape2D rect)
            rect.Size = visual.CollisionSize;
    }

    public bool CanPlace(Castle castle, string buildingType, int anchorX, int anchorY)
    {
        if (castle == null) return false;
        return castle.CanPlaceFootprint(buildingType, anchorX, anchorY);
    }

    public bool TryPlace(Castle castle, string buildingType, int anchorX, int anchorY)
    {
        if (castle == null || !castle.IsPlayerCastle)
            return false;
        return TryPlaceForCastle(castle, buildingType, anchorX, anchorY);
    }

    public bool TryPlaceForCastle(Castle castle, string buildingType, int anchorX, int anchorY)
    {
        if (castle == null) return false;
        if (!CanPlace(castle, buildingType, anchorX, anchorY)) return false;

        Building building = InstantiateBuilding();
        if (building == null) return false;

        building.BindToGrid(castle, anchorX, anchorY);
        if (!castle.PlaceBuilding(building, anchorX, anchorY, buildingType))
        {
            building.QueueFree();
            return false;
        }

        EmitSignal(SignalName.BuildingPlaced, castle, building, buildingType);
        _adjacentSystem?.OnBuildingPlaced(castle, building);
        return true;
    }

    public static Building CreateBuilding(string buildingType)
    {
        Building building = InstantiateBuilding();
        if (building == null) return null;
        building.InitFromType(buildingType);
        return building;
    }

    private static Building InstantiateBuilding()
    {
        PackedScene scene = GD.Load<PackedScene>(BuildingPrefabPath);
        return scene?.Instantiate<Building>();
    }
}
