// 亡灵阵营增益：全局生命上限加成
namespace CasualCastle.Domain.Battle;

public static class UndeadFactionBuffs
{
	public static int MaxHealthBonus { get; private set; }

	public static float DamageMultiplier { get; private set; } = 1f;

	public static void AddUndeadMaxHealthBonus(int amount)
	{
		if (amount > 0)
			MaxHealthBonus += amount;
	}

	public static void AddUndeadDamageMultiplier(float amount)
	{
		if (amount > 0f)
			DamageMultiplier += amount;
	}

	public static void Reset()
	{
		MaxHealthBonus = 0;
		DamageMultiplier = 1f;
	}
}