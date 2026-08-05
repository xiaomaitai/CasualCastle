using CasualCastle.Domain.Shared;
using System.Collections.Generic;
using System.Linq;

namespace CasualCastle.Domain.Building;

public static class HordeAdjacencyRules
{
	public static int CountAdjacencyBonusCells(
		IAdjacencyBuilding building, IReadOnlyList<IAdjacencyBuilding> allBuildings,
		IBuildingRepository buildingRepo)
	{
		Dictionary<(int x, int y), IAdjacencyBuilding> owners = CollectCellOwners(allBuildings, buildingRepo);
		string raceId = buildingRepo.GetRaceId(building.TypeId);
		int count = 0;
		foreach ((int x, int y) cell in GetSideCells(building, buildingRepo))
		{
			if (!owners.TryGetValue(cell, out IAdjacencyBuilding neighbor) || !neighbor.ContributesToAdjacency)
			{
				count++;
				continue;
			}
			if (HordeCoreRules.IsBurrow(neighbor.TypeId))
				count++;
			else if (!string.IsNullOrEmpty(raceId) && buildingRepo.GetRaceId(neighbor.TypeId) == raceId)
				count++;
		}
		return System.Math.Min(count, GameRules.HordeAdjacencyMaxCells);
	}

	public static float CalculateSpawnIntervalMultiplier(
		IAdjacencyBuilding building, IReadOnlyList<IAdjacencyBuilding> allBuildings,
		IBuildingRepository buildingRepo)
	{
		int count = CountAdjacencyBonusCells(building, allBuildings, buildingRepo);
		float rate = GameRules.HordeAdjacencySpawnSpeedBonusPerCell
			+ GameRules.HordeBurrowSpawnSpeedBonusPerBurrow * CountBurrows(allBuildings);
		return 1f - count * rate;
	}

	public static int CountBurrows(IReadOnlyList<IAdjacencyBuilding> buildings)
	{
		int count = 0;
		foreach (IAdjacencyBuilding building in buildings)
		{
			if (HordeCoreRules.IsBurrow(building.TypeId) && building.ContributesToAdjacency)
				count++;
		}
		return count;
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

	private static Dictionary<(int x, int y), IAdjacencyBuilding> CollectCellOwners(
		IReadOnlyList<IAdjacencyBuilding> buildings, IBuildingRepository buildingRepo)
	{
		Dictionary<(int x, int y), IAdjacencyBuilding> owners = new();
		foreach (IAdjacencyBuilding building in buildings)
		{
			foreach (GridCellOffset offset in buildingRepo.GetFootprint(building.TypeId))
				owners[(building.AnchorGridX + offset.X, building.AnchorGridY + offset.Y)] = building;
		}
		return owners;
	}
}
