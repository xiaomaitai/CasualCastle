// 配置数据加载：伤害矩阵、商店目录、组合配方与物品属性
using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Building;
using CasualCastle.Domain.Shared;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Text.Json;


namespace CasualCastle.Adapters.Persistence;

public static class GameDataLoader
{
	private const string DbPath = "res://assets/data/config.db";

	public static DamageMatrix DamageMatrix { get; private set; }
	public static CombineRules CombineRules { get; private set; }
	public static List<CardData> ShopCatalog { get; private set; }
	public static List<BgmDef> BgmCatalog { get; private set; }

	public static void Load(string fullPath)
	{
		using SqliteConnection connection = new($"Data Source={fullPath}");
		connection.Open();

		using SqliteCommand cmd = connection.CreateCommand();
		DamageMatrix = LoadDamageMatrix(cmd);
		ShopCatalog = LoadShopCatalog(cmd);
		CombineRules = LoadCombineRecipes(cmd);
		BgmCatalog = LoadBgmCatalog(cmd);
	}

	private static DamageMatrix LoadDamageMatrix(SqliteCommand cmd)
	{
		cmd.CommandText = "SELECT damage_type, armor_type, multiplier FROM damage_matrix";
		using SqliteDataReader reader = cmd.ExecuteReader();
		DamageMatrix dm = new DamageMatrix();
		while (reader.Read())
			dm.Set((DamageType)reader.GetInt32(0), (ArmorType)reader.GetInt32(1), DamageMatrix.PercentToMultiplier(reader.GetInt32(2)));
		return dm;
	}

	private static List<CardData> LoadShopCatalog(SqliteCommand cmd)
	{
		List<CardData> catalog = new();
		LoadBuildingCards(cmd, catalog);
		LoadItemCards(cmd, catalog);
		return catalog;
	}

	private static void LoadBuildingCards(SqliteCommand cmd, List<CardData> catalog)
	{
		cmd.CommandText = @"
			SELECT s.id, s.name, COALESCE(b.value, s.cost), s.building_type, s.weight, b.faction_id, b.type_id, s.unlock_ritual_id, s.unlock_night
			FROM shop_catalog s
			LEFT JOIN building_defs b ON b.type_id = s.building_type";
		using SqliteDataReader reader = cmd.ExecuteReader();
		while (reader.Read())
		{
			string shopId = reader.GetString(0);
			string buildingType = reader.GetString(3);
			if (reader.IsDBNull(6))
				throw new KeyNotFoundException($"商店卡 {shopId} 的建筑类型 {buildingType} 在 building_defs 中不存在");

			catalog.Add(new CardData
			{
				Id = shopId,
				Name = reader.GetString(1),
				Cost = reader.GetInt32(2),
				BuildingType = buildingType,
				Weight = reader.GetInt32(4),
				FactionId = reader.GetString(5),
				UnlockRitualId = reader.IsDBNull(7) ? "" : reader.GetString(7),
				UnlockNight = reader.GetInt32(8),
			});
		}
	}

	private static void LoadItemCards(SqliteCommand cmd, List<CardData> catalog)
	{
		cmd.CommandText = "SELECT id, name, cost, weight, faction_id, item_type, attributes FROM item_catalog";
		using SqliteDataReader reader = cmd.ExecuteReader();
		while (reader.Read())
		{
			catalog.Add(new CardData
			{
				Id = reader.GetString(0),
				Name = reader.GetString(1),
				Cost = reader.GetInt32(2),
				BuildingType = "",
				Weight = reader.GetInt32(3),
				FactionId = reader.GetString(4),
				ItemType = reader.GetString(5),
				Attributes = ParseAttributes(reader.GetString(6)),
			});
		}
	}

	private static Dictionary<string, string> ParseAttributes(string json)
	{
		if (string.IsNullOrEmpty(json))
			return new();
		return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new();
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

	private static List<BgmDef> LoadBgmCatalog(SqliteCommand cmd)
	{
		cmd.CommandText = "SELECT id, name, style, pool, uid FROM bgm_defs ORDER BY id";
		using SqliteDataReader reader = cmd.ExecuteReader();
		List<BgmDef> catalog = new();
		while (reader.Read())
		{
			catalog.Add(new BgmDef
			{
				Id = reader.GetInt32(0),
				Name = reader.GetString(1),
				Style = reader.GetString(2) == "stirring" ? BgmStyle.Stirring : BgmStyle.Soothing,
				Pool = reader.GetString(3),
				Uid = reader.GetString(4),
			});
		}

		return catalog;
	}
}