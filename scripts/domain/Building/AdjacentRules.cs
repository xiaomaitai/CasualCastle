// 邻接规则：相邻建筑查找与产速加成
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
	private const float SameLineNeighborSpeedFactor = 0.85f;
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
		Dictionary<(int x, int y), IAdjacencyBuilding> cellOwners = BuildingCellOccupancy.BuildCellOwnerMap(allBuildings, buildingRepo);

		foreach ((int x, int y) cell in BuildingCellOccupancy.GetOccupiedCells(source, buildingRepo))
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
		IBuildingRepository buildingRepo,
		System.Func<string, string, bool> isSameLine)
	{
		if (!building.ContributesToAdjacency)
			return 1f;

		int adjacentSameType = 0;
		foreach (IAdjacencyBuilding neighbor in GetAdjacentBuildings(building, allBuildings, buildingRepo))
		{
			if (isSameLine(building.TypeId, neighbor.TypeId) && neighbor.ContributesToAdjacency)
				adjacentSameType++;
		}

		return adjacentSameType > 0 ? 1f / (float)System.Math.Pow(SameLineNeighborSpeedFactor, adjacentSameType) : 1f;
	}

}
