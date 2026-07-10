using System.Collections.Generic;

namespace CasualCastle.Domain.Shared;

public class SaveData
{
    public int SlotIndex { get; set; }
    public List<BuildingSaveEntry> Buildings { get; set; } = new();
    public int Gold { get; set; }
    public int CurrentNightIndex { get; set; }
    public List<CardSaveEntry> HandCards { get; set; } = new();
    public string PendingReplayReportId { get; set; } = "";
    public string SaveTime { get; set; } = "";
}

public class BuildingSaveEntry
{
    public string TypeId { get; set; } = "";
    public int AnchorGridX { get; set; }
    public int AnchorGridY { get; set; }
    public int Health { get; set; }
}

public class CardSaveEntry
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public int Cost { get; set; }
    public string BuildingType { get; set; } = "";
    public int Weight { get; set; }
}
