using CasualCastle.Domain.Building;

namespace CasualCastle.Domain.History;

public class ReplayService
{
    private readonly BattleReportService _reportService;

    public ReplayService(BattleReportService reportService)
    {
        _reportService = reportService;
    }

    public void ApplyNightSnapshot(IReplayTarget target, int nightIndex)
    {
        CastleSnapshot snapshot = _reportService.GetSelectedNightSnapshot(nightIndex);
        if (snapshot == null)
            return;

        target.ClearNonCoreBuildings();

        foreach (BuildingSnapshot buildingSnapshot in snapshot.Buildings)
        {
            if (BuildingDefinitions.IsCoreBuilding(buildingSnapshot.TypeId))
                continue;

            target.TryPlaceMirrored(buildingSnapshot);
        }
    }
}
