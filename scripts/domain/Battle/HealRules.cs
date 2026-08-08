// 治疗规则：治疗量与上限钳制
using System;

namespace CasualCastle.Domain.Battle;

public static class HealRules
{
	public static int ApplyHeal(int currentHealth, int maxHealth, int amount)
	{
		if (amount <= 0 || currentHealth <= 0)
			return currentHealth;
		return Math.Min(maxHealth, currentHealth + amount);
	}
}
