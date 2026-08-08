// 战报仓库端口
using System.Collections.Generic;

namespace CasualCastle.Domain.History;

public interface IBattleReportRepository
{
	List<BattleReport> LoadAll();
	void SaveAll(IReadOnlyList<BattleReport> reports);
}
