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
	List<BuildingTypeSummary> LoadInventory(string factionId);
	bool IsInInventory(string factionId, string typeId);
	void AddToInventory(string factionId, string typeId);
	void RemoveFromInventory(string factionId, string typeId);
	BuildingEditData LoadBuilding(string typeId);
	void SaveBuilding(BuildingEditData building);
}

public class BuildingTypeSummary
{
	public string TypeId { get; init; }
	public string DisplayName { get; init; }
	public int Value { get; init; }
	public int MaxHealth { get; init; }
	public float SpawnInterval { get; init; }
	public string UnitTypeId { get; init; }
	public bool ShopAvailable { get; init; }
	public int GoldCost { get; init; }
	public int ShopWeight { get; init; }
}

public class BuildingEditData
{
	public string TypeId { get; init; }
	public string DisplayName { get; set; }
	public string FactionId { get; init; }
	public int MaxHealth { get; set; }
	public float SpawnInterval { get; set; }
	public int Value { get; set; }
	public string UnitTypeId { get; set; }
	public bool ShopAvailable { get; set; }
	public int GoldCost { get; set; }
	public int ShopWeight { get; set; }
	public int CollisionWidth { get; set; }
	public int CollisionHeight { get; set; }
}
