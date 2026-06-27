using CasualCastle.Domain.Ports;
using Godot;
using System.Collections.Generic;

public partial class ReplayAiSystem : Node
{
    public static ReplayAiSystem Instance { get; private set; }

    public override void _Ready()
    {
        Instance = this;
    }

    public override void _ExitTree()
    {
        if (Instance == this)
            Instance = null;
    }

    public void ApplyNightSnapshot(Castle enemyCastle, int nightIndex)
    {
        if (enemyCastle == null || enemyCastle.IsPlayerCastle)
            return;

        CastleSnapshot snapshot = BattleReportSystem.Instance?.GetSelectedNightSnapshot(nightIndex);
        if (snapshot == null)
            return;

        ClearEnemyBuildings(enemyCastle);

        foreach (BuildingSnapshot buildingSnapshot in snapshot.Buildings)
        {
            if (BuildingSystem.IsCoreBuilding(buildingSnapshot.TypeId))
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

        AdjacentSystem.Instance?.RefreshCastle(enemyCastle);
    }

    private static void ClearEnemyBuildings(Castle enemyCastle)
    {
        foreach (Building building in enemyCastle.GetBuildings())
        {
            if (BuildingSystem.IsCoreBuilding(building.TypeId))
                continue;

            enemyCastle.ReleaseBuildingFootprint(building);
            building.GetParent()?.RemoveChild(building);
            building.QueueFree();
        }
    }

    private static Vector2I MirrorAnchor(Castle enemyCastle, BuildingSnapshot snapshot)
    {
        IReadOnlyList<Vector2I> footprint = BuildingSystem.GetFootprint(snapshot.TypeId);
        int maxOffsetX = 0;
        foreach (Vector2I offset in footprint)
            maxOffsetX = Mathf.Max(maxOffsetX, offset.X);

        int mirrorX = enemyCastle.GridColumns - 1 - snapshot.AnchorGridX - maxOffsetX;
        return new Vector2I(mirrorX, snapshot.AnchorGridY);
    }

    private static bool IsBlockedByPlayerSoldier(Castle enemyCastle, Vector2I anchor, IReadOnlyList<Vector2I> footprint)
    {
        List<Vector2I> occupiedCells = new();
        foreach (Vector2I offset in footprint)
            occupiedCells.Add(new Vector2I(anchor.X + offset.X, anchor.Y + offset.Y));
        return enemyCastle.IsAnyCellOccupiedByPlayerSoldier(occupiedCells);
    }
}
