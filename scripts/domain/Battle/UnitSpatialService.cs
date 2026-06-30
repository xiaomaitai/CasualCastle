using System;
using System.Collections.Generic;

namespace CasualCastle.Domain.Battle;

public class UnitSpatialService
{
	private const float CellSize = 200f;
	private const float PushForce = 20f;
	private const float BuildingPushForce = 15f;

	private readonly List<Soldier> _playerUnits = new();
	private readonly List<Soldier> _enemyUnits = new();
	private readonly List<IBuildingRef> _buildings = new();
	private readonly Dictionary<(int, int), List<Soldier>> _grid = new();

	public interface IBuildingRef
	{
		bool IsDestroyed { get; }
		bool IsEnemyOf(Soldier soldier);
		float MinX { get; }
		float MinY { get; }
		float MaxX { get; }
		float MaxY { get; }
		object NativeObject { get; }
		object CastleObject { get; }
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

	public (Soldier nearest, float edgeDist) FindNearestEnemy(Soldier soldier)
	{
		if (!soldier.IsAlive)
			return (null, float.MaxValue);

		List<Soldier> enemies = soldier.IsPlayerUnit ? _enemyUnits : _playerUnits;
		Soldier best = null;
		float bestScore = float.MaxValue;

		foreach (Soldier candidate in enemies)
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

	public (object building, object castle) FindOverlappingBuilding(Soldier soldier)
	{
		foreach (IBuildingRef b in _buildings)
		{
			if (b.IsDestroyed)
				continue;
			if (!b.IsEnemyOf(soldier))
				continue;
			if (soldier.GameX >= b.MinX && soldier.GameX <= b.MaxX
				&& soldier.GameY >= b.MinY && soldier.GameY <= b.MaxY)
				return (b.NativeObject, b.CastleObject);
		}
		return (null, null);
	}

	public bool HasEnemyOnBuilding(IBuildingRef building)
	{
		List<Soldier> enemies = building.IsEnemyOf(_playerUnits.Count > 0 ? _playerUnits[0] : null)
			? _playerUnits : _enemyUnits;
		foreach (Soldier s in enemies)
		{
			if (!s.IsAlive)
				continue;
			float x = s.GameX;
			float y = s.GameY;
			if (x >= building.MinX && x <= building.MaxX && y >= building.MinY && y <= building.MaxY)
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

	private void AddToGrid(List<Soldier> units)
	{
		foreach (Soldier s in units)
		{
			if (!s.IsAlive)
				continue;
			(int x, int y) cell = (WorldToCell(s.GameX), WorldToCell(s.GameY));
			if (!_grid.TryGetValue(cell, out List<Soldier> list))
			{
				list = new List<Soldier>();
				_grid[cell] = list;
			}
			list.Add(s);
		}
	}

	private void PushSoldiers(float dt)
	{
		var all = new List<Soldier>(_playerUnits.Count + _enemyUnits.Count);
		all.AddRange(_playerUnits);
		all.AddRange(_enemyUnits);

		for (int i = 0; i < all.Count; i++)
		{
			Soldier a = all[i];
			if (!a.IsAlive || a.State == SoldierState.Sieging)
				continue;

			for (int j = i + 1; j < all.Count; j++)
			{
				Soldier b = all[j];
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
		var all = new List<Soldier>(_playerUnits.Count + _enemyUnits.Count);
		all.AddRange(_playerUnits);
		all.AddRange(_enemyUnits);

		foreach (Soldier s in all)
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

	private static float DistSq(Soldier a, Soldier b)
	{
		float dx = a.GameX - b.GameX;
		float dy = a.GameY - b.GameY;
		return dx * dx + dy * dy;
	}

	public void PropagateRetaliation(Soldier center, Soldier attacker)
	{
		List<Soldier> allies = center.IsPlayerUnit ? _playerUnits : _enemyUnits;
		float radius = center.VisionRange;
		foreach (Soldier ally in allies)
		{
			if (ally == center)
				continue;
			if (!ally.IsAlive)
				continue;
			if (ally.TargetEnemy != null && ally.TargetEnemy.IsAlive)
				continue;
			float dx = ally.GameX - center.GameX;
			float dy = ally.GameY - center.GameY;
			if (MathF.Sqrt(dx * dx + dy * dy) > radius)
				continue;
			ally.TargetEnemy = attacker;
		}
	}

	private static int WorldToCell(float coord) => (int)MathF.Floor(coord / CellSize);
}
