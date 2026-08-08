// 单位战斗行为：索敌、攻击、治疗与受击
using System;
using CasualCastle.Domain.Shared;

namespace CasualCastle.Domain.Battle;

public partial class Unit
{
	public int Heal(int amount)
	{
		if (!IsAlive || amount <= 0)
			return 0;

		int healed = HealRules.ApplyHeal(Health, MaxHealth, amount) - Health;
		if (healed <= 0)
			return 0;
		Health += healed;
		ApplyGraceEffects(healed);
		return healed;
	}

	public void AddShield(int amount)
	{
		if (amount <= 0)
			return;
		ShieldHealth = System.Math.Min(MaxHealth, ShieldHealth + amount);
	}

	public void AddMaxHealthBonus(int amount)
	{
		if (amount <= 0)
			return;
		MaxHealth += amount;
		Health += amount;
	}

	public void TakeDamage(int amount, IUnitHandle attacker, float attackerGameX, float attackerGameY, bool isAoe = false)
	{
		if (!IsAlive)
			return;

		_timeSinceLastCombat = 0f;
		IsAmbushing = false;

		if (Skills != null && GameCtx != null)
		{
			float dodgeChance = Skills.GetDodgeChance(GameCtx);
			if (dodgeChance > 0f && _rng.NextDouble() < dodgeChance)
				return;
		}

		int remaining = amount;
		if (ShieldHealth > 0)
		{
			int absorbed = System.Math.Min(ShieldHealth, remaining);
			ShieldHealth -= absorbed;
			remaining -= absorbed;
		}

		Health = CombatRules.ApplyDamage(Health, remaining);
		if (remaining > 0 && Skills != null && Skills.HasTrigger(StatModifierTrigger.OnDamaged))
			IsFurious = true;

		if (isAoe && IsStealthed)
		{
			IsStealthed = false;
			StartStealthCooldown();
		}

		EventPort?.OnDamaged(amount, attacker);

		if (Health <= 0)
		{
			IsAlive = false;
			TriggerPlagueBurst();
			EventPort?.OnDied();
			return;
		}

		if (attacker != null && attacker.IsAlive && attacker.IsPlayerUnit != IsPlayerUnit)
		{
			float dx = GameX - attackerGameX;
			float dy = GameY - attackerGameY;
			if (MathF.Sqrt(dx * dx + dy * dy) > VisionRange)
				TargetEnemy = attacker;
		}
	}

	public void UpdateTargeting(IUnitHandle nearestEnemy)
	{
		if (!IsAlive)
			return;

		if (AssignedTavern != null && TavernPartyService.IsNearTavern(this, AssignedTavern))
		{
			State = UnitState.Waiting;
			TargetEnemy = null;
			return;
		}

		if (HealAmount > 0f && HealRange > 0f)
		{
			TargetEnemy = null;
			IUnitHandle injured = Tactical != null ? Tactical.FindNearestInjuredAlly(this, HealRange) : null;
			if (injured != null)
			{
				HealTarget = injured;
				State = UnitState.Healing;
			}
			else
			{
				HealTarget = null;
				State = UnitState.Marching;
			}
			return;
		}

		if (nearestEnemy != null && nearestEnemy.IsAlive)
		{
			TargetEnemy = nearestEnemy;
			State = UnitState.Fighting;
		}
		else if (TargetEnemy != null && TargetEnemy.IsAlive)
		{
			State = UnitState.Retaliating;
		}
		else if (TargetBuilding != null)
		{
			TargetEnemy = null;
			State = UnitState.Sieging;
		}
		else
		{
			TargetEnemy = null;
			State = UnitState.Marching;
		}
	}

	public void UpdateBehavior(float dt, float marchTargetGameX, float marchTargetGameY)
	{
		if (!IsAlive)
			return;

		TickSkillCooldowns(dt);
		TickCombatStateTimers(dt);

		float effectiveAttackCooldown = AttackCooldown;
		if (Skills != null)
		{
			float speedMult = Skills.GetStatMultiplier("attack_speed_mult", GameCtx) * GameCtx.AuraAttackSpeedMultiplier;
			if (speedMult != 1f)
				effectiveAttackCooldown = AttackCooldown / speedMult;
		}

		if (_attackTimer > 0)
			_attackTimer -= dt;
		if (_healTimer > 0)
			_healTimer -= dt;

		switch (State)
		{
			case UnitState.Fighting:
			case UnitState.Retaliating:
				TryEngage(effectiveAttackCooldown);
				break;

			case UnitState.Sieging:
				if (_attackTimer <= 0 && TargetBuilding != null && !TargetBuilding.IsDestroyed)
				{
					TargetBuilding.TakeDamage((int)(Damage * GameCtx.AuraAttackDamageMultiplier * GetCurseDamageFactor()));
					_attackTimer = AttackCooldown;
				}
				else
				{
					NavPort.SetTarget(GameX, GameY);
				}
				break;

			case UnitState.Healing:
				TryHeal();
				break;

			case UnitState.Marching:
				TargetEnemy = null;
				NavPort.SetTarget(marchTargetGameX, marchTargetGameY);
				break;

			case UnitState.Waiting:
				TargetEnemy = null;
				NavPort.SetTarget(GameX, GameY);
				break;
		}
	}

