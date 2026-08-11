// 伤害矩阵：攻防类型倍率与护甲减伤
using System.Collections.Generic;

namespace CasualCastle.Domain.Battle;

public class DamageMatrix
{
	public const double ArmorDamageTakenMultiplier = 0.95;
	public const int PercentScale = 100;

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

	public static float PercentToMultiplier(int percent)
	{
		return percent / (float)PercentScale;
	}

	public static int MultiplierToPercent(float multiplier)
	{
		return (int)System.Math.Round(multiplier * PercentScale);
	}

	public static double CalculateReduction(int armorValue)
	{
		return 1.0 - System.Math.Pow(ArmorDamageTakenMultiplier, armorValue);
	}
}
