using CasualCastle.Domain.Building;
using CasualCastle.Domain.History;
using CasualCastle.Domain.Shared;
using CasualCastle.Adapters.Godot;
using Godot;
using System.Collections.Generic;

public partial class ReplayAiSystem : Node
{
    public static ReplayAiSystem Instance { get; private set; }

    private BattleReportSystem _battleReport;
    private AdjacentSystem _adjacentSystem;

    public override void _Ready()
    {
        Instance = this;
        AdapterRegistry.Register<ReplayAiSystem>(this);
        _battleReport = AdapterRegistry.Resolve<BattleReportSystem>();
        _adjacentSystem = AdapterRegistry.Resolve<AdjacentSystem>();
    }

    public override void _ExitTree()
    {
        if (Instance == this)
        {
            AdapterRegistry.Unregister<ReplayAiSystem>(this);
            Instance = null;
        }
    }

    public void ApplyNightSnapshot(Castle enemyCastle, int nightIndex)
    {
        if (enemyCastle == null || enemyCastle.IsPlayerCastle)
            return;

        CastleSnapshot snapshot = _battleReport?.GetSelectedNightSnapshot(nightIndex);
        if (snapshot == null)
            return;

        ClearEnemyBuildings(enemyCastle);

        foreach (BuildingSnapshot buildingSnapshot in snapshot.Buildings)
        {
            if (BuildingDefinitions.IsCoreBuilding(buildingSnapshot.TypeId))
                continue;

            Vector2I anchor = MirrorAnchor(enemyCastle, buildingSnapshot);
            IReadOnlyList<Vector2I> footprint = BuildingSystem.GetFootprint(buildingSnapshot.TypeId);
            if (IsBlockedByPlayerSoldier(enemyCastle, anchor, footprint))
                continue;

            Building building = BuildingSystem.CreateBuilding(buildingSnapshot.TypeId);
            if (building == null)
                continue;

            building.BindToGrid(enemyCastle, anchor.X, anchor.Y);
            if (!enemyCastle.PlaceBuilding(building, anchor.X, anchor.Y, buildingSnapshot.TypeId))
            {
                building.QueueFree();
                continue;
            }

            building.ApplySnapshotState(
                buildingSnapshot.Health,
                buildingSnapshot.IsManuallyPaused,
                buildingSnapshot.IsFusionProhibited);
        }

        _adjacentSystem?.RefreshCastle(enemyCastle);
    }

    private static void ClearEnemyBuildings(Castle enemyCastle)
    {
        foreach (Building building in enemyCastle.GetBuildings())
        {
            if (BuildingDefinitions.IsCoreBuilding(building.TypeId))
                continue;

            enemyCastle.ReleaseBuildingFootprint(building);
            building.GetParent()?.RemoveChild(building);
            building.QueueFree();
        }
    }

    private static Vector2I MirrorAnchor(Castle enemyCastle, BuildingSnapshot snapshot)
    {
        IReadOnlyList<GridCellOffset> domainFootprint = BuildingDefinitions.GetFootprint(snapshot.TypeId);
        (int mirrorX, int mirrorY) = MirrorRules.MirrorAnchor(
            snapshot.AnchorGridX, snapshot.AnchorGridY,
            domainFootprint, enemyCastle.GridColumns);
        return new Vector2I(mirrorX, mirrorY);
    }

    private static bool IsBlockedByPlayerSoldier(Castle enemyCastle, Vector2I anchor, IReadOnlyList<Vector2I> footprint)
    {
        List<Vector2I> occupiedCells = new();
        foreach (Vector2I offset in footprint)
            occupiedCells.Add(new Vector2I(anchor.X + offset.X, anchor.Y + offset.Y));
        return enemyCastle.IsAnyCellOccupiedByPlayerSoldier(occupiedCells);
    }
}
