using System.Collections.Generic;
using System.Linq;

namespace CasualCastle.Domain.Building;

public class AdjacencyService
{
    public void RefreshCastle(List<IBuildingState> buildings)
    {
        foreach (IBuildingState building in buildings)
            ApplyBonuses(building, buildings);
    }

    public HashSet<IBuildingState> GetAdjacentBuildings(IAdjacencyBuilding source, List<IBuildingState> allBuildings)
    {
        List<IAdjacencyBuilding> domainBuildings = allBuildings.OfType<IAdjacencyBuilding>().ToList();
        HashSet<IAdjacencyBuilding> neighbors = AdjacentRules.GetAdjacentBuildings(source, domainBuildings);
        return new HashSet<IBuildingState>(neighbors.OfType<IBuildingState>());
    }

    public IReadOnlyList<IAdjacencyBuilding> GetBarracksTargets(IAdjacencyBuilding source, List<IBuildingState> allBuildings)
    {
        List<IAdjacencyBuilding> results = new();
        if (!AdjacentRules.IsBarracksType(source.TypeId) || !source.ContributesToAdjacency)
            return results;

        HashSet<IBuildingState> neighbors = GetAdjacentBuildings(source, allBuildings);
        foreach (IBuildingState neighbor in neighbors)
        {
            if (neighbor is IAdjacencyBuilding adj &&
                AdjacentRules.IsBarracksType(adj.TypeId) &&
                adj.ContributesToAdjacency)
            {
                results.Add(adj);
            }
        }
        return results;
    }

    private static void ApplyBonuses(IBuildingState building, List<IBuildingState> allBuildings)
    {
        if (building is not IAdjacencyBuilding adjBuilding)
            return;

        List<IAdjacencyBuilding> domainBuildings = allBuildings.OfType<IAdjacencyBuilding>().ToList();
        float multiplier = AdjacentRules.CalculateWorkSpeedMultiplier(adjBuilding, domainBuildings);
        adjBuilding.SetWorkSpeedMultiplier(multiplier);
    }
}
