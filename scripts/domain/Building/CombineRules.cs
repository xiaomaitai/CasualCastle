// 组合规则：配方匹配与参与条件
using System.Collections.Generic;
using System.Linq;

namespace CasualCastle.Domain.Building;

public sealed class CombineGroup
{
	public IBuildingState Main { get; }
	public List<IBuildingState> Materials { get; }
	public CombineRecipe Recipe { get; }

	public CombineGroup(IBuildingState main, List<IBuildingState> materials, CombineRecipe recipe)
	{
		Main = main;
		Materials = materials;
		Recipe = recipe;
	}
}

public class CombineRules
{
	public const int MaxCombineTier = 4;
	private CombineRecipe[] _recipes = System.Array.Empty<CombineRecipe>();

	public void LoadRecipes(List<CombineRecipe> recipes)
	{
		_recipes = recipes.ToArray();
	}

	public IReadOnlyList<CombineRecipe> GetRecipes() => _recipes;

	public bool IsCombinableMaterial(string typeId, IBuildingRepository buildingRepo)
	{
		return buildingRepo.GetCombineTier(typeId) < MaxCombineTier
			&& !buildingRepo.IsCoreBuilding(typeId);
	}

	public bool CanParticipate(IBuildingState building, IBuildingRepository buildingRepo)
	{
		if (building == null || building.IsDestroyed || building.IsManuallyPaused)
			return false;
		if (building.IsCombineProhibited)
			return false;
		if (buildingRepo.IsCoreBuilding(building.TypeId))
			return false;
		if (!IsCombinableMaterial(building.TypeId, buildingRepo))
			return false;
		if (!building.IsPlayerOwned)
			return false;
		if (building.HasEnemyOnTop)
			return false;
		return true;
	}

	public bool CanCombineGroup(
		IBuildingState main, IReadOnlyList<IBuildingState> materials, CombineRecipe recipe,
		IBuildingRepository buildingRepo)
	{
		if (main == null || recipe == null || materials == null)
			return false;
		if (!main.IsPlayerOwned)
			return false;
		if (main.TypeId != recipe.MainTypeId)
			return false;
		if (!CanParticipate(main, buildingRepo))
			return false;
		if (materials.Count != recipe.MaterialCount)
			return false;

		int mainFootprintLength = buildingRepo.GetFootprint(main.TypeId).Count;
		HashSet<IBuildingState> seen = new() { main };
		foreach (IBuildingState material in materials)
		{
			if (material == null || material == main)
				return false;
			if (!seen.Add(material))
				return false;
			if (material.TypeId != recipe.MaterialTypeId)
				return false;
			if (!CanParticipate(material, buildingRepo))
				return false;
			if (buildingRepo.GetFootprint(material.TypeId).Count != mainFootprintLength)
				return false;
		}

		return true;
	}

	public CombineGroup FindBestCombinableGroup(
		IReadOnlyList<IBuildingState> buildings, HashSet<IBuildingState> used,
		IBuildingRepository buildingRepo, int coreLevel = int.MaxValue)
	{
		List<IBuildingState> ordered = buildings
			.Where(b => !used.Contains(b))
			.OrderBy(b => b.AnchorGridY)
			.ThenBy(b => b.AnchorGridX)
			.ToList();

		foreach (IBuildingState main in ordered)
		{
			if (!CanParticipate(main, buildingRepo))
				continue;

			foreach (CombineRecipe recipe in _recipes)
			{
				if (main.TypeId != recipe.MainTypeId)
					continue;
				if (!IsCoreLevelSatisfied(recipe, buildingRepo, coreLevel))
					continue;

				List<IBuildingState> materials = PickMaterials(main, recipe, ordered, used, buildingRepo);
				if (materials == null)
					continue;

				if (!CanCombineGroup(main, materials, recipe, buildingRepo))
					continue;

				return new CombineGroup(main, materials, recipe);
			}
		}

		return null;
	}

	public bool IsSameLine(string typeA, string typeB)
	{
		if (typeA == typeB)
			return true;

		foreach (CombineRecipe recipe in _recipes)
		{
			if ((recipe.MainTypeId == typeA && recipe.ResultTypeId == typeB)
				|| (recipe.MainTypeId == typeB && recipe.ResultTypeId == typeA))
				return true;
		}

		return false;
	}

	private static bool IsCoreLevelSatisfied(CombineRecipe recipe, IBuildingRepository buildingRepo, int coreLevel)
	{
		if (buildingRepo.GetFactionId(recipe.ResultTypeId) != FactionDef.NecromancerId
			&& buildingRepo.GetFactionId(recipe.ResultTypeId) != FactionDef.DungeonId)
			return true;
		int required = buildingRepo.GetFactionId(recipe.ResultTypeId) == FactionDef.NecromancerId
			? RitualCircleRules.GetRequiredCoreLevel(recipe.ResultTypeId, buildingRepo)
			: DungeonCoreRules.GetRequiredCoreLevel(recipe.ResultTypeId, buildingRepo);
		return coreLevel >= required;
	}

	private List<IBuildingState> PickMaterials(
		IBuildingState main, CombineRecipe recipe,
		IReadOnlyList<IBuildingState> allBuildings, HashSet<IBuildingState> used,
		IBuildingRepository buildingRepo)
	{
		HashSet<IAdjacencyBuilding> neighbors = AdjacentRules.GetAdjacentBuildings(main, allBuildings, buildingRepo);

		List<IBuildingState> candidates = neighbors
			.OfType<IBuildingState>()
			.Where(b => b != main
				&& !used.Contains(b)
				&& b.TypeId == recipe.MaterialTypeId
				&& CanParticipate(b, buildingRepo))
			.OrderBy(b => b.AnchorGridY)
			.ThenBy(b => b.AnchorGridX)
			.Take(recipe.MaterialCount)
			.ToList();

		return candidates.Count == recipe.MaterialCount ? candidates : null;
	}
}
