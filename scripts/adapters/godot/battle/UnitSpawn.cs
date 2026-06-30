using CasualCastle.Domain.Shared;
using CasualCastle.Adapters.Godot;
using Godot;
using System.Collections.Generic;

public static class UnitSpawn
{
	public static Vector2 GetSpawnGlobalPosition(
		Castle castle, IReadOnlyList<Vector2I> footprint, int anchorX, int anchorY, int spawnIndex = 0)
	{
		GridCellOffset[] offsets = GameCoordinatesAdapter.ToGridOffsets(footprint);
		GameVector2 spawnPoint = GameCoordinateRules.GetBuildingFootprintSpawnPoint(
			offsets, anchorX, anchorY, spawnIndex);
		return castle.ToGlobal(GameCoordinatesAdapter.ToLocalPixels(spawnPoint));
	}

	public static void PlaceSoldier(
		Node2D battlefield, Castle castle,SoldierLogic soldier,
		IReadOnlyList<Vector2I> footprint, int anchorX, int anchorY, int spawnIndex = 0)
	{
		battlefield.AddChild(soldier);
		soldier.GlobalPosition = GetSpawnGlobalPosition(castle, footprint, anchorX, anchorY, spawnIndex);
	}
}
