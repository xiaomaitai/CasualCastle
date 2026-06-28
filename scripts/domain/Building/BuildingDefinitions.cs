using CasualCastle.Domain.Shared;
using System.Collections.Generic;

namespace CasualCastle.Domain.Building;

public class BuildingData
{
	public string TypeId { get; init; }
	public GridCellOffset[] Footprint { get; init; }
	public GridCellOffset MainCellOffset { get; init; }
	public string DisplayName { get; init; }
	public int MaxHealth { get; init; }

	public float SpawnInterval { get; init; }
	public GridCellOffset SpawnCellOffset { get; init; }

	public string UnitTypeId { get; init; }
	public bool HasNightCombat { get; init; }
	public bool IsCore { get; init; }

	public int FusionTier { get; init; }
}

public static class BuildingDefinitions
{
	private static Dictionary<string, BuildingData> _data = new();

	public static void LoadFrom(Dictionary<string, BuildingData> data)
	{
		_data = data;
	}

	public static BuildingData Get(string typeId)
	{
		if (_data.TryGetValue(typeId, out BuildingData buildingData))
			return buildingData;
		return _data["Barracks"];
	}

	public static IReadOnlyList<GridCellOffset> GetFootprint(string typeId) => Get(typeId).Footprint;

	public static string GetDisplayName(string typeId) => Get(typeId).DisplayName;

	public static int GetMaxHealth(string typeId) => Get(typeId).MaxHealth;

	public static float GetSpawnInterval(string typeId) => Get(typeId).SpawnInterval;

	public static GridCellOffset GetMainCellOffset(string typeId) => Get(typeId).MainCellOffset;

	public static GridCellOffset GetSpawnCellOffset(string typeId) => Get(typeId).SpawnCellOffset;

	public static string GetUnitTypeId(string typeId) => Get(typeId).UnitTypeId;

	public static bool GetHasNightCombat(string typeId) => Get(typeId).HasNightCombat;

	public static int GetFusionTier(string typeId) => Get(typeId).FusionTier;

	public static bool IsCoreBuilding(string typeId) => Get(typeId).IsCore;

	public static bool IsFusibleMaterial(string typeId)
	{
		BuildingData bd = Get(typeId);
		return bd.FusionTier == 0
			&& bd.Footprint.Length == 1
			&& !bd.IsCore;
	}
}
