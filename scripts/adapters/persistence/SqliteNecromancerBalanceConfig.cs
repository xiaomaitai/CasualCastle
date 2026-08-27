// 死灵法师平衡配置仓库：SQLite 读取黑魔法产出数值
using CasualCastle.Domain.Battle;
using Microsoft.Data.Sqlite;
using System;

namespace CasualCastle.Adapters.Persistence;

public class SqliteNecromancerBalanceConfig : INecromancerBalanceConfig
{
	private readonly int _friendlyDeathYield;
	private readonly int _enemyDeathYield;

	public SqliteNecromancerBalanceConfig(string dbPath)
	{
		_friendlyDeathYield = ReadInt(dbPath, "friendly_death_yield", BlackMagicService.DefaultFriendlyDeathYield);
		_enemyDeathYield = ReadInt(dbPath, "enemy_death_yield", BlackMagicService.DefaultEnemyDeathYield);
	}

	public int FriendlyDeathYield => _friendlyDeathYield;
	public int EnemyDeathYield => _enemyDeathYield;

	private static int ReadInt(string dbPath, string key, int fallback)
	{
		using SqliteConnection connection = new($"Data Source={dbPath}");
		connection.Open();
		using SqliteCommand cmd = connection.CreateCommand();
		cmd.CommandText = $"SELECT config_value FROM {SqliteConfigRepository.BattleConfig} WHERE config_key = $key";
		cmd.Parameters.AddWithValue("$key", key);
		object result = cmd.ExecuteScalar();
		if (result == null || result is DBNull)
			return fallback;
		return Convert.ToInt32(result);
	}
}
