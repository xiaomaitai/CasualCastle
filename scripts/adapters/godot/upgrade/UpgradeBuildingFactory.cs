// 升级建筑工厂：按配方生成升级建筑
using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Building;
using Godot;

public class UpgradeBuildingFactory : IUpgradeBuildingFactory
{
	private readonly Castle _castle;

	public UpgradeBuildingFactory(Castle castle)
	{
		_castle = castle;
	}

	public IBuildingState Create(string typeId, int anchorX, int anchorY, float healthRatio)
	{
		Building building = AdapterRegistry.Resolve<BuildingSystem>().CreateBuilding(typeId);
		if (building == null)
			return null;

		building.BindToGrid(_castle, anchorX, anchorY);
		building.ApplyHealthRatio(healthRatio);
		_castle.PlaceBuilding(building, anchorX, anchorY, typeId);
		return building.Bridge;
	}

	public void Destroy(IBuildingState building)
	{
		if (building is not BuildingBridge bridge)
			return;
		Building b = bridge.Building;
		if (!GodotObject.IsInstanceValid(b))
			return;
		_castle.ReleaseBuildingFootprint(b);
		b.GetParent()?.RemoveChild(b);
		b.QueueFree();
	}
}
