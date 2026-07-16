using System.Collections.Generic;
using CasualCastle.Domain.Battle;
using Microsoft.Data.Sqlite;
using GodotProjectSettings = Godot.ProjectSettings;

namespace CasualCastle.Adapters.Persistence;

public class SqliteSkillRepository : ISkillRepository
{
	private readonly Dictionary<string, SkillDef> _skillCache = new();
	private readonly Dictionary<string, List<SkillDef>> _unitSkillCache = new();

	public SqliteSkillRepository()
	{
		string fullPath = GodotProjectSettings.GlobalizePath("res://assets/data/config.db");
		using SqliteConnection connection = new($"Data Source={fullPath}");
		connection.Open();

		using (SqliteCommand cmd = connection.CreateCommand())
		{
			cmd.CommandText = "SELECT id, display_name, skill_type, config_json FROM skill_defs";
			using (SqliteDataReader reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					string id = reader.GetString(0);
					SkillDef skill = new()
					{
						Id = id,
						DisplayName = reader.GetString(1),
						Type = reader.GetString(2) switch
						{
							"aura" => SkillType.Aura,
							"on_hit" => SkillType.OnHit,
							"special" => SkillType.Special,
							_ => SkillType.StatModifier
						},
						ConfigJson = reader.GetString(3)
					};
					_skillCache[id] = skill;
				}
			}

			cmd.CommandText = "SELECT unit_type_id, skill_id FROM unit_skills";
			using (SqliteDataReader unitReader = cmd.ExecuteReader())
			{
				while (unitReader.Read())
				{
					string unitType = unitReader.GetString(0);
					string skillId = unitReader.GetString(1);
					if (_skillCache.TryGetValue(skillId, out SkillDef skill))
					{
						if (!_unitSkillCache.ContainsKey(unitType))
							_unitSkillCache[unitType] = new List<SkillDef>();
						_unitSkillCache[unitType].Add(skill);
					}
				}
			}
		}
	}

	public SkillDef Get(string skillId)
	{
		if (_skillCache.TryGetValue(skillId, out SkillDef skill))
			return skill;
		throw new KeyNotFoundException($"Skill '{skillId}' not found");
	}

	public IReadOnlyList<SkillDef> GetByUnitType(string unitTypeId)
	{
		if (_unitSkillCache.TryGetValue(unitTypeId, out List<SkillDef> skills))
			return skills;
		return System.Array.Empty<SkillDef>();
	}
}
