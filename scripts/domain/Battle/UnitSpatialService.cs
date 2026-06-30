using System;
using System.Collections.Generic;

namespace CasualCastle.Domain.Battle;

public class UnitSpatialService
{
	private const float CellSize = 200f;
	private const float PushForce = 20f;
	private const float BuildingPushForce = 15f;

	private readonly List<ISoldierService> _playerUnits = new();
	private readonly List<ISoldierService> _enemyUnits = new();
	private readonly List<IBuildingRef> _buildings = new();
	private readonly Dictionary<(int, int), List<ISoldierService>> _grid = new();

	public interface IBuildingRef
	{
		bool IsDestroyed { get; }
		bool IsEnemyOf(ISoldierService soldier);
		float MinX { get; }
		float MinY { get; }
		float MaxX { get; }
		float MaxY { get; }
		object NativeObject { get; }
		object CastleObject { get; }
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

	public void RegisterBuilding(IBuildingRef building)
	{
		_buildings.Add(building);
	}

	public void UnregisterBuilding(IBuildingRef building)
	{
		_buildings.Remove(building);
	}

	public void Update(float dt)
	{
		RebuildGrid();
		PushSoldiers(dt);
		PushSoldiersFromBuildings(dt);
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
			if (candidate == soldier)
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

	public (IBuildingTarget building, object castle) FindOverlappingBuilding(ISoldierService soldier)
	{
		foreach (IBuildingRef b in _buildings)
		{
			if (b.IsDestroyed)
				continue;
			if (!b.IsEnemyOf(soldier))
				continue;
			if (soldier.GameX >= b.MinX && soldier.GameX <= b.MaxX
				&& soldier.GameY >= b.MinY && soldier.GameY <= b.MaxY)
				return (b.NativeObject as IBuildingTarget, b.CastleObject);
		}
		return (null, null);
	}

	public bool HasEnemyOnBuilding(IBuildingRef building)
	{
		List<ISoldierService> enemies = _playerUnits.Count > 0 && building.IsEnemyOf(_playerUnits[0])
			? _playerUnits : _enemyUnits;
		foreach (ISoldierService s in enemies)
		{
			if (!s.IsAlive)
				continue;
			if (s.GameX >= building.MinX && s.GameX <= building.MaxX
				&& s.GameY >= building.MinY && s.GameY <= building.MaxY)
				return true;
		}
		return false;
	}

	private void RebuildGrid()
	{
		_grid.Clear();
		AddToGrid(_playerUnits);
		AddToGrid(_enemyUnits);
	}

	private void AddToGrid(List<ISoldierService> units)
	{
		foreach (ISoldierService s in units)
		{
			if (!s.IsAlive)
				continue;
			(int x, int y) cell = (WorldToCell(s.GameX), WorldToCell(s.GameY));
			if (!_grid.TryGetValue(cell, out List<ISoldierService> list))
			{
				list = new List<ISoldierService>();
				_grid[cell] = list;
			}
			list.Add(s);
		}
	}

	private void PushSoldiers(float dt)
	{
		var all = new List<ISoldierService>(_playerUnits.Count + _enemyUnits.Count);
		all.AddRange(_playerUnits);
		all.AddRange(_enemyUnits);

		for (int i = 0; i < all.Count; i++)
		{
			ISoldierService a = all[i];
			if (!a.IsAlive || a.State == SoldierState.Sieging)
				continue;

			for (int j = i + 1; j < all.Count; j++)
			{
				ISoldierService b = all[j];
				if (!b.IsAlive || b.State == SoldierState.Sieging)
					continue;

				float dx = a.GameX - b.GameX;
				float dy = a.GameY - b.GameY;
				float dist = MathF.Sqrt(dx * dx + dy * dy);
				float minDist = a.CollisionRadius + b.CollisionRadius + 4f;
				if (dist < minDist && dist > 0.001f)
				{
					float pushAmount = (minDist - dist) * PushForce * dt / minDist;
					a.GameX += dx * pushAmount;
					a.GameY += dy * pushAmount;
					b.GameX -= dx * pushAmount;
					b.GameY -= dy * pushAmount;
				}
			}
		}
	}

	private void PushSoldiersFromBuildings(float dt)
	{
		var all = new List<ISoldierService>(_playerUnits.Count + _enemyUnits.Count);
		all.AddRange(_playerUnits);
		all.AddRange(_enemyUnits);

		foreach (ISoldierService s in all)
		{
			if (!s.IsAlive || s.State == SoldierState.Sieging)
				continue;

			foreach (IBuildingRef b in _buildings)
			{
				if (b.IsDestroyed)
					continue;

				float sx = s.GameX;
				float sy = s.GameY;
				float cx = Math.Clamp(sx, b.MinX, b.MaxX);
				float cy = Math.Clamp(sy, b.MinY, b.MaxY);
				float dx = sx - cx;
				float dy = sy - cy;
				float dist = MathF.Sqrt(dx * dx + dy * dy);
				if (dist < s.CollisionRadius && dist > 0.001f)
				{
					float pushAmount = (s.CollisionRadius - dist) * BuildingPushForce * dt / dist;
					s.GameX += dx * pushAmount;
					s.GameY += dy * pushAmount;
				}
			}
		}
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

	private static float DistSq(ISoldierService a, ISoldierService b)
	{
		float dx = a.GameX - b.GameX;
		float dy = a.GameY - b.GameY;
		return dx * dx + dy * dy;
	}

	private static int WorldToCell(float coord) => (int)MathF.Floor(coord / CellSize);
}
