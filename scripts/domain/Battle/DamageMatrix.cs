using System.Collections.Generic;

namespace CasualCastle.Domain.Battle;

public class DamageMatrix
{
	public const float ArmorK = 100f;

	private readonly Dictionary<(DamageType, ArmorType), float> _matrix = new();

	public void Set(DamageType damage, ArmorType armor, float multiplier)
	{
		_matrix[(damage, armor)] = multiplier;
	}

	public float GetMultiplier(DamageType damage, ArmorType armor)
	{
		if (_matrix.TryGetValue((damage, armor), out float value))
			return value;
		return 1.0f;
	}

	public static double CalculateReduction(int armorValue)
	{
		if (armorValue <= 0)
			return 0.0;
		return (double)armorValue / (armorValue + ArmorK);
	}
}
