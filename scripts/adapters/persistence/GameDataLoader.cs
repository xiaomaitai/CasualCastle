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

        InitializeDatabase(connection);

        using SqliteCommand cmd = connection.CreateCommand();
        LoadUnitStats(cmd);
        LoadBuildingDefs(cmd);
        LoadDamageMatrix(cmd);
        LoadShopCatalog(cmd);
        LoadFusionRecipes(cmd);
    }

    private static void InitializeDatabase(SqliteConnection connection)
    {
        using SqliteCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS unit_stats (
                type_id TEXT PRIMARY KEY, size INTEGER NOT NULL, attack_type INTEGER NOT NULL,
                damage_type INTEGER NOT NULL, armor_type INTEGER NOT NULL, health INTEGER NOT NULL,
                damage INTEGER NOT NULL, speed REAL NOT NULL, attack_range REAL NOT NULL,
                attack_cooldown REAL NOT NULL, has_night_combat INTEGER NOT NULL DEFAULT 0,
                unit_color INTEGER NOT NULL
            );
            CREATE TABLE IF NOT EXISTS building_defs (
                type_id TEXT PRIMARY KEY, display_name TEXT NOT NULL, max_health INTEGER NOT NULL,
                spawn_interval REAL, main_cell_x INTEGER DEFAULT 0, main_cell_y INTEGER DEFAULT 0,
                spawn_cell_x INTEGER DEFAULT 0, spawn_cell_y INTEGER DEFAULT 0,
                unit_type_id TEXT, has_night_combat INTEGER DEFAULT 0, fusion_tier INTEGER DEFAULT 0,
                is_core INTEGER DEFAULT 0, footprint_json TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS damage_matrix (
                damage_type INTEGER NOT NULL, armor_type INTEGER NOT NULL,
                multiplier REAL NOT NULL, PRIMARY KEY (damage_type, armor_type)
            );
            CREATE TABLE IF NOT EXISTS shop_catalog (
                id TEXT PRIMARY KEY, name TEXT NOT NULL, cost INTEGER NOT NULL,
                building_type TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS fusion_recipes (
                main_type_id TEXT NOT NULL, material_type_id TEXT NOT NULL,
                material_count INTEGER NOT NULL, result_type_id TEXT NOT NULL,
                PRIMARY KEY (main_type_id, material_type_id)
            );
        ";
        cmd.ExecuteNonQuery();

        cmd.CommandText = "SELECT COUNT(*) FROM unit_stats";
        long count = (long)cmd.ExecuteScalar();
        if (count > 0)
            return;

        cmd.CommandText = @"
            INSERT INTO unit_stats VALUES ('Swordsman',1,0,0,0,30,10,170,60,1,0,0xFF4488FF);
            INSERT INTO unit_stats VALUES ('Archer',1,1,1,0,20,8,150,100,1.2,0,0xFF44CC44);
            INSERT INTO unit_stats VALUES ('Cavalry',2,0,0,1,50,12,220,60,1,0,0xFFFFAA22);
            INSERT INTO unit_stats VALUES ('Werewolf',1,0,0,3,35,12,200,60,1,1,0xFF8844AA);
            INSERT INTO unit_stats VALUES ('HeavySwordsman',1,0,0,1,45,14,160,60,0.9,0,0xFF6688FF);
            INSERT INTO unit_stats VALUES ('WerewolfLord',1,0,3,3,50,16,200,60,0.9,1,0xFFCC44CC);

            INSERT INTO building_defs VALUES ('CastleHeart','城堡之心',500,NULL,0,0,0,0,NULL,0,0,1,'[[0,0],[1,0],[0,1],[1,1]]');
            INSERT INTO building_defs VALUES ('Barracks','兵营',100,5,0,0,0,0,'Swordsman',0,0,0,'[[0,0]]');
            INSERT INTO building_defs VALUES ('ArcheryRange','靶场',120,6,0,0,1,0,'Archer',0,0,0,'[[0,0],[1,0]]');
            INSERT INTO building_defs VALUES ('Stable','马厩',150,5,0,1,1,2,'Cavalry',0,0,0,'[[0,0],[0,1],[0,2],[1,2]]');
            INSERT INTO building_defs VALUES ('WolfDen','狼穴',90,6,0,0,0,0,'Werewolf',1,0,0,'[[0,0]]');
            INSERT INTO building_defs VALUES ('BarracksT2','强化兵营',130,4,0,0,0,0,'HeavySwordsman',0,1,0,'[[0,0]]');
            INSERT INTO building_defs VALUES ('WolfDenT2','强化狼穴',120,5,0,0,0,0,'WerewolfLord',1,1,0,'[[0,0]]');

            INSERT INTO damage_matrix VALUES (0,0,1.0); INSERT INTO damage_matrix VALUES (0,1,0.75);
            INSERT INTO damage_matrix VALUES (0,2,0.5);  INSERT INTO damage_matrix VALUES (0,3,1.0);
            INSERT INTO damage_matrix VALUES (1,0,0.75); INSERT INTO damage_matrix VALUES (1,1,1.5);
            INSERT INTO damage_matrix VALUES (1,2,1.0);  INSERT INTO damage_matrix VALUES (1,3,0.75);
            INSERT INTO damage_matrix VALUES (2,0,0.5);  INSERT INTO damage_matrix VALUES (2,1,1.0);
            INSERT INTO damage_matrix VALUES (2,2,1.5);  INSERT INTO damage_matrix VALUES (2,3,1.0);
            INSERT INTO damage_matrix VALUES (3,0,1.0);  INSERT INTO damage_matrix VALUES (3,1,1.0);
            INSERT INTO damage_matrix VALUES (3,2,1.25); INSERT INTO damage_matrix VALUES (3,3,1.5);

            INSERT INTO shop_catalog VALUES ('barracks','兵营',10,'Barracks');
            INSERT INTO shop_catalog VALUES ('archery_range','靶场',14,'ArcheryRange');
            INSERT INTO shop_catalog VALUES ('stable','马厩',18,'Stable');
            INSERT INTO shop_catalog VALUES ('wolf_den','狼穴',16,'WolfDen');

            INSERT INTO fusion_recipes VALUES ('Barracks','Barracks',1,'BarracksT2');
            INSERT INTO fusion_recipes VALUES ('WolfDen','WolfDen',1,'WolfDenT2');
        ";
        cmd.ExecuteNonQuery();
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
