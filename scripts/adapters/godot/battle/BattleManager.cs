using CasualCastle.Adapters.Godot;
using Godot;
using System.Collections.Generic;

public partial class BattleManager : Node
{
    private const float TargetingInterval = 0.2f;
    private const float PushDistance = 24f;
    private const float PushForce = 20f;
    private const float CellSize = 200f;

    private readonly List<Soldier> _playerUnits = new();
    private readonly List<Soldier> _enemyUnits = new();
    private readonly List<Building> _buildings = new();
    private readonly Dictionary<Vector2I, List<Soldier>> _grid = new();
    private float _targetingTimer;

    public override void _Ready()
    {
        AdapterRegistry.Register<BattleManager>(this);
    }

    public override void _ExitTree()
    {
        AdapterRegistry.Unregister<BattleManager>(this);
    }

    public void Register(Soldier soldier)
    {
        if (soldier.IsPlayerUnit)
            _playerUnits.Add(soldier);
        else
            _enemyUnits.Add(soldier);
    }

    public void Unregister(Soldier soldier)
    {
        if (soldier.IsPlayerUnit)
            _playerUnits.Remove(soldier);
        else
            _enemyUnits.Remove(soldier);
    }

    public void RegisterBuilding(Building building)
    {
        _buildings.Add(building);
    }

    public void UnregisterBuilding(Building building)
    {
        _buildings.Remove(building);
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;

        _targetingTimer -= dt;
        if (_targetingTimer <= 0f)
        {
            _targetingTimer = TargetingInterval;
            RebuildGrid();
            UpdateTargeting();
        }

        ApplyUnitPushing(dt);
    }

    private void RebuildGrid()
    {
        _grid.Clear();
        AddToGrid(_playerUnits);
        AddToGrid(_enemyUnits);
    }

    private void AddToGrid(List<Soldier> units)
    {
        foreach (Soldier s in units)
        {
            if (!s.IsAlive)
                continue;
            Vector2I cell = WorldToCell(s.GlobalPosition);
            if (!_grid.TryGetValue(cell, out List<Soldier> list))
            {
                list = new List<Soldier>();
                _grid[cell] = list;
            }
            list.Add(s);
        }
    }

    private void UpdateTargeting()
    {
        List<Soldier> enemies = _enemyUnits;
        foreach (Soldier s in _playerUnits)
            FindBestTarget(s, enemies);
        enemies = _playerUnits;
        foreach (Soldier s in _enemyUnits)
            FindBestTarget(s, enemies);
    }

