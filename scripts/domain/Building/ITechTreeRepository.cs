using System.Collections.Generic;

namespace CasualCastle.Domain.Building;

public interface ITechTreeRepository
{
    List<RaceDef> LoadRaces();
    List<TechTreeNode> LoadNodes(string raceId);
    List<CombineRecipe> LoadEdges(string raceId);
    List<BuildingTypeSummary> LoadAllBuildingTypes(string raceId);
    void SaveNodes(string raceId, List<TechTreeNode> nodes);
    void AddRecipe(CombineRecipe recipe);
    void RemoveRecipe(string mainTypeId, string materialTypeId, string resultTypeId);
    void SyncToGameTables(string raceId);
}

public class BuildingTypeSummary
{
    public string TypeId { get; init; }
    public string DisplayName { get; init; }
}
