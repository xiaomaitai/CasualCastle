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

        AddChild(new BuildingSystem());
        BuildingSystem buildingSystem = AdapterRegistry.Resolve<BuildingSystem>();

        AddChild(new BattleReportSystem());
        BattleReportSystem battleReport = AdapterRegistry.Resolve<BattleReportSystem>();

        BuildingFactory buildingFactory = new BuildingFactory();

        HandService handService = new HandService(
            new CastlePlacementAdapter(gm.PlayerCastle, buildingSystem));

        ShopService shopService = new ShopService(handService);

        BattleReportService reportService = new BattleReportService(GameManager.Get<IBattleReportRepository>());

        ReplayService replayService = new ReplayService(reportService);

        AdjacencyService adjacencyService = new AdjacencyService();

        AdapterRegistry.Register<HandService>(handService);
        AdapterRegistry.Register<ShopService>(shopService);
        AdapterRegistry.Register<BattleReportService>(reportService);
        AdapterRegistry.Register<ReplayService>(replayService);
        AdapterRegistry.Register<AdjacencyService>(adjacencyService);
        AdapterRegistry.Register<IBuildingFactory>(buildingFactory);

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
            AdjacencyService adj = AdapterRegistry.Resolve<AdjacencyService>();
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

        ReplayService replayService = AdapterRegistry.Resolve<ReplayService>();
        ReplayTarget target = new ReplayTarget(enemyCastle);
        replayService.ApplyNightSnapshot(target, gm.CurrentNightIndex);

        AdjacencyService adj = AdapterRegistry.Resolve<AdjacencyService>();
        adj.RefreshCastle(enemyCastle.GetBuildingStates());
    }
}
