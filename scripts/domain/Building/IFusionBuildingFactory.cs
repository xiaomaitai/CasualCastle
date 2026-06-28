namespace CasualCastle.Domain.Building;

public interface IFusionBuildingFactory
{
	IBuildingState Create(string typeId, int anchorX, int anchorY);
	void Destroy(IBuildingState building);
}
