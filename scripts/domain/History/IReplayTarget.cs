// 回放目标端口
using CasualCastle.Domain.Shared;
using System.Collections.Generic;

namespace CasualCastle.Domain.History;

public interface IReplayTarget
{
	int GridColumns { get; }
	void ClearNonCoreBuildings();
	bool TryPlaceMirrored(BuildingSnapshot snapshot);
}
