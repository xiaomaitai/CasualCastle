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

	public static bool CanAttack(float attackTimer)
	{
		return attackTimer <= 0f;
	}

	public static float TickCooldown(float currentTimer, float delta)
	{
		if (currentTimer <= 0f)
			return 0f;
		return currentTimer - delta;
	}

	public static float ResetCooldown(float cooldownDuration)
	{
		return cooldownDuration;
	}
}
