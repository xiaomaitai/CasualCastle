using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Building;
using CasualCastle.Domain.Shared;
using CasualCastle.Adapters.Godot;
using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class BuildingSystem : Node
{
    public static BuildingSystem Instance { get; private set; }

    private const string BuildingPrefabPath = "res://prefabs/Building.tscn";
    private const string PlaceholderTexturePath = "res://assets/art/placeholders/test_image.png";

    [Signal]
    public delegate void BuildingPlacedEventHandler(Castle castle, Building building, string buildingType);

    private static readonly Vector2I[] Footprint2x2 =
    {
        Vector2I.Zero, new(1, 0), new(0, 1), new(1, 1),
    };

    private readonly struct VisualDef
    {
        public Vector2I[] Footprint { get; init; }
        public string TexturePath { get; init; }
        public Vector2 SpriteScale { get; init; }
        public Color SpriteModulate { get; init; }
        public string MaterialPath { get; init; }
    }

    private static readonly Dictionary<string, VisualDef> Visuals = new()
    {
        ["CastleHeart"] = new()
        {
            Footprint = Footprint2x2,
            TexturePath = PlaceholderTexturePath,
            SpriteScale = new(0.125f, 0.125f),
            SpriteModulate = new Color(1f, 0.82f, 0.35f),
        },
        ["Barracks"] = new()
        {
            Footprint = Footprint2x2,
            TexturePath = PlaceholderTexturePath,
            SpriteScale = new(0.125f, 0.125f),
            SpriteModulate = new Color(0.55f, 0.6f, 0.75f),
        },
        ["ShieldCamp"] = new()
        {
            Footprint = Footprint2x2,
            TexturePath = PlaceholderTexturePath,
            SpriteScale = new(0.125f, 0.125f),
            SpriteModulate = new Color(0.4f, 0.4f, 0.45f),
        },
        ["ArcheryRange"] = new()
        {
            Footprint = Footprint2x2,
            TexturePath = PlaceholderTexturePath,
            SpriteScale = new(0.125f, 0.125f),
            SpriteModulate = new Color(0.45f, 0.75f, 0.45f),
        },
        ["Stable"] = new()
        {
            Footprint = Footprint2x2,
            TexturePath = PlaceholderTexturePath,
            SpriteScale = new(0.125f, 0.125f),
            SpriteModulate = new Color(0.65f, 0.5f, 0.3f),
        },
        ["ScoutCamp"] = new()
        {
            Footprint = Footprint2x2,
            TexturePath = PlaceholderTexturePath,
            SpriteScale = new(0.125f, 0.125f),
            SpriteModulate = new Color(0.5f, 0.65f, 0.85f),
        },
        ["Armory"] = new()
        {
            Footprint = Footprint2x2,
            TexturePath = PlaceholderTexturePath,
            SpriteScale = new(0.125f, 0.125f),
            SpriteModulate = new Color(0.65f, 0.7f, 0.85f),
        },
        ["Bulwark"] = new()
        {
            Footprint = Footprint2x2,
            TexturePath = PlaceholderTexturePath,
            SpriteScale = new(0.125f, 0.125f),
            SpriteModulate = new Color(0.5f, 0.5f, 0.55f),
        },
        ["CrossbowTower"] = new()
        {
            Footprint = Footprint2x2,
            TexturePath = PlaceholderTexturePath,
            SpriteScale = new(0.125f, 0.125f),
            SpriteModulate = new Color(0.55f, 0.85f, 0.55f),
        },
        ["Ranch"] = new()
        {
            Footprint = Footprint2x2,
            TexturePath = PlaceholderTexturePath,
            SpriteScale = new(0.125f, 0.125f),
            SpriteModulate = new Color(0.75f, 0.6f, 0.4f),
        },
        ["RangerPost"] = new()
        {
            Footprint = Footprint2x2,
            TexturePath = PlaceholderTexturePath,
            SpriteScale = new(0.125f, 0.125f),
            SpriteModulate = new Color(0.6f, 0.75f, 0.95f),
        },
    };

    private AdjacencyService _adjacencyService;

    public override void _Ready()
    {
        Instance = this;
        AdapterRegistry.Register<BuildingSystem>(this);
        _adjacencyService = GameManager.Get<AdjacencyService>();
    }

    public override void _ExitTree()
    {
        if (Instance == this)
        {
            AdapterRegistry.Unregister<BuildingSystem>(this);
            Instance = null;
        }
    }

    private void OnBuildingPlaced(Castle castle, Building placedBuilding)
    {
        List<IBuildingState> buildingStates = castle.GetBuildingStates();
        _adjacencyService.RefreshCastle(buildingStates);
        PlayAdjacencyPulses(castle, placedBuilding, buildingStates);
    }

    private static void PlayAdjacencyPulses(Castle castle, Building placedBuilding, List<IBuildingState> allBuildings)
    {
        AdjacencyService adjacencyService = GameManager.Get<AdjacencyService>();
        if (!(placedBuilding is IAdjacencyBuilding adjBuilding))
            return;

        HashSet<IBuildingState> neighbors = adjacencyService.GetAdjacentBuildings(adjBuilding, allBuildings);
        foreach (IBuildingState neighbor in neighbors)
        {
            if (neighbor is not Building b)
                continue;
            Vector2I mainGrid = b.GetMainGridPosition();
            Vector2 localPos = castle.GetCellCenter(mainGrid.X, mainGrid.Y);
            AdjacentLinkPulse pulse = new AdjacentLinkPulse();
            pulse.Configure(castle.CellSize);
            pulse.Position = localPos;
            castle.AddChild(pulse);
        }
    }

    public static IReadOnlyList<Vector2I> GetFootprint(string buildingType)
    {
        IReadOnlyList<GridCellOffset> domain = GameManager.Get<IBuildingRepository>().GetFootprint(buildingType);
        Vector2I[] result = new Vector2I[domain.Count];
        for (int i = 0; i < domain.Count; i++)
            result[i] = new Vector2I(domain[i].X, domain[i].Y);
        return result;
    }

    public static Vector2I GetMainCellOffset(string buildingType)
    {
        GridCellOffset offset = GameManager.Get<IBuildingRepository>().GetMainCellOffset(buildingType);
        return new Vector2I(offset.X, offset.Y);
    }

    private static IBuildingRepository BuildingRepo => GameManager.Get<IBuildingRepository>();

    public static string GetDisplayName(string buildingType) => BuildingRepo.GetDisplayName(buildingType);
    public static int GetMaxHealth(string buildingType) => BuildingRepo.GetMaxHealth(buildingType);
    public static float GetSpawnInterval(string buildingType) => BuildingRepo.GetSpawnInterval(buildingType);

    public static Vector2I GetSpawnCellOffset(string buildingType)
    {
        GridCellOffset offset = BuildingRepo.GetSpawnCellOffset(buildingType);
        return new Vector2I(offset.X, offset.Y);
    }

    public static bool GetHasNightCombat(string buildingType) => BuildingRepo.GetHasNightCombat(buildingType);
    public static int GetFusionTier(string buildingType) => BuildingRepo.GetFusionTier(buildingType);
    public static bool IsCoreBuilding(string buildingType) => BuildingRepo.IsCoreBuilding(buildingType);
    public static bool IsFusibleMaterial(string buildingType) => BuildingRepo.IsFusibleMaterial(buildingType);
    public static int GetCollisionWidth(string buildingType) => BuildingRepo.GetCollisionWidth(buildingType);
    public static int GetCollisionHeight(string buildingType) => BuildingRepo.GetCollisionHeight(buildingType);

    private static VisualDef GetVisual(string buildingType)
    {
        if (Visuals.TryGetValue(buildingType, out VisualDef def))
            return def;
        return Visuals["Barracks"];
    }

    public static Color GetSpriteModulate(string buildingType) => GetVisual(buildingType).SpriteModulate;

    public static void ApplySoldierSpawnStats(string buildingType,SoldierLogic soldier)
    {
        string unitTypeId = BuildingRepo.GetUnitTypeId(buildingType);
        UnitStats stats = GameManager.Get<IUnitRepository>().Get(unitTypeId);
        soldier.InitializeFromStats(stats);
    }

    public static void ApplyVisual(Building building)
    {
        VisualDef visual = GetVisual(building.TypeId);
        Sprite2D sprite = building.GetNodeOrNull<Sprite2D>("View/Sprite");
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

        IReadOnlyList<GridCellOffset> footprint = BuildingRepo.GetFootprint(building.TypeId);
        GameVector2 collisionSize = GameCoordinateRules.GetBuildingCollisionSize(footprint);
        float pixelW = GameCoordinatesAdapter.GameUnitsToPixels(collisionSize.X);
        float pixelH = GameCoordinatesAdapter.GameUnitsToPixels(collisionSize.Y);
        Vector2 pixelSize = new(pixelW, pixelH);

        CollisionShape2D shapeNode = building.GetNodeOrNull<CollisionShape2D>("Logic/CollisionShape");
        if (shapeNode?.Shape is RectangleShape2D rect)
            rect.Size = pixelSize;

        CollisionShape2D navShapeNode = building.GetNodeOrNull<CollisionShape2D>("Logic/NavigationObstacle/CollisionShape");
        if (navShapeNode?.Shape is RectangleShape2D navRect)
            navRect.Size = pixelSize;
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

        building.InitFromType(buildingType);
        building.BindToGrid(castle, anchorX, anchorY);
        if (!castle.PlaceBuilding(building, anchorX, anchorY, buildingType))
        {
            building.QueueFree();
            return false;
        }

        EmitSignal(SignalName.BuildingPlaced, castle, building, buildingType);
        OnBuildingPlaced(castle, building);
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
