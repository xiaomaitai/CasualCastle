using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Building;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;

using GodotProjectSettings = Godot.ProjectSettings;

namespace CasualCastle.Adapters.Persistence;

public static class GameDataLoader
{
    private const string DbPath = "res://assets/data/config.db";

    public static List<CardData> ShopCatalog { get; private set; }

    public static void Load()
    {
        string fullPath = GodotProjectSettings.GlobalizePath(DbPath);
        using SqliteConnection connection = new($"Data Source={fullPath}");
        connection.Open();

        using SqliteCommand cmd = connection.CreateCommand();
        LoadDamageMatrix(cmd);
        ShopCatalog = LoadShopCatalog(cmd);
        LoadFusionRecipes(cmd);
    }


    private static void LoadDamageMatrix(SqliteCommand cmd)
    {
        cmd.CommandText = "SELECT damage_type, armor_type, multiplier FROM damage_matrix";
        using SqliteDataReader reader = cmd.ExecuteReader();
        float[,] matrix = new float[4, 4];
        while (reader.Read())
            matrix[reader.GetInt32(0), reader.GetInt32(1)] = reader.GetFloat(2);
        DamageMatrix.LoadFrom(matrix);
    }

    private static List<CardData> LoadShopCatalog(SqliteCommand cmd)
    {
        cmd.CommandText = "SELECT id, name, cost, building_type, weight FROM shop_catalog";
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
            });
        }
        return catalog;
    }

    private static void LoadFusionRecipes(SqliteCommand cmd)
    {
        cmd.CommandText = "SELECT main_type_id, material_type_id, material_count, result_type_id FROM fusion_recipes";
        using SqliteDataReader reader = cmd.ExecuteReader();
        List<FusionRecipe> recipes = new();
        while (reader.Read())
        {
            recipes.Add(new FusionRecipe
            {
                MainTypeId = reader.GetString(0),
                MaterialTypeId = reader.GetString(1),
                MaterialCount = reader.GetInt32(2),
                ResultTypeId = reader.GetString(3),
            });
        }
        FusionRules.LoadRecipes(recipes);
    }
}
