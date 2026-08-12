// 色孽侍奉建筑规则：宝座四向邻接带来的舒适度加成
using System.Collections.Generic;
using System.Linq;

namespace CasualCastle.Domain.Building;

public sealed class SlaaneshComfortBuffs
{
	public float RestoreMultiplier = 1f;
	public float MaxComfortBonus;
	public float AttackCostMultiplier = 1f;
	public float HitCostMultiplier = 1f;
	public bool HalfCostBelowHalfComfort;
}

public static class SlaaneshServiceRules
{
	public const string HotSpring = "HotSpring";
	public const string BanquetHall = "BanquetHall";
	public const string HeartPortal = "HeartPortal";
	public const string MaidResidence = "MaidResidence";
	public const string PleasureStage = "PleasureStage";
	public const float HotSpringRestoreMultiplier = 1.3f;
	public const float BanquetHallMaxComfortBonus = 20f;
	public const float HeartPortalAttackCostMultiplier = 0.8f;
	public const float MaidResidenceHitCostMultiplier = 0.75f;

	public static SlaaneshComfortBuffs CalculateComfortBuffs(IReadOnlyList<IBuildingState> buildings, IBuildingRepository buildingRepo)
	{
		SlaaneshComfortBuffs buffs = new();
		IBuildingState throne = FindThrone(buildings);
		if (throne == null || throne is not IAdjacencyBuilding throneAdj)
			return buffs;

		List<IAdjacencyBuilding> allAdjacent = buildings.OfType<IAdjacencyBuilding>().ToList();
		foreach (IAdjacencyBuilding neighbor in AdjacentRules.GetAdjacentBuildings(throneAdj, allAdjacent, buildingRepo))
		{
			switch (neighbor.TypeId)
			{
				case HotSpring:
					buffs.RestoreMultiplier *= HotSpringRestoreMultiplier;
					break;
				case BanquetHall:
					buffs.MaxComfortBonus += BanquetHallMaxComfortBonus;
					break;
				case HeartPortal:
					buffs.AttackCostMultiplier *= HeartPortalAttackCostMultiplier;
					break;
				case MaidResidence:
					buffs.HitCostMultiplier *= MaidResidenceHitCostMultiplier;
					break;
				case PleasureStage:
					buffs.HalfCostBelowHalfComfort = true;
					break;
			}
		}
		return buffs;
	}

	private static IBuildingState FindThrone(IReadOnlyList<IBuildingState> buildings)
	{
		foreach (IBuildingState building in buildings)
		{
			if (!building.IsDestroyed && DungeonCoreRules.GetLevel(building.TypeId) > 0)
				return building;
		}
		return null;
	}
}