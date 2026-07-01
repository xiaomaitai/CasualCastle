using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Building;
using CasualCastle.Domain.History;
using CasualCastle.Ports;
using Godot;

public partial class InitManager : Node
{
    public override void _Ready()
    {
        GameManager gm = AdapterRegistry.Resolve<GameManager>();

        BuildingFactory buildingFactory = new BuildingFactory();
        AdapterRegistry.Register<IBuildingFactory>(buildingFactory);

        BattleReportService reportService = GameManager.Get<BattleReportService>();
        AdapterRegistry.Register<BattleReportService>(reportService);

        ReplayService replayService = GameManager.Get<ReplayService>();
        AdapterRegistry.Register<ReplayService>(replayService);

        AdjacencyService adjacencyService = GameManager.Get<AdjacencyService>();
        AdapterRegistry.Register<AdjacencyService>(adjacencyService);

        AddChild(new BattleManager());

        AddChild(new BuildingSystem());
        BuildingSystem buildingSystem = AdapterRegistry.Resolve<BuildingSystem>();

        AddChild(new BattleReportSystem());

        Hand hand = new Hand(
            new CastlePlacementAdapter(buildingSystem));
        AdapterRegistry.Register<Hand>(hand);

        Shop shopService = new Shop(hand);
        AdapterRegistry.Register<Shop>(shopService);

        gm.PhaseChanged += phase =>
        {
            shopService.SetShopAvailable(phase == GameManager.GamePhase.Day || phase == GameManager.GamePhase.Night);
            if (phase == GameManager.GamePhase.Night)
            {
                shopService.RequestOpenShop();
                ResolveNightFusions(gm);
            }
            if (phase == GameManager.GamePhase.Night && gm.CurrentNightIndex >= 1)
                ApplyReplaySnapshot(gm);
        };
        gm.GameStateChanged += state =>
        {
            shopService.SetShopAvailable(state == GameManager.GameState.Playing);
        };
    }

    private static void ResolveNightFusions(GameManager gm)
    {
        Castle playerCastle = gm.PlayerCastle;
        if (playerCastle == null)
            return;

        FusionBuildingFactory factory = new FusionBuildingFactory(playerCastle);
        FusionService fusionService = new FusionService(factory);
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

    private static void ApplyReplaySnapshot(GameManager gm)
    {
        Castle enemyCastle = gm.EnemyCastle;
        if (enemyCastle == null)
            return;

        ReplayService replayService = GameManager.Get<ReplayService>();
        ReplayTarget target = new ReplayTarget(enemyCastle);
        replayService.ApplyNightSnapshot(target, gm.CurrentNightIndex);

        AdjacencyService adj = GameManager.Get<AdjacencyService>();
        adj.RefreshCastle(enemyCastle.GetBuildingStates());
    }
}
