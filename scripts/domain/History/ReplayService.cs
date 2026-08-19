// 回放服务：应用战报快照到目标
namespace CasualCastle.Domain.History;

public class ReplayService : IReplayUseCase
{
	private readonly BattleReportService _reportService;
	private readonly IHistoryCoreLookup _coreLookup;

	public ReplayService(BattleReportService reportService, IHistoryCoreLookup coreLookup)
	{
		_reportService = reportService;
		_coreLookup = coreLookup;
	}

	public void ApplyNightSnapshot(IReplayTarget target, int nightIndex, BattlePhase phase)
	{
		CastleSnapshot snapshot = _reportService.GetSelectedPhaseSnapshot(nightIndex, phase);
		if (snapshot == null)
			return;

		target.ClearNonCoreBuildings();

		foreach (BuildingSnapshot buildingSnapshot in snapshot.Buildings)
		{
			if (_coreLookup.IsCoreBuilding(buildingSnapshot.TypeId))
				continue;

			target.TryPlaceMirrored(buildingSnapshot);
		}
	}
}
