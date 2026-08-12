// 魔王技能等级成长：习得技能随魔王等级线性提升数值
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace CasualCastle.Domain.Battle;

public static class DemonKingSkillScaling
{
	public static SkillDef Scale(SkillDef def, int learnLevel, int currentLevel)
	{
		if (def.Type != SkillType.StatModifier || currentLevel <= learnLevel)
			return def;
		StatModifierConfig config = StatModifierConfig.Parse(def.ConfigJson);
		Dictionary<string, float> scaled = new();
		foreach (KeyValuePair<string, float> kv in config.Modifiers)
		{
			float value = kv.Key.EndsWith("_mult")
				? 1f + (kv.Value - 1f) * currentLevel / learnLevel
				: kv.Value;
			scaled[kv.Key] = value;
		}
		string json = BuildJson(config, scaled);
		return new SkillDef
		{
			Id = def.Id,
			DisplayName = def.DisplayName,
			Description = def.Description,
			Type = def.Type,
			ConfigJson = json
		};
	}

	private static string BuildJson(StatModifierConfig config, Dictionary<string, float> scaled)
	{
		using MemoryStream stream = new();
		using (Utf8JsonWriter writer = new(stream))
		{
			writer.WriteStartObject();
			writer.WriteString("trigger", GetTriggerName(config.Trigger));
			if (config.TriggerParam != 0f)
				writer.WriteNumber("trigger_param", config.TriggerParam);
			if (!string.IsNullOrEmpty(config.RaceRequirement))
				writer.WriteString("race", config.RaceRequirement);
			writer.WritePropertyName("modifiers");
			writer.WriteStartObject();
			foreach (KeyValuePair<string, float> kv in scaled)
				writer.WriteNumber(kv.Key, kv.Value);
			writer.WriteEndObject();
			writer.WriteEndObject();
		}
		return Encoding.UTF8.GetString(stream.ToArray());
	}

	private static string GetTriggerName(StatModifierTrigger trigger)
	{
		switch (trigger)
		{
			case StatModifierTrigger.LowHealth: return "low_health";
			case StatModifierTrigger.OnDamaged: return "on_damaged";
			case StatModifierTrigger.TargetIsolated: return "target_isolated";
			case StatModifierTrigger.NearbyDiverse: return "nearby_diverse";
			case StatModifierTrigger.NearbyRaceCount: return "nearby_race_count";
			default: return "always";
		}
	}
}