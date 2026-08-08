// 部落核心规则：等级判定与升级门槛
using System.Collections.Generic;

using CasualCastle.Domain.Shared;

namespace CasualCastle.Domain.Building;

public static class HordeCoreRules
{
	public const string BurrowTypeId = BuildingTypeIds.Burrow;
	public const string CoreTypePrefix = BuildingTypeIds.HordeCorePrefix;
	public static readonly string[] CoreLevelTypeIds = { "HordeCoreL1", "HordeCoreL2", "HordeCoreL3", "HordeCoreL4", "HordeCoreL5" };
	private static readonly int[] UpgradeBurrowRequirements = { 1, 3, 10, 30 };

	public static bool IsBurrow(string typeId)
	{
		return typeId == BurrowTypeId;
	}

	public static int GetLevel(string typeId)
	{
		if (typeId == null || !typeId.StartsWith(CoreTypePrefix))
			return 0;
		string suffix = typeId.Substring(CoreTypePrefix.Length);
		return int.TryParse(suffix, out int level) ? level : 0;
	}

	public static bool IsCoreUpgrade(string sourceTypeId, string resultTypeId)
	{
		int level = GetLevel(sourceTypeId);
		return level > 0 && GetLevel(resultTypeId) == level + 1;
	}

	public static int GetUpgradeBurrowRequirement(int currentLevel)
	{
		if (currentLevel < 1 || currentLevel > UpgradeBurrowRequirements.Length)
			return int.MaxValue;
		return UpgradeBurrowRequirements[currentLevel - 1];
	}

	public static int GetRequiredCoreLevel(string resultTypeId, IBuildingRepository buildingRepo)
	{
		return buildingRepo.GetCombineTier(resultTypeId) + 1;
	}

	public static int CountBurrows(IReadOnlyList<IBuildingState> buildings)
	{
		int count = 0;
		foreach (IBuildingState building in buildings)
		{
			if (IsBurrow(building.TypeId) && !building.IsDestroyed)
				count++;
		}
		return count;
	}

	public static int GetCoreLevel(IReadOnlyList<IBuildingState> buildings)
	{
		int level = 0;
		foreach (IBuildingState building in buildings)
		{
			int candidate = GetLevel(building.TypeId);
			if (!building.IsDestroyed && candidate > level)
				level = candidate;
		}
		return level;
	}

	public static List<UpgradeRecipe> CreateUpgradeRecipes()
	{
		List<UpgradeRecipe> recipes = new();
		for (int i = 0; i < CoreLevelTypeIds.Length - 1; i++)
		{
			recipes.Add(new UpgradeRecipe
			{
				SourceTypeId = CoreLevelTypeIds[i],
				ResultTypeId = CoreLevelTypeIds[i + 1]
			});
		}
		return recipes;
	}
}
