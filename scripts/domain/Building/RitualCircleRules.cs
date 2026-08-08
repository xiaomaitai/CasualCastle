// 仪式之环核心规则：等级判定、升级门槛与核心升级配方
using System.Collections.Generic;

namespace CasualCastle.Domain.Building;

public static class RitualCircleRules
{
	public const string CoreTypePrefix = "RitualCircleL";
	public static readonly string[] CoreLevelTypeIds = { "RitualCircleL1", "RitualCircleL2", "RitualCircleL3", "RitualCircleL4", "RitualCircleL5" };

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