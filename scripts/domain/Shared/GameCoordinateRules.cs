// 网格坐标规则：格子中心与出生点
using System;
using System.Collections.Generic;

namespace CasualCastle.Domain.Shared;

public static class GameCoordinateRules
{
	public const int UnitsPerCell = 100;

	public const int CellGapSize = 14;
	public const int CellBlockSize = UnitsPerCell - CellGapSize;
	public const int CellBlockInset = CellGapSize / 2;

	public const int BuildingCorridorGap = 112;
	public const int MinBuildingCollisionSize = 24;

	public const int UnitSpawnCornerInset = 3;
	public const int UnitSpawnSpreadCount = 3;
	public const int UnitSpawnSpreadStepX = 3;
	public const int UnitSpawnSpreadStepY = 3;

	public static GameVector2 CellCenter(int gridX, int gridY) =>
		new(gridX * UnitsPerCell + UnitsPerCell / 2, gridY * UnitsPerCell + UnitsPerCell / 2);

	public static GameVector2 CellCorner(int gridX, int gridY) =>
		new(gridX * UnitsPerCell, gridY * UnitsPerCell);

	public static GameVector2 CellBlockOrigin(int gridX, int gridY) =>
		new(gridX * UnitsPerCell + CellBlockInset, gridY * UnitsPerCell + CellBlockInset);

	public static GameVector2 GetBuildingCollisionSize(IReadOnlyList<GridCellOffset> footprint)
	{
		GetFootprintCellSpan(footprint, out int cellsWide, out int cellsTall);
		return new GameVector2(
			Math.Max(cellsWide * UnitsPerCell - BuildingCorridorGap, MinBuildingCollisionSize),
			Math.Max(cellsTall * UnitsPerCell - BuildingCorridorGap, MinBuildingCollisionSize)
		);
	}

	public static GameVector2 GetBuildingVisualSize(IReadOnlyList<GridCellOffset> footprint)
	{
		GetFootprintCellSpan(footprint, out int cellsWide, out int cellsTall);
		return new GameVector2(
			cellsWide * UnitsPerCell - CellGapSize,
			cellsTall * UnitsPerCell - CellGapSize
		);
	}

	private static void GetFootprintCellSpan(IReadOnlyList<GridCellOffset> footprint, out int cellsWide, out int cellsTall)
	{
		int minX = footprint[0].X;
		int maxX = footprint[0].X;
		int minY = footprint[0].Y;
		int maxY = footprint[0].Y;
		for (int i = 1; i < footprint.Count; i++)
		{
			if (footprint[i].X < minX) minX = footprint[i].X;
			if (footprint[i].X > maxX) maxX = footprint[i].X;
			if (footprint[i].Y < minY) minY = footprint[i].Y;
			if (footprint[i].Y > maxY) maxY = footprint[i].Y;
		}
		cellsWide = maxX - minX + 1;
		cellsTall = maxY - minY + 1;
	}

	public static GameVector2 GetBuildingFootprintSpawnPoint(
		IReadOnlyList<GridCellOffset> footprint, int anchorX, int anchorY, int spawnIndex = 0)
	{
		int minGridX = anchorX + footprint[0].X;
		int maxGridY = anchorY + footprint[0].Y;
		for (int i = 1; i < footprint.Count; i++)
		{
			int gridX = anchorX + footprint[i].X;
			int gridY = anchorY + footprint[i].Y;
			if (gridX < minGridX)
				minGridX = gridX;
			if (gridY > maxGridY)
				maxGridY = gridY;
		}

		int blockLeft = minGridX * UnitsPerCell + CellBlockInset;
		int blockBottom = maxGridY * UnitsPerCell + CellBlockInset + CellBlockSize;

		int spreadIndex = spawnIndex % UnitSpawnSpreadCount;
		int spreadX = spreadIndex * UnitSpawnSpreadStepX;
		int spreadY = spreadIndex * UnitSpawnSpreadStepY;

		return new GameVector2(
			blockLeft + UnitSpawnCornerInset + spreadX,
			blockBottom - UnitSpawnCornerInset - spreadY
		);
	}
}
