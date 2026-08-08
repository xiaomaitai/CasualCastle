using System.Collections.Generic;
using CasualCastle.Domain.Shared;

namespace CasualCastle.Domain.Battle;

public class AuraService
{
	public AuraEffects CalculateEffects(IUnitHandle unit, IReadOnlyList<IUnitHandle> allUnits)
	{
		AuraEffects result = new();
		if (unit.Skills == null || !IsAuraEligibleRace(unit.Race))
			return result;
		HashSet<(string bonusType, string race)> applied = new();
		foreach (IUnitHandle other in allUnits)
		{
			if (!other.IsAlive || other.IsPlayerUnit != unit.IsPlayerUnit)
				continue;
			if (other.Skills == null || !other.Skills.HasAura())
				continue;
			foreach (AuraConfig cfg in other.Skills.GetAuraConfigs())
			{
				if (!IsReceiverEligible(unit.Race, cfg.Race) || !IsWithinRange(unit, other, cfg.Range))
					continue;
				foreach (AuraEffectConfig effect in cfg.Effects)
				{
					if (string.IsNullOrEmpty(effect.BonusType))
						continue;
					if (!applied.Add((effect.BonusType, other.Race)))
						continue;
					Apply(result, effect.BonusType, effect.Value);
				}
			}
		}
		return result;
	}

	private static bool IsAuraEligibleRace(string race)
	{
		return HordeRaces.IsHordeRace(race) || UndeadRules.IsUndead(race);
	}

	private static bool IsReceiverEligible(string receiverRace, string auraRace)
	{
		if (HordeRaces.IsHordeRace(receiverRace))
			return HordeRaces.IsHordeRace(auraRace);
		if (UndeadRules.IsUndead(receiverRace))
			return UndeadRules.IsUndead(auraRace);
		return false;
	}

	private static bool IsWithinRange(IUnitHandle unit, IUnitHandle other, float range)
	{
		float radius = range > 0f ? range : GameRules.AuraRangeRadius;
		float dx = unit.GameX - other.GameX;
		float dy = unit.GameY - other.GameY;
		return dx * dx + dy * dy <= radius * radius;
	}

	private static void Apply(AuraEffects result, string bonusType, float value)
	{
		switch (bonusType)
		{
			case "attack_speed":
				result.AttackSpeedMultiplier *= 1f + value;
				break;
			case "move_speed":
				result.MoveSpeedMultiplier *= 1f + value;
				break;
			case "attack_damage":
				result.AttackDamageMultiplier *= 1f + value;
				break;
			case "regen":
				result.RegenPerSecond += value;
				break;
			case "execute":
				result.ExecuteDamageMultiplier *= 1f + value;
				break;
			case "armor":
				result.ArmorBonus += (int)value;
				break;
		}
	}
}
