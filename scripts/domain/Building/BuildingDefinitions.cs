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

	public int? SoldierDamage { get; init; }
	public float? SoldierAttackRange { get; init; }
	public float? SoldierSpeed { get; init; }
	public int? SoldierHealth { get; init; }
	public bool HasNightCombat { get; init; }

	public int FusionTier { get; init; }
}

public static class BuildingDefinitions
{
	private static readonly GridCellOffset[] SingleCell = { new(0, 0) };
	private static readonly GridCellOffset[] ArcheryRangeCells = { new(0, 0), new(1, 0) };
	private static readonly GridCellOffset[] StableCells =
	{
		new(0, 0), new(0, 1), new(0, 2), new(1, 2),
	};
	private static readonly GridCellOffset[] CastleHeartCells =
	{
		new(0, 0), new(1, 0), new(0, 1), new(1, 1),
	};

	private static readonly Dictionary<string, BuildingData> Data = new()
	{
		["CastleHeart"] = new()
		{
			TypeId = "CastleHeart",
			Footprint = CastleHeartCells,
			MainCellOffset = new(0, 0),
			DisplayName = "城堡之心",
			MaxHealth = 500,
		},
		["Barracks"] = new()
		{
			TypeId = "Barracks",
			Footprint = SingleCell,
			MainCellOffset = new(0, 0),
			DisplayName = "兵营",
			MaxHealth = 100,
			SpawnInterval = 5f,
		},
		["ArcheryRange"] = new()
		{
			TypeId = "ArcheryRange",
			Footprint = ArcheryRangeCells,
			MainCellOffset = new(0, 0),
			DisplayName = "靶场",
			MaxHealth = 120,
			SpawnInterval = 6f,
			SpawnCellOffset = new(1, 0),
			SoldierDamage = 8,
			SoldierAttackRange = 50f,
		},
		["Stable"] = new()
		{
			TypeId = "Stable",
			Footprint = StableCells,
			MainCellOffset = new(0, 1),
			DisplayName = "马厩",
			MaxHealth = 150,
			SpawnInterval = 5f,
			SpawnCellOffset = new(1, 2),
			SoldierSpeed = 120f,
		},
		["WolfDen"] = new()
		{
			TypeId = "WolfDen",
			Footprint = SingleCell,
			MainCellOffset = new(0, 0),
			DisplayName = "狼穴",
			MaxHealth = 90,
			SpawnInterval = 6f,
			SoldierDamage = 12,
			SoldierSpeed = 95f,
			SoldierHealth = 35,
			HasNightCombat = true,
		},
		["BarracksT2"] = new()
		{
			TypeId = "BarracksT2",
			Footprint = SingleCell,
			MainCellOffset = new(0, 0),
			DisplayName = "强化兵营",
			MaxHealth = 130,
			SpawnInterval = 4f,
			FusionTier = 1,
		},
		["WolfDenT2"] = new()
		{
			TypeId = "WolfDenT2",
			Footprint = SingleCell,
			MainCellOffset = new(0, 0),
			DisplayName = "强化狼穴",
			MaxHealth = 120,
			SpawnInterval = 5f,
			SoldierDamage = 16,
			SoldierSpeed = 95f,
			SoldierHealth = 35,
			HasNightCombat = true,
			FusionTier = 1,
		},
	};

	public static BuildingData Get(string typeId)
	{
		if (Data.TryGetValue(typeId, out BuildingData data))
			return data;
		return Data["Barracks"];
	}

	public static IReadOnlyList<GridCellOffset> GetFootprint(string typeId) => Get(typeId).Footprint;

	public static GridCellOffset GetMainCellOffset(string typeId) => Get(typeId).MainCellOffset;

	public static string GetDisplayName(string typeId) => Get(typeId).DisplayName;

	public static int GetMaxHealth(string typeId) => Get(typeId).MaxHealth;

	public static float GetSpawnInterval(string typeId) => Get(typeId).SpawnInterval;

	public static GridCellOffset GetSpawnCellOffset(string typeId) => Get(typeId).SpawnCellOffset;

	public static bool GetHasNightCombat(string typeId) => Get(typeId).HasNightCombat;

	public static int GetFusionTier(string typeId) => Get(typeId).FusionTier;

	public static bool IsCoreBuilding(string typeId) => typeId == "CastleHeart";

	public static bool IsFusibleMaterial(string typeId)
	{
		BuildingData data = Get(typeId);
		return data.FusionTier == 0
			&& data.Footprint.Length == 1
			&& !IsCoreBuilding(typeId);
	}
}
