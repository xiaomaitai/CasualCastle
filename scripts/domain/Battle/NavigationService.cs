// 导航服务：流场缓存、邻居查询与全体单位转向移动
using System.Collections.Generic;

namespace CasualCastle.Domain.Battle;

public sealed class NavigationService : INavigationService
{
	private const int MaxSnapRadius = 8;
	private readonly INavGridSource _gridSource;
	private readonly IFieldUnitRepository _fieldRepo;
	private readonly FlowFieldService _flowFieldService;
	private readonly SteeringService _steering;
	private readonly Dictionary<int, FlowField> _playerFields = new();
	private readonly Dictionary<int, FlowField> _enemyFields = new();
	private int _cacheVersion = -1;

	public NavigationService(INavGridSource gridSource, IFieldUnitRepository fieldRepo, FlowFieldService flowFieldService, SteeringService steering)
	{
		_gridSource = gridSource;
		_fieldRepo = fieldRepo;
		_flowFieldService = flowFieldService;
		_steering = steering;
	}

	public void Step(float dt)
	{
		_gridSource.Refresh();
		SyncCache();
		NavSpatialHash hash = new NavSpatialHash(_fieldRepo.AllUnits);
		foreach (IUnitHandle unit in _fieldRepo.AllUnits)
		{
			if (!unit.IsAlive)
				continue;
			if (!unit.HasMoveGoal)
			{
				unit.SetNavVelocity(0f, 0f);
				continue;
			}
			NavGrid grid = _gridSource.GetGrid(unit.IsPlayerUnit);
			FlowField field = GetFlowField(grid, unit, unit.IsPlayerUnit);
			float queryRadius = (unit.CollisionRadius + UnitStats.HugeCollisionRadius) * SteeringService.SeparationRadiusMultiplier;
			IReadOnlyList<IUnitHandle> neighbors = hash.Query(unit.GameX, unit.GameY, queryRadius);
			_steering.Steer(unit, grid, field, neighbors, dt);
		}
	}

	private void SyncCache()
	{
		if (_cacheVersion == _gridSource.Version)
			return;
		_cacheVersion = _gridSource.Version;
		_playerFields.Clear();
		_enemyFields.Clear();
	}

	private FlowField GetFlowField(NavGrid grid, IUnitHandle unit, bool isPlayer)
	{
		int cx = grid.WorldToCellX(unit.MoveGoalX);
		int cy = grid.WorldToCellY(unit.MoveGoalY);
		if (!grid.InBounds(cx, cy) || grid.IsBlockedCell(cx, cy))
			SnapToOpen(grid, ref cx, ref cy);
		int goal = grid.Index(cx, cy);
		Dictionary<int, FlowField> cache = isPlayer ? _playerFields : _enemyFields;
		FlowField field;
		if (!cache.TryGetValue(goal, out field))
		{
			field = _flowFieldService.Compute(grid, goal);
			cache[goal] = field;
		}
		return field;
	}

	private static void SnapToOpen(NavGrid grid, ref int cx, ref int cy)
	{
		for (int radius = 0; radius <= MaxSnapRadius; radius++)
			for (int ox = -radius; ox <= radius; ox++)
				for (int oy = -radius; oy <= radius; oy++)
					if (!grid.IsBlockedCell(cx + ox, cy + oy))
					{
						cx += ox;
						cy += oy;
						return;
					}
	}
}
