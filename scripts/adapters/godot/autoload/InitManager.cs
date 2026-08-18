// 场景初始化：组装系统节点与纯 C# 服务
using CasualCastle.Adapters.Godot;
using CasualCastle.Adapters.Godot.Flow;
using CasualCastle.Application;
using CasualCastle.Domain.Building;
using CasualCastle.Domain.Shared;
using Godot;

public partial class InitManager : Node
{
	public override void _Ready()
	{
		GameManager gm = AdapterRegistry.Resolve<GameManager>();

		AddChild(new BattleManager());

		AddChild(new BuildingSystem());
		BuildingSystem buildingSystem = AdapterRegistry.Resolve<BuildingSystem>();

		UnitSpawnService spawnService = new UnitSpawnService();
		AdapterRegistry.Register<UnitSpawnService>(spawnService);

		AddChild(new BattleReportSystem());

		Player player = new Player();

		Hand hand = new Hand(
			new CastlePlacementAdapter(buildingSystem), player);
		AdapterRegistry.Register<Hand>(hand);

		RitualService ritualService = GameManager.Get<RitualService>();
		ShopRules shopRules = GameManager.Get<ShopRules>();
		shopRules.SetAvailability(
			ritualId => ritualService.IsLearned(ritualId),
			cardId => IsBookHeld(hand, cardId),
			() => GetCoreLevel(gm));
		Shop shopService = new Shop(hand, shopRules, player);
		AdapterRegistry.Register<Shop>(shopService);
		shopService.SetFaction(gm.SelectedFaction);

		if (gm.SelectedFaction == FactionDef.DungeonId)
			AddChild(new DemonKingController());

		GameSessionPorts ports = GameManager.Get<GameSessionPorts>();
		GameSessionBridge bridge = new(gm, shopService, hand);
		ports.Query = bridge;
		ports.Command = bridge;
		ports.Shop = bridge;
		ports.BattleReportCapture = AdapterRegistry.Resolve<BattleReportSystem>();

		gm.GameStateChanged += state =>
		{
			shopService.SetShopAvailable(state == GameManager.GameState.Playing);
		};

		if (gm.PendingLoadSlot >= 0)
			CallDeferred(nameof(LoadSaveIntoGameDeferred));
	}

	public void LoadSaveIntoGameDeferred()
	{
		GameManager gm = AdapterRegistry.Resolve<GameManager>();
		IGameSessionService session = GameManager.Get<IGameSessionService>();
		SaveData data = session.LoadSaveData(gm.PendingLoadSlot);
		if (data != null)
			session.RestoreSession(data);
	}

	private static bool IsBookHeld(Hand hand, string cardId)
	{
		foreach (CardData card in hand.Cards)
		{
			if (card.Id == cardId && DarkMagicBookRules.IsDarkMagicBook(card))
				return true;
		}
		return false;
	}

	private static int GetCoreLevel(GameManager gm)
	{
		Castle castle = gm.PlayerCastle;
		return castle == null ? 1 : RitualCircleRules.GetCoreLevel(castle.GetBuildingStates());
	}
}
