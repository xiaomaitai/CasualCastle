using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class FusionSystem : Node
{
    public static FusionSystem Instance { get; private set; }

    [Signal]
    public delegate void FusionCompletedEventHandler(Castle castle, Building result);

    private static readonly FusionRecipe[] Recipes =
    {
        new()
        {
            MainTypeId = "Barracks",
            MaterialTypeId = "Barracks",
            MaterialCount = 1,
            GoldCost = 8,
            ResultTypeId = "BarracksT2",
        },
        new()
        {
            MainTypeId = "WolfDen",
            MaterialTypeId = "WolfDen",
            MaterialCount = 1,
            GoldCost = 10,
            ResultTypeId = "WolfDenT2",
        },
    };

    public override void _Ready()
    {
        Instance = this;
    }

    public override void _ExitTree()
    {
        if (Instance == this)
            Instance = null;
    }

    public static IReadOnlyList<FusionRecipe> GetRecipes() => Recipes;

    public void ResolveNightFusions(Castle castle)
    {
        if (castle == null || !castle.IsPlayerCastle)
            return;

        if (GameManager.Instance?.CurrentState != GameManager.GameState.Playing)
            return;

        if (!GameManager.Instance.IsNight)
            return;

        HashSet<Building> used = new();

        while (true)
        {
            FusionGroup group = FindBestFusibleGroup(castle, used);
            if (group == null)
                break;

            if (!TryFuseGroup(castle, group))
            {
                used.Add(group.Main);
                foreach (Building material in group.Materials)
                    used.Add(material);
                continue;
            }
        }
    }

    public bool CanFuseGroup(Castle castle, Building main, IReadOnlyList<Building> materials, FusionRecipe recipe)
    {
        if (castle == null || main == null || recipe == null || materials == null)
            return false;

        if (!castle.IsPlayerCastle)
            return false;

        if (main.TypeId != recipe.MainTypeId)
            return false;

        if (!CanParticipate(main))
            return false;

        if (materials.Count != recipe.MaterialCount)
            return false;

        HashSet<Building> seen = new() { main };

        foreach (Building material in materials)
        {
            if (material == null || material == main)
                return false;

            if (!seen.Add(material))
                return false;

            if (material.TypeId != recipe.MaterialTypeId)
                return false;

            if (!CanParticipate(material))
                return false;

            if (!IsAdjacentToMain(main, material))
                return false;
        }

        if (ShopSystem.Instance == null || !ShopSystem.Instance.CanAfford(recipe.GoldCost))
            return false;

        return true;
    }

    public bool TryFuseGroup(Castle castle, FusionGroup group)
    {
        if (!CanFuseGroup(castle, group.Main, group.Materials, group.Recipe))
            return false;

        if (!ShopSystem.Instance.TrySpendGold(group.Recipe.GoldCost))
            return false;

        int anchorX = group.Main.AnchorGridX;
        int anchorY = group.Main.AnchorGridY;
        string resultTypeId = group.Recipe.ResultTypeId;

        foreach (Building material in group.Materials)
            RemoveBuilding(castle, material);

        RemoveBuilding(castle, group.Main);

        Building result = BuildingSystem.CreateBuilding(resultTypeId);
        if (result == null)
            return false;

        result.BindToGrid(castle, anchorX, anchorY);
        if (!castle.PlaceBuilding(result, anchorX, anchorY, resultTypeId))
        {
            result.QueueFree();
            return false;
        }

        AdjacentSystem.Instance?.RefreshCastle(castle);
        EmitSignal(SignalName.FusionCompleted, castle, result);
        return true;
    }

    private FusionGroup FindBestFusibleGroup(Castle castle, HashSet<Building> used)
    {
        List<Building> buildings = castle.GetBuildings()
            .Where(b => !used.Contains(b))
            .OrderBy(b => b.AnchorGridY)
            .ThenBy(b => b.AnchorGridX)
            .ToList();

        foreach (Building main in buildings)
        {
            if (!CanParticipate(main))
                continue;

            foreach (FusionRecipe recipe in Recipes)
            {
                if (main.TypeId != recipe.MainTypeId)
                    continue;

                List<Building> materials = PickMaterials(main, recipe, used);
                if (materials == null)
                    continue;

                if (!CanFuseGroup(castle, main, materials, recipe))
                    continue;

                return new FusionGroup(main, materials, recipe);
            }
        }

        return null;
    }

    private static List<Building> PickMaterials(Building main, FusionRecipe recipe, HashSet<Building> used)
    {
        if (AdjacentSystem.Instance == null)
            return null;

        List<Building> candidates = AdjacentSystem.Instance
            .GetAdjacentBuildings(main)
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

    private static bool CanParticipate(Building building)
    {
        if (building == null || building.IsDestroyed || building.IsManuallyPaused)
            return false;

        if (building.IsFusionProhibited)
            return false;

        if (BuildingSystem.IsCoreBuilding(building.TypeId))
            return false;

        if (!BuildingSystem.IsFusibleMaterial(building.TypeId))
            return false;

        Castle castle = building.GetCastle();
        if (castle == null || !castle.IsPlayerCastle)
            return false;

        if (building.HasEnemyOnTop)
            return false;

        return true;
    }

    private static bool IsAdjacentToMain(Building main, Building other)
    {
        return AdjacentSystem.Instance?.GetAdjacentBuildings(main).Contains(other) == true;
    }

    private static void RemoveBuilding(Castle castle, Building building)
    {
        castle.ReleaseBuildingFootprint(building);
        building.GetParent()?.RemoveChild(building);
        building.QueueFree();
    }

    public sealed class FusionGroup
    {
        public Building Main { get; }
        public List<Building> Materials { get; }
        public FusionRecipe Recipe { get; }

        public FusionGroup(Building main, List<Building> materials, FusionRecipe recipe)
        {
            Main = main;
            Materials = materials;
            Recipe = recipe;
        }
    }
}
