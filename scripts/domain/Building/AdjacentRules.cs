using CasualCastle.Domain.Shared;
using System.Collections.Generic;

namespace CasualCastle.Domain.Building;

public interface IAdjacencyBuilding
{
    string TypeId { get; }
    int AnchorGridX { get; }
    int AnchorGridY { get; }
    bool ContributesToAdjacency { get; }
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
        IAdjacencyBuilding source, IReadOnlyList<IAdjacencyBuilding> allBuildings)
    {
        HashSet<IAdjacencyBuilding> neighbors = new();
        Dictionary<(int x, int y), IAdjacencyBuilding> cellOwners = BuildCellOwnerMap(allBuildings);

        foreach ((int x, int y) cell in GetOccupiedCells(source))
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
        IAdjacencyBuilding source, IReadOnlyList<IAdjacencyBuilding> allBuildings, string typeId)
    {
        int count = 0;
        foreach (IAdjacencyBuilding neighbor in GetAdjacentBuildings(source, allBuildings))
        {
            if (neighbor.TypeId == typeId && neighbor.ContributesToAdjacency)
                count++;
        }
        return count;
    }

    public static float CalculateWorkSpeedMultiplier(
        IAdjacencyBuilding building, IReadOnlyList<IAdjacencyBuilding> allBuildings)
    {
        if (!building.ContributesToAdjacency || !IsBarracksType(building.TypeId))
            return 1f;

        int adjacentBarracks = 0;
        foreach (IAdjacencyBuilding neighbor in GetAdjacentBuildings(building, allBuildings))
        {
            if (IsBarracksType(neighbor.TypeId) && neighbor.ContributesToAdjacency)
                adjacentBarracks++;
        }

        return adjacentBarracks > 0 ? 1f + 0.2f * adjacentBarracks : 1f;
    }

    public static bool IsBarracksType(string typeId) =>
        typeId == "Barracks" || typeId == "BarracksT2";

    private static Dictionary<(int x, int y), IAdjacencyBuilding> BuildCellOwnerMap(
        IReadOnlyList<IAdjacencyBuilding> buildings)
    {
        Dictionary<(int x, int y), IAdjacencyBuilding> cellOwners = new();
        foreach (IAdjacencyBuilding building in buildings)
        {
            foreach ((int x, int y) cell in GetOccupiedCells(building))
                cellOwners[cell] = building;
        }
        return cellOwners;
    }

    private static IEnumerable<(int x, int y)> GetOccupiedCells(IAdjacencyBuilding building)
    {
        IReadOnlyList<GridCellOffset> footprint = BuildingDefinitions.GetFootprint(building.TypeId);
        foreach (GridCellOffset offset in footprint)
            yield return (building.AnchorGridX + offset.X, building.AnchorGridY + offset.Y);
    }
}
