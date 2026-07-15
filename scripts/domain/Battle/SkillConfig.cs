using System.Collections.Generic;
using System.Text.Json;

namespace CasualCastle.Domain.Battle;

public class StatModifierConfig
{
	public StatModifierTrigger Trigger { get; set; }
	public float TriggerParam { get; set; }
	public Dictionary<string, float> Modifiers { get; set; }

	public float GetMultiplier(string key)
	{
		if (Modifiers != null && Modifiers.TryGetValue(key, out float value))
			return value;
		if (key == "dodge_chance")
			return 0f;
		return 1f;
	}

	public static StatModifierConfig Parse(string json)
	{
		JsonElement root = JsonDocument.Parse(json).RootElement;
		StatModifierConfig config = new()
		{
			Modifiers = new Dictionary<string, float>()
		};

		if (root.TryGetProperty("trigger", out JsonElement triggerElem))
		{
			string trigger = triggerElem.GetString();
			config.Trigger = trigger switch
			{
				"low_health" => StatModifierTrigger.LowHealth,
				"target_isolated" => StatModifierTrigger.TargetIsolated,
				"nearby_diverse" => StatModifierTrigger.NearbyDiverse,
				_ => StatModifierTrigger.Always
			};
		}

		if (root.TryGetProperty("trigger_param", out JsonElement paramElem))
			config.TriggerParam = paramElem.GetSingle();

		if (root.TryGetProperty("modifiers", out JsonElement modsElem))
		{
			foreach (JsonProperty prop in modsElem.EnumerateObject())
				config.Modifiers[prop.Name] = prop.Value.GetSingle();
		}

		return config;
	}
}

public class OnHitEffect
{
	public string Type { get; set; }
	public float Value { get; set; }
	public float Duration { get; set; }
}

public class OnHitConfig
{
	public List<OnHitEffect> Effects { get; set; }

	public static OnHitConfig Parse(string json)
	{
		JsonElement root = JsonDocument.Parse(json).RootElement;
		OnHitConfig config = new() { Effects = new List<OnHitEffect>() };

		if (root.TryGetProperty("effects", out JsonElement effectsElem))
		{
			foreach (JsonElement effectElem in effectsElem.EnumerateArray())
			{
				config.Effects.Add(new OnHitEffect
				{
					Type = effectElem.GetProperty("type").GetString(),
					Value = effectElem.GetProperty("value").GetSingle(),
					Duration = effectElem.GetProperty("duration").GetSingle()
				});
			}
		}

		return config;
	}
}

public class AuraConfig
{
	public AuraTarget Target { get; set; }
	public float Range { get; set; }

	public static AuraConfig Parse(string json)
	{
		JsonElement root = JsonDocument.Parse(json).RootElement;
		return new AuraConfig
		{
			Target = root.GetProperty("target").GetString() switch
			{
				"nearby_enemies" => AuraTarget.NearbyEnemies,
				"all_allies" => AuraTarget.AllAllies,
				"all_enemies" => AuraTarget.AllEnemies,
				_ => AuraTarget.NearbyAllies
			},
			Range = root.GetProperty("range").GetSingle()
		};
	}
}

public class SpecialConfig
{
	public SpecialBehavior Behavior { get; set; }
	public float Param { get; set; }

	public static SpecialConfig Parse(string json)
	{
		JsonElement root = JsonDocument.Parse(json).RootElement;
		return new SpecialConfig
		{
			Behavior = root.GetProperty("behavior").GetString() switch
			{
				"kiting" => SpecialBehavior.Kiting,
				"sweep" => SpecialBehavior.Sweep,
				"charge" => SpecialBehavior.Charge,
				"no_battle_report" => SpecialBehavior.NoBattleReport,
				"summon" => SpecialBehavior.Summon,
				_ => SpecialBehavior.Stealth
			},
			Param = root.TryGetProperty("param", out JsonElement p) ? p.GetSingle() : 0f
		};
	}
}
