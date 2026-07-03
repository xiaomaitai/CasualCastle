using System.Collections.Generic;

namespace CasualCastle.Domain.Battle;

public interface IFieldUnitRepository
{
	void Register(ISoldierHandle soldier);
	void Unregister(ISoldierHandle soldier);
	IReadOnlyList<ISoldierHandle> AllUnits { get; }
	(ISoldierHandle nearest, float edgeDist) FindNearestEnemy(ISoldierHandle soldier);
	void PropagateRetaliation(ISoldierHandle center, ISoldierHandle attacker);

	void RegisterBuilding(IBuildingRef building);
	void UnregisterBuilding(IBuildingRef building);
	IBuildingTarget FindOverlappingBuilding(ISoldierHandle soldier);
	bool HasEnemyOnBuilding(IBuildingRef building);
}
