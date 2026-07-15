using CasualCastle.Domain.Building;
using CasualCastle.Adapters.Godot;
using Godot;

public class BuildingFactory : IBuildingFactory
{
    public Building Create(string typeId)
    {
        return BuildingSystem.CreateBuilding(typeId);
    }

    public void Destroy(Building building)
    {
        if (building == null || !GodotObject.IsInstanceValid(building))
            return;
        Castle castle = building.GetCastle();
        castle?.ReleaseBuildingFootprint(building);
        building.GetParent()?.RemoveChild(building);
        building.QueueFree();
    }
}
