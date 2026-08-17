// 存档数据库：真实 SQLite 存档文件的读写核心
using CasualCastle.Domain.Shared;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace CasualCastle.Adapters.Persistence;

public class SaveDatabase
{
	private const int SaveSlotCount = 5;

	private readonly string _saveDir;

	public SaveDatabase(string saveDir)
	{
		_saveDir = saveDir;
		Directory.CreateDirectory(_saveDir);
	}

	public string SlotPath(int slotIndex)
	{
		return Path.Combine(_saveDir, $"save_{slotIndex}.db");
	}

	public void Save(SaveData data)
	{
		string path = SlotPath(data.SlotIndex);
		if (File.Exists(path))
			File.Delete(path);

		using SqliteConnection conn = OpenConnection(path);
		using SqliteCommand cmd = conn.CreateCommand();

		CreateTables(cmd);
		WriteMeta(cmd, data);
		WriteBuildings(cmd, data.Buildings);
		WriteHandCards(cmd, data.HandCards);
	}

	public SaveData Load(int slotIndex)
	{
		string path = SlotPath(slotIndex);
		if (!File.Exists(path))
			return null;

		using SqliteConnection conn = OpenConnection(path);
		using SqliteCommand cmd = conn.CreateCommand();

		SaveData data = ReadMeta(cmd, slotIndex);
		EnsureHandCardColumns(conn);
		ReadBuildings(cmd, data.Buildings);
		ReadHandCards(cmd, data.HandCards);
		return data;
	}

	public bool HasSave(int slotIndex)
	{
		return File.Exists(SlotPath(slotIndex));
	}

	public void DeleteSave(int slotIndex)
	{
		string path = SlotPath(slotIndex);
		if (File.Exists(path))
			File.Delete(path);
	}

	public List<int> ListSlots()
	{
		List<int> slots = new();
		for (int i = 0; i < SaveSlotCount; i++)
		{
			if (HasSave(i))
				slots.Add(i);
		}
		return slots;
	}

