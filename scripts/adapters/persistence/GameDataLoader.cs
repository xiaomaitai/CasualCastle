using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Building;
using CasualCastle.Domain.Shared;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;

using GodotProjectSettings = Godot.ProjectSettings;

namespace CasualCastle.Adapters.Persistence;

public static class GameDataLoader
{
    private const string DbPath = "res://assets/data/config.db";

    public static void Load()
    {
        string fullPath = GodotProjectSettings.GlobalizePath(DbPath);
        using SqliteConnection connection = new($"Data Source={fullPath}");
        connection.Open();

        using SqliteCommand cmd = connection.CreateCommand();
        LoadUnitStats(cmd);
        LoadBuildingDefs(cmd);
        LoadDamageMatrix(cmd);
        LoadShopCatalog(cmd);
        LoadFusionRecipes(cmd);
    }

    private static void LoadUnitStats(SqliteCommand cmd)
    {
        cmd.CommandText = "SELECT type_id, size, attack_type, damage_type, armor_type, health, damage, speed, attack_range, attack_cooldown, has_night_combat, unit_color FROM unit_stats";
        using SqliteDataReader reader = cmd.ExecuteReader();
        Dictionary<string, UnitStats> stats = new();
        while (reader.Read())
        {
            stats[reader.GetString(0)] = new UnitStats
            {
                TypeId = reader.GetString(0),
                Size = (UnitSize)reader.GetInt32(1),
                AttackType = (AttackType)reader.GetInt32(2),
                DamageType = (DamageType)reader.GetInt32(3),
                ArmorType = (ArmorType)reader.GetInt32(4),
                Health = reader.GetInt32(5),
                Damage = reader.GetInt32(6),
                Speed = reader.GetFloat(7),
                AttackRange = reader.GetFloat(8),
                AttackCooldown = reader.GetFloat(9),
                HasNightCombat = reader.GetInt32(10) != 0,
                UnitColor = (uint)reader.GetInt64(11),
            };
        }
        UnitRegistry.LoadFrom(stats);
    }

    private static void LoadBuildingDefs(SqliteCommand cmd)
    {
        cmd.CommandText = "SELECT type_id, display_name, max_health, spawn_interval, main_cell_x, main_cell_y, spawn_cell_x, spawn_cell_y, unit_type_id, has_night_combat, fusion_tier, is_core, footprint_json FROM building_defs";
        using SqliteDataReader reader = cmd.ExecuteReader();
        Dictionary<string, BuildingData> data = new();
        while (reader.Read())
        {
            string footprintJson = reader.GetString(12);
            List<GridCellOffset> offsets = ParseFootprint(footprintJson);

            data[reader.GetString(0)] = new BuildingData
            {
                TypeId = reader.GetString(0),
                DisplayName = reader.GetString(1),
                MaxHealth = reader.GetInt32(2),
                SpawnInterval = reader.IsDBNull(3) ? 0 : reader.GetFloat(3),
                MainCellOffset = new(reader.GetInt32(4), reader.GetInt32(5)),
                SpawnCellOffset = new(reader.GetInt32(6), reader.GetInt32(7)),
                UnitTypeId = reader.IsDBNull(8) ? null : reader.GetString(8),
                HasNightCombat = reader.GetInt32(9) != 0,
                FusionTier = reader.GetInt32(10),
                IsCore = reader.GetInt32(11) != 0,
                Footprint = offsets.ToArray(),
            };
        }
        BuildingDefinitions.LoadFrom(data);
    }

    private static List<GridCellOffset> ParseFootprint(string json)
    {
        List<GridCellOffset> offsets = new();
        string inner = json.Trim('[', ']');
        if (string.IsNullOrEmpty(inner))
            return offsets;

        string[] pairs = inner.Split("],[");
        foreach (string pair in pairs)
        {
            string[] values = pair.Trim('[', ']').Split(',');
            if (values.Length == 2 && int.TryParse(values[0], out int x) && int.TryParse(values[1], out int y))
                offsets.Add(new GridCellOffset(x, y));
        }
        return offsets;
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

    private static void LoadShopCatalog(SqliteCommand cmd)
    {
        cmd.CommandText = "SELECT id, name, cost, building_type FROM shop_catalog";
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
            });
        }
        ShopRules.LoadCatalog(catalog);
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
