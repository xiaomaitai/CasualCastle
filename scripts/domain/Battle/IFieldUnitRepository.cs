using System.Collections.Generic;

namespace CasualCastle.Domain.Battle;

public interface IFieldUnitRepository
{
	void Register(IUnitHandle unit);
	void Unregister(IUnitHandle unit);
	IReadOnlyList<IUnitHandle> AllUnits { get; }

	void RegisterBuilding(IBuildingRef building);
	void UnregisterBuilding(IBuildingRef building);
	IReadOnlyList<IBuildingRef> AllBuildings { get; }
}
