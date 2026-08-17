// 视觉资源仓库：按 uid 精准匹配技能图标、单位肖像与建筑贴图
using CasualCastle.Domain.Shared;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using GodotProjectSettings = Godot.ProjectSettings;

namespace CasualCastle.Adapters.Persistence;

public class SqliteVisualAssetRepository : IVisualAssetRepository
{
	private readonly Dictionary<string, string> _skillIconUids = new();
	private readonly Dictionary<string, string> _portraitUids = new();
	private readonly Dictionary<string, string> _buildingTextureUids = new();

	public SqliteVisualAssetRepository()
	{
		string fullPath = GodotProjectSettings.GlobalizePath("res://assets/data/config.db");
		using SqliteConnection connection = new($"Data Source={fullPath}");
		connection.Open();
		LoadSkillIconUids(connection);
		LoadPortraitUids(connection);
		LoadBuildingTextureUids(connection);
	}

	private void LoadSkillIconUids(SqliteConnection connection)
	{
		using SqliteCommand cmd = connection.CreateCommand();
		cmd.CommandText = "SELECT id, icon_uid FROM skill_defs";
		using SqliteDataReader reader = cmd.ExecuteReader();
		while (reader.Read())
			_skillIconUids[reader.GetString(0)] = reader.IsDBNull(1) ? "" : reader.GetString(1);
	}

	private void LoadPortraitUids(SqliteConnection connection)
	{
		using SqliteCommand cmd = connection.CreateCommand();
		cmd.CommandText = "SELECT type_id, portrait_uid FROM unit_stats";
		using SqliteDataReader reader = cmd.ExecuteReader();
		while (reader.Read())
			_portraitUids[reader.GetString(0)] = reader.IsDBNull(1) ? "" : reader.GetString(1);
	}

	private void LoadBuildingTextureUids(SqliteConnection connection)
	{
		using SqliteCommand cmd = connection.CreateCommand();
		cmd.CommandText = "SELECT type_id, texture_uid FROM building_defs";
		using SqliteDataReader reader = cmd.ExecuteReader();
		while (reader.Read())
			_buildingTextureUids[reader.GetString(0)] = reader.IsDBNull(1) ? "" : reader.GetString(1);
	}

	public string GetSkillIconUid(string skillId)
	{
		return _skillIconUids.TryGetValue(skillId, out string uid) ? uid : "";
	}

	public string GetPortraitUid(string typeId)
	{
		return _portraitUids.TryGetValue(typeId, out string uid) ? uid : "";
	}

	public string GetBuildingTextureUid(string buildingTypeId)
	{
		return _buildingTextureUids.TryGetValue(buildingTypeId, out string uid) ? uid : "";
	}
}
