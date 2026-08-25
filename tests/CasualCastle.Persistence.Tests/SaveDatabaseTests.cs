using CasualCastle.Adapters.Persistence;
using CasualCastle.Domain.Shared;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace CasualCastle.Persistence.Tests;

public class SaveDatabaseTests : IDisposable
{
	private readonly string _dir;
	private readonly SaveDatabase _database;

	public SaveDatabaseTests()
	{
		_dir = Path.Combine(Path.GetTempPath(), "cc_save_tests_" + Guid.NewGuid().ToString("N"));
		_database = new SaveDatabase(_dir);
	}

	public void Dispose()
	{
		if (Directory.Exists(_dir))
			Directory.Delete(_dir, true);
	}

	[Fact]
	public void RoundTripPersistsEveryField()
	{
		SaveData source = NewFullSaveData(2);

		_database.Save(source);

		SaveData loaded = _database.Load(2);
		Assert.NotNull(loaded);
		AssertSaveDataEqual(source, loaded);
	}

	[Fact]
	public void RuntimeModifiedCardAttributesSurviveRoundTrip()
	{
		SaveData source = NewFullSaveData(0);
		source.HandCards[0].Attributes["charge"] = "7";

		_database.Save(source);

		SaveData loaded = _database.Load(0);
		Assert.NotNull(loaded);
		Assert.Equal("7", loaded.HandCards[0].Attributes["charge"]);
		AssertSaveDataEqual(source, loaded);
	}

	[Fact]
	public void LegacySaveWithoutNewColumnsLoadsWithDefaults()
	{
		CreateLegacySaveFile(1);

		SaveData loaded = _database.Load(1);
		Assert.NotNull(loaded);
		Assert.Equal(FactionIds.Alliance, loaded.FactionId);
		Assert.Equal(123, loaded.Gold);
		Assert.Equal(2, loaded.CurrentNightIndex);
		Assert.Equal("", loaded.SaveTime);
		Assert.Equal(0, loaded.RitualStudyCompletionCount);
		Assert.Empty(loaded.LearnedRitualIds);
		Assert.Equal(0, loaded.DungeonKingXp);
		Assert.Equal(new List<string> { "skill_a" }, loaded.DungeonLearnedSkillIds);
		Assert.Equal(new List<int> { 1 }, loaded.DungeonLearnedSkillLevels);
		Assert.Equal("", loaded.BattleReportJson);
		BuildingSaveEntry building = Assert.Single(loaded.Buildings);
		Assert.Equal("barracks", building.TypeId);
		Assert.Equal(4, building.AnchorGridX);
		Assert.Equal(6, building.AnchorGridY);
		Assert.Equal(55, building.Health);
		CardSaveEntry card = Assert.Single(loaded.HandCards);
		Assert.Equal("legacy_card", card.Id);
		Assert.False(card.IsPlaceable);
		Assert.Equal("", card.ItemType);
		Assert.Empty(card.Attributes);
	}

	[Fact]
	public void SaveOverwritesExistingFile()
	{
		SaveData first = NewFullSaveData(0);
		first.Gold = 10;
		_database.Save(first);

		SaveData second = NewFullSaveData(0);
		second.Gold = 20;
		second.Buildings.Clear();
		_database.Save(second);

		SaveData loaded = _database.Load(0);
		Assert.NotNull(loaded);
		Assert.Equal(20, loaded.Gold);
		Assert.Empty(loaded.Buildings);
	}

	[Fact]
	public void SlotManagementWorks()
	{
		Assert.False(_database.HasSave(0));
		Assert.Empty(_database.ListSlots());

		_database.Save(NewFullSaveData(0));
		Assert.True(_database.HasSave(0));
		Assert.Equal(new List<int> { 0 }, _database.ListSlots());

		_database.DeleteSave(0);
		Assert.False(_database.HasSave(0));
		Assert.Empty(_database.ListSlots());
	}

	private static void AssertSaveDataEqual(SaveData expected, SaveData actual)
	{
		Assert.Equal(expected.FactionId, actual.FactionId);
		Assert.Equal(expected.Gold, actual.Gold);
		Assert.Equal(expected.CurrentNightIndex, actual.CurrentNightIndex);
		Assert.Equal(expected.PendingReplayReportId, actual.PendingReplayReportId);
		Assert.Equal(expected.BattleReportJson, actual.BattleReportJson);
		Assert.Equal(expected.SaveTime, actual.SaveTime);
		Assert.Equal(expected.RitualStudyCompletionCount, actual.RitualStudyCompletionCount);
		Assert.Equal(expected.LearnedRitualIds, actual.LearnedRitualIds);
		Assert.Equal(expected.DungeonKingXp, actual.DungeonKingXp);
		Assert.Equal(expected.DungeonLearnedSkillIds, actual.DungeonLearnedSkillIds);
		Assert.Equal(expected.DungeonLearnedSkillLevels, actual.DungeonLearnedSkillLevels);
		AssertBuildingsEqual(expected.Buildings, actual.Buildings);
		AssertHandCardsEqual(expected.HandCards, actual.HandCards);
	}

	private static void AssertBuildingsEqual(List<BuildingSaveEntry> expected, List<BuildingSaveEntry> actual)
	{
		Assert.Equal(expected.Count, actual.Count);
		for (int i = 0; i < expected.Count; i++)
		{
			BuildingSaveEntry e = expected[i];
			BuildingSaveEntry a = actual[i];
			Assert.Equal(e.TypeId, a.TypeId);
			Assert.Equal(e.AnchorGridX, a.AnchorGridX);
			Assert.Equal(e.AnchorGridY, a.AnchorGridY);
			Assert.Equal(e.Health, a.Health);
		}
	}

