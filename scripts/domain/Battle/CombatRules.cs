// 战斗规则：伤害计算与血量扣减
namespace CasualCastle.Domain.Battle;

public static class CombatRules
{
	public static int CalculateDamage(int baseDamage, DamageType damageType, ArmorType armorType, int armorValue, DamageMatrix damageMatrix)
	{
		if (baseDamage <= 0)
			return 0;
		float typeMultiplier = damageMatrix.GetMultiplier(damageType, armorType);
		double reduction = DamageMatrix.CalculateReduction(armorValue);
		int finalDamage = (int)(baseDamage * typeMultiplier * (1.0 - reduction));
		return System.Math.Max(1, finalDamage);
	}

	public static int ApplyDamage(int currentHealth, int damage)
	{
		if (damage <= 0 || currentHealth <= 0)
			return currentHealth;
		return System.Math.Max(0, currentHealth - damage);
	}
}
