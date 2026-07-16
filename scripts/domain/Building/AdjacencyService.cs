using System.Collections.Generic;
using System.Linq;

namespace CasualCastle.Domain.Building;

public class AdjacencyService
{
	private readonly IBuildingRepository _buildingRepo;
	private readonly CombineRules _combineRules;

	public AdjacencyService(IBuildingRepository buildingRepo, CombineRules combineRules)
	{
		_buildingRepo = buildingRepo;
		_combineRules = combineRules;
	}

	public void RefreshCastle(List<IBuildingState> buildings)
	{
		foreach (IBuildingState building in buildings)
			ApplyBonuses(building, buildings);
	}

	public HashSet<IBuildingState> GetAdjacentBuildings(IAdjacencyBuilding source, List<IBuildingState> allBuildings)
	{
		List<IAdjacencyBuilding> domainBuildings = allBuildings.OfType<IAdjacencyBuilding>().ToList();
		HashSet<IAdjacencyBuilding> neighbors = AdjacentRules.GetAdjacentBuildings(source, domainBuildings, _buildingRepo);
		return new HashSet<IBuildingState>(neighbors.OfType<IBuildingState>());
	}

	public IReadOnlyList<IAdjacencyBuilding> GetAdjacentSameTypeTargets(IAdjacencyBuilding source, List<IBuildingState> allBuildings)
	{
		List<IAdjacencyBuilding> results = new();
		if (!source.ContributesToAdjacency)
			return results;

		HashSet<IBuildingState> neighbors = GetAdjacentBuildings(source, allBuildings);
		foreach (IBuildingState neighbor in neighbors)
		{
			if (neighbor is IAdjacencyBuilding adj &&
				_combineRules.IsSameLine(source.TypeId, adj.TypeId) &&
				adj.ContributesToAdjacency)
			{
				results.Add(adj);
			}
		}
		return results;
	}

	private void ApplyBonuses(IBuildingState building, List<IBuildingState> allBuildings)
	{
		if (building is not IAdjacencyBuilding adjBuilding)
			return;

		List<IAdjacencyBuilding> domainBuildings = allBuildings.OfType<IAdjacencyBuilding>().ToList();
		float multiplier = AdjacentRules.CalculateWorkSpeedMultiplier(adjBuilding, domainBuildings, _buildingRepo, _combineRules.IsSameLine);
		adjBuilding.SetWorkSpeedMultiplier(multiplier);
	}
}
