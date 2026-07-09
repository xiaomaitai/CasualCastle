using CasualCastle.Domain.Shared;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;

using GodotProjectSettings = Godot.ProjectSettings;

namespace CasualCastle.Adapters.Persistence;

public class SaveStorage : ISaveRepository
{
    private static readonly string SaveDir = GodotProjectSettings.GlobalizePath("user://saves");

    public SaveStorage()
    {
        Directory.CreateDirectory(SaveDir);
    }

    private static string SlotPath(int slot) => Path.Combine(SaveDir, $"save_{slot}.db");

    public void Save(SaveData data)
    {
        string path = SlotPath(data.SlotIndex);
        if (File.Exists(path))
            File.Delete(path);

        using SqliteConnection conn = new($"Data Source={path}");
        conn.Open();
        using SqliteCommand cmd = conn.CreateCommand();

        cmd.CommandText = @"
            CREATE TABLE save_meta (
                key TEXT PRIMARY KEY,
                value TEXT NOT NULL
            )";
        cmd.ExecuteNonQuery();

        cmd.CommandText = @"
            CREATE TABLE buildings (
                type_id TEXT NOT NULL,
                anchor_grid_x INTEGER NOT NULL,
                anchor_grid_y INTEGER NOT NULL,
                health INTEGER NOT NULL
            )";
        cmd.ExecuteNonQuery();

        cmd.CommandText = @"
            CREATE TABLE hand_cards (
                id TEXT NOT NULL,
                name TEXT NOT NULL,
                cost INTEGER NOT NULL,
                building_type TEXT NOT NULL,
                weight INTEGER NOT NULL
            )";
        cmd.ExecuteNonQuery();

        SetMeta(cmd, "gold", data.Gold.ToString());
        SetMeta(cmd, "current_night_index", data.CurrentNightIndex.ToString());
        SetMeta(cmd, "pending_replay_report_id", data.PendingReplayReportId ?? "");
        SetMeta(cmd, "save_time", data.SaveTime ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

        foreach (BuildingSaveEntry b in data.Buildings)
        {
            cmd.CommandText = "INSERT INTO buildings VALUES (@p0, @p1, @p2, @p3)";
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@p0", b.TypeId);
            cmd.Parameters.AddWithValue("@p1", b.AnchorGridX);
            cmd.Parameters.AddWithValue("@p2", b.AnchorGridY);
            cmd.Parameters.AddWithValue("@p3", b.Health);
            cmd.ExecuteNonQuery();
        }

        foreach (CardSaveEntry c in data.HandCards)
        {
            cmd.CommandText = "INSERT INTO hand_cards VALUES (@p0, @p1, @p2, @p3, @p4)";
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@p0", c.Id);
            cmd.Parameters.AddWithValue("@p1", c.Name);
            cmd.Parameters.AddWithValue("@p2", c.Cost);
            cmd.Parameters.AddWithValue("@p3", c.BuildingType);
            cmd.Parameters.AddWithValue("@p4", c.Weight);
            cmd.ExecuteNonQuery();
        }
    }

    public SaveData Load(int slotIndex)
    {
        string path = SlotPath(slotIndex);
        if (!File.Exists(path))
            return null;

        using SqliteConnection conn = new($"Data Source={path}");
        conn.Open();
        using SqliteCommand cmd = conn.CreateCommand();

        SaveData data = new SaveData { SlotIndex = slotIndex };
        data.Gold = int.Parse(GetMeta(cmd, "gold", "0"));
        data.CurrentNightIndex = int.Parse(GetMeta(cmd, "current_night_index", "0"));
        data.PendingReplayReportId = GetMeta(cmd, "pending_replay_report_id", "");
        data.SaveTime = GetMeta(cmd, "save_time", "");

        cmd.CommandText = "SELECT type_id, anchor_grid_x, anchor_grid_y, health FROM buildings";
        using (SqliteDataReader reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                data.Buildings.Add(new BuildingSaveEntry
                {
                    TypeId = reader.GetString(0),
                    AnchorGridX = reader.GetInt32(1),
                    AnchorGridY = reader.GetInt32(2),
                    Health = reader.GetInt32(3),
                });
            }
        }

        cmd.CommandText = "SELECT id, name, cost, building_type, weight FROM hand_cards";
        using (SqliteDataReader reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                data.HandCards.Add(new CardSaveEntry
                {
                    Id = reader.GetString(0),
                    Name = reader.GetString(1),
                    Cost = reader.GetInt32(2),
                    BuildingType = reader.GetString(3),
                    Weight = reader.GetInt32(4),
                });
            }
        }

        return data;
    }

    public bool HasSave(int slotIndex) => File.Exists(SlotPath(slotIndex));

    public void DeleteSave(int slotIndex)
    {
        string path = SlotPath(slotIndex);
        if (File.Exists(path))
            File.Delete(path);
    }

    public List<int> ListSlots()
    {
        List<int> slots = new();
        for (int i = 0; i < 5; i++)
        {
            if (HasSave(i))
                slots.Add(i);
        }
        return slots;
    }

    private static void SetMeta(SqliteCommand cmd, string key, string value)
    {
        cmd.CommandText = "INSERT INTO save_meta VALUES (@p0, @p1)";
        cmd.Parameters.Clear();
        cmd.Parameters.AddWithValue("@p0", key);
        cmd.Parameters.AddWithValue("@p1", value);
        cmd.ExecuteNonQuery();
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
