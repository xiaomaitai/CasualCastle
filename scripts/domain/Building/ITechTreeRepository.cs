// 科技树仓库端口
using System.Collections.Generic;

namespace CasualCastle.Domain.Building;

public interface ITechTreeRepository
{
	List<FactionDef> LoadFactions();
	List<TechTreeNode> LoadNodes(string factionId);
	List<CombineRecipe> LoadEdges(string factionId);
	List<UpgradeRecipe> LoadUpgradeEdges(string factionId);
	List<BuildingTypeSummary> LoadAllBuildingTypes(string factionId);
	void SaveNodes(string factionId, List<TechTreeNode> nodes);
	void SaveEdges(string factionId, List<CombineRecipe> edges);
	void SaveUpgradeEdges(string factionId, List<UpgradeRecipe> edges);
	void AddRecipe(CombineRecipe recipe);
	void RemoveRecipe(string mainTypeId, string materialTypeId, string resultTypeId);
	void SyncToGameTables(string factionId);
}

public class BuildingTypeSummary
{
	public string TypeId { get; init; }
	public string DisplayName { get; init; }
	public int Value { get; init; }
}