	private static void AssertHandCardsEqual(List<CardSaveEntry> expected, List<CardSaveEntry> actual)
	{
		Assert.Equal(expected.Count, actual.Count);
		for (int i = 0; i < expected.Count; i++)
		{
			CardSaveEntry e = expected[i];
			CardSaveEntry a = actual[i];
			Assert.Equal(e.Id, a.Id);
			Assert.Equal(e.Name, a.Name);
			Assert.Equal(e.Cost, a.Cost);
			Assert.Equal(e.BuildingType, a.BuildingType);
			Assert.Equal(e.Weight, a.Weight);
			Assert.Equal(e.IsPlaceable, a.IsPlaceable);
			Assert.Equal(e.ItemType, a.ItemType);
			Assert.Equal(e.Attributes, a.Attributes);
		}
	}

	private SaveData NewFullSaveData(int slot)
	{
		return new SaveData
		{
			SlotIndex = slot,
			FactionId = FactionIds.Dungeon,
			Gold = 456,
			CurrentNightIndex = 3,
			PendingReplayReportId = "report_1",
			BattleReportJson = "[{\"NightIndex\":1}]",
			SaveTime = "2099-01-02 03:04:05",
			RitualStudyCompletionCount = 2,
			LearnedRitualIds = new List<string> { "ritual_dark", "ritual_fire" },
			DungeonKingXp = 789,
			DungeonLearnedSkillIds = new List<string> { "skill_a", "skill_b" },
			DungeonLearnedSkillLevels = new List<int> { 1, 3 },
			Buildings = NewBuildingEntries(),
			HandCards = NewHandCardEntries(),
		};
	}

	private static List<BuildingSaveEntry> NewBuildingEntries()
	{
		return new List<BuildingSaveEntry>
		{
			new BuildingSaveEntry { TypeId = "barracks", AnchorGridX = 2, AnchorGridY = 5, Health = 80 },
			new BuildingSaveEntry { TypeId = "altar", AnchorGridX = 0, AnchorGridY = 1, Health = 40 },
		};
	}

	private static List<CardSaveEntry> NewHandCardEntries()
	{
		return new List<CardSaveEntry>
		{
			new CardSaveEntry
			{
				Id = "unit_card",
				Name = "弓手",
				Cost = 3,
				BuildingType = "barracks",
				Weight = 1,
				IsPlaceable = true,
				ItemType = "",
				Attributes = new Dictionary<string, string>(),
			},
			new CardSaveEntry
			{
				Id = "tome",
				Name = "魔法书",
				Cost = 5,
				BuildingType = "",
				Weight = 2,
				IsPlaceable = false,
				ItemType = "dark_book",
				Attributes = new Dictionary<string, string> { { "ritual", "ritual_fire" }, { "page", "12" } },
			},
		};
	}

	private void CreateLegacySaveFile(int slot)
	{
		string path = Path.Combine(_dir, $"save_{slot}.db");
		using SqliteConnection conn = new($"Data Source={path};Pooling=False");
		conn.Open();
		using SqliteCommand cmd = conn.CreateCommand();

		cmd.CommandText = "CREATE TABLE save_meta (key TEXT PRIMARY KEY, value TEXT NOT NULL)";
		cmd.ExecuteNonQuery();
		cmd.CommandText = "CREATE TABLE buildings (type_id TEXT NOT NULL, anchor_grid_x INTEGER NOT NULL, anchor_grid_y INTEGER NOT NULL, health INTEGER NOT NULL)";
		cmd.ExecuteNonQuery();
		cmd.CommandText = "CREATE TABLE hand_cards (id TEXT NOT NULL, name TEXT NOT NULL, cost INTEGER NOT NULL, building_type TEXT NOT NULL, weight INTEGER NOT NULL)";
		cmd.ExecuteNonQuery();

		SetLegacyMeta(cmd, "faction_id", FactionIds.Alliance);
		SetLegacyMeta(cmd, "gold", "123");
		SetLegacyMeta(cmd, "current_night_index", "2");
		SetLegacyMeta(cmd, "dungeon_learned_skills", "skill_a");
		SetLegacyMeta(cmd, "dungeon_learned_skill_levels", "1");

		cmd.CommandText = "INSERT INTO buildings VALUES (@p0, @p1, @p2, @p3)";
		cmd.Parameters.AddWithValue("@p0", "barracks");
		cmd.Parameters.AddWithValue("@p1", 4);
		cmd.Parameters.AddWithValue("@p2", 6);
		cmd.Parameters.AddWithValue("@p3", 55);
		cmd.ExecuteNonQuery();

		cmd.Parameters.Clear();
		cmd.CommandText = "INSERT INTO hand_cards VALUES (@p0, @p1, @p2, @p3, @p4)";
		cmd.Parameters.AddWithValue("@p0", "legacy_card");
		cmd.Parameters.AddWithValue("@p1", "旧卡");
		cmd.Parameters.AddWithValue("@p2", 2);
		cmd.Parameters.AddWithValue("@p3", "barracks");
		cmd.Parameters.AddWithValue("@p4", 1);
		cmd.ExecuteNonQuery();
	}

	private static void SetLegacyMeta(SqliteCommand cmd, string key, string value)
	{
		cmd.CommandText = "INSERT INTO save_meta VALUES (@k, @v)";
		cmd.Parameters.Clear();
		cmd.Parameters.AddWithValue("@k", key);
		cmd.Parameters.AddWithValue("@v", value);
		cmd.ExecuteNonQuery();
	}
}
