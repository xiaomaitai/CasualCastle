using System.Collections.Generic;

namespace CasualCastle.Domain.Shared;

public static class GameCoordinateRules
{
	public const int UnitsPerCell = 100;

	public const int CellGapSize = 14;
	public const int CellBlockSize = UnitsPerCell - CellGapSize;
	public const int CellBlockInset = CellGapSize / 2;

	public const int UnitSpawnCornerInset = 3;
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

		int cellsWide = maxX - minX + 1;
		int cellsTall = maxY - minY + 1;
		return new GameVector2(
			cellsWide * UnitsPerCell - CellGapSize,
			cellsTall * UnitsPerCell - CellGapSize
		);
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

		int spreadIndex = spawnIndex % 3;
		int spreadX = spreadIndex * UnitSpawnSpreadStepX;
		int spreadY = spreadIndex * UnitSpawnSpreadStepY;

		return new GameVector2(
			blockLeft + UnitSpawnCornerInset + spreadX,
			blockBottom - UnitSpawnCornerInset - spreadY
		);
	}
}
