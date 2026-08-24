// 战斗单位聚合：状态、索敌、战斗、技能与状态效果
using System;
using CasualCastle.Domain.Shared;
using System.Collections.Generic;

namespace CasualCastle.Domain.Battle;

public partial class Unit : IUnitHandle
{
	public IUnitEventPort EventPort { get; set; }

	public string TypeId { get; set; }

	public bool IsPlayerUnit { get; set; }

	public bool IsAlive { get; set; } = true;

	public float GameX { get; internal set; }

	public float GameY { get; internal set; }

	public int Health { get; private set; }

	public int MaxHealth { get; private set; }

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
	public float AuraDispelResistFraction => GameCtx.AuraDispelResistFraction;

	public string Race { get; set; }

	public bool IsFurious { get; private set; }

	public bool IsAmbushing { get; private set; }

	public bool IsStealthed { get; private set; }

	private bool _chargeUsed;

	public bool CanDetectStealth { get; set; }

	public bool HasNightCombat { get; set; }

	public float HealAmount { get; set; }

	public float HealRange { get; set; }

	public float HealInterval { get; set; }

	public int ShieldHealth { get; private set; }

	public ITacticalQueries Tactical { get; set; }

	public FactionResources Resources { get; set; }

	public SkillSet Skills { get; private set; }

	public GameContext GameCtx { get; set; } = new();

	public IUnitHandle TargetEnemy { get; set; }

	public IUnitHandle HealTarget { get; set; }

	public IBuildingTarget TargetBuilding { get; set; }

	public IBuildingRef AssignedTavern { get; set; }

	public bool IsDefender { get; private set; }

	public float PatrolCenterX { get; private set; }

	public float PatrolCenterY { get; private set; }

	public float PatrolRadius { get; private set; }

	public bool IsResting { get; private set; }

	public void SetDefender(float centerX, float centerY, float radius)
	{
		IsDefender = true;
		PatrolCenterX = centerX;
		PatrolCenterY = centerY;
		PatrolRadius = radius;
	}

	public void SetResting(bool resting)
	{
		IsResting = resting;
	}

	public void SetPatrolRadius(float radius)
	{
		PatrolRadius = radius;
	}

	public bool HasMoveGoal { get; private set; }

	public float MoveGoalX { get; private set; }

	public float MoveGoalY { get; private set; }

	public float NavVelX { get; private set; }

	public float NavVelY { get; private set; }

	public void SetNavVelocity(float vx, float vy)
	{
		NavVelX = vx;
		NavVelY = vy;
	}

	private void SetMoveGoal(float x, float y)
	{
		HasMoveGoal = true;
		MoveGoalX = x;
		MoveGoalY = y;
	}

	private void StopMoving()
	{
		HasMoveGoal = false;
	}

	public UnitState State { get; set; }

	private float _attackTimer;

	private float _healTimer;

	private DamageMatrix _damageMatrix;

	private readonly System.Random _rng = new();

	private SkillRuntime _skillRuntime;

	private StatusEffects _status;

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

	public Unit()
	{
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
		_skillRuntime = new SkillRuntime(Skills);
		_status = new StatusEffects(this);
	}

	internal void SetAmbushing(bool value)
	{
		IsAmbushing = value;
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

	public void TeleportTo(float gameX, float gameY)
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

		if (DemonKingRules.IsDemonKing(Race))
		{
			_status?.NotifyCombat();
			Resources?.Comfort?.SpendHit();
			if (Skills != null && Skills.HasTrigger(StatModifierTrigger.OnDamaged))
				IsFurious = true;
			EventPort?.OnDamaged(amount, attacker);
			return;
		}

		_status?.NotifyCombat();
		IsAmbushing = false;

		if (Skills != null && GameCtx != null)
		{
			float dodgeChance = Skills.GetDodgeChance(GameCtx);
			if (dodgeChance > 0f && _rng.NextDouble() < dodgeChance)
				return;
		}

		int remaining = amount;
		if (GameCtx != null && GameCtx.AuraDamageReductionMultiplier != 1f)
			remaining = (int)(remaining * GameCtx.AuraDamageReductionMultiplier);
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

		if (IsDefender)
		{
			UpdateDefenderTargeting(nearestEnemy);
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

	private void UpdateDefenderTargeting(IUnitHandle nearestEnemy)
	{
		if (IsResting)
		{
			TargetEnemy = null;
			State = UnitState.Marching;
			return;
		}

		float dx = GameX - PatrolCenterX;
		float dy = GameY - PatrolCenterY;
		float distanceToAnchor = MathF.Sqrt(dx * dx + dy * dy);
		if (distanceToAnchor > PatrolRadius)
		{
			TargetEnemy = null;
			State = UnitState.Marching;
			return;
		}

		if (nearestEnemy != null && nearestEnemy.IsAlive)
		{
			float enemyDx = nearestEnemy.GameX - GameX;
			float enemyDy = nearestEnemy.GameY - GameY;
			float enemyDistance = MathF.Sqrt(enemyDx * enemyDx + enemyDy * enemyDy);
			if (enemyDistance <= PatrolRadius)
			{
				TargetEnemy = nearestEnemy;
				State = UnitState.Fighting;
				return;
			}
		}

		TargetEnemy = null;
		State = UnitState.Waiting;
	}

	public void UpdateBehavior(float dt, float marchTargetGameX, float marchTargetGameY)
	{
		if (!IsAlive)
			return;

		TickSkillCooldowns(dt);
		TickSummon(dt);
		_status?.Tick(dt);

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
					int siegeDamage = (int)(Damage * GameCtx.AuraAttackDamageMultiplier * GetCurseDamageFactor());
					if (UndeadRules.IsUndead(Race))
						siegeDamage = (int)(siegeDamage * GameCtx.UndeadDamageMultiplier);
					TargetBuilding.TakeDamage(siegeDamage);
					if (GameCtx.AuraLifestealFraction > 0f)
						Heal((int)(siegeDamage * GameCtx.AuraLifestealFraction));
					_attackTimer = AttackCooldown;
				}
				else
				{
					StopMoving();
				}
				break;

			case UnitState.Healing:
				TryHeal();
				break;

			case UnitState.Marching:
				TargetEnemy = null;
				SetMoveGoal(marchTargetGameX, marchTargetGameY);
				break;

			case UnitState.Waiting:
				TargetEnemy = null;
				StopMoving();
				break;
		}
	}

