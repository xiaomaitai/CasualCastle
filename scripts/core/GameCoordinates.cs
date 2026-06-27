using Godot;
using System.Collections.Generic;

// 游戏坐标：整数，每格 100 单位；(0,0) 为城堡网格左上角，(100,0) 为右侧相邻格左上角。
// 与场景本地像素之间仅通过本类换算，逻辑层不直接写像素偏移。
public readonly struct GameVector2
{
	public int X { get; }
	public int Y { get; }

	public GameVector2(int x, int y)
	{
		X = x;
		Y = y;
	}

	public static GameVector2 operator +(GameVector2 a, GameVector2 b) => new(a.X + b.X, a.Y + b.Y);
	public static GameVector2 operator -(GameVector2 a, GameVector2 b) => new(a.X - b.X, a.Y - b.Y);
}

public static class GameCoordinates
{
	public const int UnitsPerCell = 100;
	public const int PixelsPerCell = 64;

	// 格子内绘制方块边长（游戏坐标，60px / 64px ≈ 94）
	public const int CellBlockSize = 94;
	public const int CellBlockInset = (UnitsPerCell - CellBlockSize) / 2;

	// 产兵点距建筑可视方块左下角的内缩（游戏坐标）
	public const int UnitSpawnCornerInset = 3;

	// 连续产兵时的轻微错开（游戏坐标）
	public const int UnitSpawnSpreadStepX = 3;
	public const int UnitSpawnSpreadStepY = 3;

	public static Vector2 ToLocalPixels(GameVector2 gamePosition) =>
		new(gamePosition.X * PixelsPerCell / (float)UnitsPerCell, gamePosition.Y * PixelsPerCell / (float)UnitsPerCell);

	public static Vector2 ToLocalPixels(int gameX, int gameY) =>
		new(gameX * PixelsPerCell / (float)UnitsPerCell, gameY * PixelsPerCell / (float)UnitsPerCell);

	public static GameVector2 FromLocalPixels(Vector2 localPixels) =>
		new(
			Mathf.RoundToInt(localPixels.X * UnitsPerCell / (float)PixelsPerCell),
			Mathf.RoundToInt(localPixels.Y * UnitsPerCell / (float)PixelsPerCell));

	public static GameVector2 CellCenter(int gridX, int gridY) =>
		new(gridX * UnitsPerCell + UnitsPerCell / 2, gridY * UnitsPerCell + UnitsPerCell / 2);

	public static GameVector2 CellCorner(int gridX, int gridY) =>
		new(gridX * UnitsPerCell, gridY * UnitsPerCell);

	public static GameVector2 CellBlockOrigin(int gridX, int gridY) =>
		new(gridX * UnitsPerCell + CellBlockInset, gridY * UnitsPerCell + CellBlockInset);

	public static Vector2I FloorGridFromLocalPixels(Vector2 localPixels) =>
		new(Mathf.FloorToInt(localPixels.X / PixelsPerCell), Mathf.FloorToInt(localPixels.Y / PixelsPerCell));

	// 在建筑占地框左下角（可视方块边缘）附近产兵。
	public static GameVector2 GetBuildingFootprintSpawnPoint(
		IReadOnlyList<Vector2I> footprint, int anchorX, int anchorY, int spawnIndex = 0)
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
