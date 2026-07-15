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

    [Signal]
    public delegate void BuildingPlacedEventHandler(Castle castle, Building building, string buildingType);

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
    public static float GetProductionRate(string buildingType) => BuildingRepo.GetProductionRate(buildingType);
    public static int GetUnitCost(string buildingType)
    {
        string unitTypeId = BuildingRepo.GetUnitTypeId(buildingType);
        if (string.IsNullOrEmpty(unitTypeId))
            return 0;
        return GameManager.Get<IUnitRepository>().Get(unitTypeId).UnitCost;
    }

    public static Vector2I GetSpawnCellOffset(string buildingType)
    {
        GridCellOffset offset = BuildingRepo.GetSpawnCellOffset(buildingType);
        return new Vector2I(offset.X, offset.Y);
    }

    public static bool GetHasNightCombat(string buildingType) => BuildingRepo.GetHasNightCombat(buildingType);
    public static int GetCombineTier(string buildingType) => BuildingRepo.GetCombineTier(buildingType);
    public static bool IsCoreBuilding(string buildingType) => BuildingRepo.IsCoreBuilding(buildingType);
    public static bool IsCombinableMaterial(string buildingType) => CombineRules.IsCombinableMaterial(buildingType, BuildingRepo);
    public static int GetCollisionWidth(string buildingType) => BuildingRepo.GetCollisionWidth(buildingType);
    public static int GetCollisionHeight(string buildingType) => BuildingRepo.GetCollisionHeight(buildingType);

    public static Color GetSpriteModulate(string buildingType)
    {
        IBuildingRepository repo = BuildingRepo;
        return new Color(repo.GetSpriteModulateR(buildingType), repo.GetSpriteModulateG(buildingType), repo.GetSpriteModulateB(buildingType), repo.GetSpriteModulateA(buildingType));
    }

    public static void ApplySoldierSpawnStats(string buildingType,SoldierLogic soldier)
    {
        string unitTypeId = BuildingRepo.GetUnitTypeId(buildingType);
        UnitStats stats = GameManager.Get<IUnitRepository>().Get(unitTypeId);
        soldier.InitializeFromStats(stats);
    }

    public static void ApplyVisual(Building building)
    {
        IBuildingRepository repo = BuildingRepo;
        string buildingType = building.TypeId;
        IReadOnlyList<GridCellOffset> footprint = repo.GetFootprint(buildingType);
        GameVector2 collisionSize = GameCoordinateRules.GetBuildingCollisionSize(footprint);
        float pixelW = GameCoordinatesAdapter.GameUnitsToPixels(collisionSize.X);
        float pixelH = GameCoordinatesAdapter.GameUnitsToPixels(collisionSize.Y);
        Vector2 pixelSize = new(pixelW, pixelH);

        Sprite2D sprite = building.GetNodeOrNull<Sprite2D>("View/Sprite");
        if (sprite != null)
        {
            string texturePath = repo.GetTexturePath(buildingType);
            if (!string.IsNullOrEmpty(texturePath))
            {
                Texture2D texture = GD.Load<Texture2D>(texturePath);
                if (texture != null)
                {
                    sprite.Texture = texture;
                    sprite.Scale = new Vector2(pixelW / texture.GetWidth(), pixelH / texture.GetHeight());
                }
            }
            sprite.Modulate = new Color(repo.GetSpriteModulateR(buildingType), repo.GetSpriteModulateG(buildingType), repo.GetSpriteModulateB(buildingType), repo.GetSpriteModulateA(buildingType));
            string materialPath = repo.GetMaterialPath(buildingType);
            if (!string.IsNullOrEmpty(materialPath))
            {
                Material material = GD.Load<Material>(materialPath);
                if (material != null) sprite.Material = material;
            }
        }

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
