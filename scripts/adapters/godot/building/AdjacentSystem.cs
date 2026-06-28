using CasualCastle.Domain.Building;
using CasualCastle.Adapters.Godot;
using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class AdjacentSystem : Node
{
    public static AdjacentSystem Instance { get; private set; }

    public override void _Ready()
    {
        Instance = this;
        AdapterRegistry.Register<AdjacentSystem>(this);
    }

    public override void _ExitTree()
    {
        if (Instance == this)
        {
            AdapterRegistry.Unregister<AdjacentSystem>(this);
            Instance = null;
        }
    }

    public void OnBuildingPlaced(Castle castle, Building placedBuilding)
    {
        RefreshCastle(castle);
        PlayAdjacencyPulses(castle, placedBuilding);
    }

    public void RefreshCastle(Castle castle)
    {
        if (castle == null)
            return;

        List<Building> buildings = castle.GetBuildings();
        foreach (Building building in buildings)
            ApplyBonuses(building, buildings);
    }

    public HashSet<Building> GetAdjacentBuildings(Building source)
    {
        if (source?.GetCastle() == null)
            return new HashSet<Building>();

        return GetAdjacentBuildings(source, source.GetCastle().GetBuildings());
    }

    public IReadOnlyList<Building> GetAdjacencyEffectTargets(Building source)
    {
        if (source?.GetCastle() == null)
            return System.Array.Empty<Building>();

        return GetAdjacencyEffectTargets(source, source.GetCastle().GetBuildings());
    }

    private void PlayAdjacencyPulses(Castle castle, Building placedBuilding)
    {
        List<Building> buildings = castle.GetBuildings();
        foreach (Building neighbor in GetAdjacentBuildings(placedBuilding, buildings))
            SpawnPulse(castle, neighbor);
    }

    private static void SpawnPulse(Castle castle, Building building)
    {
        Vector2I mainGrid = building.GetMainGridPosition();
        Vector2 localPos = castle.GetCellCenter(mainGrid.X, mainGrid.Y);

        AdjacentLinkPulse pulse = new AdjacentLinkPulse();
        pulse.Configure(castle.CellSize);
        pulse.Position = localPos;
        castle.AddChild(pulse);
    }

    private static HashSet<Building> GetAdjacentBuildings(Building source, List<Building> buildings)
    {
        List<IAdjacencyBuilding> domainBuildings = buildings.OfType<IAdjacencyBuilding>().ToList();
        HashSet<IAdjacencyBuilding> domainNeighbors = AdjacentRules.GetAdjacentBuildings(source, domainBuildings);
        return new HashSet<Building>(domainNeighbors.OfType<Building>());
    }

    private static List<Building> GetAdjacencyEffectTargets(Building source, List<Building> buildings)
    {
        List<Building> targets = new();
        if (!AdjacentRules.IsBarracksType(source.TypeId) || !source.ContributesToAdjacency)
            return targets;

        foreach (Building neighbor in GetAdjacentBuildings(source, buildings))
        {
            if (AdjacentRules.IsBarracksType(neighbor.TypeId) && neighbor.ContributesToAdjacency)
                targets.Add(neighbor);
        }
        return targets;
    }

    private static void ApplyBonuses(Building building, List<Building> buildings)
    {
        List<IAdjacencyBuilding> domainBuildings = buildings.OfType<IAdjacencyBuilding>().ToList();
        float multiplier = AdjacentRules.CalculateWorkSpeedMultiplier(building, domainBuildings);
        building.SetWorkSpeedMultiplier(multiplier);
    }
}
