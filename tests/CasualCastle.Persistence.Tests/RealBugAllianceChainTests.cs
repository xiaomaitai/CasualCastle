using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;
using Xunit;

namespace CasualCastle.Persistence.Tests;

public class RealBugAllianceChainTests
{
	[Fact]
	public void AllianceChain_EveryOnTreeAdvancedBuildingReachable_RealBug_DeadBuildings()
	{
		HashSet<string> reachable = ReadReachableTypeIds();

		using SqliteConnection connection = OpenConnection();
		using SqliteCommand command = connection.CreateCommand();
		command.CommandText = "select b.type_id from building_defs b where b.faction_id = 'alliance' and b.combine_tier >= 1 and b.is_core = 0 and exists (select 1 from tech_tree_nodes t where t.type_id = b.type_id)";
		using SqliteDataReader reader = command.ExecuteReader();
		while (reader.Read())
		{
			string typeId = reader.GetString(0);
			Assert.True(reachable.Contains(typeId), typeId);
		}
	}

	[Fact]
	public void FactionInventory_DoesNotOverlapTechTree()
	{
		using SqliteConnection connection = OpenConnection();
		using SqliteCommand command = connection.CreateCommand();
		command.CommandText = "select count(*) from faction_inventory f where exists (select 1 from tech_tree_nodes t where t.faction_id = f.faction_id and t.type_id = f.type_id)";
		Assert.Equal(0, Convert.ToInt32(command.ExecuteScalar()));
	}

	[Fact]
	public void FactionInventory_BuildingsExistInBuildingDefs()
	{
		using SqliteConnection connection = OpenConnection();
		using SqliteCommand command = connection.CreateCommand();
		command.CommandText = "select count(*) from faction_inventory f where not exists (select 1 from building_defs b where b.type_id = f.type_id)";
		Assert.Equal(0, Convert.ToInt32(command.ExecuteScalar()));
	}

	[Fact]
	public void AllianceT4RecipeGuard_RealBug()
	{
		using SqliteConnection connection = OpenConnection();
		Assert.False(HasUpgrade(connection, "RoyalArcheryRange", "HeavenPunishmentTower"));
		Assert.True(HasCombine(connection, "RoyalArcheryRange", "RoyalStable", "HeavenPunishmentTower"));
		Assert.True(HasUpgrade(connection, "RoyalRanger", "ShadowSanctum"));
		Assert.False(HasCombine(connection, "RoyalRanger", "RoyalRanger", "ShadowSanctum"));
		Assert.False(HasCombine(connection, "RoyalBulwark", "RoyalArmory", "HolyWall"));
		Assert.True(HasUpgrade(connection, "RoyalBulwark", "HolyWall"));
		Assert.False(HasCombine(connection, "RoyalBulwark", "RoyalBulwark", "HolyWall"));
	}

	[Fact]
	public void RangerPost_ScoutLineT2Present_RealBug_MissingMiddleBuilding()
	{
		using SqliteConnection connection = OpenConnection();
		Assert.True(HasBuilding(connection, "RangerPost"));
		Assert.True(HasUpgrade(connection, "ScoutCamp", "RangerPost"));
		Assert.True(HasUpgrade(connection, "RangerPost", "RoyalRanger"));
	}

	private static HashSet<string> ReadReachableTypeIds()
	{
		HashSet<string> reachable = new();
		using SqliteConnection connection = OpenConnection();
		using (SqliteCommand command = connection.CreateCommand())
		{
			command.CommandText = "select result_type_id from upgrade_recipes where result_type_id in (select type_id from building_defs where faction_id = 'alliance') union select result_type_id from combine_recipes where result_type_id in (select type_id from building_defs where faction_id = 'alliance')";
			using SqliteDataReader reader = command.ExecuteReader();
			while (reader.Read())
				reachable.Add(reader.GetString(0));
		}
		using (SqliteCommand command = connection.CreateCommand())
		{
			command.CommandText = "select type_id from tech_tree_nodes where shop_available = 1 and faction_id = 'alliance'";
			using SqliteDataReader reader = command.ExecuteReader();
			while (reader.Read())
				reachable.Add(reader.GetString(0));
		}
		return reachable;
	}

	private static bool HasUpgrade(SqliteConnection connection, string sourceTypeId, string resultTypeId)
	{
		using SqliteCommand command = connection.CreateCommand();
		command.CommandText = "select count(*) from upgrade_recipes where source_type_id = $source and result_type_id = $result";
		command.Parameters.AddWithValue("$source", sourceTypeId);
		command.Parameters.AddWithValue("$result", resultTypeId);
		object count = command.ExecuteScalar();
		return Convert.ToInt32(count) > 0;
	}

	private static bool HasCombine(SqliteConnection connection, string mainTypeId, string materialTypeId, string resultTypeId)
	{
		using SqliteCommand command = connection.CreateCommand();
		command.CommandText = "select count(*) from combine_recipes where main_type_id = $main and material_type_id = $material and result_type_id = $result";
		command.Parameters.AddWithValue("$main", mainTypeId);
		command.Parameters.AddWithValue("$material", materialTypeId);
		command.Parameters.AddWithValue("$result", resultTypeId);
		object count = command.ExecuteScalar();
		return Convert.ToInt32(count) > 0;
	}

	private static bool HasBuilding(SqliteConnection connection, string typeId)
	{
		using SqliteCommand command = connection.CreateCommand();
		command.CommandText = "select count(*) from building_defs where type_id = $type";
		command.Parameters.AddWithValue("$type", typeId);
		object count = command.ExecuteScalar();
		return Convert.ToInt32(count) > 0;
	}

	private static SqliteConnection OpenConnection()
	{
		SqliteConnection connection = new SqliteConnection($"Data Source={FindConfigDb()}");
		connection.Open();
		return connection;
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
