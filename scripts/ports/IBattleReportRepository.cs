using System.Collections.Generic;

namespace CasualCastle.Domain.Ports;

public interface IBattleReportRepository
{
	List<BattleReport> LoadAll();
	void SaveAll(IReadOnlyList<BattleReport> reports);
}
