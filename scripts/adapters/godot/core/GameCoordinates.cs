using CasualCastle.Domain.Shared;
using CasualCastle.Adapters.Godot;
using Godot;
using System.Collections.Generic;

public static class GameCoordinates
{
	public const int PixelsPerCell = GameCoordinatesAdapter.PixelsPerCell;

	public const int UnitsPerCell = GameCoordinateRules.UnitsPerCell;
	public const int CellBlockSize = GameCoordinateRules.CellBlockSize;
	public const int CellBlockInset = GameCoordinateRules.CellBlockInset;
	public const int UnitSpawnCornerInset = GameCoordinateRules.UnitSpawnCornerInset;
	public const int UnitSpawnSpreadStepX = GameCoordinateRules.UnitSpawnSpreadStepX;
	public const int UnitSpawnSpreadStepY = GameCoordinateRules.UnitSpawnSpreadStepY;

	public static Vector2 ToLocalPixels(GameVector2 gamePosition) =>
		GameCoordinatesAdapter.ToLocalPixels(gamePosition);

	public static Vector2 ToLocalPixels(int gameX, int gameY) =>
		GameCoordinatesAdapter.ToLocalPixels(gameX, gameY);

	public static GameVector2 FromLocalPixels(Vector2 localPixels) =>
		GameCoordinatesAdapter.FromLocalPixels(localPixels);

	public static GameVector2 CellCenter(int gridX, int gridY) =>
		GameCoordinateRules.CellCenter(gridX, gridY);

	public static GameVector2 CellCorner(int gridX, int gridY) =>
		GameCoordinateRules.CellCorner(gridX, gridY);

	public static GameVector2 CellBlockOrigin(int gridX, int gridY) =>
		GameCoordinateRules.CellBlockOrigin(gridX, gridY);

	public static Vector2I FloorGridFromLocalPixels(Vector2 localPixels) =>
		GameCoordinatesAdapter.FloorGridFromLocalPixels(localPixels);

	public static GameVector2 GetBuildingFootprintSpawnPoint(
		IReadOnlyList<Vector2I> footprint, int anchorX, int anchorY, int spawnIndex = 0)
	{
		GridCellOffset[] offsets = GameCoordinatesAdapter.ToGridOffsets(footprint);
		return GameCoordinateRules.GetBuildingFootprintSpawnPoint(offsets, anchorX, anchorY, spawnIndex);
	}
}
