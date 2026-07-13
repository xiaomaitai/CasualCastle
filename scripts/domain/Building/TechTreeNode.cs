namespace CasualCastle.Domain.Building;

public class TechTreeNode
{
    public string TypeId { get; init; }
    public string RaceId { get; set; }
    public int Tier { get; set; }
    public int Col { get; set; }
    public bool ShopAvailable { get; set; }
    public int GoldCost { get; set; }
    public int ShopWeight { get; set; }
    public int UnlockNight { get; set; }
    public string DisplayName { get; set; }
    public string UnitTypeId { get; init; }
    public int MaxHealth { get; set; }
    public float SpawnInterval { get; set; }
}