	private void TryHeal()
	{
		if (HealTarget == null || !HealTarget.IsAlive || HealTarget.Health >= HealTarget.MaxHealth)
		{
			NavPort.SetTarget(GameX, GameY);
			return;
		}

		float dx = HealTarget.GameX - GameX;
		float dy = HealTarget.GameY - GameY;
		if (MathF.Sqrt(dx * dx + dy * dy) > HealRange)
		{
			NavPort.SetTarget(HealTarget.GameX, HealTarget.GameY);
			return;
		}

		NavPort.SetTarget(GameX, GameY);
		if (_healTimer > 0f)
			return;

		int healed = HealTarget.Heal((int)HealAmount);
		if (healed > 0)
			EventPort?.OnHeal(HealTarget, healed);
		_healTimer = HealInterval;
	}

	private void TryEngage(float effectiveAttackCooldown)
	{
		if (TargetEnemy == null || !TargetEnemy.IsAlive)
			return;
		if (EdgeDistanceTo(TargetEnemy) <= AttackRange)
			TryAttack(effectiveAttackCooldown);
		else
			NavPort.SetTarget(TargetEnemy.GameX, TargetEnemy.GameY);
	}

	private void TryAttack(float effectiveAttackCooldown)
	{
		NavPort.SetTarget(GameX, GameY);
		if (_attackTimer > 0)
			return;

		IsStealthed = false;
		StartStealthCooldown();
		_timeSinceLastCombat = 0f;

		if (Skills != null && TryCastSpell())
		{
			_attackTimer = effectiveAttackCooldown;
			return;
		}

		int effectiveDamage = Damage;
		float executeMultiplier = 1f;
		if (TargetEnemy.MaxHealth > 0 && TargetEnemy.Health / (float)TargetEnemy.MaxHealth <= GameRules.WildCovenantExecuteThreshold)
			executeMultiplier = GameCtx.AuraExecuteDamageMultiplier;
		if (Skills != null)
		{
			float dmgMult = Skills.GetStatMultiplier("attack_damage_mult", GameCtx);
			effectiveDamage = (int)(Damage * dmgMult);
			SpecialConfig ambush = Skills.GetSpecial(SpecialBehavior.Ambush);
			if (IsAmbushing && ambush != null)
			{
				effectiveDamage = (int)(effectiveDamage * ambush.DamageMult);
				IsAmbushing = false;
			}
			SpecialConfig kiting = Skills.GetSpecial(SpecialBehavior.Kiting);
			if (kiting != null)
				_kiteTimer = kiting.Param;
		}
		effectiveDamage = (int)(effectiveDamage * GameCtx.AuraAttackDamageMultiplier * GetCurseDamageFactor());
		if (executeMultiplier != 1f)
			effectiveDamage = (int)(effectiveDamage * executeMultiplier);
		int finalDamage = CombatRules.CalculateDamage(effectiveDamage, DamageType, TargetEnemy.ArmorType, TargetEnemy.EffectiveArmorValue, _damageMatrix);
		finalDamage = UndeadRules.ApplyDispelMultiplier(finalDamage, DamageType, TargetEnemy.Race);
		TargetEnemy.TakeDamage(finalDamage, this, GameX, GameY);

		if (Skills != null)
		{
			foreach (OnHitEffect effect in Skills.GetOnHitEffects())
			{
				if (effect.Type == HitEffectTypes.Slow && TargetEnemy is IUnitHandle targetHandle)
					targetHandle.ApplySlow(effect.Value, effect.Duration);
				else if (effect.Type == HitEffectTypes.Poison && TargetEnemy is IUnitHandle poisonTarget)
					poisonTarget.ApplyPoison(effect.Value, effect.Duration);
				else if (effect.Type == HitEffectTypes.Explosion)
					ApplyExplosion(effect.Value, effect.Range);
			}
		}

		EventPort?.OnAttack(TargetEnemy, TargetEnemy.GameX, TargetEnemy.GameY);
		_attackTimer = effectiveAttackCooldown;
	}

	private float EdgeDistanceTo(IUnitHandle target)
	{
		float dx = GameX - target.GameX;
		float dy = GameY - target.GameY;
		return MathF.Sqrt(dx * dx + dy * dy) - CollisionRadius - target.CollisionRadius;
	}

	public void ApplyPush(float dx, float dy)
	{
		GameX += dx;
		GameY += dy;
	}

}
