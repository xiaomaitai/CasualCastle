// 技能运行时：技能冷却状态机，字段与方法集中于 SkillSet 委托
using System.Collections.Generic;

namespace CasualCastle.Domain.Battle;

public sealed class SkillRuntime
{
	private readonly SkillSet _skills;

	private readonly Dictionary<string, float> _cooldowns = new();

	public SkillRuntime(SkillSet skills)
	{
		_skills = skills;
	}

	public float GetRemaining(string skillId)
	{
		if (_cooldowns.TryGetValue(skillId, out float remaining))
			return remaining;
		return 0f;
	}

	public void Start(string skillId)
	{
		SpecialConfig cfg = _skills.GetSpecialById(skillId);
		if (cfg == null || cfg.Cooldown <= 0f)
			return;
		_cooldowns[skillId] = cfg.Cooldown;
	}

	public void StartStealth()
	{
		SpecialConfig stealth = _skills.GetSpecial(SpecialBehavior.Stealth);
		if (stealth != null)
			Start(stealth.SkillId);
	}

	public void Tick(float dt)
	{
		if (_cooldowns.Count == 0)
			return;

		List<string> expired = new();
		foreach (KeyValuePair<string, float> kv in _cooldowns)
		{
			float next = kv.Value - dt;
			if (next <= 0f)
				expired.Add(kv.Key);
			else
				_cooldowns[kv.Key] = next;
		}
		foreach (string skillId in expired)
			_cooldowns.Remove(skillId);
	}

	public void Reduce(float seconds)
	{
		if (_cooldowns.Count == 0 || seconds <= 0f)
			return;

		List<string> keys = new(_cooldowns.Keys);
		foreach (string skillId in keys)
		{
			float next = _cooldowns[skillId] - seconds;
			if (next <= 0f)
				_cooldowns.Remove(skillId);
			else
				_cooldowns[skillId] = next;
		}
	}
}
