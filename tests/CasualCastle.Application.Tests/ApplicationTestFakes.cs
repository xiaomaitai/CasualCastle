using CasualCastle.Application;
using CasualCastle.Domain.Building;
using CasualCastle.Domain.History;
using CasualCastle.Domain.Shared;
using System;
using System.Collections.Generic;

namespace CasualCastle.Application.Tests;

internal sealed class RecordingSaveRepository : ISaveRepository
{
	public int SaveCalls;
	public SaveData LastSaved;

	public void Save(SaveData data)
	{
		SaveCalls++;
		LastSaved = data;
	}

	public SaveData Load(int slotIndex)
	{
		return LastSaved;
	}

	public bool HasSave(int slotIndex)
	{
		return LastSaved != null;
	}

	public void DeleteSave(int slotIndex)
	{
		LastSaved = null;
	}

	public List<int> ListSlots()
	{
		return new List<int>();
	}
}

internal sealed class RecordingQuery : IGameSessionQuery
{
	public string FactionId { get; set; } = FactionIds.Alliance;
	public int Gold { get; set; }
	public int CurrentNightIndex { get; set; }
	public string PendingReplayReportId { get; set; } = "";
	public List<IBuildingState> PlayerBuildingStates { get; set; } = new();
	public IReadOnlyList<CardData> HandCards { get; set; } = new List<CardData>();
	public ICombineBuildingFactory CombineFactory { get; set; }
	public int RitualStudyCompletionCount { get; set; }
	public IReadOnlyList<string> LearnedRitualIds { get; set; } = new List<string>();
	public int DungeonKingXp { get; set; }
	public IReadOnlyList<string> DungeonLearnedSkillIds { get; set; } = new List<string>();
	public IReadOnlyList<int> DungeonLearnedSkillLevels { get; set; } = new List<int>();
}

internal sealed class RecordingCommand : IGameSessionCommand
{
	public int ClearCalls;
	public List<(string TypeId, int X, int Y, int Health)> Placed = new();
	public bool CoreResult = true;
	public List<(string TypeId, int Health)> CoreCalls = new();
	public int Gold = -1;
	public int ResetHandCalls;
	public List<CardData> AddedCards = new();
	public int NightIndex = -1;
	public string ReplayReportId;
	public int RitualCount = -1;
	public List<string> RitualIds;
	public int KingXp = -1;
	public List<string> SkillIds;
	public List<int> SkillLevels;
	public int RefreshPlayerCalls;
	public int RefreshAfterCombineCalls;
	public int RefreshEnemyCalls;

	public void ClearNonCoreBuildings()
	{
		ClearCalls++;
	}

	public void PlaceRestoredBuilding(string typeId, int anchorGridX, int anchorGridY, int health)
	{
		Placed.Add((typeId, anchorGridX, anchorGridY, health));
	}

	public bool RestoreCoreBuilding(string typeId, int health)
	{
		CoreCalls.Add((typeId, health));
		return CoreResult;
	}

	public void SetGold(int gold)
	{
		Gold = gold;
	}

	public void ResetHand()
	{
		ResetHandCalls++;
	}

	public void AddRestoredCard(CardData card)
	{
		AddedCards.Add(card);
	}

	public void SetNightIndex(int index)
	{
		NightIndex = index;
	}

	public void SetPendingReplayReportId(string reportId)
	{
		ReplayReportId = reportId;
	}

	public void RestoreRitualState(int studyCompletionCount, IReadOnlyList<string> learnedRitualIds)
	{
		RitualCount = studyCompletionCount;
		RitualIds = new List<string>(learnedRitualIds);
	}

	public void RestoreKingXp(int cumulativeExperience)
	{
		KingXp = cumulativeExperience;
	}

	public void RestoreKingSkills(IReadOnlyList<string> skillIds, IReadOnlyList<int> levels)
	{
		SkillIds = new List<string>(skillIds);
		SkillLevels = new List<int>(levels);
	}

	public void RefreshPlayerAdjacency()
	{
		RefreshPlayerCalls++;
	}

	public void RefreshAfterCombine()
	{
		RefreshAfterCombineCalls++;
	}

	public void RefreshEnemyAdjacency()
	{
		RefreshEnemyCalls++;
	}
}

internal sealed class RecordingShop : IShopAccess
{
	public bool Available;
	public int RefreshCalls;
	public int OpenCalls;
	public string Faction;

	public void SetShopAvailable(bool available)
	{
		Available = available;
	}

	public void RefreshOffers()
	{
		RefreshCalls++;
	}

	public void RequestOpenShop()
	{
		OpenCalls++;
	}

	public void SetFaction(string factionId)
	{
		Faction = factionId;
	}
}

internal sealed class RecordingCapture : IBattleReportCapture
{
	public string StartedReportId;
	public List<(List<BuildingSnapshot> Buildings, int Gold, List<CardSnapshot> Hand, int NightIndex, BattlePhase Phase)> Snapshots = new();
	public int DailyCalls;

	public void StartMatch(string selectedReportId)
	{
		StartedReportId = selectedReportId;
	}

	public void CapturePhaseSnapshot(IReadOnlyList<BuildingSnapshot> buildings, int gold, IReadOnlyList<CardSnapshot> handCards, int nightIndex, BattlePhase phase)
	{
		Snapshots.Add((new List<BuildingSnapshot>(buildings), gold, new List<CardSnapshot>(handCards), nightIndex, phase));
	}

	public BattleReport SaveCurrentReportDaily()
	{
		DailyCalls++;
		return null;
	}
}

internal sealed class FakeBuildingState : IBuildingState
{
	public string TypeId { get; }
	public int AnchorGridX { get; }
	public int AnchorGridY { get; }
	public int Health => 100;
	public bool IsDestroyed => false;
	public bool IsManuallyPaused => false;
	public bool IsCombineProhibited => false;
	public bool HasEnemyOnTop => false;
	public bool IsPlayerOwned => true;
	public bool ContributesToAdjacency => true;

	public FakeBuildingState(string typeId, int anchorX, int anchorY)
	{
		TypeId = typeId;
		AnchorGridX = anchorX;
		AnchorGridY = anchorY;
	}

	public void SetWorkSpeedMultiplier(float multiplier)
	{
	}
}

internal sealed class FakeCombineFactory : ICombineBuildingFactory
{
	public IBuildingState Create(string typeId, int anchorX, int anchorY)
	{
		return new FakeBuildingState(typeId, anchorX, anchorY);
	}

	public void Destroy(IBuildingState building)
	{
	}
}

internal static class TestPorts
{
	public static GameSessionPorts Create(RecordingQuery query, RecordingCommand command, RecordingShop shop, RecordingCapture capture)
	{
		return new GameSessionPorts
		{
			Query = query,
			Command = command,
			Shop = shop,
			BattleReportCapture = capture,
		};
	}
}
