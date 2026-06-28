namespace CasualCastle.Domain.Building;

public interface IBuildingPlacement
{
	bool CanPlace(string buildingType, int gridX, int gridY);
	bool TryPlace(string buildingType, int gridX, int gridY);
}
