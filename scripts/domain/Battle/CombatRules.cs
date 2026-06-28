namespace CasualCastle.Domain.Battle;

public static class CombatRules
{
	public static int ApplyDamage(int currentHealth, int damage)
	{
		if (damage <= 0 || currentHealth <= 0)
			return currentHealth;
		return System.Math.Max(0, currentHealth - damage);
	}

	public static bool CanAttack(float attackTimer) => attackTimer <= 0f;

	public static float TickCooldown(float currentTimer, float delta)
	{
		if (currentTimer <= 0f)
			return 0f;
		return currentTimer - delta;
	}

	public static float ResetCooldown(float cooldownDuration) => cooldownDuration;
}
