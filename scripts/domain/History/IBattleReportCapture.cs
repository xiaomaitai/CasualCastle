// 战报采集端口：阶段快照、开局与日报保存
using CasualCastle.Domain.Building;
using System.Collections.Generic;

namespace CasualCastle.Domain.History;

public interface IBattleReportCapture
{
	void StartMatch(string selectedReportId);
	void CapturePhaseSnapshot(IReadOnlyList<IBuildingState> buildings, int gold, IReadOnlyList<CardData> handCards, int nightIndex, BattlePhase phase);
	BattleReport SaveCurrentReportDaily();
}
