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

		IUnitHandle best = FindBestEnemy(unit, unit.IsStealthed);
		if (best == null && unit.IsStealthed)
			best = FindBestEnemy(unit, false);

		if (best == null)
			return (null, float.MaxValue);

		float edgeDist = MathF.Sqrt(DistSq(unit, best)) - unit.CollisionRadius - best.CollisionRadius;
		return (best, edgeDist);
	}

	private IUnitHandle FindBestEnemy(IUnitHandle unit, bool rangedOnly)
	{
		IUnitHandle best = null;
		float bestDistanceSq = float.MaxValue;

		foreach (IUnitHandle candidate in _repo.AllUnits)
		{
			if (!candidate.IsAlive || candidate.IsPlayerUnit == unit.IsPlayerUnit)
				continue;
			if (candidate.IsStealthed && !unit.CanDetectStealth)
				continue;
			if (rangedOnly && candidate.AttackType != AttackType.Ranged)
				continue;
			float distanceSq = DistSq(unit, candidate);
			if (distanceSq >= bestDistanceSq)
				continue;
			if (MathF.Sqrt(distanceSq) > unit.VisionRange)
				continue;
			best = candidate;
			bestDistanceSq = distanceSq;
		}

		return best;
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

	public IReadOnlyList<IUnitHandle> FindEnemiesNear(IUnitHandle center, float radius)
	{
		List<IUnitHandle> found = new();
		float radiusSq = radius * radius;
		foreach (IUnitHandle candidate in _repo.AllUnits)
		{
			if (!candidate.IsAlive || candidate.IsPlayerUnit == center.IsPlayerUnit)
				continue;
			if (candidate.IsStealthed && !center.CanDetectStealth)
				continue;
			float dx = candidate.GameX - center.GameX;
			float dy = candidate.GameY - center.GameY;
			if (dx * dx + dy * dy <= radiusSq)
				found.Add(candidate);
		}
		return found;
	}

	public IUnitHandle FindNearestInjuredAlly(IUnitHandle unit, float range)
	{
		IUnitHandle best = null;
		float bestDistanceSq = float.MaxValue;
		float rangeSq = range * range;
		foreach (IUnitHandle ally in _repo.AllUnits)
		{
			if (!ally.IsAlive || ally == unit || ally.IsPlayerUnit != unit.IsPlayerUnit)
				continue;
			if (ally.Health >= ally.MaxHealth)
				continue;
			float dx = ally.GameX - unit.GameX;
			float dy = ally.GameY - unit.GameY;
			float distanceSq = dx * dx + dy * dy;
			if (distanceSq > rangeSq || distanceSq >= bestDistanceSq)
				continue;
			best = ally;
			bestDistanceSq = distanceSq;
		}
		return best;
	}

	private static float DistSq(IUnitHandle a, IUnitHandle b)
	{
		float dx = a.GameX - b.GameX;
		float dy = a.GameY - b.GameY;
		return dx * dx + dy * dy;
	}
}
