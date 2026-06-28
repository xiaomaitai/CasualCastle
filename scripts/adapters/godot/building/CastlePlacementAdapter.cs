using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Building;

public class CastlePlacementAdapter : IBuildingPlacement
{
    private readonly BuildingSystem _buildingSystem;

    public bool IsPlayerSide => true;

    public CastlePlacementAdapter(BuildingSystem buildingSystem)
    {
        _buildingSystem = buildingSystem;
    }

    private Castle PlayerCastle => AdapterRegistry.Resolve<GameManager>().PlayerCastle;

    public bool CanPlace(string buildingType, int gridX, int gridY)
    {
        return _buildingSystem.CanPlace(PlayerCastle, buildingType, gridX, gridY);
    }

    public bool TryPlace(string buildingType, int gridX, int gridY)
    {
        return _buildingSystem.TryPlace(PlayerCastle, buildingType, gridX, gridY);
    }
}
