namespace CasualCastle.Domain.Battle;

public interface IUnitState
{
	string TypeId { get; }
	bool IsAlive { get; }
	bool IsPlayerUnit { get; }
	int Health { get; }
	int MaxHealth { get; }
	int Damage { get; }
	int UnitCost { get; }
	float Speed { get; }
	float AttackRange { get; }
	float AttackCooldown { get; }
	float VisionRange { get; }
	float CollisionRadius { get; }
	float GameX { get; }
	float GameY { get; }
	UnitState State { get; }
	ArmorType ArmorType { get; }
	int ArmorValue { get; }
	DamageType DamageType { get; }
	AttackType AttackType { get; }
	string Race { get; }
	bool IsStealthed { get; }
	bool CanDetectStealth { get; }
	SkillSet Skills { get; }
	float GetSkillCooldownRemaining(string skillId);
	IUnitHandle TargetEnemy { get; }
	string TargetDescription { get; }
}
