using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Building;
using CasualCastle.Adapters.Godot;
using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class FusionSystem : Node
{
    public static FusionSystem Instance { get; private set; }

    [Signal]
    public delegate void FusionCompletedEventHandler(Castle castle, Building result);

    private IGameState _gameState;
    private AdjacentSystem _adjacentSystem;

    public override void _Ready()
    {
        Instance = this;
        AdapterRegistry.Register<FusionSystem>(this);
        _gameState = AdapterRegistry.Resolve<IGameState>();
        _adjacentSystem = AdapterRegistry.Resolve<AdjacentSystem>();
    }

    public override void _ExitTree()
    {
        if (Instance == this)
        {
            AdapterRegistry.Unregister<FusionSystem>(this);
            Instance = null;
        }
    }

    public static IReadOnlyList<FusionRecipe> GetRecipes() => FusionRules.GetRecipes();

    public void ResolveNightFusions(Castle castle)
    {
        if (castle == null || !castle.IsPlayerCastle)
            return;

        if (!_gameState.IsPlaying || !_gameState.IsNight)
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
            }
        }
    }

    public bool CanFuseGroup(Castle castle, Building main, IReadOnlyList<Building> materials, FusionRecipe recipe)
    {
        if (castle == null || main == null || recipe == null || materials == null)
            return false;

        if (!castle.IsPlayerCastle)
            return false;

        List<IBuildingState> domainMaterials = materials.OfType<IBuildingState>().ToList();
        return FusionRules.CanFuseGroup(main, domainMaterials, recipe);
    }

    public bool TryFuseGroup(Castle castle, FusionGroup group)
    {
        if (!CanFuseGroup(castle, group.Main, group.Materials, group.Recipe))
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

        _adjacentSystem.RefreshCastle(castle);
        EmitSignal(SignalName.FusionCompleted, castle, result);
        return true;
    }

    private FusionGroup FindBestFusibleGroup(Castle castle, HashSet<Building> used)
    {
        List<IBuildingState> domainBuildings = castle.GetBuildings().OfType<IBuildingState>().ToList();
        HashSet<IBuildingState> usedDomain = new HashSet<IBuildingState>(used.OfType<IBuildingState>());

        CasualCastle.Domain.Building.FusionGroup result = FusionRules.FindBestFusibleGroup(domainBuildings, usedDomain);
        if (result == null)
            return null;

        return new FusionGroup(
            (Building)result.Main,
            result.Materials.Select(m => (Building)m).ToList(),
            result.Recipe);
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
