using CasualCastle.Domain.Building;
using CasualCastle.Domain.History;
using CasualCastle.Adapters.Godot;

public class NightOrchestrator
{
	private readonly IBuildingRepository _buildingRepo;
	private readonly IReplayUseCase _replayService;
	private readonly ICombineUseCase _combineService;
	private readonly AdjacencyService _adjacencyService;

	public NightOrchestrator(IBuildingRepository buildingRepo, IReplayUseCase replayService, ICombineUseCase combineService, AdjacencyService adjacencyService)
	{
		_buildingRepo = buildingRepo;
		_replayService = replayService;
		_combineService = combineService;
		_adjacencyService = adjacencyService;
	}

	public void ResolveNightCombines(GameManager gm)
	{
		Castle playerCastle = gm.PlayerCastle;
		if (playerCastle == null)
			return;

		CombineBuildingFactory factory = new CombineBuildingFactory(playerCastle);
		_combineService.ResolveCombines(
			playerCastle.GetBuildingStates(),
			true,
			gm.IsNight,
			gm.CurrentState == GameManager.GameState.Playing,
			factory,
			result => _adjacencyService.RefreshCastle(playerCastle.GetBuildingStates()));
	}

	public void ApplyReplaySnapshot(GameManager gm)
	{
		Castle enemyCastle = gm.EnemyCastle;
		if (enemyCastle == null)
			return;

		ReplayTarget target = new ReplayTarget(enemyCastle);
		_replayService.ApplyNightSnapshot(target, gm.CurrentNightIndex);

		_adjacencyService.RefreshCastle(enemyCastle.GetBuildingStates());
	}
}
