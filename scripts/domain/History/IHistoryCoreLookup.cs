// 战报核心建筑查询端口：History 借以判断核心建筑，不直接依赖 Building
namespace CasualCastle.Domain.History;

public interface IHistoryCoreLookup
{
	bool IsCoreBuilding(string typeId);
}
