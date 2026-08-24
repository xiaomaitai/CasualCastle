using System;
using System.IO;
using Microsoft.Data.Sqlite;
using Xunit;

namespace CasualCastle.Persistence.Tests;

public class RealBugNecromancerBalanceConfigTests
{
	[Fact]
	public void GameBalance_HasBlackMagicYields_RealBug_HardcodedValues()
	{
		string db = FindConfigDb();
		using SqliteConnection connection = new SqliteConnection($"Data Source={db}");
		connection.Open();
		using SqliteCommand command = connection.CreateCommand();
		command.CommandText = "select config_key, config_value from game_balance where config_key in ('friendly_death_yield', 'enemy_death_yield')";
		using SqliteDataReader reader = command.ExecuteReader();
		int found = 0;
		while (reader.Read())
		{
			found++;
			string key = reader.GetString(0);
			int value = reader.GetInt32(1);
			if (key == "friendly_death_yield")
				Assert.Equal(25, value);
			if (key == "enemy_death_yield")
				Assert.Equal(10, value);
		}
		Assert.Equal(2, found);
	}

	private static string FindConfigDb()
	{
		DirectoryInfo dir = new DirectoryInfo(AppContext.BaseDirectory);
		while (dir != null)
		{
			string candidate = Path.Combine(dir.FullName, "assets", "data", "config.db");
			if (File.Exists(candidate))
				return candidate;
			dir = dir.Parent;
		}
		throw new FileNotFoundException("assets/data/config.db 未找到");
	}
}
