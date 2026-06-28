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
    private static FusionRecipe[] _recipes =
    {
        new()
        {
            MainTypeId = "Barracks",
            MaterialTypeId = "Barracks",
            MaterialCount = 1,
            ResultTypeId = "BarracksT2",
        },
        new()
        {
            MainTypeId = "WolfDen",
            MaterialTypeId = "WolfDen",
            MaterialCount = 1,
            ResultTypeId = "WolfDenT2",
        },
    };

    public static IReadOnlyList<FusionRecipe> GetRecipes() => _recipes;

    public static bool CanParticipate(IBuildingState building)
    {
        if (building == null || building.IsDestroyed || building.IsManuallyPaused)
            return false;
        if (building.IsFusionProhibited)
            return false;
        if (BuildingDefinitions.IsCoreBuilding(building.TypeId))
            return false;
        if (!BuildingDefinitions.IsFusibleMaterial(building.TypeId))
            return false;
        if (!building.IsPlayerOwned)
            return false;
        if (building.HasEnemyOnTop)
            return false;
        return true;
    }

    public static bool CanFuseGroup(
        IBuildingState main, IReadOnlyList<IBuildingState> materials, FusionRecipe recipe)
    {
        if (main == null || recipe == null || materials == null)
            return false;
        if (!main.IsPlayerOwned)
            return false;
        if (main.TypeId != recipe.MainTypeId)
            return false;
        if (!CanParticipate(main))
            return false;
        if (materials.Count != recipe.MaterialCount)
            return false;

        HashSet<IBuildingState> seen = new() { main };
        foreach (IBuildingState material in materials)
        {
            if (material == null || material == main)
                return false;
            if (!seen.Add(material))
                return false;
            if (material.TypeId != recipe.MaterialTypeId)
                return false;
            if (!CanParticipate(material))
                return false;
        }

        return true;
    }

    public static FusionGroup FindBestFusibleGroup(
        IReadOnlyList<IBuildingState> buildings, HashSet<IBuildingState> used)
    {
        List<IBuildingState> ordered = buildings
            .Where(b => !used.Contains(b))
            .OrderBy(b => b.AnchorGridY)
            .ThenBy(b => b.AnchorGridX)
            .ToList();

        foreach (IBuildingState main in ordered)
        {
            if (!CanParticipate(main))
                continue;

            foreach (FusionRecipe recipe in _recipes)
            {
                if (main.TypeId != recipe.MainTypeId)
                    continue;

                List<IBuildingState> materials = PickMaterials(main, recipe, ordered, used);
                if (materials == null)
                    continue;

                if (!CanFuseGroup(main, materials, recipe))
                    continue;

                return new FusionGroup(main, materials, recipe);
            }
        }

        return null;
    }

    private static List<IBuildingState> PickMaterials(
        IBuildingState main, FusionRecipe recipe,
        IReadOnlyList<IBuildingState> allBuildings, HashSet<IBuildingState> used)
    {
        HashSet<IAdjacencyBuilding> neighbors = AdjacentRules.GetAdjacentBuildings(main, allBuildings);

        List<IBuildingState> candidates = neighbors
            .OfType<IBuildingState>()
            .Where(b => b != main
                && !used.Contains(b)
                && b.TypeId == recipe.MaterialTypeId
                && CanParticipate(b))
            .OrderBy(b => b.AnchorGridY)
            .ThenBy(b => b.AnchorGridX)
            .Take(recipe.MaterialCount)
            .ToList();

        return candidates.Count == recipe.MaterialCount ? candidates : null;
    }
}
