// 战报采集端口：阶段快照、对局开局与进度快照存取
using System.Collections.Generic;

namespace CasualCastle.Domain.History;

public interface IBattleReportCapture
{
	void StartMatch(string selectedReportId);
	void CapturePhaseSnapshot(IReadOnlyList<BuildingSnapshot> buildings, int gold, IReadOnlyList<CardSnapshot> handCards, int nightIndex, BattlePhase phase);
	IReadOnlyList<CastleSnapshot> ExportCurrentSnapshots();
	void ImportCurrentSnapshots(IReadOnlyList<CastleSnapshot> snapshots);
}
