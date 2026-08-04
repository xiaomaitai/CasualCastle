using System.Collections.Generic;
using CasualCastle.Domain.Shared;

namespace CasualCastle.Domain.Battle;

public class AuraService
{
	public float CalculateAttackMultiplier(IUnitHandle unit, IReadOnlyList<IUnitHandle> allUnits)
	{
		HashSet<string> auraRaces = new();
		foreach (IUnitHandle other in allUnits)
		{
			if (!IsAuraEmitterWithinRange(unit, other))
				continue;
			if (!string.IsNullOrEmpty(other.Race))
				auraRaces.Add(other.Race);
		}

		float multiplier = 1f;
		foreach (string race in auraRaces)
			multiplier *= GameRules.AuraAttackBonusMultiplier;
		return multiplier;
	}

	private static bool IsAuraEmitterWithinRange(IUnitHandle unit, IUnitHandle other)
	{
		if (other == unit || !other.IsAlive || other.IsPlayerUnit != unit.IsPlayerUnit)
			return false;
		if (other.Skills == null || !other.Skills.HasAura())
			return false;

		float range = other.Skills.GetAuraRange();
		float dx = unit.GameX - other.GameX;
		float dy = unit.GameY - other.GameY;
		return dx * dx + dy * dy <= range * range;
	}
}
