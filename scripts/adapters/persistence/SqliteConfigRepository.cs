// 业务配置访问器：按业务表读写 config_key/config_value 配置行
using Microsoft.Data.Sqlite;
using System;

namespace CasualCastle.Adapters.Persistence;

public class SqliteConfigRepository
{
	public const string SoundConfig = "sound_config";
	public const string BattleConfig = "battle_config";
	public const string EconomyConfig = "economy_config";
	public const string DisplayConfig = "display_config";

	private readonly string _dbPath;

	public SqliteConfigRepository(string dbPath)
	{
		_dbPath = dbPath;
	}

	public int ReadInt(string table, string key, int fallback)
	{
		object result = ReadValue(table, key);
		return result == null || result is DBNull ? fallback : Convert.ToInt32(result);
	}

	public float ReadFloat(string table, string key, float fallback)
	{
		object result = ReadValue(table, key);
		return result == null || result is DBNull ? fallback : Convert.ToSingle(result);
	}

	public void Write(string table, string key, double value)
	{
		using SqliteConnection connection = new($"Data Source={_dbPath}");
		connection.Open();
		using SqliteCommand cmd = connection.CreateCommand();
		cmd.CommandText = $"INSERT INTO {table}(config_key, config_value) VALUES($key, $value) ON CONFLICT(config_key) DO UPDATE SET config_value = $value";
		cmd.Parameters.AddWithValue("$key", key);
		cmd.Parameters.AddWithValue("$value", value);
		cmd.ExecuteNonQuery();
	}

	private object ReadValue(string table, string key)
	{
		using SqliteConnection connection = new($"Data Source={_dbPath}");
		connection.Open();
		using SqliteCommand cmd = connection.CreateCommand();
		cmd.CommandText = $"SELECT config_value FROM {table} WHERE config_key = $key";
		cmd.Parameters.AddWithValue("$key", key);
		return cmd.ExecuteScalar();
	}
}
