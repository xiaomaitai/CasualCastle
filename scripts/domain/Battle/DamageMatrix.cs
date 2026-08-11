// 伤害矩阵：攻防类型倍率与护甲减伤
using System.Collections.Generic;

namespace CasualCastle.Domain.Battle;

public class DamageMatrix
{
	public const int ArmorSegmentCount = 6;
	public const int PercentScale = 100;

	private static readonly int[] ArmorSegmentEnds = { 5, 10, 15, 20, 25, 30 };
	private static readonly double[] ArmorSegmentSlopes = { 0.05, 0.03, 0.02, 0.015, 0.01, 0.005 };

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
		long magnitude = armorValue < 0 ? -(long)armorValue : armorValue;
		double reduction = 0.0;
		long previousEnd = 0;
		for (int i = 0; i < ArmorSegmentCount; i++)
		{
			long end = ArmorSegmentEnds[i];
			double slope = ArmorSegmentSlopes[i];
			if (magnitude <= end)
			{
				reduction += slope * (magnitude - previousEnd);
				break;
			}
			reduction += slope * (end - previousEnd);
			previousEnd = end;
		}
		if (armorValue < 0)
			return -reduction;
		return reduction;
	}
}
