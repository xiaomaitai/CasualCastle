// 纳垢活体建筑规则：成长链、升级门槛与邻近再生
using CasualCastle.Domain.Shared;
using System.Collections.Generic;

namespace CasualCastle.Domain.Building;

public static class LivingBuildingRules
{
	public static readonly string[] NurgleLine = { "FleshNest", "PusPool", "RegrowthTree", "PlagueMaw", "NurgleGarden" };
	public const float BaseGrowthIntervalSeconds = 120f;
	public const float RegrowDelaySeconds = 10f;
	public const float BreedIntervalSeconds = 30f;

	public static bool IsLivingBuilding(string typeId)
	{
		return GetLineIndex(typeId) >= 0;
	}

	public static int GetLineIndex(string typeId)
	{
		for (int i = 0; i < NurgleLine.Length; i++)
		{
			if (NurgleLine[i] == typeId)
				return i;
		}
		return -1;
	}

	public static string GetNextInLine(string typeId)
	{
		int index = GetLineIndex(typeId);
		if (index < 0 || index >= NurgleLine.Length - 1)
			return null;
		return NurgleLine[index + 1];
	}

	public static int GetRequiredKingLevel(string typeId)
	{
		int index = GetLineIndex(typeId);
		return index < 0 ? 0 : index + 2;
	}

	public static float GetGrowthInterval(string typeId, int kingLevel)
	{
		int index = GetLineIndex(typeId);
		if (index < 0)
			return float.MaxValue;
		float divisor = kingLevel > 0 ? kingLevel : 1;
		return BaseGrowthIntervalSeconds / divisor;
	}

	public static bool HasLivingNeighbor(int anchorX, int anchorY, string typeId, IReadOnlyList<IBuildingState> buildings, IBuildingRepository buildingRepo)
	{
		HashSet<(int x, int y)> cells = GetCells(anchorX, anchorY, typeId, buildingRepo);
		foreach (IBuildingState other in buildings)
		{
			if (other.IsDestroyed || !IsLivingBuilding(other.TypeId))
				continue;
			if (other.AnchorGridX == anchorX && other.AnchorGridY == anchorY)
				continue;
			foreach ((int x, int y) otherCell in GetCells(other.AnchorGridX, other.AnchorGridY, other.TypeId, buildingRepo))
			{
				if (IsAdjacentToCell(cells, otherCell))
					return true;
			}
		}
		return false;
	}

	private static bool IsAdjacentToCell(HashSet<(int x, int y)> cells, (int x, int y) otherCell)
	{
		if (cells.Contains((otherCell.x - 1, otherCell.y))
			|| cells.Contains((otherCell.x + 1, otherCell.y))
			|| cells.Contains((otherCell.x, otherCell.y - 1))
			|| cells.Contains((otherCell.x, otherCell.y + 1)))
			return true;
		return false;
	}

	private static HashSet<(int x, int y)> GetCells(int anchorX, int anchorY, string typeId, IBuildingRepository buildingRepo)
	{
		HashSet<(int x, int y)> cells = new();
		foreach (GridCellOffset offset in buildingRepo.GetFootprint(typeId))
			cells.Add((anchorX + offset.X, anchorY + offset.Y));
		return cells;
	}
}