// 导航网格源：从建筑与战场边界构建双方阻塞网格
using CasualCastle.Domain.Battle;
using Godot;
using System.Collections.Generic;
using System.Text;

namespace CasualCastle.Adapters.Godot.Battle;

public sealed class NavGridSource : INavGridSource
{
	private readonly IFieldUnitRepository _fieldRepo;
	private NavGrid _playerGrid;
	private NavGrid _enemyGrid;
	private string _buildingKey;
	private int _version;

	public NavGridSource(IFieldUnitRepository fieldRepo)
	{
		_fieldRepo = fieldRepo;
	}

	public int Version => _version;

	public void Refresh()
	{
		string key = BuildKey(_fieldRepo.AllBuildings);
		if (key == _buildingKey)
			return;
		_buildingKey = key;
		_playerGrid = BuildGrid(true);
		_enemyGrid = BuildGrid(false);
		_version++;
	}

	public NavGrid GetGrid(bool isPlayerUnit)
	{
		return isPlayerUnit ? _playerGrid : _enemyGrid;
	}

	private NavGrid BuildGrid(bool isPlayerUnit)
	{
		int width = Mathf.CeilToInt(BattlefieldMetrics.WorldWidth / NavGrid.CellSize);
		int height = Mathf.CeilToInt(BattlefieldMetrics.WorldHeight / NavGrid.CellSize);
		NavGrid grid = new NavGrid(width, height, 0f, 0f);
		foreach (IBuildingRef building in _fieldRepo.AllBuildings)
		{
			if (building.IsDestroyed || building.IsCore || building.IsPlayerOwned == isPlayerUnit)
				continue;
			MarkBlocked(grid, building.MinX, building.MinY, building.MaxX, building.MaxY);
		}
		return grid;
	}

	private static void MarkBlocked(NavGrid grid, float minX, float minY, float maxX, float maxY)
	{
		int minCX = grid.WorldToCellX(minX);
		int maxCX = grid.WorldToCellX(maxX);
		int minCY = grid.WorldToCellY(minY);
		int maxCY = grid.WorldToCellY(maxY);
		for (int cy = minCY; cy <= maxCY; cy++)
			for (int cx = minCX; cx <= maxCX; cx++)
				if (grid.InBounds(cx, cy))
					grid.SetBlocked(cx, cy, true);
	}

	private static string BuildKey(IReadOnlyList<IBuildingRef> buildings)
	{
		StringBuilder key = new StringBuilder();
		foreach (IBuildingRef building in buildings)
		{
			key.Append(building.IsPlayerOwned ? "P" : "E");
			key.Append(building.IsDestroyed ? "D" : "A");
			key.Append(building.IsCore ? "C" : "N");
			key.Append((int)building.MinX).Append(',');
			key.Append((int)building.MinY).Append(',');
			key.Append((int)building.MaxX).Append(',');
			key.Append((int)building.MaxY).Append(';');
		}
		return key.ToString();
	}
}
