namespace CasualCastle.Domain.Battle;

public static class DamageMatrix
{
	private static readonly float[,] Multiplier =
	{
		//              Light  Heavy  Fortified  Beast
		/* Normal */  { 1.0f,  0.75f, 0.5f,      1.0f  },
		/* Pierce */  { 0.75f, 1.5f,  1.0f,      0.75f },
		/* Siege */   { 0.5f,  1.0f,  1.5f,      1.0f  },
		/* Magic */   { 1.0f,  1.0f,  1.25f,     1.5f  },
	};

	public static float GetMultiplier(DamageType damage, ArmorType armor)
	{
		return Multiplier[(int)damage, (int)armor];
	}
}
