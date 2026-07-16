using System;
using System.Collections.Generic;
using CasualCastle.Domain.Building;
using Godot;
using Microsoft.Data.Sqlite;
using GodotProjectSettings = Godot.ProjectSettings;

namespace CasualCastle.Adapters.Persistence;

public class SqliteTechTreeRepository : ITechTreeRepository
{
    private readonly string _connectionString;

    public SqliteTechTreeRepository()
    {
        string fullPath = GodotProjectSettings.GlobalizePath("res://assets/data/config.db");
        _connectionString = $"Data Source={fullPath}";
        RunMigrations();
    }

    private SqliteConnection OpenConnection()
    {
        SqliteConnection connection = new(_connectionString);
        connection.Open();
        return connection;
    }

    private void RunMigrations()
    {
        using SqliteConnection connection = OpenConnection();
        using SqliteCommand cmd = connection.CreateCommand();

        cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='faction_defs'";
        bool factionDefsExists = cmd.ExecuteScalar() != null;

        if (!factionDefsExists)
        {
            cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='race_defs'";
            bool oldRaceDefsExists = cmd.ExecuteScalar() != null;

            if (oldRaceDefsExists)
            {
                cmd.CommandText = "ALTER TABLE race_defs RENAME TO faction_defs";
                cmd.ExecuteNonQuery();
            }
            else
            {
                cmd.CommandText = @"
                    CREATE TABLE faction_defs (
                        id TEXT PRIMARY KEY,
                        display_name TEXT NOT NULL,
                        sort_order INTEGER NOT NULL DEFAULT 0
                    );
                    INSERT INTO faction_defs (id, display_name, sort_order) VALUES
                        ('alliance', '联盟', 0),
                        ('horde', '部落', 1),
                        ('necromancer', '死灵法师', 2),
                        ('dungeon', '地下城', 3);";
                cmd.ExecuteNonQuery();
            }
        }

        cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='tech_tree_nodes'";
        bool techTreeExists = cmd.ExecuteScalar() != null;

        if (!techTreeExists)
        {
            cmd.CommandText = @"
                CREATE TABLE tech_tree_nodes (
                    type_id TEXT PRIMARY KEY,
                    faction_id TEXT NOT NULL REFERENCES faction_defs(id),
                    tier INTEGER NOT NULL CHECK(tier BETWEEN 1 AND 5),
                    col INTEGER NOT NULL DEFAULT 0,
                    shop_available INTEGER NOT NULL DEFAULT 0,
                    gold_cost INTEGER,
                    shop_weight INTEGER,
                    display_name TEXT NOT NULL,
                    unit_type_id TEXT,
                    max_health INTEGER NOT NULL DEFAULT 200,
                    spawn_interval REAL NOT NULL DEFAULT 10.0
                );";
            cmd.ExecuteNonQuery();
        }

        cmd.CommandText = "SELECT COUNT(*) FROM tech_tree_nodes";
        long nodeCount = (long)cmd.ExecuteScalar();
        if (nodeCount == 0)
        {
            MigrateFromBuildingDefs(connection);
        }

        MigrateRaceIdToFactionId(connection);
        MigrateCombineRecipesPK(connection);
        MigrateBuildingDefsFactionId(connection);
    }

    private static void MigrateRaceIdToFactionId(SqliteConnection connection)
    {
        using SqliteCommand cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM pragma_table_info('tech_tree_nodes') WHERE name = 'race_id'";
        long hasRaceIdCol = (long)cmd.ExecuteScalar();
        if (hasRaceIdCol > 0)
        {
            cmd.CommandText = "ALTER TABLE tech_tree_nodes RENAME COLUMN race_id TO faction_id";
            cmd.ExecuteNonQuery();
        }
    }

    private static void MigrateBuildingDefsFactionId(SqliteConnection connection)
    {
        using SqliteCommand cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM pragma_table_info('building_defs') WHERE name = 'faction_id'";
        long factionIdCol = (long)cmd.ExecuteScalar();
        if (factionIdCol > 0)
            return;

        cmd.CommandText = "SELECT COUNT(*) FROM pragma_table_info('building_defs') WHERE name = 'race_id'";
        long raceIdCol = (long)cmd.ExecuteScalar();
        if (raceIdCol > 0)
        {
            cmd.CommandText = "ALTER TABLE building_defs RENAME COLUMN race_id TO faction_id";
            cmd.ExecuteNonQuery();
        }
        else
        {
            cmd.CommandText = "ALTER TABLE building_defs ADD COLUMN faction_id TEXT NOT NULL DEFAULT 'alliance'";
            cmd.ExecuteNonQuery();
        }
    }

