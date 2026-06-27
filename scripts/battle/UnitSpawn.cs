using Godot;
using System.Collections.Generic;

public static class UnitSpawn
{
	public static Vector2 GetSpawnGlobalPosition(
		Castle castle, IReadOnlyList<Vector2I> footprint, int anchorX, int anchorY, int spawnIndex = 0)
	{
		GameVector2 spawnPoint = GameCoordinates.GetBuildingFootprintSpawnPoint(
			footprint, anchorX, anchorY, spawnIndex);
		return castle.ToGlobal(GameCoordinates.ToLocalPixels(spawnPoint));
	}

	public static void PlaceSoldier(
		Node2D battlefield, Castle castle, Soldier soldier,
		IReadOnlyList<Vector2I> footprint, int anchorX, int anchorY, int spawnIndex = 0)
	{
		battlefield.AddChild(soldier);
		soldier.GlobalPosition = GetSpawnGlobalPosition(castle, footprint, anchorX, anchorY, spawnIndex);
	}
}
