using System.Collections.Generic;
using System.Linq;

namespace CasualCastle.Domain.Battle;

public class SkillSet
{
	private readonly List<SkillDef> _skills = new();
	private readonly Dictionary<string, StatModifierConfig> _statModCache = new();
	private readonly Dictionary<string, OnHitConfig> _onHitCache = new();

	public int Count => _skills.Count;

	public void Add(SkillDef skill)
	{
		_skills.Add(skill);
		if (skill.Type == SkillType.StatModifier)
			_statModCache[skill.Id] = StatModifierConfig.Parse(skill.ConfigJson);
		else if (skill.Type == SkillType.OnHit)
			_onHitCache[skill.Id] = OnHitConfig.Parse(skill.ConfigJson);
	}

	public bool Remove(string skillId)
	{
		SkillDef found = _skills.FirstOrDefault(s => s.Id == skillId);
		if (found == null)
			return false;
		_skills.Remove(found);
		_statModCache.Remove(skillId);
		_onHitCache.Remove(skillId);
		return true;
	}

	public bool HasSkill(string skillId)
	{
		return _skills.Exists(s => s.Id == skillId);
	}

	public float GetStatMultiplier(string key, GameContext ctx)
	{
		float result = key == "dodge_chance" ? 0f : 1f;
		foreach (KeyValuePair<string, StatModifierConfig> kv in _statModCache)
		{
			StatModifierConfig cfg = kv.Value;
			if (!IsTriggerMet(cfg, ctx))
				continue;
			if (cfg.Modifiers.TryGetValue(key, out float val))
			{
				if (key == "dodge_chance")
					result += val;
				else
					result *= val;
			}
		}
		return result;
	}

	public bool HasTrigger(StatModifierTrigger trigger)
	{
		return _statModCache.Values.Any(c => c.Trigger == trigger);
	}

	public float GetDodgeChance(GameContext ctx)
	{
		if (ctx.NearbyAllyRaces == null || ctx.NearbyAllyRaces.Count <= 1)
			return 0f;
		int raceCount = ctx.NearbyAllyRaces.Count;
		float baseChance = raceCount switch
		{
			2 => 0.10f,
			3 => 0.20f,
			4 => 0.30f,
			5 => 0.35f,
			_ => 0.40f
		};
		if (raceCount > 6)
			baseChance = 0.40f;
		return baseChance * GetStatMultiplier("dodge_chance", ctx);
	}

	public IEnumerable<OnHitEffect> GetOnHitEffects()
	{
		foreach (KeyValuePair<string, OnHitConfig> kv in _onHitCache)
		{
			foreach (OnHitEffect effect in kv.Value.Effects)
				yield return effect;
		}
	}

	private static bool IsTriggerMet(StatModifierConfig cfg, GameContext ctx)
	{
		return cfg.Trigger switch
		{
			StatModifierTrigger.Always => true,
			StatModifierTrigger.LowHealth => ctx.CurrentHpRatio <= cfg.TriggerParam,
			StatModifierTrigger.TargetIsolated => ctx.TargetIsIsolated,
			StatModifierTrigger.NearbyDiverse => true,
			_ => false
		};
	}
}
