using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Battle;
using Godot;
using System.Collections.Generic;

public partial class BattleManager : Node
{
    private const float TargetingInterval = 0.2f;
    private const float CellSize = 200f;

    private readonly List<SoldierLogic> _playerUnits = new();
    private readonly List<SoldierLogic> _enemyUnits = new();
    private readonly List<Building> _buildings = new();
    private readonly Dictionary<Vector2I, List<SoldierLogic>> _grid = new();
    private float _targetingTimer;

    public override void _Ready()
    {
        AdapterRegistry.Register<BattleManager>(this);
    }

    public override void _ExitTree()
    {
        AdapterRegistry.Unregister<BattleManager>(this);
    }

    public void Register(SoldierLogic soldier)
    {
        if (soldier.IsPlayerUnit)
            _playerUnits.Add(soldier);
        else
            _enemyUnits.Add(soldier);
    }

    public void Unregister(SoldierLogic soldier)
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

        UnitSpatialService spatial = AdapterRegistry.Resolve<UnitSpatialService>();
        spatial?.Update(dt);
    }

    private void RebuildGrid()
    {
        _grid.Clear();
        AddToGrid(_playerUnits);
        AddToGrid(_enemyUnits);
    }

    private void AddToGrid(List<SoldierLogic> units)
    {
        foreach (SoldierLogic s in units)
        {
            if (!s.IsAlive)
                continue;
            Vector2I cell = WorldToCell(s.GlobalPosition);
            if (!_grid.TryGetValue(cell, out List<SoldierLogic> list))
            {
                list = new List<SoldierLogic>();
                _grid[cell] = list;
            }
            list.Add(s);
        }
    }

    private void UpdateTargeting()
    {
        List<SoldierLogic> enemies = _enemyUnits;
        foreach (SoldierLogic s in _playerUnits)
            FindBestTarget(s, enemies);
        enemies = _playerUnits;
        foreach (SoldierLogic s in _enemyUnits)
            FindBestTarget(s, enemies);
    }

    private void FindBestTarget(SoldierLogic soldier, List<SoldierLogic> enemies)
    {
        if (!soldier.IsAlive)
            return;

       SoldierLogic best = null;
        float bestScore = float.MaxValue;

        Vector2I cell = WorldToCell(soldier.GlobalPosition);
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                Vector2I neighborCell = new(cell.X + dx, cell.Y + dy);
                if (!_grid.TryGetValue(neighborCell, out List<SoldierLogic> cellUnits))
                    continue;

                foreach (SoldierLogic candidate in cellUnits)
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

    public static Rect2 GetBuildingRectStatic(Building building)
    {
        Vector2 size = building.GetBuildingSize();
        Vector2 pos = building.GlobalPosition - size * 0.5f;
        return new Rect2(pos, size);
    }

    public bool HasEnemyOnBuilding(Building building)
    {
        Castle castle = building.GetCastle();
        if (castle == null)
            return false;
        Rect2 rect = GetBuildingRectStatic(building);
        List<SoldierLogic> enemies = castle.IsPlayerCastle ? _enemyUnits : _playerUnits;
        foreach (SoldierLogic s in enemies)
        {
            if (s.IsAlive && rect.HasPoint(s.GlobalPosition))
                return true;
        }
        return false;
    }

    private static Vector2I WorldToCell(Vector2 position)
    {
        return new Vector2I(
            Mathf.FloorToInt(position.X / CellSize),
            Mathf.FloorToInt(position.Y / CellSize));
    }
}
