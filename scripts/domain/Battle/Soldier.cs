using System;

namespace CasualCastle.Domain.Battle;

public class Soldier : ISoldierHandle
{
	internal INavigationPort NavPort { get; }
	public ISoldierEventPort EventPort { get; set; }

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
	public bool HasNightCombat { get; set; }
	public DamageType DamageType { get; set; }
	public ArmorType ArmorType { get; set; }

	public ISoldierHandle TargetEnemy { get; set; }
	public IBuildingTarget TargetBuilding { get; set; }
	public SoldierState State { get; set; }

	private float _attackTimer;

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

	public Soldier(INavigationPort navPort)
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
		HasNightCombat = stats.HasNightCombat;
		DamageType = stats.DamageType;
		ArmorType = stats.ArmorType;
	}

	public void SetPosition(float gameX, float gameY)
	{
		GameX = gameX;
		GameY = gameY;
	}

	public void SetEnemyTarget(ISoldierHandle target)
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

	public void TakeDamage(int amount, ISoldierHandle attacker, float attackerGameX, float attackerGameY)
	{
		if (!IsAlive)
			return;

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

	public void UpdateTargeting(ISoldierHandle nearestEnemy, float enemyEdgeDist)
	{
		if (!IsAlive)
			return;

		if (nearestEnemy != null && nearestEnemy.IsAlive)
		{
			TargetEnemy = nearestEnemy;
			State = SoldierState.Fighting;
		}
		else if (TargetEnemy != null && TargetEnemy.IsAlive)
		{
			State = SoldierState.Retaliating;
		}
		else if (TargetBuilding != null)
		{
			TargetEnemy = null;
			State = SoldierState.Sieging;
		}
		else
		{
			TargetEnemy = null;
			State = SoldierState.Marching;
		}
	}

	public void UpdateBehavior(float dt, float enemyEdgeDist, float marchTargetGameX, float marchTargetGameY)
	{
		if (!IsAlive)
			return;

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
					TargetEnemy.TakeDamage(finalDamage, this, GameX, GameY);
					EventPort?.OnAttack(TargetEnemy, TargetEnemy.GameX, TargetEnemy.GameY);
					_attackTimer = AttackCooldown;
				}
				}
				else
				{
					NavPort.SetTarget(TargetEnemy.GameX, TargetEnemy.GameY);
				}
				break;

			case SoldierState.Sieging:
				if (_attackTimer <= 0 && TargetBuilding != null && !TargetBuilding.IsDestroyed)
				{
					TargetBuilding.TakeDamage(Damage);
					_attackTimer = AttackCooldown;
				}
				break;

			case SoldierState.Marching:
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
}
