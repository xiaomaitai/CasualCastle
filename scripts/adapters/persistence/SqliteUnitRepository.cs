using System.Collections.Generic;
using CasualCastle.Domain.Battle;
using Microsoft.Data.Sqlite;
using GodotProjectSettings = Godot.ProjectSettings;

namespace CasualCastle.Adapters.Persistence;

public class SqliteUnitRepository : IUnitRepository
{
	private readonly Dictionary<string, UnitStats> _cache = new();

	public SqliteUnitRepository()
	{
		string fullPath = GodotProjectSettings.GlobalizePath("res://assets/data/config.db");
		using SqliteConnection connection = new($"Data Source={fullPath}");
		connection.Open();
		using SqliteCommand cmd = connection.CreateCommand();
		cmd.CommandText = "SELECT type_id, size, attack_type, damage_type, armor_type, health, damage, speed, attack_range, attack_cooldown, vision_range, has_night_combat, unit_color, unit_cost, race FROM unit_stats";
		using SqliteDataReader reader = cmd.ExecuteReader();
		while (reader.Read())
		{
			_cache[reader.GetString(0)] = new UnitStats
			{
				TypeId = reader.GetString(0),
				Size = (UnitSize)reader.GetInt32(1),
				AttackType = (AttackType)reader.GetInt32(2),
				DamageType = (DamageType)reader.GetInt32(3),
				ArmorType = (ArmorType)reader.GetInt32(4),
				Health = reader.GetInt32(5),
				Damage = reader.GetInt32(6),
				Speed = reader.GetFloat(7),
				AttackRange = reader.GetFloat(8),
				AttackCooldown = reader.GetFloat(9),
				VisionRange = reader.GetFloat(10),
				HasNightCombat = reader.GetInt32(11) != 0,
				UnitCost = reader.GetInt32(13),
				Race = reader.IsDBNull(14) ? "" : reader.GetString(14),
			};
		}
	}

	public UnitStats Get(string typeId)
	{
		if (_cache.TryGetValue(typeId, out UnitStats stats))
			return stats;
		throw new System.Collections.Generic.KeyNotFoundException($"Unit type '{typeId}' not found");
	}
}
