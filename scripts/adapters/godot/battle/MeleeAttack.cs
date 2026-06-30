using CasualCastle.Domain.Battle;

public class MeleeAttack : AttackBehavior
{
	public MeleeAttack(Soldier soldier) : base(soldier) { }

	public override bool TryExecute(Soldier target, float delta)
	{
		if (!TargetInRange(target))
			return false;

		int finalDamage = CombatRules.CalculateDamage(
			Soldier.Damage, Soldier.Data.DamageType, target.Data.ArmorType);
		target.TakeDamage(finalDamage, Soldier);
		return true;
	}
}
