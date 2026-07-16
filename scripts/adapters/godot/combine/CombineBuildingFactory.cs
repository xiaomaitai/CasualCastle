using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Building;
using Godot;

public class CombineBuildingFactory : ICombineBuildingFactory
{
    private readonly Castle _castle;

    public CombineBuildingFactory(Castle castle)
    {
        _castle = castle;
    }

    public IBuildingState Create(string typeId, int anchorX, int anchorY)
    {
        Building building = AdapterRegistry.Resolve<BuildingSystem>().CreateBuilding(typeId);
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
        if (!GodotObject.IsInstanceValid(b))
            return;
        _castle.ReleaseBuildingFootprint(b);
        b.GetParent()?.RemoveChild(b);
        b.QueueFree();
    }
}
