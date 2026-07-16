using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Building;
using CasualCastle.Domain.Shared;
using CasualCastle.Adapters.Godot;
using Godot;
using System.Collections.Generic;

public partial class BuildingSystem : Node
{
    private const string BuildingPrefabPath = "res://prefabs/Building.tscn";

    [Signal]
    public delegate void BuildingPlacedEventHandler(Castle castle, Building building, string buildingType);

    private AdjacencyService _adjacencyService;
    private IBuildingRepository _buildingRepo;
    private IUnitRepository _unitRepo;
    private IBuildingVisualRepository _visualRepo;

    public override void _Ready()
    {
        AdapterRegistry.Register<BuildingSystem>(this);
        _adjacencyService = GameManager.Get<AdjacencyService>();
        _buildingRepo = GameManager.Get<IBuildingRepository>();
        _unitRepo = GameManager.Get<IUnitRepository>();
        _visualRepo = GameManager.Get<IBuildingVisualRepository>();
    }

    public override void _ExitTree()
    {
        AdapterRegistry.Unregister<BuildingSystem>(this);
    }

    private void OnBuildingPlaced(Castle castle, Building placedBuilding)
    {
        List<IBuildingState> buildingStates = castle.GetBuildingStates();
        _adjacencyService.RefreshCastle(buildingStates);
        PlayAdjacencyPulses(castle, placedBuilding, buildingStates);
    }

    private void PlayAdjacencyPulses(Castle castle, Building placedBuilding, List<IBuildingState> allBuildings)
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

    public IReadOnlyList<Vector2I> GetFootprint(string buildingType)
    {
        IReadOnlyList<GridCellOffset> domain = _buildingRepo.GetFootprint(buildingType);
        Vector2I[] result = new Vector2I[domain.Count];
        for (int i = 0; i < domain.Count; i++)
            result[i] = new Vector2I(domain[i].X, domain[i].Y);
        return result;
    }

    public Vector2I GetMainCellOffset(string buildingType)
    {
        GridCellOffset offset = _buildingRepo.GetMainCellOffset(buildingType);
        return new Vector2I(offset.X, offset.Y);
    }

    public string GetDisplayName(string buildingType) => _buildingRepo.GetDisplayName(buildingType);
    public int GetMaxHealth(string buildingType) => _buildingRepo.GetMaxHealth(buildingType);
    public float GetSpawnInterval(string buildingType) => _buildingRepo.GetSpawnInterval(buildingType);
    public float GetProductionRate(string buildingType) => _buildingRepo.GetProductionRate(buildingType);

    public int GetUnitCost(string buildingType)
    {
        string unitTypeId = _buildingRepo.GetUnitTypeId(buildingType);
        if (string.IsNullOrEmpty(unitTypeId))
            return 0;
        return _unitRepo.Get(unitTypeId).UnitCost;
    }

    public Vector2I GetSpawnCellOffset(string buildingType)
    {
        GridCellOffset offset = _buildingRepo.GetSpawnCellOffset(buildingType);
        return new Vector2I(offset.X, offset.Y);
    }

    public bool GetHasNightCombat(string buildingType) => _buildingRepo.GetHasNightCombat(buildingType);
    public int GetCombineTier(string buildingType) => _buildingRepo.GetCombineTier(buildingType);
    public bool IsCoreBuilding(string buildingType) => _buildingRepo.IsCoreBuilding(buildingType);
    public bool IsCombinableMaterial(string buildingType)
    {
        CombineRules combineRules = GameManager.Get<CombineRules>();
        return combineRules.IsCombinableMaterial(buildingType, _buildingRepo);
    }
    public int GetCollisionWidth(string buildingType) => _buildingRepo.GetCollisionWidth(buildingType);
    public int GetCollisionHeight(string buildingType) => _buildingRepo.GetCollisionHeight(buildingType);

    public Color GetSpriteModulate(string buildingType)
    {
        BuildingVisualData data = _visualRepo.GetVisualData(buildingType);
        return new Color(data.SpriteModulateR, data.SpriteModulateG, data.SpriteModulateB, data.SpriteModulateA);
    }

    public void ApplySoldierSpawnStats(string buildingType, SoldierLogic soldier)
    {
        string unitTypeId = _buildingRepo.GetUnitTypeId(buildingType);
        UnitStats stats = _unitRepo.Get(unitTypeId);
        soldier.InitializeFromStats(stats);
    }

    public void ApplyVisual(Building building)
    {
        string buildingType = building.TypeId;
        IReadOnlyList<GridCellOffset> footprint = _buildingRepo.GetFootprint(buildingType);
        GameVector2 collisionSize = GameCoordinateRules.GetBuildingCollisionSize(footprint);
        float pixelW = GameCoordinatesAdapter.GameUnitsToPixels(collisionSize.X);
        float pixelH = GameCoordinatesAdapter.GameUnitsToPixels(collisionSize.Y);
        Vector2 pixelSize = new(pixelW, pixelH);

        Sprite2D sprite = building.GetNodeOrNull<Sprite2D>("View/Sprite");
        if (sprite != null)
        {
            BuildingVisualData visualData = _visualRepo.GetVisualData(buildingType);
            string texturePath = visualData.TexturePath;
            if (!string.IsNullOrEmpty(texturePath))
            {
                Texture2D texture = GD.Load<Texture2D>(texturePath);
                if (texture != null)
                {
                    sprite.Texture = texture;
                    sprite.Scale = new Vector2(pixelW / texture.GetWidth(), pixelH / texture.GetHeight());
                }
            }
            sprite.Modulate = new Color(visualData.SpriteModulateR, visualData.SpriteModulateG, visualData.SpriteModulateB, visualData.SpriteModulateA);
            string materialPath = visualData.MaterialPath;
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

    public Building CreateBuilding(string buildingType)
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