    private void FindBestTarget(Soldier soldier, List<Soldier> enemies)
    {
        if (!soldier.IsAlive)
            return;

        Soldier best = null;
        float bestScore = float.MaxValue;

        Vector2I cell = WorldToCell(soldier.GlobalPosition);
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                Vector2I neighborCell = new(cell.X + dx, cell.Y + dy);
                if (!_grid.TryGetValue(neighborCell, out List<Soldier> cellUnits))
                    continue;

                foreach (Soldier candidate in cellUnits)
                {
                    if (!candidate.IsAlive)
                        continue;
                    if (candidate.IsPlayerUnit == soldier.IsPlayerUnit)
                        continue;
                    float dist = soldier.GlobalPosition.DistanceSquaredTo(candidate.GlobalPosition);
                    if (dist < bestScore)
                    {
                        bestScore = dist;
                        best = candidate;
                    }
                }
            }
        }

        if (best != null && Mathf.Sqrt(bestScore) > soldier.VisionRange)
            return;

        if (best != null)
            soldier.SetTarget(best);
    }

    private void ApplyUnitPushing(float dt)
    {
        var allUnits = new List<Soldier>(_playerUnits.Count + _enemyUnits.Count);
        allUnits.AddRange(_playerUnits);
        allUnits.AddRange(_enemyUnits);
        PushSoldiers(allUnits, dt);
        PushSoldiersFromBuildings(allUnits, dt);
    }

    private static void PushSoldiers(List<Soldier> units, float dt)
    {
        for (int i = 0; i < units.Count; i++)
        {
            Soldier a = units[i];
            if (!a.IsAlive)
                continue;
            if (a._targetBuilding != null)
                continue;

            for (int j = i + 1; j < units.Count; j++)
            {
                Soldier b = units[j];
                if (!b.IsAlive)
                    continue;
                if (b._targetBuilding != null)
                    continue;

                float dist = a.GlobalPosition.DistanceTo(b.GlobalPosition);
                float minDist = GameCoordinatesAdapter.GameUnitsToPixels(a.CollisionRadius)
                    + GameCoordinatesAdapter.GameUnitsToPixels(b.CollisionRadius) + 4f;
                if (dist < minDist && dist > 0.001f)
                {
                    Vector2 pushDir = (a.GlobalPosition - b.GlobalPosition).Normalized();
                    float pushAmount = (minDist - dist) * PushForce * dt;
                    Vector2 push = pushDir * pushAmount;
                    a.GlobalPosition += push;
                    b.GlobalPosition -= push;
                }
            }
        }
    }


    private void PushSoldiersFromBuildings(List<Soldier> units, float dt)
    {
        const float buildingPushForce = 15f;
        foreach (Soldier s in units)
        {
            if (!s.IsAlive)
                continue;
            if (s._targetBuilding != null)
                continue;
            Vector2 pos = s.GlobalPosition;
            float sRadius = GameCoordinatesAdapter.GameUnitsToPixels(s.CollisionRadius);
            foreach (Building b in _buildings)
            {
                if (b.IsDestroyed)
                    continue;
                if (s._targetBuilding == b)
                    continue;
                Rect2 rect = GetBuildingRectStatic(b);
                Vector2 closest = new Vector2(
                    Mathf.Clamp(pos.X, rect.Position.X, rect.Position.X + rect.Size.X),
                    Mathf.Clamp(pos.Y, rect.Position.Y, rect.Position.Y + rect.Size.Y));
                float dist = pos.DistanceTo(closest);
                if (dist < sRadius && dist > 0.001f)
                {
                    Vector2 pushDir = (pos - closest).Normalized();
                    float pushAmount = (sRadius - dist) * buildingPushForce * dt;
                    s.GlobalPosition += pushDir * pushAmount;
                }
            }
        }
    }

    public static Rect2 GetBuildingRectStatic(Building building)
    {
        Vector2 size = building.GetBuildingSize();
        Vector2 pos = building.GlobalPosition - size * 0.5f;
        return new Rect2(pos, size);
    }

    public Building FindOverlappingBuilding(Soldier soldier)
    {
        Vector2 pos = soldier.GlobalPosition;
        foreach (Building b in _buildings)
        {
            if (b.IsDestroyed)
                continue;
            Castle castle = b.GetCastle();
            if (castle == null || !castle.IsAlive || castle.IsPlayerCastle == soldier.IsPlayerUnit)
                continue;
            Rect2 rect = GetBuildingRectStatic(b);
            if (rect.HasPoint(pos))
                return b;
        }
        return null;
    }

    public bool HasEnemyOnBuilding(Building building)
    {
        Castle castle = building.GetCastle();
        if (castle == null)
            return false;
        Rect2 rect = GetBuildingRectStatic(building);
        List<Soldier> enemies = castle.IsPlayerCastle ? _enemyUnits : _playerUnits;
        foreach (Soldier s in enemies)
        {
            if (s.IsAlive && rect.HasPoint(s.GlobalPosition))
                return true;
        }
        return false;
    }

    public void PropagateRetaliation(Soldier center, float radius, Soldier target)
    {
        List<Soldier> allies = center.IsPlayerUnit ? _playerUnits : _enemyUnits;
        foreach (Soldier ally in allies)
        {
            if (ally == center)
                continue;
            if (!ally.IsAlive)
                continue;
            if (ally._targetEnemy != null && ally._targetEnemy.IsAlive)
                continue;
            if (ally.GlobalPosition.DistanceTo(center.GlobalPosition) > radius)
                continue;

            ally._targetEnemy = target;
        }
    }
    private static Vector2I WorldToCell(Vector2 position)
    {
        return new Vector2I(
            Mathf.FloorToInt(position.X / CellSize),
            Mathf.FloorToInt(position.Y / CellSize));
    }
}
