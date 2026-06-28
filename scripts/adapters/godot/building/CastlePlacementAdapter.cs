using CasualCastle.Domain.Building;

public class CastlePlacementAdapter : IBuildingPlacement
{
    private readonly Castle _castle;
    private readonly BuildingSystem _buildingSystem;

    public bool IsPlayerSide => _castle.IsPlayerCastle;

    public CastlePlacementAdapter(Castle castle, BuildingSystem buildingSystem)
    {
        _castle = castle;
        _buildingSystem = buildingSystem;
    }

    public bool CanPlace(string buildingType, int gridX, int gridY)
    {
        return _buildingSystem.CanPlace(_castle, buildingType, gridX, gridY);
    }

    public bool TryPlace(string buildingType, int gridX, int gridY)
    {
        return _buildingSystem.TryPlace(_castle, buildingType, gridX, gridY);
    }
}
