using System;
using System.Collections.Generic;
using CasualCastle.Domain.Battle;

namespace CasualCastle.Adapters.Persistence;

public class FieldUnitRepository : IFieldUnitRepository
{
    private const float CellSize = 200f;

    private readonly List<ISoldierService> _playerUnits = new();
    private readonly List<ISoldierService> _enemyUnits = new();
    private readonly List<IBuildingRef> _buildings = new();

    public IReadOnlyList<ISoldierService> AllUnits
    {
        get
        {
            List<ISoldierService> all = new(_playerUnits.Count + _enemyUnits.Count);
            all.AddRange(_playerUnits);
            all.AddRange(_enemyUnits);
            return all;
        }
    }

    public void Register(ISoldierService soldier)
    {
        if (soldier.IsPlayerUnit)
            _playerUnits.Add(soldier);
        else
            _enemyUnits.Add(soldier);
    }

    public void Unregister(ISoldierService soldier)
    {
        if (soldier.IsPlayerUnit)
            _playerUnits.Remove(soldier);
        else
            _enemyUnits.Remove(soldier);
    }

    public (ISoldierService nearest, float edgeDist) FindNearestEnemy(ISoldierService soldier)
    {
        if (!soldier.IsAlive)
            return (null, float.MaxValue);

        List<ISoldierService> enemies = soldier.IsPlayerUnit ? _enemyUnits : _playerUnits;
        ISoldierService best = null;
        float bestScore = float.MaxValue;

        foreach (ISoldierService candidate in enemies)
        {
            if (!candidate.IsAlive)
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

    public void PropagateRetaliation(ISoldierService center, ISoldierService attacker)
    {
        List<ISoldierService> allies = center.IsPlayerUnit ? _playerUnits : _enemyUnits;
        float radius = center.VisionRange;
        foreach (ISoldierService ally in allies)
        {
            if (ally == center)
                continue;
            if (!ally.IsAlive)
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

    public void RegisterBuilding(IBuildingRef building)
    {
        _buildings.Add(building);
    }

    public void UnregisterBuilding(IBuildingRef building)
    {
        _buildings.Remove(building);
    }

    public IBuildingTarget FindOverlappingBuilding(ISoldierService soldier)
    {
        foreach (IBuildingRef b in _buildings)
        {
            if (b.IsDestroyed)
                continue;
            if (!b.IsEnemyOf(soldier))
                continue;
            if (soldier.GameX >= b.MinX && soldier.GameX <= b.MaxX
                && soldier.GameY >= b.MinY && soldier.GameY <= b.MaxY)
                return b.BuildingTarget;
        }
        return null;
    }

    public bool HasEnemyOnBuilding(IBuildingRef building)
    {
        foreach (ISoldierService s in _playerUnits)
        {
            if (!s.IsAlive)
                continue;
            if (!building.IsEnemyOf(s))
                continue;
            if (s.GameX >= building.MinX && s.GameX <= building.MaxX
                && s.GameY >= building.MinY && s.GameY <= building.MaxY)
                return true;
        }
        foreach (ISoldierService s in _enemyUnits)
        {
            if (!s.IsAlive)
                continue;
            if (!building.IsEnemyOf(s))
                continue;
            if (s.GameX >= building.MinX && s.GameX <= building.MaxX
                && s.GameY >= building.MinY && s.GameY <= building.MaxY)
                return true;
        }
        return false;
    }

    private static float DistSq(ISoldierService a, ISoldierService b)
    {
        float dx = a.GameX - b.GameX;
        float dy = a.GameY - b.GameY;
        return dx * dx + dy * dy;
    }
}