    private static void MigrateFromBuildingDefs(SqliteConnection connection)
    {
        using SqliteCommand cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM building_defs";
        long count = (long)cmd.ExecuteScalar();
        if (count == 0)
            return;

        cmd.CommandText = @"
            INSERT OR IGNORE INTO tech_tree_nodes (type_id, faction_id, tier, col, shop_available, gold_cost, shop_weight, display_name, unit_type_id, max_health, spawn_interval)
            SELECT b.type_id, 'alliance', b.combine_tier + 1, 0,
                   CASE WHEN s.id IS NOT NULL THEN 1 ELSE 0 END,
                   s.cost, s.weight,
                   b.display_name, b.unit_type_id, b.max_health, COALESCE(b.spawn_interval, 10.0)
            FROM building_defs b
            LEFT JOIN shop_catalog s ON s.building_type = b.type_id
            WHERE b.is_core = 0";
        cmd.ExecuteNonQuery();
    }

    public List<FactionDef> LoadFactions()
    {
        List<FactionDef> factions = new();
        using SqliteConnection connection = OpenConnection();
        using SqliteCommand cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT id, display_name, sort_order FROM faction_defs ORDER BY sort_order";
        using SqliteDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            factions.Add(new FactionDef
            {
                Id = reader.GetString(0),
                DisplayName = reader.GetString(1),
                SortOrder = reader.GetInt32(2)
            });
        }
        return factions;
    }

    public List<TechTreeNode> LoadNodes(string factionId)
    {
        List<TechTreeNode> nodes = new();
        using SqliteConnection connection = OpenConnection();
        using SqliteCommand cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT type_id, faction_id, tier, col, shop_available, gold_cost, shop_weight, display_name, unit_type_id, max_health, spawn_interval FROM tech_tree_nodes WHERE faction_id = @factionId ORDER BY tier, col";
        cmd.Parameters.AddWithValue("@factionId", factionId);
        using SqliteDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            nodes.Add(new TechTreeNode
            {
                TypeId = reader.GetString(0),
                FactionId = reader.GetString(1),
                Tier = reader.GetInt32(2),
                Col = reader.GetInt32(3),
                ShopAvailable = reader.GetInt32(4) != 0,
                GoldCost = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                ShopWeight = reader.IsDBNull(6) ? 0 : reader.GetInt32(6),
                DisplayName = reader.GetString(7),
                UnitTypeId = reader.IsDBNull(8) ? null : reader.GetString(8),
                MaxHealth = reader.GetInt32(9),
                SpawnInterval = reader.GetFloat(10)
            });
        }
        return nodes;
    }

    public List<CombineRecipe> LoadEdges(string factionId)
    {
        List<CombineRecipe> edges = new();
        using SqliteConnection connection = OpenConnection();
        using SqliteCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT c.main_type_id, c.material_type_id, c.material_count, c.result_type_id
            FROM combine_recipes c
            INNER JOIN tech_tree_nodes n ON n.type_id = c.main_type_id
            WHERE n.faction_id = @factionId";
        cmd.Parameters.AddWithValue("@factionId", factionId);
        using SqliteDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            edges.Add(new CombineRecipe
            {
                MainTypeId = reader.GetString(0),
                MaterialTypeId = reader.GetString(1),
                MaterialCount = reader.GetInt32(2),
                ResultTypeId = reader.GetString(3)
            });
        }
        return edges;
    }

    public List<BuildingTypeSummary> LoadAllBuildingTypes(string factionId)
    {
        List<BuildingTypeSummary> types = new();
        using SqliteConnection connection = OpenConnection();
        using SqliteCommand cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT type_id, display_name FROM building_defs WHERE is_core = 0 AND faction_id = @factionId ORDER BY display_name";
        cmd.Parameters.AddWithValue("@factionId", factionId);
        using SqliteDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            types.Add(new BuildingTypeSummary
            {
                TypeId = reader.GetString(0),
                DisplayName = reader.GetString(1)
            });
        }
        return types;
    }

    public void SaveNodes(string factionId, List<TechTreeNode> nodes)
    {
        using SqliteConnection connection = OpenConnection();
        using SqliteTransaction transaction = connection.BeginTransaction();
        using SqliteCommand cmd = connection.CreateCommand();

        cmd.CommandText = "DELETE FROM tech_tree_nodes WHERE faction_id = @factionId";
        cmd.Parameters.AddWithValue("@factionId", factionId);
        GD.Print($"[SaveNodes] 清空派系旧节点数据: DELETE FROM tech_tree_nodes WHERE faction_id = '{factionId}'");
        cmd.ExecuteNonQuery();

        cmd.CommandText = @"INSERT INTO tech_tree_nodes (type_id, faction_id, tier, col, shop_available, gold_cost, shop_weight, display_name, unit_type_id, max_health, spawn_interval)
            VALUES (@typeId, @factionId, @tier, @col, @shopAvail, @goldCost, @shopWeight, @displayName, @unitTypeId, @maxHealth, @spawnInterval)";

        foreach (TechTreeNode node in nodes)
        {
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@typeId", node.TypeId);
            cmd.Parameters.AddWithValue("@factionId", factionId);
            cmd.Parameters.AddWithValue("@tier", node.Tier);
            cmd.Parameters.AddWithValue("@col", node.Col);
            cmd.Parameters.AddWithValue("@shopAvail", node.ShopAvailable ? 1 : 0);
            cmd.Parameters.AddWithValue("@goldCost", node.ShopAvailable ? node.GoldCost : DBNull.Value);
            cmd.Parameters.AddWithValue("@shopWeight", node.ShopAvailable ? node.ShopWeight : DBNull.Value);
            cmd.Parameters.AddWithValue("@displayName", node.DisplayName);
            cmd.Parameters.AddWithValue("@unitTypeId", node.UnitTypeId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@maxHealth", node.MaxHealth);
            cmd.Parameters.AddWithValue("@spawnInterval", node.SpawnInterval);
            cmd.ExecuteNonQuery();

            GD.Print($"[SaveNodes] 写入节点 [{node.DisplayName}]: INSERT INTO tech_tree_nodes (type_id, faction_id, tier, col, shop_available, gold_cost, shop_weight, display_name, unit_type_id, max_health, spawn_interval) VALUES ('{node.TypeId}', '{factionId}', {node.Tier}, {node.Col}, {(node.ShopAvailable ? 1 : 0)}, {(node.ShopAvailable ? node.GoldCost.ToString() : "NULL")}, {(node.ShopAvailable ? node.ShopWeight.ToString() : "NULL")}, '{node.DisplayName}', '{node.UnitTypeId ?? "NULL"}', {node.MaxHealth}, {node.SpawnInterval})");
        }

        transaction.Commit();
    }

    public void AddRecipe(CombineRecipe recipe)
    {
        using SqliteConnection connection = OpenConnection();
        using SqliteCommand cmd = connection.CreateCommand();
        cmd.CommandText = "INSERT OR REPLACE INTO combine_recipes (main_type_id, material_type_id, material_count, result_type_id) VALUES (@main, @mat, @count, @result)";
        cmd.Parameters.AddWithValue("@main", recipe.MainTypeId);
        cmd.Parameters.AddWithValue("@mat", recipe.MaterialTypeId);
        cmd.Parameters.AddWithValue("@count", recipe.MaterialCount);
        cmd.Parameters.AddWithValue("@result", recipe.ResultTypeId);
        cmd.ExecuteNonQuery();
    }

    private static void MigrateCombineRecipesPK(SqliteConnection connection)
    {
        using SqliteCommand cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM pragma_table_info('combine_recipes') WHERE pk > 0";
        int pkCount = Convert.ToInt32(cmd.ExecuteScalar());
        if (pkCount >= 3)
            return;

        cmd.CommandText = @"
            CREATE TABLE combine_recipes_new (
                main_type_id TEXT NOT NULL,
                material_type_id TEXT NOT NULL,
                material_count INTEGER NOT NULL,
                result_type_id TEXT NOT NULL,
                PRIMARY KEY (main_type_id, material_type_id, result_type_id)
            );
            INSERT OR IGNORE INTO combine_recipes_new SELECT * FROM combine_recipes;
            DROP TABLE combine_recipes;
            ALTER TABLE combine_recipes_new RENAME TO combine_recipes;";
        cmd.ExecuteNonQuery();
    }

    public void RemoveRecipe(string mainTypeId, string materialTypeId, string resultTypeId)
    {
        using SqliteConnection connection = OpenConnection();
        using SqliteCommand cmd = connection.CreateCommand();
        cmd.CommandText = "DELETE FROM combine_recipes WHERE main_type_id = @main AND material_type_id = @mat AND result_type_id = @result";
        cmd.Parameters.AddWithValue("@main", mainTypeId);
        cmd.Parameters.AddWithValue("@mat", materialTypeId);
        cmd.Parameters.AddWithValue("@result", resultTypeId);
        cmd.ExecuteNonQuery();
    }

    public void SyncToGameTables(string factionId)
    {
        using SqliteConnection connection = OpenConnection();
        using SqliteTransaction transaction = connection.BeginTransaction();
        using SqliteCommand cmd = connection.CreateCommand();

        cmd.CommandText = @"
            INSERT OR REPLACE INTO building_defs (type_id, display_name, max_health, spawn_interval, main_cell_x, main_cell_y, spawn_cell_x, spawn_cell_y, unit_type_id, combine_tier, is_core, faction_id, footprint_json, collision_width, collision_height)
            SELECT n.type_id, n.display_name, n.max_health, n.spawn_interval,
                   0, 0, 1, 1,
                   n.unit_type_id, n.tier - 1, 0, @factionId,
                   COALESCE(b.footprint_json, '[[0,0],[1,0],[0,1],[1,1]]'),
                   COALESCE(b.collision_width, 188),
                   COALESCE(b.collision_height, 188)
            FROM tech_tree_nodes n
            LEFT JOIN building_defs b ON b.type_id = n.type_id
            WHERE n.faction_id = @factionId";
        cmd.Parameters.AddWithValue("@factionId", factionId);
        GD.Print($"[SyncToGameTables] 同步科技树节点到建筑定义表(faction_id={factionId}): INSERT OR REPLACE INTO building_defs (..., faction_id) SELECT ..., @factionId FROM tech_tree_nodes n LEFT JOIN building_defs b ON b.type_id = n.type_id WHERE n.faction_id = '{factionId}'");
        cmd.ExecuteNonQuery();

        cmd.Parameters.Clear();
        cmd.CommandText = @"
            DELETE FROM shop_catalog WHERE id IN (
                SELECT s.id FROM shop_catalog s
                INNER JOIN tech_tree_nodes n ON s.building_type = n.type_id
                WHERE n.faction_id = @factionId AND n.shop_available = 0
            )";
        cmd.Parameters.AddWithValue("@factionId", factionId);
        GD.Print($"[SyncToGameTables] 移除不可购买的商店条目: DELETE FROM shop_catalog WHERE id IN (SELECT s.id FROM shop_catalog s INNER JOIN tech_tree_nodes n ON s.building_type = n.type_id WHERE n.faction_id = '{factionId}' AND n.shop_available = 0)");
        cmd.ExecuteNonQuery();

        cmd.Parameters.Clear();
        cmd.CommandText = @"
            INSERT OR REPLACE INTO shop_catalog (id, name, cost, building_type, weight)
            SELECT n.type_id, n.display_name, n.gold_cost, n.type_id, n.shop_weight
            FROM tech_tree_nodes n
            WHERE n.faction_id = @factionId AND n.shop_available = 1";
        cmd.Parameters.AddWithValue("@factionId", factionId);
        GD.Print($"[SyncToGameTables] 同步可购买节点到商店目录: INSERT OR REPLACE INTO shop_catalog (id, name, cost, building_type, weight) SELECT n.type_id, n.display_name, n.gold_cost, n.type_id, n.shop_weight FROM tech_tree_nodes n WHERE n.faction_id = '{factionId}' AND n.shop_available = 1");
        cmd.ExecuteNonQuery();

        transaction.Commit();
    }
}
