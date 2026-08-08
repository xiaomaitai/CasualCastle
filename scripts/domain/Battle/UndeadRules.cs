// 亡灵规则：亡灵种族判定、神圣驱散倍率与非生命体免疫集合
using System.Collections.Generic;

namespace CasualCastle.Domain.Battle;

public static class UndeadRules
{
	public const string UndeadRace = "Undead";
	public const float DispelDamageMultiplier = 2f;

	private static readonly HashSet<string> NonLivingRaces = new() { "Mechanical", "Elemental", "Golem", "Construct" };

	public static bool IsUndead(string race)
	{
		return race == UndeadRace;
	}

	public static bool IsNonLiving(string race)
	{
		return NonLivingRaces.Contains(race);
	}

	public static bool IsLiving(string race)
	{
		return !NonLivingRaces.Contains(race);
	}

	public static int ApplyDispelMultiplier(int damage, DamageType damageType, string targetRace)
	{
		if (damageType == DamageType.Holy && IsUndead(targetRace))
			return (int)(damage * DispelDamageMultiplier);
		return damage;
	}
}