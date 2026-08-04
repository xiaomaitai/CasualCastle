using System;
using System.Collections.Generic;
using CasualCastle.Domain.Shared;

namespace CasualCastle.Domain.Battle;

public class Unit : IUnitHandle
{
	internal INavigationPort NavPort { get; }
	public IUnitEventPort EventPort { get; set; }

	public string TypeId { get; set; }
	public bool IsPlayerUnit { get; set; }
	public bool IsAlive { get; set; } = true;
	public float GameX { get; internal set; }
	public float GameY { get; internal set; }

	public int Health { get; set; }
	public int MaxHealth { get; set; }
	public int Damage { get; set; }
	public int UnitCost { get; set; }
	public float Speed { get; set; }
	public float AttackRange { get; set; }
	public float AttackCooldown { get; set; }
	public float VisionRange { get; set; }
	public float CollisionRadius { get; set; }
	public DamageType DamageType { get; set; }
	public AttackType AttackType { get; set; }
	public ArmorType ArmorType { get; set; }
	public int ArmorValue { get; set; }
	public string Race { get; set; }
	public bool IsStealthed { get; private set; }
	public bool CanDetectStealth { get; set; }
	public float HealAmount { get; set; }
	public float HealRange { get; set; }
	public float HealInterval { get; set; }
	public int ShieldHealth { get; private set; }
	public ITacticalQueries Tactical { get; set; }
	public SkillSet Skills { get; private set; }
	public GameContext GameCtx { get; set; } = new();

	public IUnitHandle TargetEnemy { get; set; }
	public IUnitHandle HealTarget { get; set; }
	public IBuildingTarget TargetBuilding { get; set; }
	public UnitState State { get; set; }

	private float _attackTimer;
	private float _healTimer;
	private DamageMatrix _damageMatrix;
	private readonly System.Random _rng = new();
	private readonly Dictionary<string, float> _skillCooldowns = new();

	public string TargetDescription
	{
		get
		{
			if (TargetEnemy != null && TargetEnemy.IsAlive)
				return $"敌方单位 ({TargetEnemy.GameX:F0}, {TargetEnemy.GameY:F0})";
			if (TargetBuilding != null)
				return "建筑";
			return "行军目标";
		}
	}

	public Unit(INavigationPort navPort)
	{
		NavPort = navPort;
	}

	public void Initialize(UnitStats stats, bool isPlayerUnit)
	{
		TypeId = stats.TypeId;
		IsPlayerUnit = isPlayerUnit;
		MaxHealth = stats.Health;
		Health = stats.Health;
		Damage = stats.Damage;
		UnitCost = stats.UnitCost;
		Speed = stats.Speed;
		AttackRange = stats.AttackRange;
		AttackCooldown = stats.AttackCooldown;
		VisionRange = stats.VisionRange;
		CollisionRadius = stats.CollisionRadius;
		DamageType = stats.DamageType;
		AttackType = stats.AttackType;
		ArmorType = stats.ArmorType;
		ArmorValue = stats.ArmorValue;
		Race = stats.Race;
		CanDetectStealth = stats.CanDetectStealth;
		HealAmount = stats.HealAmount;
		HealRange = stats.HealRange;
		HealInterval = stats.HealInterval;
		Skills = new SkillSet();
	}

	public void SetGameContext(GameContext ctx)
	{
		GameCtx = ctx;
	}

	public void SetDamageMatrix(DamageMatrix dm)
	{
		_damageMatrix = dm;
	}

	public void SetPosition(float gameX, float gameY)
	{
		GameX = gameX;
		GameY = gameY;
	}

	public void SetEnemyTarget(IUnitHandle target)
	{
		TargetEnemy = target;
	}

	public void SetStealth(bool stealthed)
	{
		IsStealthed = stealthed;
	}

	public int Heal(int amount)
	{
		if (!IsAlive || amount <= 0)
			return 0;

		int healable = MaxHealth - Health;
		if (healable <= 0)
			return 0;

		int healed = System.Math.Min(healable, amount);
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

	private void ApplyGraceEffects(int healed)
	{
		if (Skills == null)
			return;
		foreach (OnHealEffect effect in Skills.GetOnHealEffects())
		{
			if (effect.Type == "shield")
				AddShield((int)(healed * GameRules.GraceShieldMult));
			else if (effect.Type == "cooldown_reduction")
				ReduceSkillCooldowns(GameRules.GraceCooldownReduction);
			else if (effect.Type == "burn")
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

	public float GetCurrentSpeed()
	{
		if (!IsStealthed)
			return Speed;
		SpecialConfig stealth = Skills.GetSpecial(SpecialBehavior.Stealth);
		if (stealth == null)
			return Speed;
		return Speed * stealth.SpeedMult;
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

	public void SetBuildingTarget(IBuildingTarget building)
	{
		TargetBuilding = building;
	}

	public void ClearBuildingTarget()
	{
		TargetBuilding = null;
	}

	public void TakeDamage(int amount, IUnitHandle attacker, float attackerGameX, float attackerGameY, bool isAoe = false)
	{
		if (!IsAlive)
			return;

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

		if (isAoe && IsStealthed)
		{
			IsStealthed = false;
			StartStealthCooldown();
		}

		EventPort?.OnDamaged(amount, attacker);

		if (Health <= 0)
		{
			IsAlive = false;
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

		float effectiveAttackCooldown = AttackCooldown;
		if (Skills != null)
		{
			float speedMult = Skills.GetStatMultiplier("attack_speed_mult", GameCtx);
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
					TargetBuilding.TakeDamage(Damage);
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

		int effectiveDamage = Damage;
		if (Skills != null)
		{
			float dmgMult = Skills.GetStatMultiplier("attack_damage_mult", GameCtx);
			effectiveDamage = (int)(Damage * dmgMult);
		}
		int finalDamage = CombatRules.CalculateDamage(effectiveDamage, DamageType, TargetEnemy.ArmorType, TargetEnemy.ArmorValue, _damageMatrix);
		TargetEnemy.TakeDamage(finalDamage, this, GameX, GameY);

		if (Skills != null)
		{
			foreach (OnHitEffect effect in Skills.GetOnHitEffects())
			{
				if (effect.Type == "slow" && TargetEnemy is IUnitHandle targetHandle)
					ApplySlow(targetHandle, effect.Value, effect.Duration);
			}
		}

		EventPort?.OnAttack(TargetEnemy, TargetEnemy.GameX, TargetEnemy.GameY);
		_attackTimer = effectiveAttackCooldown;
	}

	private void StartStealthCooldown()
	{
		SpecialConfig stealth = Skills.GetSpecial(SpecialBehavior.Stealth);
		if (stealth != null)
			StartSkillCooldown(stealth.SkillId);
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

	private static void ApplySlow(IUnitHandle target, float slowValue, float duration)
	{
	}
}
