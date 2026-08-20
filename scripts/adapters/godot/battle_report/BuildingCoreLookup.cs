// 战报核心建筑查询适配：桥接 Building.IBuildingRepository → History.IHistoryCoreLookup
using CasualCastle.Domain.Building;
using CasualCastle.Domain.History;

public sealed class BuildingCoreLookup : IHistoryCoreLookup
{
	private readonly IBuildingRepository _buildingRepo;

	public BuildingCoreLookup(IBuildingRepository buildingRepo)
	{
		_buildingRepo = buildingRepo;
	}

	public bool IsCoreBuilding(string typeId)
	{
		return _buildingRepo.IsCoreBuilding(typeId);
	}
}
