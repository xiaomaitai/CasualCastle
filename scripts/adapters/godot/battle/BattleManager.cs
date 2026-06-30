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
        PushInList(allUnits, dt);
    }

    private static void PushInList(List<Soldier> units, float dt)
    {
        for (int i = 0; i < units.Count; i++)
        {
            Soldier a = units[i];
            if (!a.IsAlive)
                continue;

            for (int j = i + 1; j < units.Count; j++)
            {
                Soldier b = units[j];
                if (!b.IsAlive)
                    continue;

                float dist = a.GlobalPosition.DistanceTo(b.GlobalPosition);
                float minDist = GameCoordinatesAdapter.GameUnitsToPixels(a.Data.CollisionRadius())
                    + GameCoordinatesAdapter.GameUnitsToPixels(b.Data.CollisionRadius()) + 4f;
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
