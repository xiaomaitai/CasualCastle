// 单位状态效果：减速、中毒、诅咒与持续回复
using System;
using CasualCastle.Domain.Shared;

namespace CasualCastle.Domain.Battle;

public partial class Unit
{
	public float GetCurrentSpeed()
	{
		float speedMult = Skills.GetStatMultiplier("move_speed_mult", GameCtx) * GameCtx.AuraMoveSpeedMultiplier;
		if (_kiteTimer > 0f)
		{
			SpecialConfig kiting = Skills.GetSpecial(SpecialBehavior.Kiting);
			if (kiting != null)
				speedMult *= kiting.SpeedMult;
		}
		if (_slowRemaining > 0f)
			speedMult *= 1f - _slowFactor;
		if (!IsStealthed)
			return Speed * speedMult;
		SpecialConfig stealth = Skills.GetSpecial(SpecialBehavior.Stealth);
		if (stealth == null)
			return Speed * speedMult;
		return Speed * stealth.SpeedMult * speedMult;
	}

	public void ApplySlow(float slowValue, float duration)
	{
		if (slowValue <= 0f || duration <= 0f)
			return;
		_slowFactor = slowValue;
		_slowRemaining = duration;
	}

	public void ApplyPoison(float damagePerSecond, float duration)
	{
		if (damagePerSecond <= 0f || duration <= 0f)
			return;
		_poisonDamagePerSecond = damagePerSecond;
		_poisonRemaining = duration;
		_poisonAccumulator = 0f;
	}

	public void ApplyCurse(float factor, float duration)
	{
		if (factor <= 0f || duration <= 0f)
			return;
		_curseFactor = factor;
		_curseRemaining = duration;
	}

	private float GetCurseDamageFactor()
	{
		return _curseRemaining > 0f ? 1f - _curseFactor : 1f;
	}

	private void TickCombatStateTimers(float dt)
	{
		if (_kiteTimer > 0f)
			_kiteTimer -= dt;
		if (_slowRemaining > 0f)
			_slowRemaining -= dt;
		if (_curseRemaining > 0f)
			_curseRemaining -= dt;
		TickPoison(dt);
		_timeSinceLastCombat += dt;
		UpdateAmbushState();
		ApplyWildHeartRegen(dt);
		ApplyAuraRegen(dt);
	}

	private void UpdateAmbushState()
	{
		SpecialConfig ambush = Skills != null ? Skills.GetSpecial(SpecialBehavior.Ambush) : null;
		if (ambush != null && _timeSinceLastCombat >= ambush.Param)
			IsAmbushing = true;
	}

	private void ApplyWildHeartRegen(float dt)
	{
		SpecialConfig regen = Skills != null ? Skills.GetSpecial(SpecialBehavior.Regen) : null;
		if (regen == null || _timeSinceLastCombat < regen.Param)
			return;
		if (Health >= MaxHealth)
		{
			_wildHeartAccumulator = 0f;
			return;
		}
		_wildHeartAccumulator += MaxHealth * GameRules.WildHeartRegenPercentPerSecond * dt;
		int amount = (int)_wildHeartAccumulator;
		if (amount <= 0)
			return;
		_wildHeartAccumulator -= amount;
		Heal(amount);
	}

	private void ApplyAuraRegen(float dt)
	{
		if (GameCtx.AuraRegenPerSecond <= 0f)
			return;
		if (Health >= MaxHealth)
		{
			_auraRegenAccumulator = 0f;
			return;
		}
		_auraRegenAccumulator += MaxHealth * GameCtx.AuraRegenPerSecond * dt;
		int amount = (int)_auraRegenAccumulator;
		if (amount <= 0)
			return;
		_auraRegenAccumulator -= amount;
		Heal(amount);
	}

	private void ApplyExplosion(float damage, float range)
	{
		if (Tactical == null || damage <= 0f || range <= 0f)
			return;
		foreach (IUnitHandle enemy in Tactical.FindEnemiesNearPoint(this, TargetEnemy.GameX, TargetEnemy.GameY, range))
		{
			if (enemy == TargetEnemy)
				continue;
			enemy.TakeDamage((int)damage, this, GameX, GameY);
		}
	}

	private void TickPoison(float dt)
	{
		if (_poisonRemaining <= 0f)
			return;
		_poisonRemaining -= dt;
		_poisonAccumulator += _poisonDamagePerSecond * dt;
		int poisonDamage = (int)_poisonAccumulator;
		if (poisonDamage > 0)
		{
			_poisonAccumulator -= poisonDamage;
			TakeDamage(poisonDamage, null, GameX, GameY);
		}
		if (_poisonRemaining <= 0f)
		{
			_poisonDamagePerSecond = 0f;
			_poisonAccumulator = 0f;
		}
	}

	private void TriggerPlagueBurst()
	{
		SpecialConfig burst = Skills != null ? Skills.GetSpecial(SpecialBehavior.PlagueBurst) : null;
		if (burst == null || Tactical == null)
			return;
		int damage = (int)burst.DamageMult;
		if (damage <= 0 || burst.Param <= 0f)
			return;
		foreach (IUnitHandle enemy in Tactical.FindEnemiesNear(this, burst.Param))
			enemy.TakeDamage(damage, this, GameX, GameY);
	}

}
