namespace CasualCastle.Domain.Battle;

public class DamageMatrix
{
	private float[,] _multiplier;

	public DamageMatrix()
	{
		_multiplier = new float[4, 4];
		for (int d = 0; d < 4; d++)
			for (int a = 0; a < 4; a++)
				_multiplier[d, a] = 1.0f;
	}

	public void LoadFrom(float[,] matrix)
	{
		_multiplier = matrix;
	}

	public float GetMultiplier(DamageType damage, ArmorType armor)
	{
		return _multiplier[(int)damage, (int)armor];
	}
}
