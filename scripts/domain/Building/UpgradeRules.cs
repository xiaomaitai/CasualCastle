using System.Collections.Generic;
using System.Linq;

namespace CasualCastle.Domain.Building;

public class UpgradeRules
{
	private UpgradeRecipe[] _recipes = System.Array.Empty<UpgradeRecipe>();

	public void LoadRecipes(List<UpgradeRecipe> recipes)
	{
		_recipes = recipes.ToArray();
	}

	public IReadOnlyList<UpgradeRecipe> GetRecipes() => _recipes;

	public UpgradeRecipe FindRecipe(string sourceTypeId)
	{
		return _recipes.FirstOrDefault(r => r.SourceTypeId == sourceTypeId);
	}

	public int CalculateCost(string sourceTypeId, string resultTypeId, IBuildingRepository buildingRepo)
	{
		return buildingRepo.GetValue(resultTypeId) - buildingRepo.GetValue(sourceTypeId);
	}

	public bool CanUpgrade(IBuildingState building, IBuildingRepository buildingRepo)
	{
		return CanUpgrade(building, buildingRepo, int.MaxValue, 0);
	}

	public bool CanUpgrade(IBuildingState building, IBuildingRepository buildingRepo, int coreLevel, int burrowCount)
	{
		if (building == null || building.IsDestroyed || !building.IsPlayerOwned)
			return false;

		UpgradeRecipe recipe = FindRecipe(building.TypeId);
		if (recipe == null)
			return false;

		if (HordeCoreRules.IsCoreUpgrade(building.TypeId, recipe.ResultTypeId))
			return burrowCount >= HordeCoreRules.GetUpgradeBurrowRequirement(HordeCoreRules.GetLevel(building.TypeId));

		if (RitualCircleRules.IsCoreUpgrade(building.TypeId, recipe.ResultTypeId))
			return true;

		if (buildingRepo.GetFactionId(building.TypeId) == FactionDef.HordeId)
			return coreLevel >= HordeCoreRules.GetRequiredCoreLevel(recipe.ResultTypeId, buildingRepo);

		if (buildingRepo.GetFactionId(building.TypeId) == FactionDef.NecromancerId)
			return coreLevel >= RitualCircleRules.GetRequiredCoreLevel(recipe.ResultTypeId, buildingRepo);

		return true;
	}
}
