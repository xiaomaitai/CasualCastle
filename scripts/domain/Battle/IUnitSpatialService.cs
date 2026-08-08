// 空间推挤服务端口
using System.Collections.Generic;

namespace CasualCastle.Domain.Battle;

public interface IUnitSpatialService
{
	void PushUnits(IReadOnlyList<IUnitHandle> allUnits, IReadOnlyList<IBuildingRef> buildings, float dt);
}
