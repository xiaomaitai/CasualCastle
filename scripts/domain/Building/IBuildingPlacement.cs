namespace CasualCastle.Domain.Building;

public interface IBuildingPlacement
{
    bool CanPlace(OccupancyGrid grid, string buildingType, int anchorX, int anchorY);
}
