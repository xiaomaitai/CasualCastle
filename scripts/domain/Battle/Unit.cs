// 战斗单位聚合：状态、索敌、战斗、技能与状态效果
using System;
using CasualCastle.Domain.Shared;
using System.Collections.Generic;

namespace CasualCastle.Domain.Battle;

public partial class Unit : IUnitHandle
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

	public int EffectiveArmorValue => ArmorValue + GameCtx.AuraArmorBonus;

	public string Race { get; set; }

	public bool IsFurious { get; private set; }

	public bool IsAmbushing { get; private set; }

	public bool IsStealthed { get; private set; }

	public bool CanDetectStealth { get; set; }

	public bool HasNightCombat { get; set; }

	public float HealAmount { get; set; }

	public float HealRange { get; set; }

	public float HealInterval { get; set; }

	public int ShieldHealth { get; private set; }

	public ITacticalQueries Tactical { get; set; }

	public BlackMagicService BlackMagic { get; set; }

	public SkillSet Skills { get; private set; }

	public GameContext GameCtx { get; set; } = new();

	public IUnitHandle TargetEnemy { get; set; }

	public IUnitHandle HealTarget { get; set; }

	public IBuildingTarget TargetBuilding { get; set; }

	public IBuildingRef AssignedTavern { get; set; }

	public UnitState State { get; set; }

	private float _attackTimer;

	private float _healTimer;

	private DamageMatrix _damageMatrix;

	private readonly System.Random _rng = new();

	private readonly Dictionary<string, float> _skillCooldowns = new();

	private float _kiteTimer;

	private float _slowRemaining;

	private float _slowFactor;

	private float _curseRemaining;

	private float _curseFactor;

	private float _poisonDamagePerSecond;

	private float _poisonRemaining;

	private float _poisonAccumulator;

	private float _timeSinceLastCombat;

	private float _auraRegenAccumulator;

	private float _wildHeartAccumulator;

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
		HasNightCombat = stats.HasNightCombat;
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

	public void SetBuildingTarget(IBuildingTarget building)
	{
		TargetBuilding = building;
	}

	public void ClearBuildingTarget()
	{
		TargetBuilding = null;
	}


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
