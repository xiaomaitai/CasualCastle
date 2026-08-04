using System;
using System.Collections.Generic;

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
	public SkillSet Skills { get; private set; }
	public GameContext GameCtx { get; set; } = new();

	public IUnitHandle TargetEnemy { get; set; }
	public IBuildingTarget TargetBuilding { get; set; }
	public UnitState State { get; set; }

	private float _attackTimer;
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

		Health = CombatRules.ApplyDamage(Health, amount);

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

			case UnitState.Marching:
				TargetEnemy = null;
				NavPort.SetTarget(marchTargetGameX, marchTargetGameY);
				break;
		}
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
