using CasualCastle.Domain.Building;

namespace CasualCastle.Domain.History;

public class ReplayService : IReplayUseCase
{
    private readonly BattleReportService _reportService;
    private readonly IBuildingRepository _buildingRepo;

    public ReplayService(BattleReportService reportService, IBuildingRepository buildingRepo)
    {
        _reportService = reportService;
        _buildingRepo = buildingRepo;
    }

    public void ApplyNightSnapshot(IReplayTarget target, int nightIndex)
    {
        CastleSnapshot snapshot = _reportService.GetSelectedNightSnapshot(nightIndex);
        if (snapshot == null)
            return;

        target.ClearNonCoreBuildings();

        foreach (BuildingSnapshot buildingSnapshot in snapshot.Buildings)
        {
            if (_buildingRepo.IsCoreBuilding(buildingSnapshot.TypeId))
                continue;

            target.TryPlaceMirrored(buildingSnapshot);
        }
    }
}
