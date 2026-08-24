using System;
using System.IO;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using Xunit;

namespace CasualCastle.Persistence.Tests;

public class RealBugMotleyCrewConfigTests
{
	private const string SkillId = "motley_crew";
	private const float ExpectedDodgeChanceMultiplier = 1.0f;

	[Fact]
	public void MotleyCrewSkill_DodgeChanceConfiguredAsOne_RealBug_DodgeNeverAppliesWhenZero()
	{
		string configJson = ReadConfigJson(SkillId);

		using JsonDocument doc = JsonDocument.Parse(configJson);
		JsonElement modifiers = doc.RootElement.GetProperty("modifiers");
		float dodgeChance = modifiers.GetProperty("dodge_chance").GetSingle();

		Assert.Equal(ExpectedDodgeChanceMultiplier, dodgeChance);
	}

	private static string ReadConfigJson(string skillId)
	{
		string dbPath = FindConfigDb();
		using SqliteConnection connection = new SqliteConnection($"Data Source={dbPath}");
		connection.Open();
		using SqliteCommand command = connection.CreateCommand();
		command.CommandText = "select config_json from skill_defs where id = $id";
		command.Parameters.AddWithValue("$id", skillId);
		object result = command.ExecuteScalar();
		Assert.NotNull(result);
		return (string)result;
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