	private static void CreateTables(SqliteCommand cmd)
	{
		Execute(cmd, @"
			CREATE TABLE save_meta (
				key TEXT PRIMARY KEY,
				value TEXT NOT NULL
			)");
		Execute(cmd, @"
			CREATE TABLE buildings (
				type_id TEXT NOT NULL,
				anchor_grid_x INTEGER NOT NULL,
				anchor_grid_y INTEGER NOT NULL,
				health INTEGER NOT NULL
			)");
		Execute(cmd, @"
			CREATE TABLE hand_cards (
				id TEXT NOT NULL,
				name TEXT NOT NULL,
				cost INTEGER NOT NULL,
				building_type TEXT NOT NULL,
				weight INTEGER NOT NULL,
				is_placeable INTEGER NOT NULL,
				item_type TEXT NOT NULL,
				attributes TEXT NOT NULL
			)");
	}

	private static void WriteMeta(SqliteCommand cmd, SaveData data)
	{
		SetMeta(cmd, "faction_id", data.FactionId);
		SetMeta(cmd, "gold", data.Gold.ToString());
		SetMeta(cmd, "current_night_index", data.CurrentNightIndex.ToString());
		SetMeta(cmd, "pending_replay_report_id", data.PendingReplayReportId);
		SetMeta(cmd, "save_time", ResolveSaveTime(data.SaveTime));
		SetMeta(cmd, "ritual_study_completion_count", data.RitualStudyCompletionCount.ToString());
		SetMeta(cmd, "learned_rituals", string.Join(",", data.LearnedRitualIds));
		SetMeta(cmd, "dungeon_king_xp", data.DungeonKingXp.ToString());
		SetMeta(cmd, "dungeon_learned_skills", string.Join(",", data.DungeonLearnedSkillIds));
		SetMeta(cmd, "dungeon_learned_skill_levels", string.Join(",", data.DungeonLearnedSkillLevels));
	}

	private static void WriteBuildings(SqliteCommand cmd, List<BuildingSaveEntry> buildings)
	{
		foreach (BuildingSaveEntry b in buildings)
		{
			Execute(cmd, "INSERT INTO buildings (type_id, anchor_grid_x, anchor_grid_y, health) VALUES (@p0, @p1, @p2, @p3)",
				("@p0", b.TypeId), ("@p1", b.AnchorGridX), ("@p2", b.AnchorGridY), ("@p3", b.Health));
		}
	}

	private static void WriteHandCards(SqliteCommand cmd, List<CardSaveEntry> cards)
	{
		foreach (CardSaveEntry c in cards)
		{
			Execute(cmd, "INSERT INTO hand_cards (id, name, cost, building_type, weight, is_placeable, item_type, attributes) VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7)",
				("@p0", c.Id), ("@p1", c.Name), ("@p2", c.Cost), ("@p3", c.BuildingType), ("@p4", c.Weight),
				("@p5", c.IsPlaceable ? 1 : 0), ("@p6", c.ItemType), ("@p7", SerializeAttributes(c.Attributes)));
		}
	}

	private static SaveData ReadMeta(SqliteCommand cmd, int slotIndex)
	{
		SaveData data = new SaveData { SlotIndex = slotIndex };
		data.FactionId = GetMeta(cmd, "faction_id", FactionIds.Alliance);
		data.Gold = int.Parse(GetMeta(cmd, "gold", "0"));
		data.CurrentNightIndex = int.Parse(GetMeta(cmd, "current_night_index", "0"));
		data.PendingReplayReportId = GetMeta(cmd, "pending_replay_report_id", "");
		data.SaveTime = GetMeta(cmd, "save_time", "");
		data.RitualStudyCompletionCount = int.Parse(GetMeta(cmd, "ritual_study_completion_count", "0"));
		AddSplit(data.LearnedRitualIds, GetMeta(cmd, "learned_rituals", ""));
		data.DungeonKingXp = int.Parse(GetMeta(cmd, "dungeon_king_xp", "0"));
		AddSplit(data.DungeonLearnedSkillIds, GetMeta(cmd, "dungeon_learned_skills", ""));
		AddSplitLevels(data.DungeonLearnedSkillLevels, GetMeta(cmd, "dungeon_learned_skill_levels", ""));
		return data;
	}

	private static void ReadBuildings(SqliteCommand cmd, List<BuildingSaveEntry> buildings)
	{
		cmd.CommandText = "SELECT type_id, anchor_grid_x, anchor_grid_y, health FROM buildings";
		using (SqliteDataReader reader = cmd.ExecuteReader())
		{
			while (reader.Read())
			{
				buildings.Add(new BuildingSaveEntry
				{
					TypeId = reader.GetString(0),
					AnchorGridX = reader.GetInt32(1),
					AnchorGridY = reader.GetInt32(2),
					Health = reader.GetInt32(3),
				});
			}
		}
	}

	private static void ReadHandCards(SqliteCommand cmd, List<CardSaveEntry> cards)
	{
		cmd.CommandText = "SELECT id, name, cost, building_type, weight, is_placeable, item_type, attributes FROM hand_cards";
		using (SqliteDataReader reader = cmd.ExecuteReader())
		{
			while (reader.Read())
			{
				cards.Add(new CardSaveEntry
				{
					Id = reader.GetString(0),
					Name = reader.GetString(1),
					Cost = reader.GetInt32(2),
					BuildingType = reader.GetString(3),
					Weight = reader.GetInt32(4),
					IsPlaceable = reader.GetInt32(5) == 1,
					ItemType = reader.GetString(6),
					Attributes = ParseAttributes(reader.GetString(7)),
				});
			}
		}
	}

	private static void EnsureHandCardColumns(SqliteConnection conn)
	{
		HashSet<string> columns = ReadHandCardColumns(conn);
		if (!columns.Contains("is_placeable"))
			RunMigration(conn, "ALTER TABLE hand_cards ADD COLUMN is_placeable INTEGER NOT NULL DEFAULT 0");
		if (!columns.Contains("item_type"))
			RunMigration(conn, "ALTER TABLE hand_cards ADD COLUMN item_type TEXT NOT NULL DEFAULT ''");
		if (!columns.Contains("attributes"))
			RunMigration(conn, "ALTER TABLE hand_cards ADD COLUMN attributes TEXT NOT NULL DEFAULT ''");
	}

	private static HashSet<string> ReadHandCardColumns(SqliteConnection conn)
	{
		HashSet<string> columns = new();
		using SqliteCommand cmd = conn.CreateCommand();
		cmd.CommandText = "PRAGMA table_info(hand_cards)";
		using SqliteDataReader reader = cmd.ExecuteReader();
		while (reader.Read())
			columns.Add(reader.GetString(1));
		return columns;
	}

	private static void RunMigration(SqliteConnection conn, string sql)
	{
		using SqliteCommand cmd = conn.CreateCommand();
		cmd.CommandText = sql;
		cmd.ExecuteNonQuery();
	}

	private static SqliteConnection OpenConnection(string path)
	{
		SqliteConnection conn = new($"Data Source={path};Pooling=False");
		conn.Open();
		return conn;
	}

	private static string ResolveSaveTime(string saveTime)
	{
		if (string.IsNullOrEmpty(saveTime))
			return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
		return saveTime;
	}

	private static string SerializeAttributes(Dictionary<string, string> attributes)
	{
		return JsonSerializer.Serialize(attributes);
	}

	private static Dictionary<string, string> ParseAttributes(string json)
	{
		if (string.IsNullOrEmpty(json))
			return new Dictionary<string, string>();
		try
		{
			Dictionary<string, string> attributes = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
			return attributes ?? new Dictionary<string, string>();
		}
		catch (JsonException)
		{
			return new Dictionary<string, string>();
		}
	}

	private static void AddSplit(List<string> target, string joined)
	{
		if (!string.IsNullOrEmpty(joined))
			target.AddRange(joined.Split(","));
	}

	private static void AddSplitLevels(List<int> target, string joined)
	{
		if (string.IsNullOrEmpty(joined))
			return;
		foreach (string part in joined.Split(","))
		{
			if (int.TryParse(part, out int level))
				target.Add(level);
		}
	}

	private static void Execute(SqliteCommand cmd, string sql)
	{
		cmd.CommandText = sql;
		cmd.Parameters.Clear();
		cmd.ExecuteNonQuery();
	}

	private static void Execute(SqliteCommand cmd, string sql, params (string Name, object Value)[] args)
	{
		cmd.CommandText = sql;
		cmd.Parameters.Clear();
		foreach ((string Name, object Value) arg in args)
			cmd.Parameters.AddWithValue(arg.Name, arg.Value);
		cmd.ExecuteNonQuery();
	}

	private static void SetMeta(SqliteCommand cmd, string key, string value)
	{
		Execute(cmd, "INSERT INTO save_meta VALUES (@p0, @p1)", ("@p0", key), ("@p1", value));
	}

	private static string GetMeta(SqliteCommand cmd, string key, string fallback)
	{
		cmd.CommandText = "SELECT value FROM save_meta WHERE key = @p0";
		cmd.Parameters.Clear();
		cmd.Parameters.AddWithValue("@p0", key);
		object result = cmd.ExecuteScalar();
		return result?.ToString() ?? fallback;
	}
}
