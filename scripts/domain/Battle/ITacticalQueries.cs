// 战术查询端口：索敌、建筑重叠与范围查询
using System.Collections.Generic;

namespace CasualCastle.Domain.Battle;

public interface ITacticalQueries
{
	(IUnitHandle nearest, float edgeDist) FindNearestEnemy(IUnitHandle unit);
	void PropagateRetaliation(IUnitHandle center, IUnitHandle attacker);
	IBuildingTarget FindOverlappingBuilding(IUnitHandle unit);
	bool HasEnemyOnBuilding(IBuildingRef building);
	IReadOnlyList<IUnitHandle> FindEnemiesNear(IUnitHandle center, float radius);
	IReadOnlyList<IUnitHandle> FindEnemiesNearPoint(IUnitHandle unit, float centerX, float centerY, float radius);
	IUnitHandle FindNearestInjuredAlly(IUnitHandle unit, float range);
}
