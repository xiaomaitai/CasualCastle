// 建筑桥接对象：领域接口投影集中于此，Building 只持有节点生命周期
using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Building;
using Godot;

public sealed class BuildingBridge : IBuildingState, IBuildingRef, IBuildingTarget
{
	private readonly Building _building;

	private readonly BuildingSystem _buildingSystem;

	public BuildingBridge(Building building, BuildingSystem buildingSystem)
	{
		_building = building;
		_buildingSystem = buildingSystem;
	}

	public Building Building => _building;

	string IAdjacencyBuilding.TypeId => _building.TypeId;

	int IAdjacencyBuilding.AnchorGridX => _building.AnchorGridX;

	int IAdjacencyBuilding.AnchorGridY => _building.AnchorGridY;

	bool IAdjacencyBuilding.ContributesToAdjacency => !_building.IsDestroyed;

	void IAdjacencyBuilding.SetWorkSpeedMultiplier(float multiplier)
	{
		_building.SetWorkSpeedMultiplier(multiplier);
	}

	int IBuildingState.Health => _building.Health;

	bool IBuildingState.IsDestroyed => _building.IsDestroyed;

	bool IBuildingState.IsManuallyPaused => _building.IsManuallyPaused;

	bool IBuildingState.IsCombineProhibited => _building.IsCombineProhibited;

	bool IBuildingState.HasEnemyOnTop => _building.HasEnemyOnTop;

	bool IBuildingState.IsPlayerOwned => _building.IsPlayerOwned;

	string IBuildingRef.TypeId => _building.TypeId;

	bool IBuildingRef.IsDestroyed => _building.IsDestroyed;

	bool IBuildingRef.IsCore => _buildingSystem.IsCoreBuilding(_building.TypeId);

	bool IBuildingRef.IsPlayerOwned => _building.IsPlayerOwned;

	bool IBuildingRef.IsEnemyOf(IUnitState unit)
	{
		Castle castle = _building.GetCastle();
		if (castle == null)
			return false;
		return castle.IsPlayerCastle != unit.IsPlayerUnit;
	}

	float IBuildingRef.MinX => GameCoordinatesAdapter.PixelsToGameUnits(_building.GlobalPosition.X - _building.GetBuildingSize().X * 0.5f);

	float IBuildingRef.MinY => GameCoordinatesAdapter.PixelsToGameUnits(_building.GlobalPosition.Y - _building.GetBuildingSize().Y * 0.5f);

	float IBuildingRef.MaxX => GameCoordinatesAdapter.PixelsToGameUnits(_building.GlobalPosition.X + _building.GetBuildingSize().X * 0.5f);

	float IBuildingRef.MaxY => GameCoordinatesAdapter.PixelsToGameUnits(_building.GlobalPosition.Y + _building.GetBuildingSize().Y * 0.5f);

	IBuildingTarget IBuildingRef.BuildingTarget => this;

	bool IBuildingTarget.IsDestroyed => _building.IsDestroyed;

	void IBuildingTarget.TakeDamage(int amount)
	{
		_building.TakeDamage(amount);
	}
}
