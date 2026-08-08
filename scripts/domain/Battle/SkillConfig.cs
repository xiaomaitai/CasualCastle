// 技能配置解析：JSON 配置转领域配置对象
using System.Collections.Generic;
using System.Text.Json;
using CasualCastle.Domain.Shared;

namespace CasualCastle.Domain.Battle;

public class StatModifierConfig
{
	public StatModifierTrigger Trigger { get; set; }
	public float TriggerParam { get; set; }
	public string RaceRequirement { get; set; }
	public float CountBonus { get; set; }
	public int CountCap { get; set; }
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
			config.Trigger = triggerElem.GetString() switch
			{
				"low_health" => StatModifierTrigger.LowHealth,
				"on_damaged" => StatModifierTrigger.OnDamaged,
				"target_isolated" => StatModifierTrigger.TargetIsolated,
				"nearby_diverse" => StatModifierTrigger.NearbyDiverse,
				"nearby_race_count" => StatModifierTrigger.NearbyRaceCount,
				_ => StatModifierTrigger.Always
			};
		}

		if (root.TryGetProperty("trigger_param", out JsonElement paramElem))
			config.TriggerParam = paramElem.GetSingle();
		if (root.TryGetProperty("race", out JsonElement raceElem))
			config.RaceRequirement = raceElem.GetString();
		if (root.TryGetProperty("count_bonus", out JsonElement countBonusElem))
			config.CountBonus = countBonusElem.GetSingle();
		if (root.TryGetProperty("count_cap", out JsonElement countCapElem))
			config.CountCap = countCapElem.GetInt32();

		if (root.TryGetProperty("modifiers", out JsonElement modsElem))
		{
			foreach (JsonProperty prop in modsElem.EnumerateObject())
				config.Modifiers[prop.Name] = prop.Value.GetSingle();
		}

		return config;
	}
}

public class AuraEffectConfig
{
	public string BonusType { get; set; }
	public float Value { get; set; }
}

public class AuraConfig
{
	public AuraTarget Target { get; set; }
	public float Range { get; set; }
	public string Race { get; set; }
	public List<AuraEffectConfig> Effects { get; set; }

	public static AuraConfig Parse(string json)
	{
		JsonElement root = JsonDocument.Parse(json).RootElement;
		AuraConfig config = new()
		{
			Target = root.GetProperty("target").GetString() switch
			{
				"nearby_enemies" => AuraTarget.NearbyEnemies,
				"all_allies" => AuraTarget.AllAllies,
				"all_enemies" => AuraTarget.AllEnemies,
				_ => AuraTarget.NearbyAllies
			},
			Range = root.GetProperty("range").GetSingle(),
			Race = root.TryGetProperty("race", out JsonElement auraRace) ? auraRace.GetString() : "",
			Effects = new List<AuraEffectConfig>()
		};

		if (root.TryGetProperty("effects", out JsonElement effectsElem))
		{
			foreach (JsonElement effectElem in effectsElem.EnumerateArray())
			{
				config.Effects.Add(new AuraEffectConfig
				{
					BonusType = effectElem.GetProperty("bonus_type").GetString(),
					Value = effectElem.TryGetProperty("value", out JsonElement effectValue) ? effectValue.GetSingle() : 0f
				});
			}
		}
		else
		{
			config.Effects.Add(new AuraEffectConfig
			{
				BonusType = root.TryGetProperty("bonus_type", out JsonElement bonusType) ? bonusType.GetString() : "",
				Value = root.TryGetProperty("value", out JsonElement auraValue) ? auraValue.GetSingle() : 0f
			});
		}

		return config;
	}
}

public class OnHitEffect
{
	public string Type { get; set; }
	public float Value { get; set; }
	public float Duration { get; set; }
	public float Range { get; set; }
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
					Value = effectElem.TryGetProperty("value", out JsonElement hitValue) ? hitValue.GetSingle() : 0f,
					Duration = effectElem.TryGetProperty("duration", out JsonElement hitDuration) ? hitDuration.GetSingle() : 0f,
					Range = effectElem.TryGetProperty("range", out JsonElement hitRange) ? hitRange.GetSingle() : 0f
				});
			}
		}

		return config;
	}
}

public class OnHealEffect
{
	public string Type { get; set; }
	public float Value { get; set; }
	public float Range { get; set; }
}

public class OnHealConfig
{
	public List<OnHealEffect> Effects { get; set; }

	public static OnHealConfig Parse(string json)
	{
		JsonElement root = JsonDocument.Parse(json).RootElement;
		OnHealConfig config = new() { Effects = new List<OnHealEffect>() };

		if (root.TryGetProperty("effects", out JsonElement effectsElem))
		{
			foreach (JsonElement effectElem in effectsElem.EnumerateArray())
			{
				OnHealEffect effect = new()
				{
					Type = effectElem.GetProperty("type").GetString(),
					Value = effectElem.TryGetProperty("value", out JsonElement valueElem) ? valueElem.GetSingle() : 0f,
					Range = effectElem.TryGetProperty("range", out JsonElement rangeElem) ? rangeElem.GetSingle() : 0f
				};
				config.Effects.Add(effect);
			}
		}

		return config;
	}
}

public class SpecialConfig
{
	public string SkillId { get; set; }
	public SpecialBehavior Behavior { get; set; }
	public float Param { get; set; }
	public float SpeedMult { get; set; }
	public float DamageMult { get; set; }
	public float Cooldown { get; set; }
	public int Cost { get; set; }
	public float Value { get; set; }

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
				"regen" => SpecialBehavior.Regen,
				"ambush" => SpecialBehavior.Ambush,
				"plague_burst" => SpecialBehavior.PlagueBurst,
				"curse" => SpecialBehavior.Curse,
				"shadow_nova" => SpecialBehavior.ShadowNova,
				_ => SpecialBehavior.Stealth
			},
			Param = root.TryGetProperty("param", out JsonElement p) ? p.GetSingle() : 0f,
			SpeedMult = root.TryGetProperty("speed_mult", out JsonElement s) ? s.GetSingle() : GameRules.StealthSpeedMult,
			DamageMult = root.TryGetProperty("damage_mult", out JsonElement d) ? d.GetSingle() : 1f,
			Cooldown = root.TryGetProperty("cooldown", out JsonElement c) ? c.GetSingle() : 0f,
			Cost = root.TryGetProperty("cost", out JsonElement cost) ? cost.GetInt32() : 0,
			Value = root.TryGetProperty("value", out JsonElement value) ? value.GetSingle() : 0f
		};
	}
}
