namespace CasualCastle.Domain.Building;

public interface ICombineBuildingFactory
{
	IBuildingState Create(string typeId, int anchorX, int anchorY);
	void Destroy(IBuildingState building);
}
