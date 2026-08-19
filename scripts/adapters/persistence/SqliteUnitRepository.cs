// 单位配置仓库：SQLite 读取单位属性
using System.Collections.Generic;
using CasualCastle.Domain.Battle;
using Microsoft.Data.Sqlite;

namespace CasualCastle.Adapters.Persistence;

public class SqliteUnitRepository : IUnitRepository
{
	private readonly Dictionary<string, UnitStats> _cache = new();

	public SqliteUnitRepository(string dbPath)
	{
		string fullPath = dbPath;
		using SqliteConnection connection = new($"Data Source={fullPath}");
		connection.Open();
		using SqliteCommand cmd = connection.CreateCommand();
		cmd.CommandText = "SELECT type_id, size, attack_type, damage_type, armor_type, health, damage, speed, attack_range, attack_cooldown, has_night_combat, vision_range, unit_color, unit_cost, race, armor_value, can_detect_stealth, heal_amount, heal_range, heal_interval FROM unit_stats";
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
				HasNightCombat = reader.GetInt32(10) != 0,
				VisionRange = reader.GetFloat(11),
				UnitCost = reader.GetInt32(13),
				Race = reader.IsDBNull(14) ? "" : reader.GetString(14),
				ArmorValue = reader.GetInt32(15),
				CanDetectStealth = reader.GetInt32(16) != 0,
				HealAmount = reader.GetFloat(17),
				HealRange = reader.GetFloat(18),
				HealInterval = reader.GetFloat(19),
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
