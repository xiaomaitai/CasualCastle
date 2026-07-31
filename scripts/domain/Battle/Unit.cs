using System;

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
	public float Speed { get; set; }
	public float AttackRange { get; set; }
	public float AttackCooldown { get; set; }
	public float VisionRange { get; set; }
	public float CollisionRadius { get; set; }
	public DamageType DamageType { get; set; }
	public ArmorType ArmorType { get; set; }
	public int ArmorValue { get; set; }
	public string Race { get; set; }
	public SkillSet Skills { get; private set; }
	public GameContext GameCtx { get; set; } = new();

	public IUnitHandle TargetEnemy { get; set; }
	public IBuildingTarget TargetBuilding { get; set; }
	public UnitState State { get; set; }

	private float _attackTimer;
	private DamageMatrix _damageMatrix;
	private readonly System.Random _rng = new();

	public string TargetDescription
	{
		get
		{
			if (TargetEnemy != null && TargetEnemy.IsAlive)
				return $"敌方士兵 ({TargetEnemy.GameX:F0}, {TargetEnemy.GameY:F0})";
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
		Speed = stats.Speed;
		AttackRange = stats.AttackRange;
		AttackCooldown = stats.AttackCooldown;
		VisionRange = stats.VisionRange;
		CollisionRadius = stats.CollisionRadius;
		DamageType = stats.DamageType;
		ArmorType = stats.ArmorType;
		ArmorValue = stats.ArmorValue;
		Race = stats.Race;
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

	public void SetBuildingTarget(IBuildingTarget building)
	{
		TargetBuilding = building;
	}

	public void ClearBuildingTarget()
	{
		TargetBuilding = null;
	}

	public void TakeDamage(int amount, IUnitHandle attacker, float attackerGameX, float attackerGameY)
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

	public void UpdateTargeting(IUnitHandle nearestEnemy, float enemyEdgeDist)
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

	public void UpdateBehavior(float dt, float enemyEdgeDist, float marchTargetGameX, float marchTargetGameY)
	{
		if (!IsAlive)
			return;

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
				if (TargetEnemy == null || !TargetEnemy.IsAlive)
					break;
				if (enemyEdgeDist <= AttackRange)
				{
					if (_attackTimer <= 0)
					{
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
				}
				else
				{
					NavPort.SetTarget(TargetEnemy.GameX, TargetEnemy.GameY);
				}
				break;

			case UnitState.Sieging:
				if (_attackTimer <= 0 && TargetBuilding != null && !TargetBuilding.IsDestroyed)
				{
					TargetBuilding.TakeDamage(Damage);
					_attackTimer = AttackCooldown;
				}
				break;

			case UnitState.Marching:
				TargetEnemy = null;
				NavPort.SetTarget(marchTargetGameX, marchTargetGameY);
				break;
		}
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
