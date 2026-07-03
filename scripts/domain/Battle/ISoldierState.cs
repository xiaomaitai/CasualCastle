namespace CasualCastle.Domain.Battle;

public interface ISoldierState
{
	bool IsAlive { get; }
	bool IsPlayerUnit { get; }
	int Health { get; }
	int MaxHealth { get; }
	int Damage { get; }
	float Speed { get; }
	float AttackRange { get; }
	float AttackCooldown { get; }
	float VisionRange { get; }
	float CollisionRadius { get; }
	bool HasNightCombat { get; }
	float GameX { get; }
	float GameY { get; }
	SoldierState State { get; }
	ArmorType ArmorType { get; }
	DamageType DamageType { get; }
	string TargetDescription { get; }
}
