// 单位技能：冷却、施法触发与治疗/命中效果
using System;
using System.Collections.Generic;
using CasualCastle.Domain.Shared;

namespace CasualCastle.Domain.Battle;

public partial class Unit
{
	private void ApplyGraceEffects(int healed)
	{
		if (Skills == null)
			return;
		foreach (OnHealEffect effect in Skills.GetOnHealEffects())
		{
			if (effect.Type == HealEffectTypes.Shield)
				AddShield((int)(healed * GameRules.GraceShieldMult));
			else if (effect.Type == HealEffectTypes.CooldownReduction)
				ReduceSkillCooldowns(GameRules.GraceCooldownReduction);
			else if (effect.Type == HealEffectTypes.Burn)
				ApplyBurn(effect);
		}
	}

	private void ReduceSkillCooldowns(float seconds)
	{
		if (_skillCooldowns.Count == 0 || seconds <= 0f)
			return;

		List<string> keys = new(_skillCooldowns.Keys);
		foreach (string skillId in keys)
		{
			float next = _skillCooldowns[skillId] - seconds;
			if (next <= 0f)
				_skillCooldowns.Remove(skillId);
			else
				_skillCooldowns[skillId] = next;
		}
	}

	private void ApplyBurn(OnHealEffect effect)
	{
		int burnDamage = (int)effect.Value;
		if (Tactical == null || burnDamage <= 0 || effect.Range <= 0f)
			return;
		foreach (IUnitHandle enemy in Tactical.FindEnemiesNear(this, effect.Range))
			enemy.TakeDamage(burnDamage, this, GameX, GameY);
	}

	public float GetSkillCooldownRemaining(string skillId)
	{
		if (_skillCooldowns.TryGetValue(skillId, out float remaining))
			return remaining;
		return 0f;
	}

	public void StartSkillCooldown(string skillId)
	{
		SpecialConfig cfg = Skills.GetSpecialById(skillId);
		if (cfg == null || cfg.Cooldown <= 0f)
			return;
		_skillCooldowns[skillId] = cfg.Cooldown;
	}

	public void TickSkillCooldowns(float dt)
	{
		if (_skillCooldowns.Count == 0)
			return;

		List<string> expired = new();
		foreach (KeyValuePair<string, float> kv in _skillCooldowns)
		{
			float next = kv.Value - dt;
			if (next <= 0f)
				expired.Add(kv.Key);
			else
				_skillCooldowns[kv.Key] = next;
		}
		foreach (string skillId in expired)
			_skillCooldowns.Remove(skillId);
	}

	private void StartStealthCooldown()
	{
		SpecialConfig stealth = Skills.GetSpecial(SpecialBehavior.Stealth);
		if (stealth != null)
			StartSkillCooldown(stealth.SkillId);
	}

	private bool TryCastSpell()
	{
		if (BlackMagic == null || TargetEnemy == null || !TargetEnemy.IsAlive)
			return false;
		SpecialConfig nova = Skills.GetSpecial(SpecialBehavior.ShadowNova);
		if (nova != null && CanCastSpell(nova) && Tactical != null
			&& Tactical.FindEnemiesNearPoint(this, TargetEnemy.GameX, TargetEnemy.GameY, nova.Param).Count > 1)
			return CastNova(nova);
		SpecialConfig curse = Skills.GetSpecial(SpecialBehavior.Curse);
		if (curse != null && CanCastSpell(curse))
			return CastCurse(curse);
		return false;
	}

	private bool CanCastSpell(SpecialConfig cfg)
	{
		return cfg.Cost > 0 && BlackMagic.Value >= cfg.Cost && GetSkillCooldownRemaining(cfg.SkillId) <= 0f;
	}

	private bool CastCurse(SpecialConfig cfg)
	{
		BlackMagic.TrySpend(cfg.Cost);
		StartSkillCooldown(cfg.SkillId);
		TargetEnemy.ApplySlow(cfg.Value, cfg.Param);
		TargetEnemy.ApplyCurse(cfg.Value, cfg.Param);
		EventPort?.OnAttack(TargetEnemy, TargetEnemy.GameX, TargetEnemy.GameY);
		return true;
	}

	private bool CastNova(SpecialConfig cfg)
	{
		BlackMagic.TrySpend(cfg.Cost);
		StartSkillCooldown(cfg.SkillId);
		foreach (IUnitHandle enemy in Tactical.FindEnemiesNearPoint(this, TargetEnemy.GameX, TargetEnemy.GameY, cfg.Param))
			enemy.TakeDamage((int)cfg.Value, this, GameX, GameY);
		EventPort?.OnAttack(TargetEnemy, TargetEnemy.GameX, TargetEnemy.GameY);
		return true;
	}

}
