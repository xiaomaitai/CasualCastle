using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Building;
using Godot;

public partial class InitManager : Node
{
    private NightOrchestrator _nightOrchestrator;

    public override void _Ready()
    {
        GameManager gm = AdapterRegistry.Resolve<GameManager>();

        _nightOrchestrator = new NightOrchestrator();

        AddChild(new BattleManager());

        AddChild(new BuildingSystem());
        BuildingSystem buildingSystem = AdapterRegistry.Resolve<BuildingSystem>();

        AddChild(new BattleReportSystem());

        Player player = new Player();

        Hand hand = new Hand(
            new CastlePlacementAdapter(buildingSystem), player);
        AdapterRegistry.Register<Hand>(hand);

        Shop shopService = new Shop(hand, GameManager.Get<ShopRules>(), player);
        AdapterRegistry.Register<Shop>(shopService);

        gm.PhaseChanged += phase =>
        {
            shopService.SetShopAvailable(phase == GameManager.GamePhase.Day || phase == GameManager.GamePhase.Night);
            if (phase == GameManager.GamePhase.Night)
            {
                shopService.RequestOpenShop();
                _nightOrchestrator.ResolveNightFusions(gm);
            }
            if (phase == GameManager.GamePhase.Night && gm.CurrentNightIndex >= 1)
                _nightOrchestrator.ApplyReplaySnapshot(gm);
        };
        gm.GameStateChanged += state =>
        {
            shopService.SetShopAvailable(state == GameManager.GameState.Playing);
        };
    }
}
