using System.Collections.Generic;

namespace CasualCastle.Domain.Battle;

public static class UnitRegistry
{
    private static Dictionary<string, UnitStats> _stats = new()
    {
        ["Swordsman"] = new()
        {
            TypeId = "Swordsman", Size = UnitSize.Medium, AttackType = AttackType.Melee,
            DamageType = DamageType.Normal, ArmorType = ArmorType.Light,
            Health = 30, Damage = 10, Speed = 350f, AttackRange = 125f,
            AttackCooldown = 1f, UnitColor = 0xFF4488FFu,
        },
    };

    public static void LoadFrom(Dictionary<string, UnitStats> stats)
    {
        _stats = stats;
    }

    public static UnitStats Get(string typeId)
    {
        if (_stats.TryGetValue(typeId, out UnitStats stats))
            return stats;
        return _stats["Swordsman"];
    }
}
