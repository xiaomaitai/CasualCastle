// 对局会话用例：存档采集、持久化与恢复
using CasualCastle.Domain.Building;
using CasualCastle.Domain.History;
using CasualCastle.Domain.Shared;
using System.Collections.Generic;
using System.Text.Json;

namespace CasualCastle.Application;

public class GameSessionService : IGameSessionService
{
	private readonly ISaveRepository _saveRepository;
	private readonly IReadOnlyList<CardData> _catalog;
	private readonly IBuildingRepository _buildingRepository;
	private readonly GameSessionPorts _ports;

	public GameSessionService(ISaveRepository saveRepository, IReadOnlyList<CardData> catalog, IBuildingRepository buildingRepository, GameSessionPorts ports)
	{
		_saveRepository = saveRepository;
		_catalog = catalog;
		_buildingRepository = buildingRepository;
		_ports = ports;
	}

	public void SaveGame(int slot)
	{
		IGameSessionQuery query = _ports.Query;
		SaveData data = new SaveData
		{
			SlotIndex = slot,
			FactionId = query.FactionId,
			Gold = query.Gold,
			CurrentNightIndex = query.CurrentNightIndex,
			PendingReplayReportId = query.PendingReplayReportId,
			RitualStudyCompletionCount = query.RitualStudyCompletionCount,
			DungeonKingXp = query.DungeonKingXp,
		};
		data.LearnedRitualIds.AddRange(query.LearnedRitualIds);
		data.DungeonLearnedSkillIds.AddRange(query.DungeonLearnedSkillIds);
		data.DungeonLearnedSkillLevels.AddRange(query.DungeonLearnedSkillLevels);
		foreach (IBuildingState building in query.PlayerBuildingStates)
			data.Buildings.Add(SaveDataMapper.ToBuildingEntry(building));
		foreach (CardData card in query.HandCards)
			data.HandCards.Add(SaveDataMapper.ToCardEntry(card));
		data.BattleReportJson = ExportBattleReportJson();
		_saveRepository.Save(data);
	}

	public SaveData LoadSaveData(int slot)
	{
		return _saveRepository.Load(slot);
	}

	public bool HasSave(int slot)
	{
		return _saveRepository.HasSave(slot);
	}

	public void DeleteSave(int slot)
	{
		_saveRepository.DeleteSave(slot);
	}

	public void RestoreSession(SaveData data)
	{
		IGameSessionCommand command = _ports.Command;
		command.ClearNonCoreBuildings();

		foreach (BuildingSaveEntry entry in data.Buildings)
		{
			if (entry.TypeId == BuildingTypeIds.CastleHeart || _buildingRepository.IsCoreBuilding(entry.TypeId))
				continue;
			command.PlaceRestoredBuilding(entry.TypeId, entry.AnchorGridX, entry.AnchorGridY, entry.Health);
		}

		foreach (BuildingSaveEntry entry in data.Buildings)
		{
			if (entry.TypeId == BuildingTypeIds.CastleHeart)
				continue;
			if (_buildingRepository.IsCoreBuilding(entry.TypeId) && command.RestoreCoreBuilding(entry.TypeId, entry.Health))
				break;
		}

		command.SetGold(data.Gold);
		command.ResetHand();
		foreach (CardSaveEntry card in data.HandCards)
			command.AddRestoredCard(SaveDataMapper.ToCardData(card, _catalog));
		command.SetNightIndex(data.CurrentNightIndex);
		if (!string.IsNullOrEmpty(data.PendingReplayReportId))
			command.SetPendingReplayReportId(data.PendingReplayReportId);
		command.RestoreRitualState(data.RitualStudyCompletionCount, data.LearnedRitualIds);
		command.RestoreKingXp(data.DungeonKingXp);
		command.RestoreKingSkills(data.DungeonLearnedSkillIds, data.DungeonLearnedSkillLevels);
		_ports.Shop.SetFaction(data.FactionId);
		command.RefreshPlayerAdjacency();
		ImportBattleReportJson(data.BattleReportJson);
	}

	private string ExportBattleReportJson()
	{
		IReadOnlyList<CastleSnapshot> snapshots = _ports.BattleReportCapture.ExportCurrentSnapshots();
		if (snapshots.Count == 0)
			return "";
		return JsonSerializer.Serialize(snapshots);
	}

	private void ImportBattleReportJson(string json)
	{
		if (string.IsNullOrWhiteSpace(json))
			return;
		try
		{
			List<CastleSnapshot> snapshots = JsonSerializer.Deserialize<List<CastleSnapshot>>(json);
			if (snapshots != null && snapshots.Count > 0)
				_ports.BattleReportCapture.ImportCurrentSnapshots(snapshots);
		}
		catch (JsonException)
		{
		}
	}
}
