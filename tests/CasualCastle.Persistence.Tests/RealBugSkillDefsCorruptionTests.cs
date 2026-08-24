using System;
using System.IO;
using Microsoft.Data.Sqlite;
using Xunit;

namespace CasualCastle.Persistence.Tests;

public class RealBugSkillDefsCorruptionTests
{
	[Fact]
	public void SkillDefs_AllConfigsValidJson_RealBug_BackslashCorruption()
	{
		string db = FindConfigDb();
		using SqliteConnection connection = new SqliteConnection($"Data Source={db}");
		connection.Open();
		using SqliteCommand command = connection.CreateCommand();
		command.CommandText = "select count(*) from skill_defs where json_valid(config_json) = 0";
		object invalid = command.ExecuteScalar();
		Assert.Equal(0L, Convert.ToInt64(invalid));
	}

	[Fact]
	public void SkillDefs_NoBackslashEscapedConfigs_RealBug_BackslashCorruption()
	{
		string db = FindConfigDb();
		using SqliteConnection connection = new SqliteConnection($"Data Source={db}");
		connection.Open();
		using SqliteCommand command = connection.CreateCommand();
		command.CommandText = "select count(*) from skill_defs where config_json like '%\\%'";
		object corrupted = command.ExecuteScalar();
		Assert.Equal(0L, Convert.ToInt64(corrupted));
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
