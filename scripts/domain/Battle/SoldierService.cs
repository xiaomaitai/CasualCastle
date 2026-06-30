namespace CasualCastle.Domain.Battle;

public class SoldierService : ISoldierService
{
	internal Soldier Aggregate { get; }
	public ISoldierEventPort EventPort { get; set; }

	public SoldierService()
	{
		Aggregate = new Soldier();
		Aggregate.Self = this;
	}

	public bool IsAlive => Aggregate.IsAlive;
	public bool IsPlayerUnit => Aggregate.IsPlayerUnit;
	public int Health => Aggregate.Health;
	public int MaxHealth => Aggregate.MaxHealth;
	public int Damage => Aggregate.Damage;
	public float Speed => Aggregate.Speed;
	public float AttackRange => Aggregate.AttackRange;
	public float AttackCooldown => Aggregate.AttackCooldown;
	public float VisionRange => Aggregate.VisionRange;
	public float CollisionRadius => Aggregate.CollisionRadius;
	public bool HasNightCombat { get => Aggregate.HasNightCombat; set => Aggregate.HasNightCombat = value; }
	public float GameX { get => Aggregate.GameX; set => Aggregate.GameX = value; }
	public float GameY { get => Aggregate.GameY; set => Aggregate.GameY = value; }
	public SoldierState State => Aggregate.State;
	public ArmorType ArmorType => Aggregate.ArmorType;
	public IBuildingTarget TargetBuilding { get => Aggregate.TargetBuilding; set => Aggregate.TargetBuilding = value; }
	public object TargetCastle { get => Aggregate.TargetCastle; set => Aggregate.TargetCastle = value; }

	public void Initialize(UnitStats stats, bool isPlayerUnit)
	{
		Aggregate.Initialize(stats, isPlayerUnit);
	}

	public void SetEnemyTarget(ISoldierService target)
	{
		Aggregate.TargetEnemy = target;
	}

	public void UpdateTargeting(ISoldierService nearestEnemy, float enemyEdgeDist)
	{
		Aggregate.UpdateTargeting(nearestEnemy, enemyEdgeDist);
	}

	public (float gameX, float gameY) UpdateBehavior(float dt, float enemyEdgeDist, float marchGameX, float marchGameY)
	{
		return Aggregate.UpdateBehavior(dt, enemyEdgeDist, marchGameX, marchGameY);
	}

	public void TakeDamage(int amount, ISoldierService attacker, float attackerGameX, float attackerGameY)
	{
		Aggregate.TakeDamage(amount, attacker, attackerGameX, attackerGameY);
		EventPort?.OnDamaged(amount, attacker);
		if (!Aggregate.IsAlive)
			EventPort?.OnDied();
	}
}
