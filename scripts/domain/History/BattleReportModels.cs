using System.Collections.Generic;

namespace CasualCastle.Domain.History;

public class BuildingSnapshot
{
	public string TypeId { get; set; } = "";
	public int AnchorGridX { get; set; }
	public int AnchorGridY { get; set; }
	public int Health { get; set; }
	public bool IsManuallyPaused { get; set; }
	public bool IsFusionProhibited { get; set; }
}

public class CastleSnapshot
{
	public int NightIndex { get; set; }
	public List<BuildingSnapshot> Buildings { get; set; } = new();
}

public class BattleReport
{
	public string ReportId { get; set; } = "";
	public string DisplayName { get; set; } = "";
	public long SavedAtUnix { get; set; }
	public List<CastleSnapshot> Nights { get; set; } = new();
}
