using System.Collections.Generic;
using System.Linq;
using CasualCastle.Domain.Shared;

namespace CasualCastle.Domain.Battle;

public class SkillSet
{
	private readonly List<SkillDef> _skills = new();
	private readonly Dictionary<string, StatModifierConfig> _statModCache = new();
	private readonly Dictionary<string, OnHitConfig> _onHitCache = new();
	private readonly Dictionary<string, OnHealConfig> _onHealCache = new();
	private readonly Dictionary<string, SpecialConfig> _specialCache = new();
	private readonly Dictionary<string, AuraConfig> _auraCache = new();

	public int Count => _skills.Count;
	public IReadOnlyList<SkillDef> All => _skills;

	public void Add(SkillDef skill)
	{
		_skills.Add(skill);
		if (skill.Type == SkillType.StatModifier)
			_statModCache[skill.Id] = StatModifierConfig.Parse(skill.ConfigJson);
		else if (skill.Type == SkillType.OnHit)
			_onHitCache[skill.Id] = OnHitConfig.Parse(skill.ConfigJson);
		else if (skill.Type == SkillType.OnHeal)
			_onHealCache[skill.Id] = OnHealConfig.Parse(skill.ConfigJson);
		else if (skill.Type == SkillType.Special)
		{
			SpecialConfig cfg = SpecialConfig.Parse(skill.ConfigJson);
			cfg.SkillId = skill.Id;
			_specialCache[skill.Id] = cfg;
		}
		else if (skill.Type == SkillType.Aura)
			_auraCache[skill.Id] = AuraConfig.Parse(skill.ConfigJson);
	}

	public bool Remove(string skillId)
	{
		SkillDef found = _skills.FirstOrDefault(s => s.Id == skillId);
		if (found == null)
			return false;
		_skills.Remove(found);
		_statModCache.Remove(skillId);
		_onHitCache.Remove(skillId);
		_onHealCache.Remove(skillId);
		_specialCache.Remove(skillId);
		_auraCache.Remove(skillId);
		return true;
	}

	public bool HasSkill(string skillId)
	{
		return _skills.Exists(s => s.Id == skillId);
	}

	public SpecialConfig GetSpecial(SpecialBehavior behavior)
	{
		foreach (SpecialConfig cfg in _specialCache.Values)
		{
			if (cfg.Behavior == behavior)
				return cfg;
		}
		return null;
	}

	public SpecialConfig GetSpecialById(string skillId)
	{
		if (_specialCache.TryGetValue(skillId, out SpecialConfig cfg))
			return cfg;
		return null;
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

	public bool HasAura()
	{
		return _auraCache.Count > 0;
	}

	public float GetAuraRange()
	{
		foreach (AuraConfig cfg in _auraCache.Values)
		{
			if (cfg.Range > 0f)
				return cfg.Range;
		}
		return GameRules.AuraRangeRadius;
	}

	public float GetDodgeChance(GameContext ctx)
	{
		if (ctx.NearbyAllyRaces == null || ctx.NearbyAllyRaces.Count <= 1)
			return 0f;
		int raceCount = ctx.NearbyAllyRaces.Count;
		int idx = raceCount - 1;
		if (idx >= GameRules.DodgeChanceByAllyRaceCount.Length)
			idx = GameRules.DodgeChanceByAllyRaceCount.Length - 1;
		float baseChance = GameRules.DodgeChanceByAllyRaceCount[idx];
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

	public IEnumerable<OnHealEffect> GetOnHealEffects()
	{
		foreach (KeyValuePair<string, OnHealConfig> kv in _onHealCache)
		{
			foreach (OnHealEffect effect in kv.Value.Effects)
				yield return effect;
		}
	}

	private static bool IsTriggerMet(StatModifierConfig cfg, GameContext ctx)
	{
		if (!string.IsNullOrEmpty(cfg.RaceRequirement) && ctx.Race != cfg.RaceRequirement)
			return false;
		return cfg.Trigger switch
		{
			StatModifierTrigger.Always => true,
			StatModifierTrigger.LowHealth => ctx.CurrentHpRatio <= cfg.TriggerParam,
			StatModifierTrigger.OnDamaged => ctx.IsFurious,
			StatModifierTrigger.TargetIsolated => ctx.TargetIsIsolated,
			StatModifierTrigger.NearbyDiverse => true,
			_ => false
		};
	}
}
