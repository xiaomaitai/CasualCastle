// 建筑视觉仓库端口：按类型查询视觉数据
namespace CasualCastle.Domain.Building;

public interface IBuildingVisualRepository
{
	BuildingVisualData GetVisualData(string typeId);
}
