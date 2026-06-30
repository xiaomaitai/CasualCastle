namespace CasualCastle.Domain.Battle;

public interface ISoldierService
{
	bool IsAlive { get; }
	bool IsPlayerUnit { get; }
	int Health { get; }
	int Damage { get; }
	float Speed { get; }
	float AttackRange { get; }
	float AttackCooldown { get; }
	float VisionRange { get; }
	float CollisionRadius { get; }
	bool HasNightCombat { get; set; }
	float GameX { get; set; }
	float GameY { get; set; }
	SoldierState State { get; }
	ArmorType ArmorType { get; }
	object TargetBuilding { get; set; }
	object TargetCastle { get; set; }

	void Initialize(UnitStats stats, bool isPlayerUnit);
	void SetEnemyTarget(ISoldierService target);
	void UpdateTargeting(ISoldierService nearestEnemy, float enemyEdgeDist);
	(float gameX, float gameY) UpdateBehavior(float dt, float enemyEdgeDist, float marchGameX, float marchGameY);
	void TakeDamage(int amount, ISoldierService attacker, float attackerGameX, float attackerGameY);
}
