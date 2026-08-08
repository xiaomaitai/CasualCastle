// 建筑视觉仓库端口：按类型查询视觉数据
public interface IBuildingVisualRepository
{
	BuildingVisualData GetVisualData(string typeId);
}
