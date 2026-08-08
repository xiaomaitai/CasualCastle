// 战报数据模型
using System.Collections.Generic;

namespace CasualCastle.Domain.History;

public enum BattlePhase
{
	Day,
	Night,
	Combine
}

public class BuildingSnapshot
{
	public string TypeId { get; set; } = "";
	public int AnchorGridX { get; set; }
	public int AnchorGridY { get; set; }
	public int Health { get; set; }
	public bool IsManuallyPaused { get; set; }
	public bool IsCombineProhibited { get; set; }
}

public class CardSnapshot
{
	public string BuildingType { get; set; } = "";
	public int Cost { get; set; }
}

public class CastleSnapshot
{
	public int NightIndex { get; set; }
	public BattlePhase Phase { get; set; }
	public List<BuildingSnapshot> Buildings { get; set; } = new();
	public int Gold { get; set; }
	public List<CardSnapshot> HandCards { get; set; } = new();
}

public class BattleReport
{
	public string ReportId { get; set; } = "";
	public string DisplayName { get; set; } = "";
	public string FactionId { get; set; } = "";
	public long SavedAtUnix { get; set; }
	public List<CastleSnapshot> Nights { get; set; } = new();
}
