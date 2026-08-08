// 升级建筑工厂端口
namespace CasualCastle.Domain.Building;

public interface IUpgradeBuildingFactory
{
	IBuildingState Create(string typeId, int anchorX, int anchorY, float healthRatio);
	void Destroy(IBuildingState building);
}
