// 单位状态端口：只读属性查询
using System.Collections.Generic;

namespace CasualCastle.Domain.Battle;

public interface IUnitState
{
	string TypeId { get; }
	bool IsAlive { get; }
	bool IsPlayerUnit { get; }
	int Health { get; }
	int MaxHealth { get; }
	int Damage { get; }
	int ShieldHealth { get; }
	int UnitCost { get; }
	float Speed { get; }
	float AttackRange { get; }
	float AttackCooldown { get; }
	float VisionRange { get; }
	float CollisionRadius { get; }
	float GameX { get; }
	float GameY { get; }
	float PatrolRadius { get; }
	UnitState State { get; }
	ArmorType ArmorType { get; }
	int ArmorValue { get; }
	int EffectiveArmorValue { get; }
	float AuraDispelResistFraction { get; }
	DamageType DamageType { get; }
	AttackType AttackType { get; }
	string Race { get; }
	bool IsStealthed { get; }
	bool CanDetectStealth { get; }
	bool IsFurious { get; }
	bool IsAmbushing { get; }
	float HealAmount { get; }
	float HealRange { get; }
	float HealInterval { get; }
	SkillSet Skills { get; }
	float GetSkillCooldownRemaining(string skillId);
	IReadOnlyList<ActiveStatusEffect> GetActiveStatusEffects();
	IUnitHandle TargetEnemy { get; }
	string TargetDescription { get; }
}
