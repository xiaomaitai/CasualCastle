using CasualCastle.Domain.Building;
using CasualCastle.Domain.History;
using CasualCastle.Adapters.Godot;

public class NightOrchestrator
{
    private readonly IBuildingRepository _buildingRepo;
    private readonly IReplayUseCase _replayService;

    public NightOrchestrator(IBuildingRepository buildingRepo, IReplayUseCase replayService)
    {
        _buildingRepo = buildingRepo;
        _replayService = replayService;
    }

    public void ResolveNightCombines(GameManager gm)
    {
        Castle playerCastle = gm.PlayerCastle;
        if (playerCastle == null)
            return;

        CombineBuildingFactory factory = new CombineBuildingFactory(playerCastle);
        CombineService combineService = new CombineService(factory, _buildingRepo);
        combineService.CombineCompleted += result =>
        {
            AdjacencyService adj = GameManager.Get<AdjacencyService>();
            adj.RefreshCastle(playerCastle.GetBuildingStates());
        };

        combineService.ResolveCombines(
            playerCastle.GetBuildingStates(),
            true,
            gm.IsNight,
            gm.CurrentState == GameManager.GameState.Playing);
    }

    public void ApplyReplaySnapshot(GameManager gm)
    {
        Castle enemyCastle = gm.EnemyCastle;
        if (enemyCastle == null)
            return;

        ReplayTarget target = new ReplayTarget(enemyCastle);
        _replayService.ApplyNightSnapshot(target, gm.CurrentNightIndex);

        AdjacencyService adj = GameManager.Get<AdjacencyService>();
        adj.RefreshCastle(enemyCastle.GetBuildingStates());
    }
}
