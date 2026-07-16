using System.Collections.Generic;

namespace CasualCastle.Domain.Battle;

public interface IFieldUnitRepository
{
	void Register(ISoldierHandle soldier);
	void Unregister(ISoldierHandle soldier);
	IReadOnlyList<ISoldierHandle> AllUnits { get; }

	void RegisterBuilding(IBuildingRef building);
	void UnregisterBuilding(IBuildingRef building);
	IReadOnlyList<IBuildingRef> AllBuildings { get; }
}
