// 对局会话桥接：把 Godot 侧会话对象翻译为应用层端口
using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Building;
using System.Collections.Generic;

namespace CasualCastle.Adapters.Godot.Flow;

public class GameSessionBridge : IGameSessionQuery, IGameSessionCommand, IShopAccess
{
	private readonly GameManager _gm;
	private readonly Shop _shop;
	private readonly Hand _hand;

	public GameSessionBridge(GameManager gm, Shop shop, Hand hand)
	{
		_gm = gm;
		_shop = shop;
		_hand = hand;
	}

	public string FactionId => _shop.FactionId;
	public int Gold => _shop.Gold;
	public int CurrentNightIndex => _gm.CurrentNightIndex;
	public string PendingReplayReportId => _gm.PendingReplayReportId;
	public List<IBuildingState> PlayerBuildingStates => _gm.PlayerCastle.GetBuildingStates();
	public IReadOnlyList<CardData> HandCards => _hand.Cards;
	public ICombineBuildingFactory CombineFactory => new CombineBuildingFactory(_gm.PlayerCastle);
	public int RitualStudyCompletionCount => GameManager.Get<RitualService>().StudyCompletionCount;
	public IReadOnlyList<string> LearnedRitualIds => GameManager.Get<RitualService>().GetLearnedRitualIds();
	public int DungeonKingXp => GameManager.Get<KingExperienceService>().GetCumulativeExperience();
	public IReadOnlyList<string> DungeonLearnedSkillIds => GameManager.Get<KingSkillSet>().GetAll();
	public IReadOnlyList<int> DungeonLearnedSkillLevels => CollectSkillLevels();

	private static IReadOnlyList<int> CollectSkillLevels()
	{
		KingSkillSet skills = GameManager.Get<KingSkillSet>();
		List<int> levels = new();
		foreach (string skillId in skills.GetAll())
			levels.Add(skills.GetLearnLevel(skillId));
		return levels;
	}

	public void ClearNonCoreBuildings()
	{
		_gm.PlayerCastle.ClearNonCoreBuildings();
	}

	public void PlaceRestoredBuilding(string typeId, int anchorGridX, int anchorGridY, int health)
	{
		Building building = AdapterRegistry.Resolve<BuildingSystem>().CreateBuilding(typeId);
		if (building == null)
			return;
		building.InitFromType(typeId);
		building.ApplySnapshotState(health, false, false);
		building.BindToGrid(_gm.PlayerCastle, anchorGridX, anchorGridY);
		_gm.PlayerCastle.PlaceBuilding(building, anchorGridX, anchorGridY, typeId);
	}

	public bool RestoreCoreBuilding(string typeId, int health)
	{
		if (typeId == _gm.PlayerCastle.Heart.TypeId)
			return false;
		_gm.PlayerCastle.ReplaceHeart(typeId, health);
		return true;
	}

	public void SetGold(int gold)
	{
		_shop.TrySpendGold(_shop.Gold);
		_shop.AddGold(gold);
	}

	public void ResetHand()
	{
		_hand.ResetHand();
	}

	public void AddRestoredCard(CardData card)
	{
		_hand.TryAddCard(card);
	}

	public void SetNightIndex(int index)
	{
		_gm.SetNightIndex(index);
	}

	public void SetPendingReplayReportId(string reportId)
	{
		_gm.SetPendingReplayReportId(reportId);
	}

	public void RestoreRitualState(int studyCompletionCount, IReadOnlyList<string> learnedRitualIds)
	{
		GameManager.Get<RitualService>().Restore(studyCompletionCount, learnedRitualIds);
	}

	public void RestoreKingXp(int cumulativeExperience)
	{
		GameManager.Get<KingExperienceService>().Restore(cumulativeExperience);
	}

	public void RestoreKingSkills(IReadOnlyList<string> skillIds, IReadOnlyList<int> levels)
	{
		KingSkillSet skills = GameManager.Get<KingSkillSet>();
		skills.Reset();
		for (int i = 0; i < skillIds.Count; i++)
		{
			int level = i < levels.Count ? levels[i] : 1;
			skills.Add(skillIds[i], level);
		}
	}

	public void RefreshPlayerAdjacency()
	{
		GameManager.Get<AdjacencyService>().RefreshCastle(_gm.PlayerCastle.GetBuildingStates());
	}

	public void RefreshAfterCombine()
	{
		RefreshPlayerAdjacency();
		Sfx.Play(SfxEvents.Combine);
	}

	public void RefreshEnemyAdjacency()
	{
		GameManager.Get<AdjacencyService>().RefreshCastle(_gm.EnemyCastle.GetBuildingStates());
	}

	public void SetShopAvailable(bool available)
	{
		_shop.SetShopAvailable(available);
	}

	public void RefreshOffers()
	{
		_shop.RefreshOffers();
	}

	public void RequestOpenShop()
	{
		_shop.RequestOpenShop();
	}

	public void SetFaction(string factionId)
	{
		_shop.SetFaction(factionId);
	}
}
