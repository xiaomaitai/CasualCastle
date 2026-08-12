// 地下城核心规则：魔王宝座等级判定与升级配方
using CasualCastle.Domain.Shared;
using System.Collections.Generic;

namespace CasualCastle.Domain.Building;

public static class DungeonCoreRules
{
	public const string CoreTypePrefix = BuildingTypeIds.DungeonThronePrefix;
	public static readonly string[] CoreLevelTypeIds = { "DungeonThroneL1", "DungeonThroneL2", "DungeonThroneL3", "DungeonThroneL4", "DungeonThroneL5" };

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

	public static string GetNextLevelTypeId(string typeId)
	{
		int level = GetLevel(typeId);
		if (level <= 0 || level >= CoreLevelTypeIds.Length)
			return null;
		return CoreLevelTypeIds[level];
	}

	public static int GetRequiredCoreLevel(string resultTypeId, IBuildingRepository buildingRepo)
	{
		return buildingRepo.GetCombineTier(resultTypeId) + 1;
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