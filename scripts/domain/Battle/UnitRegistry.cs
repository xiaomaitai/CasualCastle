using System.Collections.Generic;

namespace CasualCastle.Domain.Battle;

public static class UnitRegistry
{
	private static readonly Dictionary<string, UnitStats> Stats = new()
	{
		["Swordsman"] = new()
		{
			TypeId = "Swordsman",
			Size = UnitSize.Medium,
			AttackType = AttackType.Melee,
			DamageType = DamageType.Normal,
			ArmorType = ArmorType.Light,
			Health = 30,
			Damage = 10,
			Speed = 170f,
			AttackRange = 60f,
			AttackCooldown = 1f,
		},
		["Archer"] = new()
		{
			TypeId = "Archer",
			Size = UnitSize.Medium,
			AttackType = AttackType.Ranged,
			DamageType = DamageType.Pierce,
			ArmorType = ArmorType.Light,
			Health = 20,
			Damage = 8,
			Speed = 150f,
			AttackRange = 100f,
			AttackCooldown = 1.2f,
		},
		["Cavalry"] = new()
		{
			TypeId = "Cavalry",
			Size = UnitSize.Large,
			AttackType = AttackType.Melee,
			DamageType = DamageType.Normal,
			ArmorType = ArmorType.Heavy,
			Health = 50,
			Damage = 12,
			Speed = 220f,
			AttackRange = 60f,
			AttackCooldown = 1f,
		},
		["Werewolf"] = new()
		{
			TypeId = "Werewolf",
			Size = UnitSize.Medium,
			AttackType = AttackType.Melee,
			DamageType = DamageType.Normal,
			ArmorType = ArmorType.Beast,
			Health = 35,
			Damage = 12,
			Speed = 200f,
			AttackRange = 60f,
			AttackCooldown = 1f,
			HasNightCombat = true,
		},
		["HeavySwordsman"] = new()
		{
			TypeId = "HeavySwordsman",
			Size = UnitSize.Medium,
			AttackType = AttackType.Melee,
			DamageType = DamageType.Normal,
			ArmorType = ArmorType.Heavy,
			Health = 45,
			Damage = 14,
			Speed = 160f,
			AttackRange = 60f,
			AttackCooldown = 0.9f,
		},
		["WerewolfLord"] = new()
		{
			TypeId = "WerewolfLord",
			Size = UnitSize.Medium,
			AttackType = AttackType.Melee,
			DamageType = DamageType.Magic,
			ArmorType = ArmorType.Beast,
			Health = 50,
			Damage = 16,
			Speed = 200f,
			AttackRange = 60f,
			AttackCooldown = 0.9f,
			HasNightCombat = true,
		},
	};

	public static UnitStats Get(string typeId)
	{
		if (Stats.TryGetValue(typeId, out UnitStats stats))
			return stats;
		return Stats["Swordsman"];
	}
}
