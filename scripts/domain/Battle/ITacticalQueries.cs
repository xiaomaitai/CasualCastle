using System.Collections.Generic;

namespace CasualCastle.Domain.Battle;

public interface ITacticalQueries
{
	(ISoldierHandle nearest, float edgeDist) FindNearestEnemy(ISoldierHandle soldier);
	void PropagateRetaliation(ISoldierHandle center, ISoldierHandle attacker);
	IBuildingTarget FindOverlappingBuilding(ISoldierHandle soldier);
	bool HasEnemyOnBuilding(IBuildingRef building);
}
