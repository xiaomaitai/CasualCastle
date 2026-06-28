using CasualCastle.Domain.Building;

public class FusionBuildingFactory : IFusionBuildingFactory
{
    private readonly Castle _castle;

    public FusionBuildingFactory(Castle castle)
    {
        _castle = castle;
    }

    public IBuildingState Create(string typeId, int anchorX, int anchorY)
    {
        Building building = BuildingSystem.CreateBuilding(typeId);
        if (building == null)
            return null;

        building.BindToGrid(_castle, anchorX, anchorY);
        _castle.PlaceBuilding(building, anchorX, anchorY, typeId);
        return building;
    }

    public void Destroy(IBuildingState building)
    {
        if (building is not Building b)
            return;
        _castle.ReleaseBuildingFootprint(b);
        b.GetParent()?.RemoveChild(b);
        b.QueueFree();
    }
}
