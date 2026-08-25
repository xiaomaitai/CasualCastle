// 战报采集端口：阶段快照与对局开局
using System.Collections.Generic;

namespace CasualCastle.Domain.History;

public interface IBattleReportCapture
{
	void StartMatch(string selectedReportId);
	void CapturePhaseSnapshot(IReadOnlyList<BuildingSnapshot> buildings, int gold, IReadOnlyList<CardSnapshot> handCards, int nightIndex, BattlePhase phase);
}
