using CasualCastle.Domain.Shared;

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
	public int CollisionWidth { get; init; } = GameCoordinateRules.CellBlockSize;
	public int CollisionHeight { get; init; } = GameCoordinateRules.CellBlockSize;
}
