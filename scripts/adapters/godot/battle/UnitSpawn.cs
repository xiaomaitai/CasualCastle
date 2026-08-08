// 单位生成工具：出生点计算与战场放置
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

	public static void PlaceUnit(
		Node2D battlefield, Castle castle, Unit unit,
		IReadOnlyList<Vector2I> footprint, int anchorX, int anchorY, int spawnIndex = 0)
	{
		unit.GlobalPosition = GetSpawnGlobalPosition(castle, footprint, anchorX, anchorY, spawnIndex);
		battlefield.AddChild(unit);
	}
}
