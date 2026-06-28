using System.Collections.Generic;

namespace CasualCastle.Domain.Battle;

public static class UnitRegistry
{
	public static readonly UnitStats Swordsman = new()
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
		UnitColor = 0xFF4488FFu,
	};

	public static readonly UnitStats Archer = new()
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
		UnitColor = 0xFF44CC44u,
	};

	public static readonly UnitStats Cavalry = new()
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
		UnitColor = 0xFFFFAA22u,
	};

	public static readonly UnitStats Werewolf = new()
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
		UnitColor = 0xFF8844AAu,
	};

	public static readonly UnitStats HeavySwordsman = new()
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
		UnitColor = 0xFF6688FFu,
	};

	public static readonly UnitStats WerewolfLord = new()
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
		UnitColor = 0xFFCC44CCu,
	};

	private static readonly Dictionary<string, UnitStats> Stats = new()
	{
		["Swordsman"] = Swordsman,
		["Archer"] = Archer,
		["Cavalry"] = Cavalry,
		["Werewolf"] = Werewolf,
		["HeavySwordsman"] = HeavySwordsman,
		["WerewolfLord"] = WerewolfLord,
	};

	public static UnitStats Get(string typeId)
	{
		if (Stats.TryGetValue(typeId, out UnitStats stats))
			return stats;
		return Swordsman;
	}
}
