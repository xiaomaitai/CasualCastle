using System;
using System.IO;
using Microsoft.Data.Sqlite;
using Xunit;

namespace CasualCastle.Persistence.Tests;

public class RealBugTextCorruptionTests
{
	[Fact]
	public void TextColumns_NoQuestionMarkGarbage_RealBug_MojibakeNames()
	{
		string db = FindConfigDb();
		using SqliteConnection connection = new SqliteConnection($"Data Source={db}");
		connection.Open();
		using SqliteCommand command = connection.CreateCommand();
		command.CommandText = @"
			select count(*) from (
				select display_name as text from building_defs
				union all
				select display_name as text from skill_defs
				union all
				select description as text from skill_defs
				union all
				select name as text from shop_catalog
				union all
				select name as text from item_catalog
				union all
				select display_name as text from tech_tree_nodes
			) where text like '%?%'";
		object corrupted = command.ExecuteScalar();
		Assert.Equal(0L, Convert.ToInt64(corrupted));
	}

	[Fact]
	public void NecromancerBuildings_ProperChineseNames_RealBug_MojibakeNames()
	{
		string db = FindConfigDb();
		using SqliteConnection connection = new SqliteConnection($"Data Source={db}");
		connection.Open();
		using SqliteCommand command = connection.CreateCommand();
		command.CommandText = @"
			select type_id || '|' || display_name from building_defs
			where type_id in ('BloodAltar', 'NecromancySanctum', 'DeathGate')";
		using SqliteDataReader reader = command.ExecuteReader();
		int count = 0;
		while (reader.Read())
		{
			count++;
			Assert.DoesNotContain("?", reader.GetString(0));
		}
		Assert.Equal(3, count);
	}

	[Fact]
	public void UndeadAuraSkills_ProperNames_RealBug_MojibakeNames()
	{
		string db = FindConfigDb();
		using SqliteConnection connection = new SqliteConnection($"Data Source={db}");
		connection.Open();
		using SqliteCommand command = connection.CreateCommand();
		command.CommandText = @"
			select id || '|' || display_name from skill_defs
			where id in ('lifesteal_aura', 'anti_magic_shield', 'stability_aura', 'slaanesh_grace')";
		using SqliteDataReader reader = command.ExecuteReader();
		int count = 0;
		while (reader.Read())
		{
			count++;
			Assert.DoesNotContain("?", reader.GetString(0));
		}
		Assert.Equal(4, count);
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
