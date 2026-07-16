using System;
using System.Collections.Generic;

namespace CasualCastle.Domain.Battle;

public class TacticalQueryService : ITacticalQueries
{
	private readonly IFieldUnitRepository _repo;

	public TacticalQueryService(IFieldUnitRepository repo)
	{
		_repo = repo;
	}

	public (ISoldierHandle nearest, float edgeDist) FindNearestEnemy(ISoldierHandle soldier)
	{
		if (!soldier.IsAlive)
			return (null, float.MaxValue);

		ISoldierHandle best = null;
		float bestScore = float.MaxValue;

		foreach (ISoldierHandle candidate in _repo.AllUnits)
		{
			if (!candidate.IsAlive || candidate.IsPlayerUnit == soldier.IsPlayerUnit)
				continue;
			float dist = DistSq(soldier, candidate);
			if (dist < bestScore)
			{
				bestScore = dist;
				best = candidate;
			}
		}

		if (best == null)
			return (null, float.MaxValue);

		float edgeDist = MathF.Sqrt(bestScore) - soldier.CollisionRadius - best.CollisionRadius;
		if (MathF.Sqrt(bestScore) > soldier.VisionRange)
			return (null, float.MaxValue);

		return (best, edgeDist);
	}

	public void PropagateRetaliation(ISoldierHandle center, ISoldierHandle attacker)
	{
		float radius = center.VisionRange;
		foreach (ISoldierHandle ally in _repo.AllUnits)
		{
			if (ally == center || !ally.IsAlive || ally.IsPlayerUnit != center.IsPlayerUnit)
				continue;
			if (ally.State == SoldierState.Fighting)
				continue;
			float dx = ally.GameX - center.GameX;
			float dy = ally.GameY - center.GameY;
			if (MathF.Sqrt(dx * dx + dy * dy) > radius)
				continue;
			ally.SetEnemyTarget(attacker);
		}
	}

	public IBuildingTarget FindOverlappingBuilding(ISoldierHandle soldier)
	{
		foreach (IBuildingRef b in _repo.AllBuildings)
		{
			if (b.IsDestroyed || !b.IsEnemyOf(soldier))
				continue;
			if (soldier.GameX >= b.MinX && soldier.GameX <= b.MaxX
				&& soldier.GameY >= b.MinY && soldier.GameY <= b.MaxY)
				return b.BuildingTarget;
		}
		return null;
	}

	public bool HasEnemyOnBuilding(IBuildingRef building)
	{
		foreach (ISoldierHandle s in _repo.AllUnits)
		{
			if (!s.IsAlive || !building.IsEnemyOf(s))
				continue;
			if (s.GameX >= building.MinX && s.GameX <= building.MaxX
				&& s.GameY >= building.MinY && s.GameY <= building.MaxY)
				return true;
		}
		return false;
	}

	private static float DistSq(ISoldierHandle a, ISoldierHandle b)
	{
		float dx = a.GameX - b.GameX;
		float dy = a.GameY - b.GameY;
		return dx * dx + dy * dy;
	}
}
