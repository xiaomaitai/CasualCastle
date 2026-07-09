using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Building;
using CasualCastle.Domain.Shared;
using Godot;
using System.Collections.Generic;

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
                _nightOrchestrator.ResolveNightCombines(gm);
            }
            if (phase == GameManager.GamePhase.Night && gm.CurrentNightIndex >= 1)
                _nightOrchestrator.ApplyReplaySnapshot(gm);
            if (phase == GameManager.GamePhase.Day && gm.CurrentNightIndex >= 1)
                gm.SaveGame(0);
        };
        gm.GameStateChanged += state =>
        {
            shopService.SetShopAvailable(state == GameManager.GameState.Playing);
        };

        if (gm.PendingLoadSlot >= 0)
            LoadSaveIntoGame(gm, shopService, hand);
    }

    private static void LoadSaveIntoGame(GameManager gm, Shop shopService, Hand hand)
    {
        SaveData data = gm.LoadSaveData(gm.PendingLoadSlot);
        if (data == null)
            return;

        Castle playerCastle = gm.PlayerCastle;
        if (playerCastle == null)
            return;

        List<Building> existingBuildings = playerCastle.GetBuildings();
        foreach (Building b in existingBuildings)
        {
            if (b == playerCastle.Heart)
                continue;
            playerCastle.ReleaseBuildingFootprint(b);
            b.GetParent()?.RemoveChild(b);
            b.QueueFree();
        }

        foreach (BuildingSaveEntry entry in data.Buildings)
        {
            if (entry.TypeId == "CastleHeart")
                continue;

            Building building = BuildingSystem.CreateBuilding(entry.TypeId);
            if (building == null)
                continue;

            building.InitFromType(entry.TypeId);
            building.ApplySnapshotState(entry.Health, false, false);
            building.BindToGrid(playerCastle, entry.AnchorGridX, entry.AnchorGridY);
            playerCastle.PlaceBuilding(building, entry.AnchorGridX, entry.AnchorGridY, entry.TypeId);
        }

        shopService.TrySpendGold(shopService.Gold);
        shopService.AddGold(data.Gold);

        hand.ResetHand();
        foreach (CardSaveEntry card in data.HandCards)
        {
            hand.TryAddCard(new CardData
            {
                Id = card.Id,
                Name = card.Name,
                Cost = card.Cost,
                BuildingType = card.BuildingType,
                Weight = card.Weight,
            });
        }

        gm.CurrentNightIndex = data.CurrentNightIndex;
        if (!string.IsNullOrEmpty(data.PendingReplayReportId))
            gm.SetPendingReplayReportId(data.PendingReplayReportId);

        GameManager.Get<AdjacencyService>().RefreshCastle(playerCastle.GetBuildingStates());
    }
}