	private void TryHeal()
	{
		if (HealTarget == null || !HealTarget.IsAlive || HealTarget.Health >= HealTarget.MaxHealth)
		{
			StopMoving();
			return;
		}

		float dx = HealTarget.GameX - GameX;
		float dy = HealTarget.GameY - GameY;
		if (MathF.Sqrt(dx * dx + dy * dy) > HealRange)
		{
			SetMoveGoal(HealTarget.GameX, HealTarget.GameY);
			return;
		}

		StopMoving();
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
		{
			StopMoving();
			return;
		}
		if (EdgeDistanceTo(TargetEnemy) <= AttackRange)
			TryAttack(effectiveAttackCooldown);
		else
			SetMoveGoal(TargetEnemy.GameX, TargetEnemy.GameY);
	}

	private void TryAttack(float effectiveAttackCooldown)
	{
		StopMoving();
		if (_attackTimer > 0)
			return;

		IsStealthed = false;
		StartStealthCooldown();
		_status?.NotifyCombat();
		if (DemonKingRules.IsDemonKing(Race))
			Resources?.Comfort?.SpendAttack();

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
			if (kiting != null && _status != null)
				_status.MarkKite(kiting.Param);
			SpecialConfig charge = Skills.GetSpecial(SpecialBehavior.Charge);
			if (!_chargeUsed && charge != null)
			{
				effectiveDamage = (int)(effectiveDamage * charge.DamageMult);
				_chargeUsed = true;
			}
		}
		effectiveDamage = (int)(effectiveDamage * GameCtx.AuraAttackDamageMultiplier * GetCurseDamageFactor());
		if (UndeadRules.IsUndead(Race))
			effectiveDamage = (int)(effectiveDamage * GameCtx.UndeadDamageMultiplier);
		if (executeMultiplier != 1f)
			effectiveDamage = (int)(effectiveDamage * executeMultiplier);
		int finalDamage = CombatRules.CalculateDamage(effectiveDamage, DamageType, TargetEnemy.ArmorType, TargetEnemy.EffectiveArmorValue, _damageMatrix);
		finalDamage = UndeadRules.ApplyDispelMultiplier(finalDamage, DamageType, TargetEnemy.Race, TargetEnemy.AuraDispelResistFraction);
		TargetEnemy.TakeDamage(finalDamage, this, GameX, GameY);
		if (GameCtx.AuraLifestealFraction > 0f)
			Heal((int)(finalDamage * GameCtx.AuraLifestealFraction));

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

