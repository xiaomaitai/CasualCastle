using CasualCastle.Domain.Building;
using CasualCastle.Domain.History;
using CasualCastle.Adapters.Godot;

public class NightOrchestrator
{
    public void ResolveNightFusions(GameManager gm)
    {
        Castle playerCastle = gm.PlayerCastle;
        if (playerCastle == null)
            return;

        FusionBuildingFactory factory = new FusionBuildingFactory(playerCastle);
        FusionService fusionService = new FusionService(factory, GameManager.Get<IBuildingRepository>());
        fusionService.FusionCompleted += result =>
        {
            AdjacencyService adj = GameManager.Get<AdjacencyService>();
            adj.RefreshCastle(playerCastle.GetBuildingStates());
        };

        fusionService.ResolveFusions(
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

        IReplayUseCase replayService = GameManager.Get<IReplayUseCase>();
        ReplayTarget target = new ReplayTarget(enemyCastle);
        replayService.ApplyNightSnapshot(target, gm.CurrentNightIndex);

        AdjacencyService adj = GameManager.Get<AdjacencyService>();
        adj.RefreshCastle(enemyCastle.GetBuildingStates());
    }
}
