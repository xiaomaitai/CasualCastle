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
		if (building == null || building.IsDestroyed || !building.IsPlayerOwned)
			return false;
		if (buildingRepo.IsCoreBuilding(building.TypeId))
			return false;
		return FindRecipe(building.TypeId) != null;
	}
}