	private void ApplyGraceEffects(int healed)
	{
		if (Skills == null)
			return;
		foreach (OnHealEffect effect in Skills.GetOnHealEffects())
		{
			if (effect.Type == HealEffectTypes.Shield)
				AddShield((int)(healed * GameRules.GraceShieldMult));
			else if (effect.Type == HealEffectTypes.CooldownReduction)
				_skillRuntime?.Reduce(GameRules.GraceCooldownReduction);
			else if (effect.Type == HealEffectTypes.Burn)
				ApplyBurn(effect);
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
		return _skillRuntime?.GetRemaining(skillId) ?? 0f;
	}

	public void StartSkillCooldown(string skillId)
	{
		_skillRuntime?.Start(skillId);
	}

	public void TickSkillCooldowns(float dt)
	{
		_skillRuntime?.Tick(dt);
	}

	private void TickSummon(float dt)
	{
		if (Skills == null || EventPort == null)
			return;
		SpecialConfig summon = Skills.GetSpecial(SpecialBehavior.Summon);
		if (summon == null || string.IsNullOrEmpty(summon.UnitType) || GetSkillCooldownRemaining(summon.SkillId) > 0f)
			return;
		EventPort.OnSummon(summon.UnitType, GameX, GameY);
		StartSkillCooldown(summon.SkillId);
	}

	private void StartStealthCooldown()
	{
		_skillRuntime?.StartStealth();
	}

	private bool TryCastSpell()
	{
		if (TargetEnemy == null || !TargetEnemy.IsAlive)
			return false;
		SpecialConfig nova = Skills.GetSpecial(SpecialBehavior.ShadowNova);
		if (nova != null && Resources?.BlackMagic != null && CanCastSpell(nova) && Tactical != null
			&& Tactical.FindEnemiesNearPoint(this, TargetEnemy.GameX, TargetEnemy.GameY, nova.Param).Count > 1)
			return CastNova(nova);
		SpecialConfig curse = Skills.GetSpecial(SpecialBehavior.Curse);
		if (curse != null && CanCastSpell(curse))
			return CastCurse(curse);
		SpecialConfig manaStorm = Skills.GetSpecial(SpecialBehavior.ManaStorm);
		if (manaStorm != null && CanCastManaStorm(manaStorm) && Tactical != null
			&& Tactical.FindEnemiesNearPoint(this, TargetEnemy.GameX, TargetEnemy.GameY, manaStorm.Param).Count > 0)
			return CastManaStorm(manaStorm);
		return false;
	}

	private bool CanCastSpell(SpecialConfig cfg)
	{
		BlackMagicService blackMagic = Resources?.BlackMagic;
		return cfg.Cost > 0 && blackMagic != null && blackMagic.Value >= cfg.Cost && GetSkillCooldownRemaining(cfg.SkillId) <= 0f;
	}

	private bool CanCastManaStorm(SpecialConfig cfg)
	{
		ManaService mana = Resources?.Mana;
		return mana != null && cfg.Cost > 0 && mana.Value >= cfg.Cost
			&& GetSkillCooldownRemaining(cfg.SkillId) <= 0f;
	}

	private bool CastManaStorm(SpecialConfig cfg)
	{
		Resources?.Mana?.TrySpend(cfg.Cost);
		StartSkillCooldown(cfg.SkillId);
		foreach (IUnitHandle enemy in Tactical.FindEnemiesNearPoint(this, TargetEnemy.GameX, TargetEnemy.GameY, cfg.Param))
			enemy.TakeDamage((int)cfg.Value, this, GameX, GameY);
		EventPort?.OnAttack(TargetEnemy, TargetEnemy.GameX, TargetEnemy.GameY);
		return true;
	}

	private bool CastCurse(SpecialConfig cfg)
	{
		Resources?.BlackMagic?.TrySpend(cfg.Cost);
		StartSkillCooldown(cfg.SkillId);
		TargetEnemy.ApplySlow(cfg.Value, cfg.Param);
		TargetEnemy.ApplyCurse(cfg.Value, cfg.Param);
		EventPort?.OnAttack(TargetEnemy, TargetEnemy.GameX, TargetEnemy.GameY);
		return true;
	}

	private bool CastNova(SpecialConfig cfg)
	{
		Resources?.BlackMagic?.TrySpend(cfg.Cost);
		StartSkillCooldown(cfg.SkillId);
		foreach (IUnitHandle enemy in Tactical.FindEnemiesNearPoint(this, TargetEnemy.GameX, TargetEnemy.GameY, cfg.Param))
			enemy.TakeDamage((int)cfg.Value, this, GameX, GameY);
		EventPort?.OnAttack(TargetEnemy, TargetEnemy.GameX, TargetEnemy.GameY);
		return true;
	}


	public float GetCurrentSpeed()
	{
		float speedMult = Skills.GetStatMultiplier("move_speed_mult", GameCtx) * GameCtx.AuraMoveSpeedMultiplier;
		speedMult = _status != null ? _status.ApplyDebuffMultipliers(speedMult, Skills) : speedMult;
		if (!IsStealthed)
			return Speed * speedMult;
		SpecialConfig stealth = Skills.GetSpecial(SpecialBehavior.Stealth);
		if (stealth == null)
			return Speed * speedMult;
		return Speed * stealth.SpeedMult * speedMult;
	}

	public void ApplySlow(float slowValue, float duration)
	{
		_status?.ApplySlow(slowValue, duration);
	}

	public void ApplyPoison(float damagePerSecond, float duration)
	{
		_status?.ApplyPoison(damagePerSecond, duration);
	}

	public void ApplyCurse(float factor, float duration)
	{
		_status?.ApplyCurse(factor, duration);
	}

	private float GetCurseDamageFactor()
	{
		return _status?.GetCurseDamageFactor() ?? 1f;
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
