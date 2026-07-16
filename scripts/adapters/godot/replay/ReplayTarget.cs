using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Building;
using CasualCastle.Domain.History;
using CasualCastle.Domain.Shared;
using Godot;
using System.Collections.Generic;

public class ReplayTarget : IReplayTarget
{
    private readonly Castle _castle;

    public int GridColumns => _castle.GridColumns;

    public ReplayTarget(Castle castle)
    {
        _castle = castle;
    }

    public void ClearNonCoreBuildings()
    {
        _castle.ClearNonCoreBuildings();
    }

    public bool TryPlaceMirrored(BuildingSnapshot snapshot)
    {
        IReadOnlyList<GridCellOffset> domainFootprint = GameManager.Get<IBuildingRepository>().GetFootprint(snapshot.TypeId);
        (int mirrorX, int mirrorY) = MirrorRules.MirrorAnchor(
            snapshot.AnchorGridX, snapshot.AnchorGridY,
            domainFootprint, _castle.GridColumns);

        IReadOnlyList<Vector2I> footprint = AdapterRegistry.Resolve<BuildingSystem>().GetFootprint(snapshot.TypeId);
        if (IsBlockedByPlayerSoldier(mirrorX, mirrorY, footprint))
            return false;

        Building building = AdapterRegistry.Resolve<BuildingSystem>().CreateBuilding(snapshot.TypeId);
        if (building == null)
            return false;

        building.BindToGrid(_castle, mirrorX, mirrorY);
        if (!_castle.PlaceBuilding(building, mirrorX, mirrorY, snapshot.TypeId))
        {
            building.QueueFree();
            return false;
        }

        building.ApplySnapshotState(
            snapshot.Health,
            snapshot.IsManuallyPaused,
            snapshot.IsCombineProhibited);
        return true;
    }

    private bool IsBlockedByPlayerSoldier(int anchorX, int anchorY, IReadOnlyList<Vector2I> footprint)
    {
        List<Vector2I> occupiedCells = new();
        foreach (Vector2I offset in footprint)
            occupiedCells.Add(new Vector2I(anchorX + offset.X, anchorY + offset.Y));
        return _castle.IsAnyCellOccupiedByPlayerSoldier(occupiedCells);
    }
}
