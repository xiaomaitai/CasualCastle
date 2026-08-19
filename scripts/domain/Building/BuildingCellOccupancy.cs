// 网格单元归属：唯一实现，供邻接与部落规则共用
using CasualCastle.Domain.Shared;
using System.Collections.Generic;

namespace CasualCastle.Domain.Building;

public static class BuildingCellOccupancy
{
	public static Dictionary<(int x, int y), IAdjacencyBuilding> BuildCellOwnerMap(
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

	public static IEnumerable<(int x, int y)> GetOccupiedCells(IAdjacencyBuilding building, IBuildingRepository buildingRepo)
	{
		IReadOnlyList<GridCellOffset> footprint = buildingRepo.GetFootprint(building.TypeId);
		foreach (GridCellOffset offset in footprint)
			yield return (building.AnchorGridX + offset.X, building.AnchorGridY + offset.Y);
	}
}
