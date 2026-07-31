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

	public (IUnitHandle nearest, float edgeDist) FindNearestEnemy(IUnitHandle unit)
	{
		if (!unit.IsAlive)
			return (null, float.MaxValue);

		IUnitHandle best = null;
		float bestScore = float.MaxValue;

		foreach (IUnitHandle candidate in _repo.AllUnits)
		{
			if (!candidate.IsAlive || candidate.IsPlayerUnit == unit.IsPlayerUnit)
				continue;
			float dist = DistSq(unit, candidate);
			if (dist < bestScore)
			{
				bestScore = dist;
				best = candidate;
			}
		}

		if (best == null)
			return (null, float.MaxValue);

		float edgeDist = MathF.Sqrt(bestScore) - unit.CollisionRadius - best.CollisionRadius;
		if (MathF.Sqrt(bestScore) > unit.VisionRange)
			return (null, float.MaxValue);

		return (best, edgeDist);
	}

	public void PropagateRetaliation(IUnitHandle center, IUnitHandle attacker)
	{
		float radius = center.VisionRange;
		foreach (IUnitHandle ally in _repo.AllUnits)
		{
			if (ally == center || !ally.IsAlive || ally.IsPlayerUnit != center.IsPlayerUnit)
				continue;
			if (ally.State == UnitState.Fighting)
				continue;
			float dx = ally.GameX - center.GameX;
			float dy = ally.GameY - center.GameY;
			if (MathF.Sqrt(dx * dx + dy * dy) > radius)
				continue;
			ally.SetEnemyTarget(attacker);
		}
	}

	public IBuildingTarget FindOverlappingBuilding(IUnitHandle unit)
	{
		foreach (IBuildingRef b in _repo.AllBuildings)
		{
			if (b.IsDestroyed || !b.IsEnemyOf(unit))
				continue;
			if (unit.GameX >= b.MinX && unit.GameX <= b.MaxX
				&& unit.GameY >= b.MinY && unit.GameY <= b.MaxY)
				return b.BuildingTarget;
		}
		return null;
	}

	public bool HasEnemyOnBuilding(IBuildingRef building)
	{
		foreach (IUnitHandle s in _repo.AllUnits)
		{
			if (!s.IsAlive || !building.IsEnemyOf(s))
				continue;
			if (s.GameX >= building.MinX && s.GameX <= building.MaxX
				&& s.GameY >= building.MinY && s.GameY <= building.MaxY)
				return true;
		}
		return false;
	}

	private static float DistSq(IUnitHandle a, IUnitHandle b)
	{
		float dx = a.GameX - b.GameX;
		float dy = a.GameY - b.GameY;
		return dx * dx + dy * dy;
	}
}
