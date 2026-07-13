using CasualCastle.Domain.Shared;
using Godot;
using System;
using System.Collections.Generic;

namespace CasualCastle.Adapters.Godot;

public static class GameCoordinatesAdapter
{
	public const int PixelsPerCell = 100;

	public static float GameUnitsToPixels(float gameUnits) =>
		gameUnits * PixelsPerCell / (float)GameCoordinateRules.UnitsPerCell;

	public static float PixelsToGameUnits(float pixels) =>
		pixels * GameCoordinateRules.UnitsPerCell / PixelsPerCell;

	public static Vector2 ToLocalPixels(GameVector2 gamePosition) =>
		new(
			gamePosition.X * PixelsPerCell / (float)GameCoordinateRules.UnitsPerCell,
			gamePosition.Y * PixelsPerCell / (float)GameCoordinateRules.UnitsPerCell);

	public static Vector2 ToLocalPixels(int gameX, int gameY) =>
		ToLocalPixels(new GameVector2(gameX, gameY));

	public static GameVector2 FromLocalPixels(Vector2 localPixels) =>
		new(
			(int)Math.Round(localPixels.X * GameCoordinateRules.UnitsPerCell / (float)PixelsPerCell),
			(int)Math.Round(localPixels.Y * GameCoordinateRules.UnitsPerCell / (float)PixelsPerCell));

	public static Vector2I FloorGridFromLocalPixels(Vector2 localPixels) =>
		new(
			(int)Math.Floor(localPixels.X / PixelsPerCell),
			(int)Math.Floor(localPixels.Y / PixelsPerCell));

	public static GridCellOffset[] ToGridOffsets(IReadOnlyList<Vector2I> footprint)
	{
		GridCellOffset[] offsets = new GridCellOffset[footprint.Count];
		for (int i = 0; i < footprint.Count; i++)
			offsets[i] = new GridCellOffset(footprint[i].X, footprint[i].Y);
		return offsets;
	}
}
