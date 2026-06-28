namespace CasualCastle.Domain.Battle;

public static class DamageMatrix
{
    private static float[,] _multiplier = new float[4, 4];

    static DamageMatrix()
    {
        for (int d = 0; d < 4; d++)
            for (int a = 0; a < 4; a++)
                _multiplier[d, a] = 1.0f;
    }

    public static void LoadFrom(float[,] matrix)
    {
        _multiplier = matrix;
    }

    public static float GetMultiplier(DamageType damage, ArmorType armor)
    {
        return _multiplier[(int)damage, (int)armor];
    }
}
