using System;

namespace CasualCastle.Domain.Battle;

public class Soldier
{
	public string TypeId { get; set; }
	public bool IsPlayerUnit { get; set; }
	public bool IsAlive { get; set; } = true;
	public float GameX { get; set; }
	public float GameY { get; set; }

	public int Health { get; set; }
	public int MaxHealth { get; set; }
	public int Damage { get; set; }
	public float Speed { get; set; }
	public float AttackRange { get; set; }
	public float AttackCooldown { get; set; }
	public float VisionRange { get; set; }
	public float CollisionRadius { get; set; }
	public bool HasNightCombat { get; set; }
	public DamageType DamageType { get; set; }
	public ArmorType ArmorType { get; set; }

	internal ISoldierService Self { get; set; }

	public ISoldierService TargetEnemy { get; set; }
	public object TargetBuilding { get; set; }
	public object TargetCastle { get; set; }
	public SoldierState State { get; set; }

	private float _attackTimer;

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
		HasNightCombat = stats.HasNightCombat;
		DamageType = stats.DamageType;
		ArmorType = stats.ArmorType;
	}

	public void TakeDamage(int amount, ISoldierService attacker, float attackerGameX, float attackerGameY)
	{
		if (!IsAlive)
			return;

		Health = CombatRules.ApplyDamage(Health, amount);

		if (Health <= 0)
		{
			IsAlive = false;
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

	public void UpdateTargeting(ISoldierService nearestEnemy, float enemyEdgeDist)
	{
		if (!IsAlive)
			return;

		if (nearestEnemy != null && nearestEnemy.IsAlive)
			State = enemyEdgeDist <= VisionRange ? SoldierState.Fighting : SoldierState.Retaliating;
		else if (TargetBuilding != null)
			State = SoldierState.Sieging;
		else
			State = SoldierState.Marching;
	}

	public (float gameX, float gameY) UpdateBehavior(float dt, float enemyEdgeDist, float marchTargetGameX, float marchTargetGameY)
	{
		if (!IsAlive)
			return (GameX, GameY);

		if (_attackTimer > 0)
			_attackTimer -= dt;

		switch (State)
		{
			case SoldierState.Fighting:
			case SoldierState.Retaliating:
				if (TargetEnemy == null || !TargetEnemy.IsAlive)
					break;
				if (enemyEdgeDist <= AttackRange)
				{
					if (_attackTimer <= 0)
					{
						int finalDamage = CombatRules.CalculateDamage(Damage, DamageType, TargetEnemy.ArmorType);
						TargetEnemy.TakeDamage(finalDamage, Self, GameX, GameY);
						_attackTimer = AttackCooldown;
					}
				}
				else
				{
					return MoveToward(dt, TargetEnemy.GameX, TargetEnemy.GameY);
				}
				break;

			case SoldierState.Sieging:
				if (_attackTimer <= 0 && TargetBuilding != null)
					_attackTimer = AttackCooldown;
				break;

			case SoldierState.Marching:
				TargetEnemy = null;
				return MoveToward(dt, marchTargetGameX, marchTargetGameY);
		}

		return (GameX, GameY);
	}

	private (float, float) MoveToward(float dt, float targetGameX, float targetGameY)
	{
		float dx = targetGameX - GameX;
		float dy = targetGameY - GameY;
		float dist = MathF.Sqrt(dx * dx + dy * dy);
		if (dist < 0.001f)
			return (GameX, GameY);

		float moveAmount = Speed * dt;
		float ratio = moveAmount / dist;
		return (GameX + dx * ratio, GameY + dy * ratio);
	}
}
