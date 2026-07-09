using System.Collections.Generic;
using System.Linq;

namespace CasualCastle.Domain.Building;

public sealed class FusionGroup
{
    public IBuildingState Main { get; }
    public List<IBuildingState> Materials { get; }
    public FusionRecipe Recipe { get; }

    public FusionGroup(IBuildingState main, List<IBuildingState> materials, FusionRecipe recipe)
    {
        Main = main;
        Materials = materials;
        Recipe = recipe;
    }
}

public static class FusionRules
{
    private static FusionRecipe[] _recipes = System.Array.Empty<FusionRecipe>();

    public static void LoadRecipes(List<FusionRecipe> recipes)
    {
        _recipes = recipes.ToArray();
    }

    public static IReadOnlyList<FusionRecipe> GetRecipes() => _recipes;

    public static bool CanParticipate(IBuildingState building, IBuildingRepository buildingRepo)
    {
        if (building == null || building.IsDestroyed || building.IsManuallyPaused)
            return false;
        if (building.IsFusionProhibited)
            return false;
        if (buildingRepo.IsCoreBuilding(building.TypeId))
            return false;
        if (!buildingRepo.IsFusibleMaterial(building.TypeId))
            return false;
        if (!building.IsPlayerOwned)
            return false;
        if (building.HasEnemyOnTop)
            return false;
        return true;
    }

    public static bool CanFuseGroup(
        IBuildingState main, IReadOnlyList<IBuildingState> materials, FusionRecipe recipe,
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

    public static FusionGroup FindBestFusibleGroup(
        IReadOnlyList<IBuildingState> buildings, HashSet<IBuildingState> used,
        IBuildingRepository buildingRepo)
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

            foreach (FusionRecipe recipe in _recipes)
            {
                if (main.TypeId != recipe.MainTypeId)
                    continue;

                List<IBuildingState> materials = PickMaterials(main, recipe, ordered, used, buildingRepo);
                if (materials == null)
                    continue;

                if (!CanFuseGroup(main, materials, recipe, buildingRepo))
                    continue;

                return new FusionGroup(main, materials, recipe);
            }
        }

        return null;
    }

    private static List<IBuildingState> PickMaterials(
        IBuildingState main, FusionRecipe recipe,
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
