using System;
using System.IO;
using Microsoft.Data.Sqlite;
using Xunit;

namespace CasualCastle.Persistence.Tests;

public class RealBugHighTierUnitSkillsTests
{
	[Theory]
	[InlineData("King", "king_aura")]
	[InlineData("King", "king_summon")]
	[InlineData("HolyShieldGuardian", "holy_guardian_aura")]
	[InlineData("RoyalKnight", "royal_knight_charge")]
	[InlineData("ShadowLord", "shadow_lord_stealth")]
	[InlineData("ShadowLord", "backstab")]
	[InlineData("RoyalScout", "royal_scout_stealth")]
	[InlineData("DragonRiderCommander", "dragon_rider_kiting")]
	[InlineData("RoyalSharpshooter", "slowing_strike")]
	[InlineData("DeathAngel", "death_angel_rage")]
	public void HighTierUnit_HasConfiguredSkill(string unitTypeId, string skillId)
	{
		string db = FindConfigDb();
		using SqliteConnection connection = new SqliteConnection($"Data Source={db}");
		connection.Open();
		using SqliteCommand command = connection.CreateCommand();
		command.CommandText = "select count(*) from unit_skills where unit_type_id = @u and skill_id = @s";
		command.Parameters.AddWithValue("@u", unitTypeId);
		command.Parameters.AddWithValue("@s", skillId);
		object result = command.ExecuteScalar();
		Assert.Equal(1L, Convert.ToInt64(result));
	}

	[Theory]
	[InlineData("king_aura")]
	[InlineData("king_summon")]
	[InlineData("holy_guardian_aura")]
	[InlineData("royal_knight_charge")]
	[InlineData("shadow_lord_stealth")]
	[InlineData("royal_scout_stealth")]
	[InlineData("dragon_rider_kiting")]
	[InlineData("death_angel_rage")]
	public void NewSkill_ExistsWithValidJson(string skillId)
	{
		string db = FindConfigDb();
		using SqliteConnection connection = new SqliteConnection($"Data Source={db}");
		connection.Open();
		using SqliteCommand command = connection.CreateCommand();
		command.CommandText = "select json_valid(config_json) from skill_defs where id = @id";
		command.Parameters.AddWithValue("@id", skillId);
		object result = command.ExecuteScalar();
		Assert.Equal(1L, Convert.ToInt64(result));
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
