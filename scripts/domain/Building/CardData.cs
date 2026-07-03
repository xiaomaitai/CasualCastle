namespace CasualCastle.Domain.Building;

public class CardData
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public int Cost { get; set; }
    public string BuildingType { get; set; } = "Barracks";

    public CardData Clone()
    {
        return new CardData
        {
            Id = Id,
            Name = Name,
            Cost = Cost,
            BuildingType = BuildingType,
        };
    }
}
