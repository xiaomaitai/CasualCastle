using System.Collections.Generic;
using CasualCastle.Domain.Shared;

namespace CasualCastle.Domain.Battle;

public class SkillService
{
	public void UpdateContexts(IReadOnlyList<ISoldierHandle> allUnits)
	{
		float nearbyRadiusSq = GameRules.SkillNearbyAllyRadius * GameRules.SkillNearbyAllyRadius;
		float isolatedRadiusSq = GameRules.SkillTargetIsolatedRadius * GameRules.SkillTargetIsolatedRadius;

		for (int i = 0; i < allUnits.Count; i++)
		{
			ISoldierHandle unit = allUnits[i];
			if (!unit.IsAlive)
				continue;

			GameContext ctx = new();
			HashSet<string> nearbyRaces = new() { unit.Race };

			for (int j = 0; j < allUnits.Count; j++)
			{
				if (i == j)
					continue;
				ISoldierHandle other = allUnits[j];
				if (!other.IsAlive || other.IsPlayerUnit != unit.IsPlayerUnit)
					continue;

				float dx = unit.GameX - other.GameX;
				float dy = unit.GameY - other.GameY;
				if (dx * dx + dy * dy <= nearbyRadiusSq)
				{
					if (!string.IsNullOrEmpty(other.Race))
						nearbyRaces.Add(other.Race);
				}
			}

			ctx.CurrentHpRatio = unit.MaxHealth > 0 ? (float)unit.Health / unit.MaxHealth : 0f;
			ctx.NearbyAllyRaces = nearbyRaces;

			bool isolated = true;
			ISoldierHandle target = unit.TargetEnemy;
			if (target != null && target.IsAlive)
			{
				for (int j = 0; j < allUnits.Count; j++)
				{
					ISoldierHandle other = allUnits[j];
					if (other == target || !other.IsAlive || other.IsPlayerUnit != target.IsPlayerUnit)
						continue;

					float dx = target.GameX - other.GameX;
					float dy = target.GameY - other.GameY;
					if (dx * dx + dy * dy <= isolatedRadiusSq)
					{
						isolated = false;
						break;
					}
				}
			}
			ctx.TargetIsIsolated = isolated && target != null && target.IsAlive;

			unit.SetGameContext(ctx);
		}
	}
}
