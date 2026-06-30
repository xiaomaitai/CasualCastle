namespace CasualCastle.Domain.Building;

public interface IBuildingRepository
{
	BuildingData Get(string typeId);
}
