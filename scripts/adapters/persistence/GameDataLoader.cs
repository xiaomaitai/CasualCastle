// 配置数据加载：伤害矩阵、商店目录与组合配方
using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Building;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;

using GodotProjectSettings = Godot.ProjectSettings;

namespace CasualCastle.Adapters.Persistence;

public static class GameDataLoader
{
	private const string DbPath = "res://assets/data/config.db";

	public static DamageMatrix DamageMatrix { get; private set; }
	public static CombineRules CombineRules { get; private set; }
	public static List<CardData> ShopCatalog { get; private set; }

	public static void Load()
	{
		string fullPath = GodotProjectSettings.GlobalizePath(DbPath);
		using SqliteConnection connection = new($"Data Source={fullPath}");
		connection.Open();

		using SqliteCommand cmd = connection.CreateCommand();
		DamageMatrix = LoadDamageMatrix(cmd);
		ShopCatalog = LoadShopCatalog(cmd);
		CombineRules = LoadCombineRecipes(cmd);
	}

	private static DamageMatrix LoadDamageMatrix(SqliteCommand cmd)
	{
		cmd.CommandText = "SELECT damage_type, armor_type, multiplier FROM damage_matrix";
		using SqliteDataReader reader = cmd.ExecuteReader();
		DamageMatrix dm = new DamageMatrix();
		while (reader.Read())
			dm.Set((DamageType)reader.GetInt32(0), (ArmorType)reader.GetInt32(1), reader.GetFloat(2));
		return dm;
	}

	private static List<CardData> LoadShopCatalog(SqliteCommand cmd)
	{
		cmd.CommandText = @"
			SELECT s.id, s.name, COALESCE(b.value, s.cost), s.building_type, s.weight, COALESCE(b.faction_id, 'alliance')
			FROM shop_catalog s
			LEFT JOIN building_defs b ON b.type_id = s.building_type";
		using SqliteDataReader reader = cmd.ExecuteReader();
		List<CardData> catalog = new();
		while (reader.Read())
		{
			catalog.Add(new CardData
			{
				Id = reader.GetString(0),
				Name = reader.GetString(1),
				Cost = reader.GetInt32(2),
				BuildingType = reader.GetString(3),
				Weight = reader.GetInt32(4),
				FactionId = reader.GetString(5),
			});
		}
		return catalog;
	}

	private static CombineRules LoadCombineRecipes(SqliteCommand cmd)
	{
		cmd.CommandText = "SELECT main_type_id, material_type_id, material_count, result_type_id FROM combine_recipes";
		using SqliteDataReader reader = cmd.ExecuteReader();
		List<CombineRecipe> recipes = new();
		while (reader.Read())
		{
			recipes.Add(new CombineRecipe
			{
				MainTypeId = reader.GetString(0),
				MaterialTypeId = reader.GetString(1),
				MaterialCount = reader.GetInt32(2),
				ResultTypeId = reader.GetString(3),
			});
		}
		CombineRules rules = new CombineRules();
		rules.LoadRecipes(recipes);
		return rules;
	}
}
