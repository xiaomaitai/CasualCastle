using CasualCastle.Domain.Shared;
using System.Collections.Generic;
using System.Linq;

namespace CasualCastle.Domain.Building;

public static class HordeAdjacencyRules
{
	public static int CountAdjacentEmptyCells(
		IAdjacencyBuilding building, IReadOnlyList<IAdjacencyBuilding> allBuildings,
		IBuildingRepository buildingRepo)
	{
		HashSet<(int x, int y)> occupied = CollectOccupiedCells(allBuildings, buildingRepo);
		int count = 0;
		foreach ((int x, int y) cell in GetSideCells(building, buildingRepo))
		{
			if (!occupied.Contains(cell))
				count++;
		}
		return System.Math.Min(count, GameRules.HordeAdjacencyMaxEmptyCells);
	}

	public static float CalculateSpawnIntervalMultiplier(
		IAdjacencyBuilding building, IReadOnlyList<IAdjacencyBuilding> allBuildings,
		IBuildingRepository buildingRepo)
	{
		int emptyCount = CountAdjacentEmptyCells(building, allBuildings, buildingRepo);
		return 1f - emptyCount * GameRules.HordeAdjacencySpawnSpeedBonusPerEmptyCell;
	}

	private static IEnumerable<(int x, int y)> GetSideCells(
		IAdjacencyBuilding building, IBuildingRepository buildingRepo)
	{
		IReadOnlyList<GridCellOffset> footprint = buildingRepo.GetFootprint(building.TypeId);
		int minX = building.AnchorGridX + footprint.Min(f => f.X);
		int maxX = building.AnchorGridX + footprint.Max(f => f.X);
		int minY = building.AnchorGridY + footprint.Min(f => f.Y);
		int maxY = building.AnchorGridY + footprint.Max(f => f.Y);

		yield return (minX, minY - 1);
		yield return (minX, maxY + 1);
		yield return (minX - 1, minY);
		yield return (maxX + 1, minY);
	}

	private static HashSet<(int x, int y)> CollectOccupiedCells(
		IReadOnlyList<IAdjacencyBuilding> buildings, IBuildingRepository buildingRepo)
	{
		HashSet<(int x, int y)> occupied = new();
		foreach (IAdjacencyBuilding building in buildings)
		{
			foreach (GridCellOffset offset in buildingRepo.GetFootprint(building.TypeId))
				occupied.Add((building.AnchorGridX + offset.X, building.AnchorGridY + offset.Y));
		}
		return occupied;
	}
}
