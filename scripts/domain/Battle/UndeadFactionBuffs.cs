namespace CasualCastle.Domain.Battle;

public static class UndeadFactionBuffs
{
	public static int MaxHealthBonus { get; private set; }

	public static void AddUndeadMaxHealthBonus(int amount)
	{
		if (amount > 0)
			MaxHealthBonus += amount;
	}

	public static void Reset()
	{
		MaxHealthBonus = 0;
	}
}