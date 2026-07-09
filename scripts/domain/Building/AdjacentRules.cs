using CasualCastle.Domain.Shared;
using System.Collections.Generic;

namespace CasualCastle.Domain.Building;

public interface IAdjacencyBuilding
{
    string TypeId { get; }
    int AnchorGridX { get; }
    int AnchorGridY { get; }
    bool ContributesToAdjacency { get; }
    void SetWorkSpeedMultiplier(float multiplier);
}

public static class AdjacentRules
{
    private static readonly GridCellOffset[] Directions =
    {
        new(0, -1),
        new(0, 1),
        new(-1, 0),
        new(1, 0),
    };

    public static HashSet<IAdjacencyBuilding> GetAdjacentBuildings(
        IAdjacencyBuilding source, IReadOnlyList<IAdjacencyBuilding> allBuildings,
        IBuildingRepository buildingRepo)
    {
        HashSet<IAdjacencyBuilding> neighbors = new();
        Dictionary<(int x, int y), IAdjacencyBuilding> cellOwners = BuildCellOwnerMap(allBuildings, buildingRepo);

        foreach ((int x, int y) cell in GetOccupiedCells(source, buildingRepo))
        {
            foreach (GridCellOffset direction in Directions)
            {
                (int nx, int ny) neighborCell = (cell.x + direction.X, cell.y + direction.Y);
                if (!cellOwners.TryGetValue(neighborCell, out IAdjacencyBuilding neighbor))
                    continue;
                if (neighbor == source)
                    continue;
                neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }

    public static int CountAdjacentOfType(
        IAdjacencyBuilding source, IReadOnlyList<IAdjacencyBuilding> allBuildings, string typeId,
        IBuildingRepository buildingRepo)
    {
        int count = 0;
        foreach (IAdjacencyBuilding neighbor in GetAdjacentBuildings(source, allBuildings, buildingRepo))
        {
            if (neighbor.TypeId == typeId && neighbor.ContributesToAdjacency)
                count++;
        }
        return count;
    }

    public static float CalculateWorkSpeedMultiplier(
        IAdjacencyBuilding building, IReadOnlyList<IAdjacencyBuilding> allBuildings,
        IBuildingRepository buildingRepo)
    {
        if (!building.ContributesToAdjacency)
            return 1f;

        int adjacentSameType = 0;
        foreach (IAdjacencyBuilding neighbor in GetAdjacentBuildings(building, allBuildings, buildingRepo))
        {
            if (neighbor.TypeId == building.TypeId && neighbor.ContributesToAdjacency)
                adjacentSameType++;
        }

        return adjacentSameType > 0 ? 1f + 0.2f * adjacentSameType : 1f;
    }

    private static Dictionary<(int x, int y), IAdjacencyBuilding> BuildCellOwnerMap(
        IReadOnlyList<IAdjacencyBuilding> buildings, IBuildingRepository buildingRepo)
    {
        Dictionary<(int x, int y), IAdjacencyBuilding> cellOwners = new();
        foreach (IAdjacencyBuilding building in buildings)
        {
            foreach ((int x, int y) cell in GetOccupiedCells(building, buildingRepo))
                cellOwners[cell] = building;
        }
        return cellOwners;
    }

    private static IEnumerable<(int x, int y)> GetOccupiedCells(IAdjacencyBuilding building, IBuildingRepository buildingRepo)
    {
        IReadOnlyList<GridCellOffset> footprint = buildingRepo.GetFootprint(building.TypeId);
        foreach (GridCellOffset offset in footprint)
            yield return (building.AnchorGridX + offset.X, building.AnchorGridY + offset.Y);
    }
}
