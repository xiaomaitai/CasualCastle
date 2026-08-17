// 场景初始化：组装系统节点与纯 C# 服务
using CasualCastle.Adapters.Godot;
using CasualCastle.Adapters.Persistence;
using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Building;
using CasualCastle.Domain.History;
using CasualCastle.Domain.Shared;
using Godot;
using System.Collections.Generic;

public partial class InitManager : Node
{
	private NightOrchestrator _nightOrchestrator;

	public override void _Ready()
	{
		GameManager gm = AdapterRegistry.Resolve<GameManager>();

		_nightOrchestrator = new NightOrchestrator(GameManager.Get<IBuildingRepository>(), GameManager.Get<IReplayUseCase>(), GameManager.Get<ICombineUseCase>(), GameManager.Get<AdjacencyService>());

		AddChild(new BattleManager());

		AddChild(new BuildingSystem());
		BuildingSystem buildingSystem = AdapterRegistry.Resolve<BuildingSystem>();

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

		gm.PhaseChanged += phase =>
		{
			shopService.SetShopAvailable(phase == MatchPhase.Combat || phase == MatchPhase.Shop);
			if (phase == MatchPhase.Shop)
			{
				shopService.RefreshOffers();
				shopService.RequestOpenShop();
			}
			if (phase == MatchPhase.Combine)
			{
				_nightOrchestrator.ResolveNightCombines(gm);
			}
			if (phase == MatchPhase.Combat)
				_nightOrchestrator.ApplyReplaySnapshot(gm, gm.CurrentNightIndex + 1, PhaseMapping.ToBattlePhase(phase));
			if (phase == MatchPhase.Combat && gm.CurrentNightIndex >= 1)
			{
				try
				{
					gm.SaveGame(0);
				}
				catch (System.Exception ex)
				{
					GD.Print($"自动存档失败：{ex.Message}");
				}
				try
				{
					AdapterRegistry.Resolve<BattleReportSystem>()?.SaveCurrentReportDaily();
				}
				catch (System.Exception ex)
				{
					GD.Print($"战报自动保存失败：{ex.Message}");
				}
			}
		};
		gm.GameStateChanged += state =>
		{
			shopService.SetShopAvailable(state == GameManager.GameState.Playing);
		};

		if (gm.PendingLoadSlot >= 0)
			CallDeferred(nameof(LoadSaveIntoGameDeferred));
	}

	public void LoadSaveIntoGameDeferred()
	{
		LoadSaveIntoGame(
			AdapterRegistry.Resolve<GameManager>(),
			AdapterRegistry.Resolve<Shop>(),
			AdapterRegistry.Resolve<Hand>());
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

	private static CardData CreateRestoredCard(CardSaveEntry card)
	{
		CardData restored = new()
		{
			Id = card.Id,
			Name = card.Name,
			Cost = card.Cost,
			BuildingType = card.BuildingType,
			Weight = card.Weight,
			ItemType = card.ItemType,
			Attributes = new Dictionary<string, string>(card.Attributes),
		};
		if (string.IsNullOrEmpty(restored.ItemType) && !card.IsPlaceable)
			CopyItemFromCatalog(restored);
		return restored;
	}

	private static void CopyItemFromCatalog(CardData restored)
	{
		CardData catalogCard = FindCatalogCard(restored.Id);
		if (catalogCard == null)
			return;
		restored.ItemType = catalogCard.ItemType;
		restored.Attributes = new Dictionary<string, string>(catalogCard.Attributes);
	}

	private static CardData FindCatalogCard(string cardId)
	{
		foreach (CardData catalogCard in GameDataLoader.ShopCatalog)
		{
			if (catalogCard.Id == cardId)
				return catalogCard;
		}
		return null;
	}

	private void LoadSaveIntoGame(GameManager gm, Shop shopService, Hand hand)
	{
		SaveData data = gm.LoadSaveData(gm.PendingLoadSlot);
		if (data == null)
			return;

		Castle playerCastle = gm.PlayerCastle;
		if (playerCastle == null)
			return;

		playerCastle.ClearNonCoreBuildings();

		foreach (BuildingSaveEntry entry in data.Buildings)
		{
			if (entry.TypeId == BuildingTypeIds.CastleHeart || GameManager.Get<IBuildingRepository>().IsCoreBuilding(entry.TypeId))
				continue;

			Building building = AdapterRegistry.Resolve<BuildingSystem>().CreateBuilding(entry.TypeId);
			if (building == null)
				continue;

			building.InitFromType(entry.TypeId);
			building.ApplySnapshotState(entry.Health, false, false);
			building.BindToGrid(playerCastle, entry.AnchorGridX, entry.AnchorGridY);
			playerCastle.PlaceBuilding(building, entry.AnchorGridX, entry.AnchorGridY, entry.TypeId);
		}

		foreach (BuildingSaveEntry entry in data.Buildings)
		{
			if (entry.TypeId == BuildingTypeIds.CastleHeart)
				continue;
			if (GameManager.Get<IBuildingRepository>().IsCoreBuilding(entry.TypeId)
				&& entry.TypeId != playerCastle.Heart.TypeId)
			{
				playerCastle.ReplaceHeart(entry.TypeId, entry.Health);
				break;
			}
		}

		shopService.TrySpendGold(shopService.Gold);
		shopService.AddGold(data.Gold);

		hand.ResetHand();
		foreach (CardSaveEntry card in data.HandCards)
			hand.TryAddCard(CreateRestoredCard(card));

		gm.SetNightIndex(data.CurrentNightIndex);
		if (!string.IsNullOrEmpty(data.PendingReplayReportId))
			gm.SetPendingReplayReportId(data.PendingReplayReportId);

		RitualService ritualService = GameManager.Get<RitualService>();
		if (ritualService != null)
			ritualService.Restore(data.RitualStudyCompletionCount, data.LearnedRitualIds);
		KingExperienceService kingXp = GameManager.Get<KingExperienceService>();
		if (kingXp != null)
			kingXp.Restore(data.DungeonKingXp);
		KingSkillSet kingSkills = GameManager.Get<KingSkillSet>();
		if (kingSkills != null)
		{
			kingSkills.Reset();
			for (int i = 0; i < data.DungeonLearnedSkillIds.Count; i++)
			{
				int level = i < data.DungeonLearnedSkillLevels.Count ? data.DungeonLearnedSkillLevels[i] : 1;
				kingSkills.Add(data.DungeonLearnedSkillIds[i], level);
			}
		}
		shopService.SetFaction(data.FactionId);
		GameManager.Get<AdjacencyService>().RefreshCastle(playerCastle.GetBuildingStates());
	}
}
