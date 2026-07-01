using System.Collections.Generic;
using CasualCastle.Domain.Building;
using CasualCastle.Domain.Shared;
using Microsoft.Data.Sqlite;
using GodotProjectSettings = Godot.ProjectSettings;

namespace CasualCastle.Adapters.Persistence;

public class SqliteBuildingRepository : IBuildingRepository
{
	private readonly Dictionary<string, BuildingData> _cache = new();

	public SqliteBuildingRepository()
	{
		string fullPath = GodotProjectSettings.GlobalizePath("res://assets/data/config.db");
		using SqliteConnection connection = new($"Data Source={fullPath}");
		connection.Open();
		using SqliteCommand cmd = connection.CreateCommand();
		cmd.CommandText = "SELECT type_id, display_name, max_health, spawn_interval, main_cell_x, main_cell_y, spawn_cell_x, spawn_cell_y, unit_type_id, has_night_combat, fusion_tier, is_core, footprint_json, collision_width, collision_height FROM building_defs";
		using SqliteDataReader reader = cmd.ExecuteReader();
		while (reader.Read())
		{
			string footprintJson = reader.GetString(12);
			List<GridCellOffset> offsets = ParseFootprint(footprintJson);

			_cache[reader.GetString(0)] = new BuildingData
			{
				TypeId = reader.GetString(0),
				DisplayName = reader.GetString(1),
				MaxHealth = reader.GetInt32(2),
				SpawnInterval = reader.IsDBNull(3) ? 0 : reader.GetFloat(3),
				MainCellOffset = new(reader.GetInt32(4), reader.GetInt32(5)),
				SpawnCellOffset = new(reader.GetInt32(6), reader.GetInt32(7)),
				UnitTypeId = reader.IsDBNull(8) ? null : reader.GetString(8),
				HasNightCombat = reader.GetInt32(9) != 0,
				FusionTier = reader.GetInt32(10),
				IsCore = reader.GetInt32(11) != 0,
				Footprint = offsets.ToArray(),
				CollisionWidth = reader.GetInt32(13),
				CollisionHeight = reader.GetInt32(14),
			};
		}
	}

	public BuildingData Get(string typeId)
	{
		if (_cache.TryGetValue(typeId, out BuildingData data))
			return data;
		throw new System.Collections.Generic.KeyNotFoundException($"Building type '{typeId}' not found");
	}

	private static List<GridCellOffset> ParseFootprint(string json)
	{
		List<GridCellOffset> offsets = new();
		string inner = json.Trim('[', ']');
		if (string.IsNullOrEmpty(inner))
			return offsets;
		string[] pairs = inner.Split("],[");
		foreach (string pair in pairs)
		{
			string[] values = pair.Trim('[', ']').Split(',');
			if (values.Length == 2 && int.TryParse(values[0], out int x) && int.TryParse(values[1], out int y))
				offsets.Add(new GridCellOffset(x, y));
		}
		return offsets;
	}
}
