// 战场仓库端口：单位与建筑的登记查询
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
